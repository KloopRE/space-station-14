
using System.Numerics;
using Content.Server.Body.Systems;
using Content.Shared.DeadSpace.Necroobelisk.Components;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Content.Shared.DeadSpace.InfectionDead.Components;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Random;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Configuration;
using Content.Shared.CCVar;
using Robust.Shared.Map.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Station.Systems;
using Content.Server.Station.Components;
using Content.Server.DeadSpace.InfectionDead;
using Content.Shared.Humanoid;
using Content.Shared.Mobs.Components;
using Content.Shared.DeadSpace.Unitology.Components;
using Content.Shared.Damage;
using Content.Shared.Destructible;
using Content.Shared.Audio;
using Robust.Shared.Player;
using Content.Server.Station.Systems;

namespace Content.Server.DeadSpace.Necroobelisk;

public sealed class TileNecroobeliskSystem : EntitySystem
{
    [Dependency] private readonly IMapManager _map = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ITileDefinitionManager _tiledef = default!;
    [Dependency] private readonly TileSystem _tile = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] protected readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly BodySystem _bodySystem = default!;
    [Dependency] private readonly IConfigurationManager _configuration = default!;
    [Dependency] private readonly AtmosphereSystem _atmosphere = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly StationSystem _stationSystem = default!;
    [Dependency] private readonly InfectionDeadSystem _infection = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NecroobeliskComponent, SanityCheckEvent>(DoSanity);
        SubscribeLocalEvent<NecroobeliskComponent, NecroobeliskSpawnArmyEvent>(DoArmy);

        SubscribeLocalEvent<NecroobeliskComponent, NecroobeliskPulseEvent>(OnSeverityChanged);
        SubscribeLocalEvent<NecroobeliskComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<NecroobeliskComponent, DestructionEventArgs>(OnDestr);
    }

    private void OnDestr(EntityUid uid, NecroobeliskComponent component, DestructionEventArgs args)
    {
       // _audio.Shutdown();
       // _audio.PlayPvs("/Audio/DeadSpace/Necromorfs/smash_through_wall_12.ogg", uid, AudioParams.Default.WithVariation(1f).WithVolume(-4f));
        var msg = new GameGlobalSoundEvent("/Audio/DeadSpace/Necromorfs/examobelisk.ogg", AudioParams.Default.WithVariation(1f).WithVolume(-5f));
        var stationFilter = _stationSystem.GetInOwningStation(uid);
        stationFilter.AddPlayersByPvs(uid, entityManager: EntityManager);
        RaiseNetworkEvent(msg, stationFilter);

        //_audio.PlayGlobal("/Audio/DeadSpace/Necromorfs/examobelisk.ogg", Filter.Local(), false, AudioParams.Default.WithVariation(1f).WithVolume(-5f));
    }

    private void OnMapInit(EntityUid uid, NecroobeliskComponent component, MapInitEvent args)
    {
        var xform = Transform(uid);
        Spawn(component.SupercriticalSpawn, xform.Coordinates);

        var msg = new GameGlobalSoundEvent("/Audio/DeadSpace/Necromorfs/markersound.ogg", AudioParams.Default.WithVariation(1f).WithVolume(-5f));
        var stationFilter = _stationSystem.GetInOwningStation(uid);
        stationFilter.AddPlayersByPvs(uid, entityManager: EntityManager);
        RaiseNetworkEvent(msg, stationFilter);
    }

    private void OnSeverityChanged(EntityUid uid, NecroobeliskComponent component, ref NecroobeliskPulseEvent args)
    {

        if (_mobState.IsDead(uid))
            {
                return;
            }

        var xform = Transform(uid);
        if (!_map.TryGetGrid(xform.GridUid, out var grid))
            return;

        var radius = component.SpawnRange;
        var fleshTile = (ContentTileDefinition) _tiledef[component.FloorTileId];
        var localpos = xform.Coordinates.Position;
        var tilerefs = grid.GetLocalTilesIntersecting(
        new Box2(localpos + new Vector2(-radius, -radius), localpos + new Vector2(radius, radius)));
        foreach (var tileref in tilerefs)
        {
            if (!_random.Prob(0.33f))
                continue;
            _tile.ReplaceTile(tileref, fleshTile);
        }
    }

    private void DoSanity(EntityUid uid, NecroobeliskComponent component, ref SanityCheckEvent args)
    {
        if (!HasComp<MobStateComponent>(args.victinUID) || !HasComp<HumanoidAppearanceComponent>(args.victinUID))
        {return;}

        if(HasComp<UnitologyComponent>(args.victinUID))
        {return;}

        DamageSpecifier dspec = new();
        dspec.DamageDict.Add("Cellular", 5f);
        _damage.TryChangeDamage(args.victinUID, dspec, true, false);

        if(HasComp<ImmunitetInfectionDeadComponent>(args.victinUID))
        {return;}

        if (HasComp<InfectionDeadComponent>(args.victinUID))
            {return;}
        EnsureComp<InfectionDeadComponent>(args.victinUID);

        if (_mobState.IsDead(args.victinUID))
        {
        _audio.PlayPvs("/Audio/Effects/Fluids/splat.ogg", args.victinUID, AudioParams.Default.WithVariation(0.2f).WithVolume(-4f));
        _infection.Drop(args.victinUID, args.victinUID);
        InfectionDeadComponent comp = new InfectionDeadComponent();
        QueueDel(args.victinUID);
        _infection.SpawnMob(args.victinUID, comp);
        return;
        }


    }

    private void DoArmy(EntityUid uid, NecroobeliskComponent component, ref NecroobeliskSpawnArmyEvent args)
    {

        var xform = Transform(uid);

        if (_station.GetStationInMap(xform.MapID) is not { } station ||
            !TryComp<StationDataComponent>(station, out var data) ||
            _station.GetLargestGrid(data) is not { } grid)
        {
            if (xform.GridUid == null)
                return;
            grid = xform.GridUid.Value;
        }

        if (component.Pulselvl > 9 && component.Pulselvl < 10)
        {
            SpawnOnRandomGridLocation(grid, "MobSlasher");
        }

        if (component.Pulselvl > 13 && component.Pulselvl < 15)
        {
            SpawnOnRandomGridLocation(grid, "MobTwitcher");
        }

        if (component.Pulselvl > 19 && component.Pulselvl < 20)
        {
            SpawnOnRandomGridLocation(grid, "MobInfector");
        }

        if (component.Pulselvl >= 49 && component.Pulselvl < 50)
        {

            SpawnOnRandomGridLocation(grid, "MobBrute");
            component.Pulselvl = 0;
        }

        if (component.Pulselvl >= 50)
        {

            SpawnOnRandomGridLocation(grid, "MobSlasher");
            component.Pulselvl = 0;
        }
    }


    public void SpawnOnRandomGridLocation(EntityUid grid, string toSpawn)
    {
        if (!TryComp<MapGridComponent>(grid, out var gridComp))
            return;

        var xform = Transform(grid);

        var targetCoords = xform.Coordinates;
        var gridBounds = gridComp.LocalAABB.Scale(_configuration.GetCVar(CCVars.AnomalyGenerationGridBoundsScale));

        for (var i = 0; i < 25; i++)
        {
            var randomX = _random.Next((int) gridBounds.Left, (int) gridBounds.Right);
            var randomY = _random.Next((int) gridBounds.Bottom, (int) gridBounds.Top);

            var tile = new Vector2i(randomX, randomY);

            // no air-blocked areas.
            if (_atmosphere.IsTileSpace(grid, xform.MapUid, tile, mapGridComp: gridComp) ||
                _atmosphere.IsTileAirBlocked(grid, tile, mapGridComp: gridComp))
            {
                continue;
            }

            // don't spawn inside of solid objects
            var physQuery = GetEntityQuery<PhysicsComponent>();
            var valid = true;
            foreach (var ent in gridComp.GetAnchoredEntities(tile))
            {
                if (!physQuery.TryGetComponent(ent, out var body))
                    continue;
                if (body.BodyType != BodyType.Static ||
                    !body.Hard ||
                    (body.CollisionLayer & (int) CollisionGroup.Impassable) == 0)
                    continue;

                valid = false;
                break;
            }
            if (!valid)
                continue;

            targetCoords = gridComp.GridTileToLocal(tile);
            break;
        }

        Spawn(toSpawn, targetCoords);
    }

}
