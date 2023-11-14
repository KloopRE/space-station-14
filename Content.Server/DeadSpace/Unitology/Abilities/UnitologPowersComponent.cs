using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Abilities.DeadSpace.Unitolog
{
    /// <summary>
    /// Lets its owner entity use Unitolog powers, like placing invisible walls.
    /// </summary>
    [RegisterComponent]
    public sealed partial class UnitologPowersComponent : Component
    {

        /// <summary>
        /// The wall prototype to use.
        /// </summary>
        [DataField("wallPrototype", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string WallPrototype = "StructureObelisk";

        [DataField("ObeliskAction", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string? ObeliskAction = "ActionUnitologObelisk";

        [DataField("ObeliskActionEntity")] public EntityUid? ObeliskActionEntity;


        [DataField("infectedDuration")]
        public TimeSpan Duration = TimeSpan.FromSeconds(60);

        public int countDeads = 3;
        public List<EntityUid> entityUidList = new();

        [DataField("Range")]
        public float range = 100f;

    }
}
