using Verse;
using UnityEngine;

namespace XiaoTiCompTicker
{
    public class PlaceWorker_ShowAcceleratorRadius : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            var props = def.GetCompProperties<CompProperties_Accelerator>();
            if (props == null || props.range <= 0f || props.range >= GenRadial.MaxRadialPatternRadius)
                return;
            GenDraw.DrawRadiusRing(center, props.range);
        }
    }
}
