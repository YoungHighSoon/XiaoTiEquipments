using System;
using System.Linq;
using Verse;

namespace XiaoTiEquipment
{
    public static class XiaoTiDoDamage
    {
        private static readonly DamageWorker_TrueDamage _trueDamageWorker = new DamageWorker_TrueDamage();

        public static DamageInfo WithPreferredPart(this DamageInfo dinfo, Pawn pawn, BodyPartDef preferredDef)
        {
            var preferred = pawn.health.hediffSet.GetNotMissingParts()
                .FirstOrDefault(p => p.def == preferredDef);

            if (preferred != null)
                dinfo.SetHitPart(preferred);
            return dinfo;
        }

        public static void DoDamage(Thing thing, DamageDef damageDef, float damageAmount, float armorPenetration)
        {
            if (thing != null)
            {
                DamageInfo dinfo = new DamageInfo(damageDef, damageAmount, armorPenetration);
                thing.TakeDamage(dinfo);
            }
        }

        public static void DoDamage(Thing thing, DamageDef damageDef, float damageAmount, float armorPenetration, Thing instigator)
        {
            if (thing != null)
            {
                DamageInfo dinfo = new DamageInfo(damageDef, damageAmount, armorPenetration, -1f, instigator);
                thing.TakeDamage(dinfo);
            }
        }

        public static void ApplyDamage(Thing thing, DamageDef damageDef, float damageAmount, float armorPenetration, String targetpart)
        {
            if (thing != null)
            {
                DamageInfo dinfo = new DamageInfo(damageDef, damageAmount, armorPenetration);
                _trueDamageWorker.Apply(dinfo, thing, targetpart);
            }
        }

        public static void ApplyDamage(Thing thing, DamageDef damageDef, float damageAmount, float armorPenetration, Thing instigator, String targetpart)
        {
            if (thing != null)
            {
                DamageInfo dinfo = new DamageInfo(damageDef, damageAmount, armorPenetration, -1f, instigator);
                _trueDamageWorker.Apply(dinfo, thing, targetpart);
            }
        }
    }
}
