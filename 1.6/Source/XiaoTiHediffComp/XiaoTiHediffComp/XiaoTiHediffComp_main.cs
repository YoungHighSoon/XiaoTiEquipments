using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace XiaoTiEquipment
{
    // 主模组类，负责应用 Harmony 补丁
    public class XiaoTiHediffComp_main : Mod
    {
        // Harmony 实例
        private static Harmony harmony;

        public XiaoTiHediffComp_main(ModContentPack content) : base(content)
        {
            try
            {
                // 初始化日志
                Log.Message("[XiaoTiDeathRefused] 模组初始化开始");

                // 确保 DefOf 初始化
               //Log.Message($"[XiaoTiDeathRefused] 检查 Combat_Stimulant，当前值: {XiaoTiDefOf.Combat_Stimulant?.defName ?? "null"}");
                //if (XiaoTiDefOf.Combat_Stimulant == null)
                //{
                //    Log.Error("[XiaoTiDeathRefused] Combat_Stimulant HediffDef 未加载！");
                //    // 尝试直接从 DefDatabase 获取
                //    HediffDef def = DefDatabase<HediffDef>.GetNamed("Combat_Stimulant", false);
                //    Log.Message($"[XiaoTiDeathRefused] DefDatabase 查找结果: {def?.defName ?? "null"}");
                //}
                //else
                //{
                //    Log.Message($"[XiaoTiDeathRefused] Combat_Stimulant HediffDef 已加载: {XiaoTiDefOf.Combat_Stimulant.defName}");
                //}

                // 应用 Harmony 补丁
                harmony = new Harmony("XiaoTiEquipment.DeathRefused");
                harmony.PatchAll();

                //Log.Message("[XiaoTiDeathRefused] Harmony 补丁应用完成");
                Log.Message("[XiaoTiDeathRefused] 死亡拦截功能已启用 - 当 pawn 拥有 Combat_Stimulant Hediff 时死亡将被阻止并治愈");
            }
            catch (Exception ex)
            {
                Log.Error($"[XiaoTiDeathRefused] 初始化失败: {ex}");
            }
        }
    }
}