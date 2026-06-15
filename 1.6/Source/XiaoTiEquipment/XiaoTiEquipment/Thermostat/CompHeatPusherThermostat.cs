using RimWorld;
using Verse;

namespace XiaoTiEquipment
{
    public class CompHeatPusherThermostat : ThingComp
    {
        private const int HeatPushInterval = 60;

        private CompTempControl tempControl;

        public CompProperties_HeatPusherThermostat Props => (CompProperties_HeatPusherThermostat)props;

        private float HeatToPush
        {
            get
            {
                float ambient = parent.AmbientTemperature;
                float target = TempControl.targetTemperature;
                float tolerance = Props.tempTolerance;
                if (ambient < target - tolerance)
                {
                    return Props.heatPerSecond;
                }
                return -Props.coolPerSecond;
            }
        }

        public CompTempControl TempControl
        {
            get
            {
                if (tempControl == null)
                {
                    tempControl = parent.GetComp<CompTempControl>();
                }
                return tempControl;
            }
        }

        public bool ShouldPushHeatNow
        {
            get
            {
                if (!parent.SpawnedOrAnyParentSpawned)
                {
                    return false;
                }
                if (TempControl == null)
                {
                    return false;
                }
                float ambientTemperature = parent.AmbientTemperature;
                float target = TempControl.targetTemperature;
                float tolerance = Props.tempTolerance;
                return ambientTemperature < target - tolerance || ambientTemperature > target + tolerance;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (parent.IsHashIntervalTick(HeatPushInterval))
            {
                if (ShouldPushHeatNow)
                {
                    GenTemperature.PushHeat(parent.PositionHeld, parent.MapHeld, HeatToPush);
                    TempControl.operatingAtHighPower = true;
                }
                else if (TempControl != null)
                {
                    TempControl.operatingAtHighPower = false;
                }
            }
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            if (ShouldPushHeatNow)
            {
                GenTemperature.PushHeat(parent.PositionHeld, parent.MapHeld, HeatToPush * 4.1666665f);
                if (TempControl != null)
                {
                    TempControl.operatingAtHighPower = true;
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
        }
    }
}
