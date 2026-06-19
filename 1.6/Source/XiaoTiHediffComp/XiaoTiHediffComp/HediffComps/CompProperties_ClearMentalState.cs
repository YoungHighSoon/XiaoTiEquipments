using Verse;

namespace XiaoTiHediffComp.HediffComps
{
    public class HediffCompProperties_ClearMentalState : HediffCompProperties
    {
        public int intervalTicks = 2500; // 2500ticks = 1 hour in game
        public bool clearwhenadd = false;

        public HediffCompProperties_ClearMentalState()
        {
            compClass = typeof(HediffComp_ClearMentalState);
        }
    }
}
