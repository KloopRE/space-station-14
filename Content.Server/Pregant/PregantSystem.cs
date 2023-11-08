using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Audio;
using Content.Shared.Pregant.Components;

namespace Content.Server.Pregant
{
    public sealed class PregantSystem : EntitySystem
    {
        [Dependency] private readonly MobStateSystem _mobState = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<PregantComponent, MobStateChangedEvent>(OnDamage);
        }

        private void OnDamage(EntityUid uid, PregantComponent component, MobStateChangedEvent args)
        {
            if (_mobState.IsDead(uid))
            {
                _audio.PlayPvs("/Audio/Effects/Fluids/splat.ogg", uid, AudioParams.Default.WithVariation(0.2f).WithVolume(1f));
                Spawn(component.ArmyMobSpawnId, Transform(uid).Coordinates);
            }
        }
    }
}
