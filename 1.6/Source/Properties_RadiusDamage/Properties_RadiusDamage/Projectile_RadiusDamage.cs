using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace XiaoTiEquipment
{
    public class Projectile_RadiusDamage : Projectile_Explosive
    {
        private CompProperties_RadiusDamage cachedCompProps = null;
        private HashSet<Thing> _radiusThings = new HashSet<Thing>();

        private CompProperties_RadiusDamage Props
        {
            get
            {
                if (cachedCompProps == null)
                    cachedCompProps = def.GetCompProperties<CompProperties_RadiusDamage>();
                return cachedCompProps;
            }
        }

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            Map map = Map;
            IntVec3 position = Position;
            Thing launcher = this.launcher;
            int damageAmount = Mathf.FloorToInt(Props.Damage <= 0 ? DamageAmount : Props.Damage);
            DamageDef projectileDamageDef = DamageDef;
            float armorPenetration = ArmorPenetration;
            ThingDef equipmentDef = this.equipmentDef;
            LocalTargetInfo intendedTarget = this.intendedTarget;

            var radiusProps = Props;

            if (radiusProps != null)
            {
                ApplyRadiusDamage(map, position, launcher, damageAmount, projectileDamageDef,
                    armorPenetration, equipmentDef, intendedTarget, radiusProps);
            }

            base.Impact(hitThing, blockedByShield);
        }

        private void ApplyRadiusDamage(Map map, IntVec3 position, Thing launcher, int baseDamage,
            DamageDef projectileDamageDef, float armorPenetration, ThingDef equipmentDef,
            LocalTargetInfo intendedTarget, CompProperties_RadiusDamage props)
        {
            if (props.radius <= 0f || map == null)
                return;

            int radiusDamage = Mathf.RoundToInt(baseDamage * props.damageMultiplier);
            if (radiusDamage <= 0)
                return;

            DamageDef damageDef = props.damageDef ?? projectileDamageDef;
            if (damageDef == null)
                return;

            Thing instigator = launcher ?? this;
            float finalArmorPenetration = Mathf.Max(0f, armorPenetration * props.armorPenetrationMultiplier);

            // 用HashSet收集半径内所有不重复的Thing，避免跨格重复处理和每格List拷贝
            _radiusThings.Clear();
            float effectiveRadius = Mathf.Min(props.radius, GenRadial.MaxRadialPatternRadius - 0.01f);
            int numCells = GenRadial.NumCellsInRadius(effectiveRadius);
            for (int i = 0; i < numCells; i++)
            {
                IntVec3 cell = GenRadial.RadialPattern[i] + position;
                if (!cell.InBounds(map))
                    continue;

                List<Thing> things = map.thingGrid.ThingsListAt(cell);
                for (int j = 0, count = things.Count; j < count; j++)
                {
                    Thing thing = things[j];
                    if (thing != null && !thing.Destroyed)
                        _radiusThings.Add(thing);
                }
            }

            // 对每个去重后的Thing施加伤害
            foreach (Thing thing in _radiusThings)
            {
                if (thing.Destroyed || !thing.Spawned)
                    continue;

                if (props.onlydmghostile && !thing.HostileTo(Faction.OfPlayer))
                    continue;
                if (props.nofriendlyfire && thing.Faction != null && !thing.Faction.HostileTo(Faction.OfPlayer))
                    continue;

                int finalDamage = radiusDamage;
                if (thing.def.category == ThingCategory.Item)
                    finalDamage = Mathf.FloorToInt(radiusDamage * props.dmgtoitem);
                else if (thing is Building)
                    finalDamage = Mathf.FloorToInt(radiusDamage * props.dmgtobuilding);
                else if (thing is Plant)
                    finalDamage = Mathf.FloorToInt(radiusDamage * props.dmgtoplant);

                if (finalDamage <= 0)
                    continue;

                for (int n = 0; n < props.applytimes; n++)
                {
                    if (thing.Destroyed)
                        break;

                    try
                    {
                        DamageInfo dinfo = new DamageInfo(
                            def: damageDef,
                            amount: finalDamage,
                            armorPenetration: finalArmorPenetration,
                            instigator: instigator,
                            hitPart: null,
                            weapon: equipmentDef,
                            category: DamageInfo.SourceCategory.ThingOrUnknown,
                            intendedTarget: intendedTarget.Thing
                        );

                        if (thing is Pawn pawn)
                        {
                            if (Props.forcedDmg)
                            {
                                XiaoTiDoDamage.ApplyDamage(thing, damageDef, finalDamage, 999.0f, Props.preferredHitPart);
                            }
                            else if(Props.preferredHitPart != null)
                            {
                                var bodyDef = DefDatabase<BodyPartDef>.GetNamed(Props.preferredHitPart, errorOnFail: false);
                                thing.TakeDamage(bodyDef != null
                                    ? dinfo.WithPreferredPart(pawn, bodyDef)
                                    : dinfo);
                            }
                            else
                            {
                                thing.TakeDamage(dinfo);
                            }
                        }
                        else
                        {
                            thing.TakeDamage(dinfo);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Error applying radius damage to {thing}: {ex}");
                    }
                }
            }
        }
    }
}
