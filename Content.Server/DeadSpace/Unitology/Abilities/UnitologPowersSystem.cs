using Content.Server.Popups;
using Content.Server.Speech.Muting;
using Content.Shared.Actions;
using Content.Shared.Actions.Events;
using Content.Shared.Alert;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Maps;
using Content.Shared.Mobs.Components;
using Content.Shared.Physics;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Timing;
using Content.Shared.DeadSpace.Necroobelisk.Components;
using Content.Shared.Humanoid;
using Content.Shared.Mobs.Systems;
using Content.Shared.DoAfter;
using Content.Shared.DeadSpace.UnitologyPowerSystem;
using Content.Shared.Popups;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.DeadSpace.Unitology.Components;
using Content.Shared.Coordinates;
using Content.Server.Body.Systems;

namespace Content.Server.Abilities.DeadSpace.Unitolog
{
    public sealed class UnitologPowersSystem : EntitySystem
    {
        [Dependency] private readonly MobStateSystem _mobState = default!;
        [Dependency] private readonly PopupSystem _popupSystem = default!;
        [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
        [Dependency] private readonly AlertsSystem _alertsSystem = default!;
        [Dependency] private readonly EntityLookupSystem _lookupSystem = default!;
        [Dependency] private readonly TurfSystem _turf = default!;
        [Dependency] private readonly IMapManager _mapMan = default!;
        [Dependency] private readonly SharedContainerSystem _container = default!;
        [Dependency] private readonly IGameTiming _timing = default!;
        [Dependency] private readonly EntityLookupSystem _lookup = default!;
        [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
        [Dependency] private readonly SharedMindSystem _mindSystem = default!;
        [Dependency] private readonly BodySystem _body = default!;
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<UnitologPowersComponent, ComponentInit>(OnComponentInit);
            SubscribeLocalEvent<UnitologPowersComponent, ObeliskActionEvent>(OnInvisibleWall);

            SubscribeLocalEvent<UnitologPowersComponent, ObeliskSpawnDoAfterEvent>(OnDoAfter);
        }

        private void OnComponentInit(EntityUid uid, UnitologPowersComponent component, ComponentInit args)
        {
            _actionsSystem.AddAction(uid, ref component.ObeliskActionEntity, component.ObeliskAction, uid);
        }

        /// <summary>
        /// Creates an invisible wall in a free space after some checks.
        /// </summary>
        private void OnInvisibleWall(EntityUid uid, UnitologPowersComponent component, ObeliskActionEvent args)
        {


            if (_container.IsEntityOrParentInContainer(uid))
                return;


            int count = 0;
            var victims = _lookup.GetEntitiesInRange(uid, 3f);
            foreach(var victinUID in victims)
            {
                if (EntityManager.HasComponent<HumanoidAppearanceComponent>(victinUID))
                {
                if (_mobState.IsDead(victinUID))
                {

                        count += 1;
                        component.entityUidList.Add(victinUID);
                        if(count >= component.countDeads)
                        {
                            BeginSpawn(uid, victinUID, component);
                            args.Handled = true;
                            return;
                        }

                }

                }
            }

            _popupSystem.PopupEntity(Loc.GetString("Вы должны принести три жертвы, чтобы призвать обелиск"), uid, uid);
            return;
        }


         private void BeginSpawn(EntityUid uid, EntityUid target, UnitologPowersComponent component)
        {

                    var searchDoAfter = new DoAfterArgs(EntityManager, uid, component.Duration, new ObeliskSpawnDoAfterEvent(), uid, target: target)
                    {
                        DistanceThreshold = 3,
                        BreakOnUserMove = true
                    };



                    if (!_doAfter.TryStartDoAfter(searchDoAfter))
                        return;


        }

        private void OnDoAfter(EntityUid uid, UnitologPowersComponent component, ObeliskSpawnDoAfterEvent args)
        {
            if (args.Cancelled || args.Handled || args.Args.Target == null)
            return;

            foreach (var entityUid in component.entityUidList)
            {
                QueueDel(entityUid);
            }
            Spawn(component.WallPrototype, Transform(args.Args.Target.Value).Coordinates);

            if (!_mindSystem.TryGetMind(uid, out var mindIduid, out var minduid))
                return;
            _body.GibBody(uid);
            var ent = Spawn("MobNecromant", Transform(uid).Coordinates);
            _mindSystem.TransferTo(mindIduid, ent, mind: minduid);

            var victims = _lookup.GetEntitiesInRange(uid, component.range);

            foreach(var vict in victims)
            {
                if(HasComp<UnitologyComponent>(vict) && HasComp<HumanoidAppearanceComponent>(vict))
                {
                    if (!_mindSystem.TryGetMind(vict, out var mindId, out var mind))
                    return;
                    _body.GibBody(vict);
                    ent = Spawn("MobTwitcherlvl2", Transform(vict).Coordinates);
                    _mindSystem.TransferTo(mindId, ent, mind: mind);
                }
            }
            _actionsSystem.RemoveAction(uid, component.ObeliskActionEntity);

        }

    }
}
