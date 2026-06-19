using Verse;

namespace XiaoTiHediffComp.HediffComps
{
    public class HediffComp_ClearMentalState : HediffComp
    {
        private int tickCounter;

        private HediffCompProperties_ClearMentalState Props =>
            (HediffCompProperties_ClearMentalState)props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (++tickCounter >= Props.intervalTicks)
            {
                tickCounter = 0;
                TryClearMentalState();
            }
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            if (Props.clearwhenadd) TryClearMentalState();
        }

        private void TryClearMentalState()
        {
            Pawn pawn = parent.pawn;
            if (pawn?.mindState?.mentalStateHandler == null)
                return;
            if (pawn.mindState.mentalStateHandler.InMentalState)
                pawn.MentalState.RecoverFromState();
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref tickCounter, "tickCounter");
        }
    }
}
