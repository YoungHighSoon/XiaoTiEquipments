using RimWorld;
using System;
using Verse;

namespace XiaoTiEquipment
{
    public class CompBuildingSelfRepair : ThingComp
    {
        private int ticksSinceLastRepair;

        public CompProperties_BuildingSelfRepair Props => (CompProperties_BuildingSelfRepair)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            ticksSinceLastRepair = 0;
        }

        public override void CompTick()
        {
            base.CompTick();

            ticksSinceLastRepair++;
            if (ticksSinceLastRepair >= Props.repairIntervalTicks)
            {
                // 检查是否满足修复条件
                if (!ShouldRepair())
                    return;
                PerformRepair();
                ticksSinceLastRepair = 0;
            }
        }

        private bool ShouldRepair()
        {
            if (parent == null || parent.Destroyed)
                return false;

            // 检查建筑是否有生命值
            if (!(parent is Building building))
                return false;

            // 如果仅当受损时修复且生命值已满，则跳过
            if (Props.repairOnlyWhenDamaged && building.HitPoints >= building.MaxHitPoints)
                return false;

            //// 检查最低修复条件（HP百分比）
            //float hpRatio = (float)building.HitPoints / building.MaxHitPoints;
            //if (hpRatio < Props.minRepairCondition)
            //    return false;

            //// 检查最高修复条件
            //if (hpRatio >= Props.maxRepairCondition)
            //    return false;

            return true;
        }

        private void PerformRepair()
        {
            if (!(parent is Building building))
                return;
            int RepairAmount = (int)Props.repairAmount;
            float oldHP = building.HitPoints;
            float maxHP = building.MaxHitPoints;
            float newHP;
            if (Props.UsePercentRepair){
                newHP = Math.Min(maxHP, oldHP * (Props.repairPercent + 1));
            }
            else {
                newHP = Math.Min(maxHP, oldHP + RepairAmount);
            }
            // 计算修复量，确保不超过最大生命值
            //int newHP = building.HitPoints + (int)Props.repairAmount;
            //if (newHP > building.MaxHitPoints)
            //    newHP = building.MaxHitPoints;

            // 应用修复
            building.HitPoints = (int)newHP;

            // 显示修复效果
            //if (Props.showRepairEffect && building.Spawned)
            //{
            //    // 尝试使用机械族维修特效
            //    EffecterDef repairEffect = DefDatabase<EffecterDef>.GetNamed("MechRepairing", false);
            //    if (repairEffect != null)
            //    {
            //        repairEffect.SpawnAttached(building, building.Map, 1f);
            //    }
            //    else
            //    {
            //        // 备用方案：使用类似机械族维修的简单效果（灰尘+闪光）
            //        FleckMaker.ThrowDustPuff(building.DrawPos, building.Map, 1f);
            //        if (Rand.Value < 0.3f)
            //        {
            //            FleckMaker.ThrowLightningGlow(building.DrawPos, building.Map, 0.5f);
            //        }
            //    }

            //    // 可选：播放维修声音
            //    // SoundDef repairSound = DefDatabase<SoundDef>.GetNamedSilentFail("MechRepair");
            //    //if (repairSound != null)
            //    //    repairSound.PlayOneShot(new TargetInfo(building.Position, building.Map));
            //    //else if (SoundDefOf.MetalHit != null)
            //    //    SoundDefOf.MetalHit.PlayOneShot(new TargetInfo(building.Position, building.Map));
            //}
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref ticksSinceLastRepair, "ticksSinceLastRepair", 0);
        }

        public override string CompInspectStringExtra()
        {
            if (ShouldRepair() && Props.showrepairinfo)
            {
                float progress = (float)ticksSinceLastRepair / Props.repairIntervalTicks;
                return $"自我修复进度: {progress:P0} (下次修复: {Props.repairAmount} HP)";
            }
            return null;
        }
    }
}
