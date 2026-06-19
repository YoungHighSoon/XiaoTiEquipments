using System.Linq;
using RimWorld;
using Verse;

namespace XiaoTiEquipment
{
    public class DamageWorker_TrueDamage : DamageWorker_AddInjury
    {

        public new DamageResult Apply(DamageInfo dinfo, Thing thing, string forcedhitpart = null)
        {
            def = dinfo.Def;
            if (thing is Pawn pawn)
                return ApplyToPawn(dinfo, pawn, forcedhitpart);
            return base.Apply(dinfo, thing);
        }

        private DamageResult ApplyToPawn(DamageInfo dinfo, Pawn pawn, string forcedhitpart)
        {
            DamageResult result = new DamageResult();
            if (dinfo.Amount <= 0f) return result;

            BodyPartDef targetDef = forcedhitpart != null
                ? DefDatabase<BodyPartDef>.GetNamed(forcedhitpart, errorOnFail: false)
                : null;
            BodyPartRecord part = (targetDef != null
                ? pawn.health.hediffSet.GetNotMissingParts().FirstOrDefault(p => p.def == targetDef)
                : null) ?? ResolveHitPart(dinfo, pawn);
            if (part == null) return result;
            dinfo.SetHitPart(part);

            float num = dinfo.Amount;

            if (!dinfo.InstantPermanentInjury && !dinfo.IgnoreArmor)
            {
                DamageDef damageDef = dinfo.Def;
                num = ArmorUtility.GetPostArmorDamage(pawn, num, dinfo.ArmorPenetrationInt,
                    dinfo.HitPart, ref damageDef, out _, out _);
                dinfo.Def = damageDef;
                if (num < dinfo.Amount) result.diminished = true;
            }
            // IncomingDamageFactor 被跳过

            if (num <= 0f)
            {
                result.AddPart(pawn, dinfo.HitPart);
                result.deflected = true;
                return result;
            }

            FinalizeAndAddInjury(pawn, num, dinfo, result);
            return result;
        }

        private BodyPartRecord ResolveHitPart(DamageInfo dinfo, Pawn pawn)
        {
            if (dinfo.HitPart != null)
            {
                if (pawn.health.hediffSet.GetNotMissingParts()
                    .ToList().Any(x => x == dinfo.HitPart))
                    return dinfo.HitPart;
                return null;
            }
            return ChooseHitPart(dinfo, pawn);
        }
    }
}
