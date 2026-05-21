using HarmonyLib;
using RimWorld;
using Verse;

namespace XiaoTiEquipment
{
    [HarmonyPatch(typeof(TendUtility), "DoTend")]
    public static class Patch_Tend_ApplyHediff
    {
        public static void Postfix(Pawn doctor, Pawn patient, Medicine medicine)
        {
            if (medicine?.def != XiaoTiDefOf.Repair_GreenPin)
            {
                return;
            }

            HediffDef hediffDef = XiaoTiDefOf.GreenPin_Repair;
            if (hediffDef == null)
            {
                return;
            }

            if (patient.health.hediffSet.HasHediff(hediffDef))
            {
                return;
            }

            Hediff hediff = HediffMaker.MakeHediff(hediffDef, patient);
            patient.health.AddHediff(hediff);
        }
    }
}
