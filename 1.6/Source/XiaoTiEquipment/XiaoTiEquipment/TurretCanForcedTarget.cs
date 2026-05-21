using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace XiaoTiEquipment
{
    [StaticConstructorOnStartup]
    public class TurretCanForcedTarget : Building_TurretGun
    {
        protected override bool CanSetForcedTarget => base.Faction == Faction.OfPlayer;
    }

}
