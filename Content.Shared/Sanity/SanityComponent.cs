using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Sanity.Components
{
    [RegisterComponent, NetworkedComponent]
    public sealed partial class SanityComponent : Component
    {
        [ViewVariables]
        public int lvl = 100;

        [ViewVariables(VVAccess.ReadWrite)]
        public TimeSpan CheckDuration = TimeSpan.FromSeconds(5);

        [DataField("nextCheckTime", customTypeSerializer: typeof(TimeOffsetSerializer)), ViewVariables(VVAccess.ReadWrite)]
        public TimeSpan NextCheckTime = TimeSpan.Zero;
    }

    [ByRefEvent]
    public readonly record struct SanityEvent();
}
