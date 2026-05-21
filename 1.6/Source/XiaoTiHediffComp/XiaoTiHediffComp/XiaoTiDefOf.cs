using RimWorld;
using Verse;

namespace XiaoTiEquipment
{

    [DefOf]
    public static class XiaoTiDefOf
    {
        public static HediffDef Combat_Stimulant;
        public static HediffDef Cross_The_Bleach;
        public static HediffDef GreenPin_Repair;
        public static ThingDef Repair_GreenPin;

        static XiaoTiDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(XiaoTiDefOf));
            if (Combat_Stimulant == null)
            {
                Combat_Stimulant = DefDatabase<HediffDef>.GetNamed("Combat_Stimulant");
            }
            if (GreenPin_Repair == null)
            {
                GreenPin_Repair = DefDatabase<HediffDef>.GetNamed("GreenPin_Repair");
            }
        }
    }
}
