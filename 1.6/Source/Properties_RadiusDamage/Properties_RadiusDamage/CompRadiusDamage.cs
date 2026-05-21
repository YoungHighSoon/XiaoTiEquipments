using System;
using Verse;

namespace XiaoTiEquipment
{
    public class CompRadiusDamage : ThingComp
    {
        public CompProperties_RadiusDamage Props => (CompProperties_RadiusDamage)props;

        // 可以添加一些辅助方法，但主要逻辑在Projectile中
    }
}