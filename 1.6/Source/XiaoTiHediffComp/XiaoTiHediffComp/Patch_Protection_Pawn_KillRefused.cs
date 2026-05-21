using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Lifetime;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using XiaoTiEquipment;
using static HarmonyLib.Code;

namespace XiaoTiEquipment
{
    // 神佑之护 - 绝对不死与空间锚定逻辑

    [HarmonyPatch(typeof(Pawn), "Kill")]
    public static class Patch_Protection_Pawn_Kill_Refused
    {
        public static bool Prefix(Pawn __instance, DamageInfo? dinfo, Hediff exactCulprit = null)
        {
            if (__instance.Destroyed || !__instance.Spawned || __instance.Dead) return true;

            if (__instance.health != null && __instance.health.hediffSet.HasHediff(XiaoTiDefOf.Cross_The_Bleach))
            {
                // 阻止死亡并重置状态
                // 阻止死亡并重置状态
                HealAndReset(__instance);

                // 撕裂空间特效
                PlayTearSpaceEffect(__instance);
                Messages.Message("XiaoTiEquipment.Protection.Refused".Translate(__instance.LabelShort), __instance, MessageTypeDefOf.PositiveEvent, true);

                // 中断任务防止自杀
                __instance.jobs?.EndCurrentJob(JobCondition.InterruptForced);

                return false;
            }
            return true;
        }

        public static void PlayTearSpaceEffect(Pawn p)
        {
            if (p.Map == null) return;

            // 播放强力特效
            FleckMaker.ThrowLightningGlow(p.DrawPos, p.Map, 3.0f);
            FleckMaker.ThrowSmoke(p.DrawPos, p.Map, 2.0f);
            EffecterDefOf.Interceptor_BlockedProjectile.Spawn(p.Position, p.Map).Cleanup();

            // 播放声音
            SoundDefOf.EnergyShield_Reset.PlayOneShot(new TargetInfo(p.Position, p.Map));
        }

        // 治愈伤口移除负面状态
        public static void HealAndReset(Pawn p)
        {
            if (p.health == null) return;

            // 移除所有伤害 (Hediff_Injury)
            //private static readonly HashSet<HediffDef> Whitelist = new HashSet<HediffDef> { XiaoTiDefOf.Combat_Stimulant, XiaoTiDefOf.Cross_The_Bleach };
            HashSet < HediffDef > whitelist = new HashSet<HediffDef> {XiaoTiDefOf.Combat_Stimulant,XiaoTiDefOf.Cross_The_Bleach};
            var hediffs = p.health.hediffSet.hediffs;
            for (int i = hediffs.Count - 1; i >= 0; i--)
            {
                Hediff hediff = hediffs[i];
                if (whitelist.Contains(hediff.def))
                {
                    continue; // 保留白名单中的hediff
                }
                if (hediff is Hediff_Injury || hediff is Hediff_MissingPart)
                {
                    p.health.RemoveHediff(hediff);
                    continue;
                }
                if (hediff is HediffWithComps hediffWithComps)
                {
                    HediffComp_SeverityPerDay comp = hediffWithComps.GetComp<HediffComp_SeverityPerDay>();
                    HediffComp_Disappears disappearsComp = hediffWithComps.GetComp<HediffComp_Disappears>();
                    if (comp != null && comp.severityPerDay != 0)
                    {
                        //Log.Message($"清除了限时Hediff：{hediff.Label}");
                        p.health.RemoveHediff(hediff);
                        continue;
                    }
                    if(disappearsComp != null)
                    {
                        disappearsComp.disappearsAfterTicks = 0;
                        continue;
                    }
                }
                // 移除更多负面状态
                if(hediff.def.isBad)
                {
                    p.health.RemoveHediff(hediff);
                    continue;
                }
            }
            //添加防删Hediff
            var hediffDef = XiaoTiDefOf.Combat_Stimulant;
            if (p.health.hediffSet.HasHediff(hediffDef))
            {
                Hediff existingHediff = p.health.hediffSet.GetFirstHediffOfDef(hediffDef);
                var severity = existingHediff.Severity + 0.34f;
                existingHediff.Severity = Math.Min(severity, hediffDef.maxSeverity);
                Log.Message($"调整Hediff-Combat_Stimulant，新严重度: {existingHediff.Severity}");
            }
            else {
                Hediff newhediff = HediffMaker.MakeHediff(hediffDef, p);
                newhediff.Severity = 0.33f;
                p.health.AddHediff(newhediff);
                Log.Message($"添加Hediff-Combat_Stimulant: {newhediff.Severity}");
            }
            // 确保血量显示更新
            p.health.Notify_HediffChanged(null);
        }
    }

    // 记录并验证离地
    //[HarmonyPatch(typeof(Pawn), "DeSpawn")]
    //public static class Patch_GodProtection_Pawn_DeSpawn
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
    //        var comp = Find.World.GetComponent<WorldComponent_DespawnProtection>();
    //        // 空间锚定核心：只有显式保护名单中的 Pawn（非载具）才会被记录位置以便进行离地异步拉回
    //        // 这样不再拦截复杂的 Destroy 流程，能彻底解决任何底层的补丁冲突
    //        if (__instance.Spawned && __instance.Map != null && comp != null && comp.IsProtected(__instance))
    //        {
    //            __state.map = __instance.Map;
    //            __state.pos = __instance.Position;
    //            __state.active = true;
    //        }
    //    }

    //    public static void Postfix(Pawn __instance, State __state)
    //    {
    //        if (__state != null && __state.active && __state.map != null)
    //        {
    //            // 检查是否安全
    //            bool isSafe = __instance.Spawned || __instance.holdingOwner != null || Find.WorldPawns.Contains(__instance);

    //            if (!isSafe)
    //            {
    //                // 地图销毁会自动转移
    //                // 避免重复调用报错
    //                // 注册到组件下一帧检查
    //                // 若在世界中则安全
    //                // 否则视为非正常离地拉回
    //            }

    //            // 调度离地检查逻辑
    //            var comp = Find.World.GetComponent<WorldComponent_DespawnProtection>();
    //            comp?.RequestRecovery(__instance, __state.map, __state.pos);
    //        }
    //    }
    //}

}
