using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;
using XiaoTiEquipment;

namespace XiaoTiEquipment
{
    //[HarmonyPatch(typeof(Pawn), "DeSpawn")]
    //public static class Patch_Pawn_DeSpawn_Reject
    //{
    //    public class State
    //    {
    //        public Map map;

    //        public IntVec3 pos;

    //        public bool active;
    //    }

    //    public static void Prefix(Pawn __instance, out State __state)
    //    {
    //        __state = new State();
    //        WorldComponent_DespawnProtection component = Find.World.GetComponent<WorldComponent_DespawnProtection>();
    //        if (__instance.Spawned && __instance.Map != null && component != null && component.IsProtected(__instance))
    //        {
    //            __state.map = __instance.Map;
    //            __state.pos = __instance.Position;
    //            __state.active = true;
    //        }
    //    }

    //    public static void Postfix(Pawn __instance, State __state)
    //    {
    //        //if (__state != null && __state.active && __state.map != null)
    //        //{
    //        //    if (__instance.Spawned || __instance.holdingOwner != null)
    //        //    {
    //        //        _ = 1;
    //        //    }
    //        //    else
    //        //        Find.WorldPawns.Contains(__instance);
    //        //    Find.World.GetComponent<WorldComponent_DespawnProtection>()?.RequestRecovery(__instance, __state.map, __state.pos);
    //        //}
    //        // 修正：
    //        if (!__instance.Spawned && __instance.holdingOwner == null)
    //        {
    //            Find.World.GetComponent<WorldComponent_DespawnProtection>()?
    //                .RequestRecovery(__instance, __state.map, __state.pos);
    //        }
    //    }
    //}
    [HarmonyPatch(typeof(Pawn), "DeSpawn")]
    //[HarmonyPriority(Priority.High)]
    public static class Patch_Thing_DeSpawn_PreventIfHediff
    {
        // 要检查的 HediffDef（根据你的 MOD 修改）
        private static readonly HediffDef TargetHediff = XiaoTiDefOf.Combat_Stimulant;


        public static bool Prefix(Thing __instance, DestroyMode mode)
        {
            // 只处理 Pawn 类型
            if (__instance is Pawn pawn)
            {
                pawn = (Pawn)__instance;
                if (pawn == null || pawn.Destroyed || pawn.Dead) return true;
                // 检查是否持有目标 Hediff
                Hediff targetHediff = pawn.health?.hediffSet?.GetFirstHediffOfDef(TargetHediff);
                if (targetHediff != null && mode == DestroyMode.Vanish)
                {
                    Messages.Message("XiaoTiEquipment.Protection.Refuseddespawn".Translate(pawn.LabelShort), MessageTypeDefOf.NeutralEvent);

                    // 返回 false 跳过原始方法的所有后续步骤
                    pawn.health.RemoveHediff(targetHediff);
                    return false;
                }
            }

            // 没有目标 Hediff，正常执行
            return true;
        }
    }
    //阻止离开地图（包括一些正常游戏会遇到的比如组成远征队）
    [HarmonyPatch(typeof(Pawn), "Discard")]
    public static class Patch_Thing_Discard_PreventIfHediff
    {
        // 要检查的 HediffDef（根据你的 MOD 修改）
        private static readonly HediffDef TargetHediff = XiaoTiDefOf.Combat_Stimulant;


        public static bool Prefix(Thing __instance, bool silentlyRemoveReferences = false)
        {
            // 只处理 Pawn 类型
            if (__instance is Pawn pawn)
            {
                pawn = (Pawn)__instance;
                if (pawn == null || pawn.Destroyed || pawn.Dead) return true;
                // 检查是否持有目标 Hediff
                Hediff targetHediff = pawn.health?.hediffSet?.GetFirstHediffOfDef(TargetHediff);
                if (targetHediff != null)
                {
                    Messages.Message("XiaoTiEquipment.Protection.Refuseddiscard".Translate(pawn.LabelShort),MessageTypeDefOf.NeutralEvent);

                    // 返回 false 跳过原始方法的所有后续步骤
                    pawn.health.RemoveHediff(targetHediff);
                    return false;
                }
            }
            // 没有目标 Hediff，正常执行
            return true;
        }
    }
}
