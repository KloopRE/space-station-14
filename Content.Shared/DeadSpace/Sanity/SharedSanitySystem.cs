using Content.Shared.DeadSpace.Sanity.Components;

using Robust.Shared.Timing;


namespace Content.Shared.DeadSpace.Sanity;

public sealed class SharedSanitySystem : EntitySystem
{
    [Dependency] protected readonly IGameTiming Timing = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<SanityComponent, EntityUnpausedEvent>(OnSanityUnpause);
    }

    private void OnSanityUnpause(EntityUid uid, SanityComponent component, ref EntityUnpausedEvent args)
    {
        component.NextCheckTime += args.PausedTime;
        Dirty(component);
    }

    public void DoSanityCheck(EntityUid uid, SanityComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (!Timing.IsFirstTimePredicted)
            return;

        component.NextCheckTime = Timing.CurTime + component.CheckDuration;
        var ev = new SanityEvent();
        RaiseLocalEvent(uid, ref ev);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var SanityQuery = EntityQueryEnumerator<SanityComponent>();
        while (SanityQuery.MoveNext(out var ent, out var sanity))
        {
            if (Timing.CurTime > sanity.NextCheckTime)
            {
                DoSanityCheck(ent, sanity);
            }
        }
    }
}
