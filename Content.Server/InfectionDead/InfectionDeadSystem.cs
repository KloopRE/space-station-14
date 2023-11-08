
using Robust.Server.GameObjects;
using Content.Server.Body.Systems;
using Content.Shared.Damage;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Audio;
using Content.Shared.InfectionDead.Components;
using Content.Shared.Inventory;


namespace Content.Server.InfectionDead
{
    public sealed class InfectionDeadSystem : EntitySystem
    {
        [Dependency] private readonly BodySystem _bodySystem = default!;
        [Dependency] private readonly TransformSystem _xform = default!;
        [Dependency] private readonly MobStateSystem _mobState = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        [Dependency] private readonly DamageableSystem _damage = default!;
        [Dependency] private readonly InventorySystem _inventory = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<InfectionDeadComponent, InfectionDeadDamageEvent>(GetDamage);
            SubscribeLocalEvent<InfectionDeadComponent, MobStateChangedEvent>(OnState);
        }

        private void GetDamage(EntityUid uid, InfectionDeadComponent component, ref InfectionDeadDamageEvent args)
        {
            if (_mobState.IsDead(uid))
                {return;}
            DamageSpecifier dspec = new();
            dspec.DamageDict.Add("Cellular", 15f);
            _damage.TryChangeDamage(uid, dspec, true, false);
        }

        private void OnState(EntityUid uid, InfectionDeadComponent component, MobStateChangedEvent args)
        {
            if (_mobState.IsDead(uid))
                {
                   // DamageSpecifier dspec = new();
                   // dspec.DamageDict.Add("Slash", 200f);
                   // _damage.TryChangeDamage(uid, dspec, true, false);
                    _audio.PlayPvs("/Audio/Effects/Fluids/splat.ogg", uid, AudioParams.Default.WithVariation(0.2f).WithVolume(-4f));
                    //_bodySystem.GibBody(uid);
                    Drop(uid, uid);
                    QueueDel(uid);
                    Spawn(component.ArmyMobSpawnId, Transform(uid).Coordinates);
                }
        }


        public void Drop(EntityUid uid, EntityUid target)
        {
            if (!_inventory.TryGetContainerSlotEnumerator(uid, out var enumerator))
                return;

            Dictionary<string, EntityUid> inventoryEntities = new();
            var slots = _inventory.GetSlots(uid);
            while (enumerator.MoveNext(out var containerSlot))
            {
                foreach (var slot in slots)
                {
                    if (_inventory.TryGetSlotContainer(target, slot.Name, out var conslot, out _) &&
                        conslot.ID == containerSlot.ID &&
                        containerSlot.ContainedEntity is { } containedEntity)
                    {
                        inventoryEntities.Add(slot.Name, containedEntity);
                    }
                }
                _inventory.TryUnequip(uid, containerSlot.ID, true, true);
            }
        }


    }
}
