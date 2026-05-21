using System;
using Verse;
using System.Collections.Generic;

namespace XiaoTiEquipment
{
    public class CompProperties_ApparelAddHediff : CompProperties
    {
        public HediffDef hediffToAdd;          // 要添加的hediff（向后兼容）
        public List<HediffDef> hediffsToAdd;   // 要添加的hediff列表（新）
        public bool removeWhenUnequipped = true; // 卸下时是否移除hediff
        public bool checkIfAlreadyHas = true;   // 检查是否已存在相同hediff
        public float hediffSeverity = 1f;      // hediff的严重程度
        public bool useHediffInitialSeverity = true; // 使用hediff定义的初始严重程度

        public CompProperties_ApparelAddHediff()
        {
            compClass = typeof(CompApparelAddHediff);
        }
    }
}