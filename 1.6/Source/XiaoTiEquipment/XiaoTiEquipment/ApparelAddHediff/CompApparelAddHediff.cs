using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace XiaoTiEquipment
{
    public class CompApparelAddHediff : ThingComp
    {
        public CompProperties_ApparelAddHediff Props => (CompProperties_ApparelAddHediff)props;

        // 获取要添加的hediff列表（支持新旧格式）
        private List<HediffDef> GetHediffsToAdd()
        {
            if (Props.hediffsToAdd != null && Props.hediffsToAdd.Count > 0)
            {
                return Props.hediffsToAdd;
            }
            if (Props.hediffToAdd != null)
            {
                return new List<HediffDef> { Props.hediffToAdd };
            }
            return null;
        }

        // 当装备时调用
        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            if (pawn.Faction != Faction.OfPlayer) return;
            // 获取要添加的hediff列表
            List<HediffDef> hediffs = GetHediffsToAdd();
            if (hediffs.NullOrEmpty())
            {
                Log.Warning($"CompApparelAddHediff: No hediffs to add for {parent.def.defName}");
                return;
            }

            foreach (HediffDef hediffDef in hediffs)
            {
                if (hediffDef == null)
                {
                    Log.Warning($"CompApparelAddHediff: hediffDef is null in list for {parent.def.defName}");
                    continue;
                }

                // 如果检查已存在且pawn已有此hediff，则跳过
                if (Props.checkIfAlreadyHas && pawn.health.hediffSet.HasHediff(hediffDef))
                {
                    continue;
                }

                // 添加hediff
                Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn);

                // 设置严重程度
                if (!Props.useHediffInitialSeverity)
                {
                    hediff.Severity = Props.hediffSeverity;
                    Log.Message($"{hediff}:严重度已调整为{hediff.Severity}");
                }

                pawn.health.AddHediff(hediff);
            }
        }

        // 当卸下时调用
        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);

            if (!Props.removeWhenUnequipped)
            {
                return;
            }

            List<HediffDef> hediffs = GetHediffsToAdd();
            if (hediffs.NullOrEmpty())
            {
                return;
            }

            foreach (HediffDef hediffDef in hediffs)
            {
                if (hediffDef == null)
                {
                    continue;
                }
                // 移除该hediff的第一个实例
                Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
                if (hediff != null)
                {
                    pawn.health.RemoveHediff(hediff);
                }
            }
        }

        // 可选：在物品信息中显示效果
        public override string CompInspectStringExtra()
        {
            List<HediffDef> hediffs = GetHediffsToAdd();
            if (hediffs.NullOrEmpty())
            {
                return null;
            }

            if (hediffs.Count == 1)
            {
                return $"装备时提供: {hediffs[0].LabelCap}";
            }

            string labels = string.Join(", ", hediffs.Select(h => h.LabelCap));
            return $"装备时提供: {labels}";
        }
    }
}