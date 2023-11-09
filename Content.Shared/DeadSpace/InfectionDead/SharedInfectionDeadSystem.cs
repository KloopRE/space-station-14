
using Content.Shared.DeadSpace.InfectionDead.Components;
using Robust.Shared.Timing;



namespace Content.Shared.DeadSpace.InfectionDead;

public sealed class SharedInfectionDeadSystem : EntitySystem
{
    [Dependency] protected readonly IGameTiming Timing = default!;


    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<InfectionDeadComponent, EntityUnpausedEvent>(OnInfectionDeadUnpause);

    }



    private void OnInfectionDeadUnpause(EntityUid uid, InfectionDeadComponent component, ref EntityUnpausedEvent args)
    {
        component.NextDamageTime += args.PausedTime;
        Dirty(component);
    }

    public void DoInfectionDeadDamage(EntityUid uid, InfectionDeadComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (!Timing.IsFirstTimePredicted)
            return;

        component.NextDamageTime = Timing.CurTime + component.DamageDuration;


        var ev = new InfectionDeadDamageEvent();
        RaiseLocalEvent(uid, ref ev);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var infectionDeadQuery = EntityQueryEnumerator<InfectionDeadComponent>();
        while (infectionDeadQuery.MoveNext(out var ent, out var infectionDead))
        {
            // if the stability is under the death threshold,
            // update it every second to start killing it slowly.

            if (Timing.CurTime > infectionDead.NextDamageTime)
            {
                DoInfectionDeadDamage(ent, infectionDead);
            }
        }

    }

}
