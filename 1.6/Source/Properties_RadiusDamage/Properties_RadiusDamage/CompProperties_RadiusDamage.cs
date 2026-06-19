using System;
using Verse;

namespace XiaoTiEquipment
{
    public class CompProperties_RadiusDamage : CompProperties
    {
        public float radius = 1.5f; // 伤害半径
        public float damageMultiplier = 1.0f; // 伤害倍数（基于子弹基础伤害）
        public DamageDef damageDef = null; // 伤害类型，如果为null则使用子弹的伤害类型
        public bool ignoreLineOfSight = true; // 是否忽略视线遮挡（无视建筑遮挡）
        public float armorPenetrationMultiplier = 1.0f; // 护甲穿透倍数
        public int applytimes = 1;
        public float dmgtoitem = 1.0f;
        public float dmgtobuilding = 1.0f;
        public float dmgtoplant = 1.0f;
        public bool nofriendlyfire = false;
        public bool onlydmghostile = false;
        public bool forcedDmg = false;
        public string preferredHitPart = null;
        public float Damage = -1f;
        //public bool noreason = false;

        public CompProperties_RadiusDamage()
        {
            compClass = typeof(CompRadiusDamage);
        }
    }
}