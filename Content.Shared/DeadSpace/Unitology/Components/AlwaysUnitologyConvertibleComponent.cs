using Robust.Shared.GameStates;

namespace Content.Shared.DeadSpace.Unitology.Components;

/// <summary>
/// Component used for allowing non-humans to be converted. (Mainly monkeys)
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedUnitologySystem))]
public sealed partial class AlwaysUnitologyConvertibleComponent : Component
{
}
