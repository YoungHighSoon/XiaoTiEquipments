using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace XiaoTiEquipment
{
    public class ExtraDamageBullet : Bullet
    {
        private CompProperties_ExtraDamageBullet Props =>
            def.GetCompProperties<CompProperties_ExtraDamageBullet>();

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            if (Props == null || hitThing == null || hitThing.Destroyed)
            {
                base.Impact(hitThing, blockedByShield);
                return;
            }

            if (Props.onlydmghostile && !hitThing.HostileTo(Faction.OfPlayer)) { Destroy(); return; }
            if (Props.nofriendlyfire && hitThing.Faction != null
                && !hitThing.Faction.HostileTo(Faction.OfPlayer)) { Destroy(); return; }

            int baseDamage = Mathf.FloorToInt(
                Props.Damage > 0 ? Props.Damage * Props.damageMultiplier : DamageAmount * Props.damageMultiplier);

            base.Impact(hitThing, blockedByShield);

            if (baseDamage <= 0) return;
            ApplyExtraDamage(hitThing, baseDamage);
        }

        private void ApplyExtraDamage(Thing thing, int baseDamage)
        {
            DamageDef damageDef = Props.damageDef ?? DamageDef;
            if (damageDef == null) return;

            int finalDamage = baseDamage;
            if (thing.def.category == ThingCategory.Item)
                finalDamage = Mathf.FloorToInt(baseDamage * Props.dmgtoitem);
            else if (thing is Building)
                finalDamage = Mathf.FloorToInt(baseDamage * Props.dmgtobuilding);
            else if (thing is Plant)
                finalDamage = Mathf.FloorToInt(baseDamage * Props.dmgtoplant);
            if (finalDamage <= 0) return;

            Thing instigator = launcher ?? this;
            float armorPenetration = Mathf.Max(0f, ArmorPenetration * Props.armorPenetrationMultiplier);

            for (int i = 0; i < Props.applytimes; i++)
            {
                if (thing.Destroyed) break;

                try
                {
                    if (Props.forcedDmg)
                    {
                        XiaoTiDoDamage.ApplyDamage(thing, damageDef, finalDamage, armorPenetration, Props.preferredHitPart);
                    }
                    else if (thing is Pawn pawn && Props.preferredHitPart != null)
                    {
                        var bodyDef = DefDatabase<BodyPartDef>.GetNamed(
                            Props.preferredHitPart, errorOnFail: false);
                        if (bodyDef != null)
                            thing.TakeDamage(ToDamageInfo(thing, damageDef, finalDamage, armorPenetration, instigator)
                                .WithPreferredPart(pawn, bodyDef));
                        else
                            thing.TakeDamage(ToDamageInfo(thing, damageDef, finalDamage, armorPenetration, instigator));
                    }
                    else
                    {
                        thing.TakeDamage(ToDamageInfo(thing, damageDef, finalDamage, armorPenetration, instigator));
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"ExtraDamageBullet: {ex}");
                }
            }
        }

        private DamageInfo ToDamageInfo(Thing thing, DamageDef def, int amount, float pen, Thing instigator)
        {
            return new DamageInfo(
                def: def,
                amount: amount,
                armorPenetration: pen,
                instigator: instigator,
                hitPart: null,
                weapon: equipmentDef,
                category: DamageInfo.SourceCategory.ThingOrUnknown,
                intendedTarget: intendedTarget.Thing
            );
        }
    }
}
