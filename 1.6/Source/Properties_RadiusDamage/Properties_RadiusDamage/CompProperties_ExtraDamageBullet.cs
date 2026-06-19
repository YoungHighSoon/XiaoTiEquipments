using Verse;

namespace XiaoTiEquipment
{
    public class CompProperties_ExtraDamageBullet : CompProperties
    {
        public float Damage = -1f;          // 固定额外伤害，-1 = 用子弹伤害×倍率
        public float damageMultiplier = 1.0f;
        public DamageDef damageDef = null;       // null = 继承子弹伤害类型
        public float armorPenetrationMultiplier = 1.0f;
        public int applytimes = 1;
        public float dmgtoitem = 1.0f;
        public float dmgtobuilding = 1.0f;
        public float dmgtoplant = 1.0f;
        public bool nofriendlyfire = false;
        public bool onlydmghostile = false;
        public bool forcedDmg = false;
        public string preferredHitPart = null;   // null = 随机部位

        public CompProperties_ExtraDamageBullet()
        {
            compClass = typeof(CompRadiusDamage);
        }
    }
}
