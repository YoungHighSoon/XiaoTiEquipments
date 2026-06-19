using Verse;

namespace XiaoTiCompTicker
{
    public class CompProperties_Accelerator : CompProperties
    {
        public float range = 5f;
        public float factor = 1f; // Adjustable 1-4, additive across accelerators, cap 16
        public bool allowPawnBoost = false; // Disabled by default, toggle manually

        public CompProperties_Accelerator()
        {
            compClass = typeof(CompAccelerator);
        }
    }
}
