using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using XiaoTiEquipment;

namespace XiaoTiHediffComp.HediffComps
{
    public class HediffComp_ClearBadhediff : HediffComp
    {
        private HediffCompProperties_ClearBadhediff Props =>
           (HediffCompProperties_ClearBadhediff)props;
        private Pawn MyPawn => parent.pawn;
        //public Pawn p => parent.pawn;
        public void ClearBadHediffs()
        {
            Pawn p = MyPawn;
            if (p.health == null) return;
            var hediffs = p.health.hediffSet.hediffs;
            HashSet<HediffDef> whitelist = new HashSet<HediffDef> { XiaoTiDefOf.Combat_Stimulant, XiaoTiDefOf.Cross_The_Bleach };
            for (int i = hediffs.Count - 1; i >= 0; i--)
            {
                Hediff hediff = hediffs[i];
                if (whitelist.Contains(hediff.def))
                {
                    continue; // 保留白名单中的hediff
                }
                if (Props.repairbodypart && (hediff is Hediff_Injury || hediff is Hediff_MissingPart))
                {
                    p.health.RemoveHediff(hediff);
                    continue;
                }
                // 移除更多负面状态
                if (hediff.def.isBad)
                {
                    p.health.RemoveHediff(hediff);
                    continue;
                }
            }
        }
        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            ClearBadHediffs();
        }
    }
}
