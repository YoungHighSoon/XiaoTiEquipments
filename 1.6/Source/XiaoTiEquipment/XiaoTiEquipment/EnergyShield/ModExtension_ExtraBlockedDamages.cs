using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace XiaoTiEquipment
{
    public class ModExtension_ExtraBlockedDamages : DefModExtension
    {
        // DamageDef 的 defName 白名单
        public List<string> extraDamageDefNames;

        // 可选：反向，只拦名单里的（覆写默认isRanged/isExplosive）
        public bool whitelistOnly = false;
    }
}
