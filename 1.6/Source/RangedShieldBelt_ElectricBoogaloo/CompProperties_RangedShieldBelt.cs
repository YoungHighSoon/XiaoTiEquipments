using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RH_ShieldApparel_2ElectricBoogaloo
{
    public class CompProperties_RangedShieldBelt : CompProperties
    {
        public string bubbleMatPath;
        public ShaderTypeDef shader;
        public int startingTicksToReset = 180;
        public float minDrawSize = 1.2f;
        public float maxDrawSize = 1.55f;
        public float energyLossPerDamage = 0.01f;
        public float energyOnReset = 2.0f;
        public bool blocksRangedWeapons = true;
        public float spinRate = 1.0f;  
        public float flickerRate = 1.0f;

        public CompProperties_RangedShieldBelt()
        {
            compClass = typeof(CompRangedShieldBelt);
        }
    }

}
