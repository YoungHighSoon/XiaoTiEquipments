using System.Collections.Generic;
using Verse;

namespace XiaoTiEquipment
{
    public class CompProperties_WearableTurretMount : CompProperties
    {
        public List<ThingDef> mountedGunDefs;
        public float burstWarmupTime = 0.5f;
        public float burstCooldownTime = -1f;
        public bool attackUndrafted = false;

        public CompProperties_WearableTurretMount()
        {
            compClass = typeof(CompWearableTurretMount);
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (string item in base.ConfigErrors(parentDef))
            {
                yield return item;
            }
            if (mountedGunDefs.NullOrEmpty())
            {
                yield return "mountedGunDefs is null or empty.";
            }
            if (!typeof(ThingWithComps).IsAssignableFrom(parentDef.thingClass))
            {
                yield return "CompWearableTurretMount can only be added to ThingWithComps.";
            }
        }
    }
}
