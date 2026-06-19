using System;
using System.Text;
using HarmonyLib;
using RimWorld;
using Verse;

namespace XiaoTiEquipment
{
    [HarmonyPatch(typeof(MassUtility), "Capacity")]
    [HarmonyPatch(new Type[] { typeof(Pawn), typeof(StringBuilder) })]
    public static class Patch_MassUtility_ExtraCaravanCapacity
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn p, ref float __result, StringBuilder explanation = null)
        {
            float statValue = p.GetStatValue(XiaoTiDefOf.XiaoTi_ExtraCaravanCarryCapacity);
            if (statValue != 0f)
            {
                if (explanation != null)
                {
                    explanation.Append(" (+" + statValue.ToString("F1") + " kg)");
                }
                __result += statValue;
            }
        }
    }
}
