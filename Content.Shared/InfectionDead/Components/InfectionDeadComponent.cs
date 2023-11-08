using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using System.Numerics;
using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Content.Shared.Maps;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Shared.InfectionDead.Components
{
    [RegisterComponent, NetworkedComponent, Access(typeof(SharedInfectionDeadSystem))]
    public sealed partial class InfectionDeadComponent : Component
    {

        [DataField("endTime", customTypeSerializer: typeof(TimeOffsetSerializer)), ViewVariables(VVAccess.ReadWrite)]
        public TimeSpan EndTime;

        /// <summary>
        /// How long the Damage visual lasts
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
         public TimeSpan DamageDuration = TimeSpan.FromSeconds(60);


        [DataField("nextDamageTime", customTypeSerializer: typeof(TimeOffsetSerializer)), ViewVariables(VVAccess.ReadWrite)]
        public TimeSpan NextDamageTime = TimeSpan.Zero;


        [ViewVariables(VVAccess.ReadWrite), DataField("divaderMobSpawnId", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string DivaderMobSpawnId = "MobDivader";

        public float shanceMobSpawnDivader = 0.1f;

        [ViewVariables(VVAccess.ReadWrite), DataField("twitcherMobSpawnId", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string TwitcherMobSpawnId = "MobTwitcher";

        public float shanceMobSpawnTwitcher = 0.3f;

        [ViewVariables(VVAccess.ReadWrite), DataField("pregnantMobSpawnId", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string PregnantMobSpawnId = "MobPregnant";

        public float shanceMobSpawnPregnant = 0.2f;

        [ViewVariables(VVAccess.ReadWrite), DataField("slasherMobSpawnId", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string SlasherMobSpawnId = "MobSlasher";

        public float shanceMobSpawnSlasher = 0.4f;

    }

    [Serializable, NetSerializable]
    public sealed class InfectionDeadComponentState : ComponentState
    {
        public TimeSpan NextDamageTime;
    }

    [ByRefEvent]

    public readonly record struct InfectionDeadDamageEvent();
}
