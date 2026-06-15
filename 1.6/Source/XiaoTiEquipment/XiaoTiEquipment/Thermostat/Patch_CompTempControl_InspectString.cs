using HarmonyLib;
using RimWorld;

namespace XiaoTiEquipment
{
    [HarmonyPatch(typeof(CompTempControl), "CompInspectStringExtra")]
    public static class Patch_CompTempControl_InspectString
    {
        public static void Postfix(ref string __result)
        {
            __result = __result.TrimEnd();
        }
    }
}
