using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace XiaoTiEquipment
{
    public class HediffCompProperties_MalevolentCap : HediffCompProperties
    {
        public float maxMalevolent = 1000f;        // 固定上限值
        public float capacityPercent = 0.5f;       // 容量百分比（二选一）
        public bool usePercent = false;            // 是否使用百分比模式
        public HediffCompProperties_MalevolentCap()
        {
            compClass = typeof(HediffComp_MalevolentCap);
        }
    }
    //public class HediffCompProperties_XiaoTihungerRate : HediffCompProperties {

    //    public HediffCompProperties_XiaoTihungerRate() {
    //        compClass = typeof(XiaoTihungerRate);
    //    }
    //}
}
