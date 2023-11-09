
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;

namespace Content.Server.DeadSpace.Divader
{
    public sealed class DivaderSystem : EntitySystem
    {
        [Dependency] private readonly MobStateSystem _mobState = default!;


        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<DivaderComponent, MobStateChangedEvent>(OnState);
        }

        private void OnState(EntityUid uid, DivaderComponent component, MobStateChangedEvent args)
        {
            if (_mobState.IsDead(uid))
            {
                Spawn(component.RHMobSpawnId, Transform(uid).Coordinates);
                Spawn(component.HMobSpawnId, Transform(uid).Coordinates);
                Spawn(component.LHMobSpawnId, Transform(uid).Coordinates);
             }
        }
    }
}
