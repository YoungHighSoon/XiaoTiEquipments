using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace XiaoTiEquipment
{
    [StaticConstructorOnStartup]
    public class TurretCanForcedTarget : Building_TurretGun
    {
        protected override bool CanSetForcedTarget => base.Faction == Faction.OfPlayer;

        public override void DrawExtraSelectionOverlays()
        {
            float effectiveRange = AttackVerb.EffectiveRange;
            if (effectiveRange < GenRadial.MaxRadialPatternRadius)
            {
                base.DrawExtraSelectionOverlays();
                return;
            }
            // 攻击范围超过预计算上限时不绘制范围圈，只绘制强制目标连线
            if (forcedTarget.IsValid && (!forcedTarget.HasThing || forcedTarget.Thing.Spawned))
            {
                Vector3 b = forcedTarget.HasThing
                    ? forcedTarget.Thing.DrawPos
                    : forcedTarget.Cell.ToVector3Shifted();
                Vector3 a = DrawPos;
                b.y = AltitudeLayer.MetaOverlays.AltitudeFor();
                a.y = b.y;
                GenDraw.DrawLineBetween(a, b, ForcedTargetLineMat);
            }
        }
    }
}
