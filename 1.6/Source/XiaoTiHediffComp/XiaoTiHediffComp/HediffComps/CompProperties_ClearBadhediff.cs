using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using XiaoTiEquipment;

namespace XiaoTiHediffComp.HediffComps
{
    public class HediffCompProperties_ClearBadhediff : HediffCompProperties
    {
        public bool repairbodypart = false;
        public HediffCompProperties_ClearBadhediff()
        {
            compClass = typeof(HediffComp_ClearBadhediff);
        }
    }
}
