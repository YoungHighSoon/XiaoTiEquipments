using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace XiaoTiEquipment
{
    public class Projectile_RadiusDamage : Projectile_Explosive
    {
        private CompProperties_RadiusDamage cachedCompProps = null;

        private CompProperties_RadiusDamage RadiusDamageProps
        {
            get
            {
                if (cachedCompProps == null)
                {
                    // 从子弹的ThingDef中获取组件属性
                    var comp = def.GetCompProperties<CompProperties_RadiusDamage>();
                    cachedCompProps = comp;
                }
                return cachedCompProps;
            }
        }
        public static void MakePowerBeamMote(IntVec3 cell, Map map)
        {
            Mote obj = (Mote)ThingMaker.MakeThing(ThingDefOf.Mote_PowerBeam);
            obj.exactPosition = cell.ToVector3Shifted();
            obj.Scale = 45f;
            obj.rotationRate = 1.2f;
            GenSpawn.Spawn(obj, cell, map);
        }
        public void Explosioneffecter(IntVec3 cell, Map map, CompProperties_RadiusDamage props)
        {
            if (map == null || !cell.InBounds(map)) return;
            //MakePowerBeamMote(cell, map);
            FleckMaker.Static(cell.ToVector3Shifted(), map, FleckDefOf.ExplosionFlash, props.radius);
            Effecter effecter = EffecterDefOf.Vaporize_Heatwave.Spawn();
            effecter.Trigger(new TargetInfo(cell, map), TargetInfo.Invalid);
            effecter.Cleanup();
        }

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            // 在执行任何操作前保存所有必要状态，因为子弹可能在基类Impact中被销毁
            Map map = Map;
            IntVec3 position = Position;
            Thing launcher = this.launcher;
            int damageAmount = DamageAmount;
            DamageDef projectileDamageDef = DamageDef;
            float armorPenetration = ArmorPenetration;
            ThingDef equipmentDef = this.equipmentDef;
            LocalTargetInfo intendedTarget = this.intendedTarget;

            var radiusProps = RadiusDamageProps;

            // 如果子弹有半径伤害组件，先执行半径伤害
            if (radiusProps != null)
            {
                ApplyRadiusDamage(map, position, launcher, damageAmount, projectileDamageDef,
                    armorPenetration, equipmentDef, intendedTarget, radiusProps);
            }

            // 然后调用基类Impact进行正常子弹伤害
            base.Impact(hitThing, blockedByShield);
        }

        private void ApplyRadiusDamage(Map map, IntVec3 position, Thing launcher, int baseDamage,
            DamageDef projectileDamageDef, float armorPenetration, ThingDef equipmentDef,
            LocalTargetInfo intendedTarget, CompProperties_RadiusDamage props)
        {

            if (props.radius <= 0f)
                return;

            if (map == null)
                return;

            // 计算伤害
            int radiusDamage = Mathf.RoundToInt(baseDamage * props.damageMultiplier);
            if (radiusDamage <= 0)
                return;

            // 确定伤害类型
            DamageDef damageDef = props.damageDef ?? projectileDamageDef;
            if (damageDef == null)
                return;

            // 确定责任方（谁造成的伤害）
            Thing instigator = launcher ?? this;

            // 获取半径内的所有单元格
            IEnumerable<IntVec3> cells = GetRadiusCells(map, position, props.radius, props.ignoreLineOfSight);
            //爆炸中心特效
            //Explosioneffecter(position, map, props);
            // 对每个单元格内的东西造成伤害
            foreach (IntVec3 cell in cells)
            {
                
                if (!cell.InBounds(map))
                    continue;

                // 获取单元格内的所有东西（创建副本避免枚举时列表被修改）
                List<Thing> things = map.thingGrid.ThingsListAt(cell);
                if (things.Count == 0)
                    continue;

                // 创建副本以避免在遍历时列表被修改（当物体被销毁时会从原列表中移除）
                List<Thing> thingsCopy = new List<Thing>(things);
                foreach (Thing thing in thingsCopy)
                {

                    if (thing == null || thing.Destroyed || thing.def == null)
                        continue;
                    ApplyDamageToThing(thing, radiusDamage, damageDef, instigator,
                            armorPenetration, equipmentDef, intendedTarget, props);

                }
            }
        }

        private IEnumerable<IntVec3> GetRadiusCells(Map map, IntVec3 center, float radius, bool ignoreLineOfSight)
        {
            if (map == null)
                yield break;

            if (ignoreLineOfSight)
            {
                // 无视视线遮挡，直接使用圆形区域
                foreach (IntVec3 cell in GenRadial.RadialCellsAround(center, radius, true))
                {
                    yield return cell;
                }
            }
            else
            {
                // 如果需要考虑视线遮挡，可以使用原版的爆炸逻辑
                // 这里简化处理，仍然使用圆形区域
                foreach (IntVec3 cell in GenRadial.RadialCellsAround(center, radius, true))
                {
                    yield return cell;
                }
            }
        }

        private void ApplyDamageToThing(Thing thing, int damageAmount, DamageDef damageDef, Thing instigator,
            float armorPenetration, ThingDef equipmentDef, LocalTargetInfo intendedTarget, CompProperties_RadiusDamage props)
        {
            if (damageAmount <= 0 || thing.Destroyed)
                return;

            try
            {
                // 创建伤害信息
                DamageInfo dinfo = new DamageInfo(
                    def: damageDef,
                    amount: damageAmount,
                    armorPenetration: Mathf.Max(0f, armorPenetration * props.armorPenetrationMultiplier),
                    instigator: instigator,
                    hitPart: null,
                    weapon: equipmentDef,
                    category: DamageInfo.SourceCategory.ThingOrUnknown,
                    intendedTarget: intendedTarget.Thing
                );

                // 应用伤害
                thing.TakeDamage(dinfo);
            }
            catch (Exception ex)
            {
                Log.Error($"Error applying radius damage to {thing}: {ex}");
            }
        }
    }
}
