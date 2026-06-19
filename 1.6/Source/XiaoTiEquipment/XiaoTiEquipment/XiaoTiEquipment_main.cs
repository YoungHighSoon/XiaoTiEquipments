using HarmonyLib;
using Verse;

namespace XiaoTiEquipment
{
    public class XiaoTiEquipment_main : Mod
    {
        private static Harmony harmony;

        public XiaoTiEquipment_main(ModContentPack content) : base(content)
        {
            harmony = new Harmony("XiaoTiEquipment.Thermostat");
            harmony.PatchAll();
        }
    }
}
