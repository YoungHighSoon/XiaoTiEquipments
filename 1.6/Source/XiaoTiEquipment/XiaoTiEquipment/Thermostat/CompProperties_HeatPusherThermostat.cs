using Verse;

namespace XiaoTiEquipment
{
    public class CompProperties_HeatPusherThermostat : CompProperties
    {
        public float heatPerSecond = 21f;
        public float coolPerSecond = 21f;
        public float tempTolerance = 1f;

        public CompProperties_HeatPusherThermostat()
        {
            compClass = typeof(CompHeatPusherThermostat);
        }
    }
}
