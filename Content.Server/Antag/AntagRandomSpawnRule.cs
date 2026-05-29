using Content.Server.Antag.Components;
using Content.Server.GameTicking.Rules;

namespace Content.Server.Antag;

public sealed class AntagRandomSpawnSystem : GameRuleSystem<AntagRandomSpawnComponent>
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AntagRandomSpawnComponent, AntagSelectLocationEvent>(OnSelectLocation);
    }

    private void OnSelectLocation(Entity<AntagRandomSpawnComponent> ent, ref AntagSelectLocationEvent args)
    {
        if (ent.Comp.Coords == null && TryFindRandomTile(out _, out _, out _, out var coords))
            ent.Comp.Coords = coords;

        if (TryFindRandomTile(out _, out _, out _, out var coords))
            args.Coordinates.Add(_transform.ToMapCoordinates(coords));
    }
}
