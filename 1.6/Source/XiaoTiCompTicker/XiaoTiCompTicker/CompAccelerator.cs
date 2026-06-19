using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace XiaoTiCompTicker
{
    public class CompAccelerator : ThingComp
    {
        public CompProperties_Accelerator Props => (CompProperties_Accelerator)props;
        private float factor;
        private bool factorInitialized;
        public bool enablePawnBoost;

        public float Factor
        {
            get
            {
                if (!factorInitialized)
                {
                    factor = Props.factor;
                    factorInitialized = true;
                }
                return factor;
            }
            set
            {
                factor = Mathf.Clamp(value, 1f, 4f);
                factorInitialized = true;
                var manager = parent?.Map?.GetComponent<MapComponent_AccelerationManager>();
                manager?.ForceRefresh();
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            var map = parent.Map;
            if (map == null) return;
            var manager = map.GetComponent<MapComponent_AccelerationManager>();
            if (manager == null)
            {
                Log.Error("[XiaoTiCompTicker] MapComponent_AccelerationManager not found on map! CompTick acceleration will not work.");
                return;
            }
            manager.Register(this);
            Log.Message($"[XiaoTiCompTicker] Accelerator registered. Factor={Factor}x, Range={Props.range}, Total accelerators on map: {manager.AcceleratorCount}");
        }

        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            map?.GetComponent<MapComponent_AccelerationManager>()?.Unregister(this);
            base.PostDeSpawn(map, mode);
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            previousMap?.GetComponent<MapComponent_AccelerationManager>()?.Unregister(this);
            base.PostDestroy(mode, previousMap);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            // Speed multiplier adjustment
            yield return new Command_Action
            {
                defaultLabel = "XiaoTi_SpeedMultiplierLabel".Translate(Factor),
                defaultDesc = "XiaoTi_SpeedMultiplierDesc".Translate(Props.range.ToString("F0")),
                icon = TexCommand.FireAtWill,
                action = delegate
                {
                    Find.WindowStack.Add(new FloatMenu(FloatMenuOptions()));
                }
            };

            // Pawn boost toggle
            yield return new Command_Toggle
            {
                defaultLabel = "XiaoTi_PawnBoostLabel".Translate(enablePawnBoost ? "On".Translate() : "Off".Translate()),
                defaultDesc = enablePawnBoost
                    ? "XiaoTi_PawnBoostDescOn".Translate()
                    : "XiaoTi_PawnBoostDescOff".Translate(),
                icon = TexCommand.Attack,
                isActive = () => enablePawnBoost,
                toggleAction = delegate
                {
                    enablePawnBoost = !enablePawnBoost;
                    var manager = parent.Map?.GetComponent<MapComponent_AccelerationManager>();
                    manager?.ForceRefresh();
                    if (enablePawnBoost)
                        Messages.Message("XiaoTi_PawnBoostEnabledMsg".Translate(), MessageTypeDefOf.CautionInput);
                }
            };
        }

        private List<FloatMenuOption> FloatMenuOptions()
        {
            var list = new List<FloatMenuOption>();
            for (float f = 1f; f <= 4f; f += 1f)
            {
                float val = f;
                list.Add(new FloatMenuOption(val + "x", delegate
                {
                    Factor = val;
                    var manager = parent.Map?.GetComponent<MapComponent_AccelerationManager>();
                    manager?.ForceRefresh();
                }));
            }
            return list;
        }

        public override string CompInspectStringExtra()
        {
            string s = "XiaoTi_InspectSpeedMultiplier".Translate(Factor);
            if (enablePawnBoost)
                s += "\n" + "XiaoTi_InspectPawnBoostWarning".Translate();
            return s;
        }

        public override void PostDrawExtraSelectionOverlays()
        {
            base.PostDrawExtraSelectionOverlays();
            if (Props.range <= 0f || Props.range >= GenRadial.MaxRadialPatternRadius)
                return;
            GenDraw.DrawRadiusRing(parent.Position, Props.range, Color.white);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref factor, "factor", Props.factor);
            Scribe_Values.Look(ref enablePawnBoost, "enablePawnBoost");
            if (Scribe.mode == LoadSaveMode.LoadingVars)
                factorInitialized = true;
        }
    }
}
