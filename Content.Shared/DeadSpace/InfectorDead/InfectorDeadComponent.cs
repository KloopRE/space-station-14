
using Robust.Shared.GameStates;


namespace Content.Shared.DeadSpace.InfectorDead.Components
{

    [RegisterComponent, NetworkedComponent]
    public sealed partial class InfectorDeadComponent : Component
    {
        [DataField("infectedDuration")]
        public float InfectedDuration = 2.5f;

    }

}
