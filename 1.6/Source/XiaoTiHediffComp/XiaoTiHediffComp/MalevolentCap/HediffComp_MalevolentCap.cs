using RimWorld;
using Verse;
using RigorMortis;
using UnityEngine;
using Axolotl;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace XiaoTiEquipment
{
    public class HediffComp_MalevolentCap : HediffComp
    {
        private HediffCompProperties_MalevolentCap Props =>
            (HediffCompProperties_MalevolentCap)props;
        public Pawn GetPawn => parent.pawn;

        private float MaxLimit
        {
            get
            {
                var comp = GetPawn.TryGetComp<CompYinAndMalevolent>();
                if (comp == null) return float.MaxValue;

                if (Props.usePercent)
                    return comp.MalevolentCapacity * Props.capacityPercent;
                else
                    return Props.maxMalevolent;
            }
        }
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            // 每60 tick检查一次，避免性能问题
            if (!GetPawn.IsHashIntervalTick(60)) return;

            var comp = GetPawn.TryGetComp<CompYinAndMalevolent>();
            if (comp == null) return;

            float limit = MaxLimit;
            if (comp.Malevolent > limit)
            {
                // 使用原模组的修改方法
                float reduce = comp.Malevolent - limit;
                comp.ChangeMalevolent(-reduce, true);

                // 可选：显示提示信息
                if (PawnUtility.ShouldSendNotificationAbout(GetPawn))
                {
                    Log.Message($"{GetPawn.LabelShort}的煞气逸散已被限制在{MaxLimit}");
                }
            }
        }
    }
    //public class XiaoTihungerRate : HediffComp
    //{
    //    private HediffCompProperties_MalevolentCap Props =>
    //        (HediffCompProperties_MalevolentCap)props;
    //    //Pawn GetPawn => parent.pawn;
    //    //float hungerRateFactor = GetPawn.health.hediffSet.GetHungerRateFactor();
    //    public Pawn GetPawn => this.Pawn;
    //    public override void CompPostTick(ref float severityAdjustment)
    //    {
    //        base.CompPostTick(ref severityAdjustment);

    //        // 每60 tick检查一次，避免性能问题
    //        if (!GetPawn.IsHashIntervalTick(60)) return;

    //    }

    //}
}
