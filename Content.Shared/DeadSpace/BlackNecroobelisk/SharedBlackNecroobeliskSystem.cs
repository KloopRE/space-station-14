using Content.Shared.DeadSpace.BlackNecroobelisk.Components;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Robust.Shared.Network;
using Robust.Shared.Utility;
using Robust.Shared.Audio;
using Content.Shared.DeadSpace.Sanity.Components;

namespace Content.Shared.DeadSpace.BlackNecroobelisk;

public sealed class SharedBlackNecroobeliskSystem : EntitySystem
{
    [Dependency] protected readonly IGameTiming Timing = default!;
    [Dependency] protected readonly SharedAudioSystem Audio = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] protected readonly IRobustRandom Random = default!;
    [Dependency] protected readonly ISharedAdminLogManager Log = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BlackNecroobeliskComponent, EntityUnpausedEvent>(OnBlackNecroobeliskUnpause);

        _sawmill = Logger.GetSawmill("blacknecroobelisk");
    }

    private void OnBlackNecroobeliskUnpause(EntityUid uid, BlackNecroobeliskComponent component, ref EntityUnpausedEvent args)
    {
        component.NextPulseTime += args.PausedTime;
        component.NextCheckTimeSanity += args.PausedTime;
        Dirty(component);
    }

    public void DoSanityCheck(EntityUid uid, BlackNecroobeliskComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (!Timing.IsFirstTimePredicted)
            return;

        // _appearance.SetData(uid, RevenantVisuals.Harvesting, true);
        var victims = _lookup.GetEntitiesInRange(uid, component.RangeSanity);

        foreach(var victinUID in victims)
        {
            if (EntityManager.HasComponent<SanityComponent>(victinUID))
            {
                if (!EntityManager.TryGetComponent<SanityComponent>(victinUID, out var xform))
                return;


                //_popup.PopupEntity(Loc.GetString(a.ToString()), victinUID);
                if(component.Active >= 1)
                {
                    xform.lvl -= 1;
                }

                if (xform.lvl <= 0)
                {
                    var ev2 = new SanityCheckEvent(victinUID);
                    RaiseLocalEvent(uid, ref ev2);
                }
            }
        }
        component.NextCheckTimeSanity = Timing.CurTime + component.CheckDurationSanity;

    }

    public void DoBlackNecroobeliskPulse(EntityUid uid, BlackNecroobeliskComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (!Timing.IsFirstTimePredicted)
            return;

        DebugTools.Assert(component.MinPulseLength > TimeSpan.FromSeconds(3)); // this is just to prevent lagspikes mispredicting pulses
        var variation = Random.NextFloat(-component.PulseVariation, component.PulseVariation) + 1;
        component.NextPulseTime = Timing.CurTime + GetPulseLength(component) * variation;

        if (component.Active >= 1)
        {
        component.Pulselvl += 1;


        if (_net.IsServer)
            _sawmill.Info($"Performing BlackNecroobelisk pulse. Entity: {ToPrettyString(uid)}");

        Log.Add(LogType.Anomaly, LogImpact.Medium, $"BlackNecroobelisk {ToPrettyString(uid)}.");
        if (_net.IsServer)
            Audio.PlayPvs("/Audio/DeadSpace/Necromorfs/obelisk4.ogg", uid, AudioParams.Default.WithVariation(0.2f).WithVolume(15f));

        var ev1 = new BlackNecroobeliskSpawnArmyEvent();
        RaiseLocalEvent(uid, ref ev1);

        var ev = new BlackNecroobeliskPulseEvent();
        RaiseLocalEvent(uid, ref ev);
        }
    }

    /// </remarks>
    /// <param name="component"></param>
    /// <returns>The length of time as a TimeSpan, not including random variation.</returns>
    public TimeSpan GetPulseLength(BlackNecroobeliskComponent component)
    {
        DebugTools.Assert(component.MaxPulseLength > component.MinPulseLength);
        return (component.MaxPulseLength - component.MinPulseLength) * 1 + component.MinPulseLength;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var blacknecroobeliskQuery = EntityQueryEnumerator<BlackNecroobeliskComponent>();
        while (blacknecroobeliskQuery.MoveNext(out var ent, out var blacknecroobelisk))
        {
            // if the stability is under the death threshold,
            // update it every second to start killing it slowly.
            if (Timing.CurTime > blacknecroobelisk.NextPulseTime)
            {
                DoBlackNecroobeliskPulse(ent, blacknecroobelisk);
            }
            if (Timing.CurTime > blacknecroobelisk.NextCheckTimeSanity)
            {
                DoSanityCheck(ent, blacknecroobelisk);
            }
        }
    }
}
