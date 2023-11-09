using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.DeadSpace.NecroobeliskStoper;
using Content.Shared.DeadSpace.Necroobelisk.Components;

namespace Content.Server.DeadSpace.Necroobelisk;

/// <summary>
/// This handles the anomaly scanner and it's UI updates.
/// </summary>
public sealed class NecroobeliskStoperSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] protected readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NecroobeliskStoperComponent, AfterInteractEvent>(OnScannerAfterInteract); //AfterInteractEvent
        SubscribeLocalEvent<NecroobeliskStoperComponent, NecroobeliskStoperDoAfterEvent>(OnDoAfter);
    }

    private void OnScannerAfterInteract(EntityUid uid, NecroobeliskStoperComponent component, AfterInteractEvent args)
    {
        if (args.Target is not { } target)
            return;
        if (!HasComp<NecroobeliskComponent>(target))
            return;

        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, args.User, component.ScanDoAfterDuration, new NecroobeliskStoperDoAfterEvent(), uid, target: target, used: uid)
        {
            DistanceThreshold = 2f
        });
    }

    private void OnDoAfter(EntityUid uid, NecroobeliskStoperComponent component, DoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Args.Target == null)
            return;

        if (!EntityManager.TryGetComponent<NecroobeliskComponent>(args.Args.Target.Value, out var xform))
            return;

        xform.Active -= 1;

        _audio.PlayPvs(component.CompleteSound, uid);

        //QueueDel(args.Args.Target.Value);
        QueueDel(uid);

        args.Handled = true;
    }
}
