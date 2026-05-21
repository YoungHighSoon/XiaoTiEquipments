using System;
using Verse;

namespace XiaoTiEquipment
{
    public class CompProperties_BuildingSelfRepair : CompProperties
    {
        public int repairIntervalTicks = 60; // 每次修复的tick间隔，默认1秒（60 ticks）
        public float repairAmount = 1f;      // 每次修复的HP量
        public bool repairOnlyWhenDamaged = true; // 仅当受损时修复（HP < MaxHP）
        public bool UsePercentRepair = false; // 是否使用百分比修复
        public float repairPercent = 0.33f;      // 每次修复的百分比
        public bool showRepairEffect = true; // 是否显示修复效果（例如火花）
        public float minRepairCondition = 0f; // 最低修复条件（HP百分比低于此值时停止修复，0表示总是修复）
        public float maxRepairCondition = 1f; // 最高修复条件（HP百分比达到此值时停止修复，1表示修复到满）
        public bool showrepairinfo = true;

        public CompProperties_BuildingSelfRepair()
        {
            compClass = typeof(CompBuildingSelfRepair);
        }
    }
}