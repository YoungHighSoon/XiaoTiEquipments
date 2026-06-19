using RimWorld;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace XiaoTiEquipment
{
    public class CompRangedShieldBelt : ThingComp
    {
        // --- Fields ---
        private Material bubbleMat;
        protected float energy;
        protected int ticksToReset = -1;
        protected int lastKeepDisplayTick = -9999;
        private Vector3 impactAngleVect;
        private int lastAbsorbDamageTick = -9999;

        private const float MaxDamagedJitterDist = 0.05f;
        private const int JitterDurationTicks = 8;
        private const int KeepDisplayingTicks = 1000;
        private const float ApparelScorePerEnergyMax = 0.25f;

        // Per-tick cache — avoids repeated GetComp / GetStatValue / pattern-match
        private int cacheTick = -1;
        private Pawn cachedPawnOwner;
        private ShieldState cachedShieldState;
        private float cachedEnergyMax;
        private float cachedEnergyGainPerTick;

        // --- Properties ---
        public CompProperties_RangedShieldBelt Props => (CompProperties_RangedShieldBelt)props;

        private void RebuildCache()
        {
            int tick = Find.TickManager.TicksGame;
            if (cacheTick == tick) return;
            cacheTick = tick;

            // Resolve PawnOwner
            if (parent is Apparel apparel)
                cachedPawnOwner = apparel.Wearer;
            else if (parent is Pawn pawn)
                cachedPawnOwner = pawn;
            else
                cachedPawnOwner = null;

            // Energy stats
            cachedEnergyMax = parent.GetStatValue(StatDefOf.EnergyShieldEnergyMax);
            cachedEnergyGainPerTick = parent.GetStatValue(StatDefOf.EnergyShieldRechargeRate) / 60f;

            // Shield state
            if (cachedPawnOwner != null && (cachedPawnOwner.IsCharging() || cachedPawnOwner.IsSelfShutdown()))
            {
                cachedShieldState = ShieldState.Disabled;
            }
            else
            {
                CompCanBeDormant dormantComp = parent.GetComp<CompCanBeDormant>();
                if (dormantComp != null && !dormantComp.Awake)
                    cachedShieldState = ShieldState.Disabled;
                else if (ticksToReset <= 0)
                    cachedShieldState = ShieldState.Active;
                else
                    cachedShieldState = ShieldState.Resetting;
            }
        }

        private void InvalidateCache()
        {
            cacheTick = -1;
        }

        private Material BubbleMat
        {
            get
            {
                if (bubbleMat == null)
                    bubbleMat = MaterialPool.MatFrom(Props.bubbleMatPath, Props.shader.Shader);
                return bubbleMat;
            }
        }

        private float EnergyMax { get { RebuildCache(); return cachedEnergyMax; } }
        private float EnergyGainPerTick { get { RebuildCache(); return cachedEnergyGainPerTick; } }
        public float Energy => energy;

        public ShieldState ShieldState
        {
            get
            {
                RebuildCache();
                return cachedShieldState;
            }
        }

        private Pawn PawnOwner
        {
            get
            {
                RebuildCache();
                return cachedPawnOwner;
            }
        }

        private bool ShouldDisplay
        {
            get
            {
                Pawn owner = PawnOwner;
                if (!owner.Spawned || owner.Dead || owner.Downed)
                    return false;
                if (owner.InAggroMentalState)
                    return true;
                if (owner.Drafted)
                    return true;
                if (owner.Faction.HostileTo(Faction.OfPlayer) && !owner.IsPrisoner)
                    return true;
                if (Find.TickManager.TicksGame < lastKeepDisplayTick + KeepDisplayingTicks)
                    return true;
                if (ModsConfig.BiotechActive && owner.IsColonyMech && Find.Selector.SingleSelectedThing == owner)
                    return true;
                return false;
            }
        }

        public bool IsApparel => parent is Apparel;
        private bool IsBuiltIn => !IsApparel;

        // --- Expose ---
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref energy, "energy", 0f);
            Scribe_Values.Look(ref ticksToReset, "ticksToReset", -1);
            Scribe_Values.Look(ref lastKeepDisplayTick, "lastKeepDisplayTick", 0);
        }

        // --- Gizmos ---
        public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
        {
            foreach (Gizmo g in base.CompGetWornGizmosExtra())
                yield return g;

            if (IsApparel)
            {
                foreach (Gizmo g in GetGizmos())
                    yield return g;
            }

            if (!DebugSettings.ShowDevGizmos)
                yield break;

            yield return new Command_Action
            {
                defaultLabel = "DEV: Break",
                action = Break
            };
            if (ticksToReset > 0)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEV: Clear reset",
                    action = delegate { ticksToReset = 0; InvalidateCache(); }
                };
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo g in base.CompGetGizmosExtra())
                yield return g;

            if (!IsBuiltIn)
                yield break;

            foreach (Gizmo g in GetGizmos())
                yield return g;
        }

        private IEnumerable<Gizmo> GetGizmos()
        {
            bool showGizmo = PawnOwner.Faction == Faction.OfPlayer
                || (parent is Pawn p && p.RaceProps.IsMechanoid);

            if (showGizmo && Find.Selector.SingleSelectedThing == PawnOwner)
            {
                yield return new Gizmo_EnergyShieldStatus { shield = this };
            }
        }

        // --- Apparel scoring ---
        public override float CompGetSpecialApparelScoreOffset()
        {
            return EnergyMax * ApparelScorePerEnergyMax;
        }

        // --- Tick ---
        public override void CompTick()
        {
            base.CompTick();

            Pawn owner = PawnOwner;
            if (owner == null || owner.Faction != Faction.OfPlayer)
            {
                energy = 0f;
                return;
            }

            ShieldState state = ShieldState; // single evaluation
            if (state == ShieldState.Resetting)
            {
                ticksToReset--;
                if (ticksToReset <= 0)
                    Reset();
            }
            else if (state == ShieldState.Active)
            {
                float gain = EnergyGainPerTick;
                energy += gain;
                float max = EnergyMax;
                if (energy > max)
                    energy = max;
            }
        }

        // --- Damage ---
        public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            absorbed = false;
            if (ShieldState != ShieldState.Active || PawnOwner == null)
                return;
            var ext = parent.def.GetModExtension<ModExtension_ExtraBlockedDamages>();
            bool inBlacklist = ext?.extraDamageDefNames?.Contains(dinfo.Def.defName) ?? false;
            if (dinfo.Def == DamageDefOf.EMP)
            {
                energy += 100f;
                //Break();
                return;
            }
            //else if (!dinfo.Def.ignoreShields && (dinfo.Def.isRanged || dinfo.Def.isExplosive))
            else if (dinfo.Def.isRanged || dinfo.Def.isExplosive || inBlacklist)
            {
                energy -= dinfo.Amount * Props.energyLossPerDamage;
                if (energy < 0f)
                {
                    Break();
                }
                else
                {
                    AbsorbedDamage(dinfo);
                }
                absorbed = true;
            }
        }

        public void KeepDisplaying()
        {
            lastKeepDisplayTick = Find.TickManager.TicksGame;
        }

        private void AbsorbedDamage(DamageInfo dinfo)
        {
            Pawn owner = PawnOwner;
            SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(owner.Position, owner.Map));
            impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
            Vector3 loc = owner.TrueCenter() + impactAngleVect.RotatedBy(180f) * 0.5f;
            float num = Mathf.Min(10f, 2f + dinfo.Amount / 10f);
            FleckMaker.Static(loc, owner.Map, FleckDefOf.ExplosionFlash, num);
            int count = (int)num;
            for (int i = 0; i < count; i++)
                FleckMaker.ThrowDustPuff(loc, owner.Map, Rand.Range(0.8f, 1.2f));

            lastAbsorbDamageTick = Find.TickManager.TicksGame;
            KeepDisplaying();
        }

        private void Break()
        {
            if (parent.Spawned)
            {
                float scale = Mathf.Lerp(Props.minDrawSize, Props.maxDrawSize, energy);
                EffecterDefOf.Shield_Break.SpawnAttached(parent, parent.MapHeld, scale);
                Pawn owner = PawnOwner;
                FleckMaker.Static(owner.TrueCenter(), owner.Map, FleckDefOf.ExplosionFlash, 12f);
                for (int i = 0; i < 6; i++)
                {
                    FleckMaker.ThrowDustPuff(
                        owner.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle(Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f),
                        owner.Map, Rand.Range(0.8f, 1.2f));
                }
            }
            energy = 0f;
            ticksToReset = Props.startingTicksToReset;
            InvalidateCache();
        }

        private void Reset()
        {
            Pawn owner = PawnOwner;
            if (owner.Spawned)
            {
                SoundDefOf.EnergyShield_Reset.PlayOneShot(new TargetInfo(owner.Position, owner.Map));
                FleckMaker.ThrowLightningGlow(owner.TrueCenter(), owner.Map, 3f);
            }
            ticksToReset = -1;
            energy = Props.energyOnReset;
            InvalidateCache();
        }

        // --- Drawing ---
        public override void CompDrawWornExtras()
        {
            if (IsApparel) Draw();
        }

        public override void PostDraw()
        {
            if (IsBuiltIn) Draw();
        }

        private void Draw()
        {
            if (ShieldState != ShieldState.Active || !ShouldDisplay)
                return;

            float size = Mathf.Lerp(Props.minDrawSize, Props.maxDrawSize, energy);
            Pawn owner = PawnOwner;
            Vector3 drawPos = owner.Drawer.DrawPos;
            drawPos.y = AltitudeLayer.MoteOverhead.AltitudeFor();

            int ticksSinceDamage = Find.TickManager.TicksGame - lastAbsorbDamageTick;
            if (ticksSinceDamage < JitterDurationTicks)
            {
                float damageEffect = (float)(JitterDurationTicks - ticksSinceDamage) / JitterDurationTicks * MaxDamagedJitterDist;
                drawPos += impactAngleVect * damageEffect;
                size -= damageEffect;
            }

            float angle = (Find.TickManager.TicksGame * Props.spinRate + Rand.Range(0, 360) * Props.flickerRate) % 360f;
            Vector3 scale = new Vector3(size, 1f, size);
            Matrix4x4 matrix = Matrix4x4.TRS(drawPos, Quaternion.AngleAxis(angle, Vector3.up), scale);
            Graphics.DrawMesh(MeshPool.plane10, matrix, BubbleMat, 0);
        }

        // --- Weapon restriction ---
        public override bool CompAllowVerbCast(Verb verb)
        {
            if (Props.blocksRangedWeapons)
                return !(verb is Verb_LaunchProjectile);
            return true;
        }
    }

    // --- Gizmo ---
    [StaticConstructorOnStartup]
    public class Gizmo_EnergyShieldStatus : Gizmo
    {
        public CompRangedShieldBelt shield;

        private static readonly Texture2D FullBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.24f));
        private static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);

        public Gizmo_EnergyShieldStatus()
        {
            Order = -100f;
        }

        public override float GetWidth(float maxWidth)
        {
            return 140f;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            Rect inner = rect.ContractedBy(6f);
            Widgets.DrawWindowBackground(rect);

            Rect labelRect = inner;
            labelRect.height = inner.height / 2f;
            Text.Font = GameFont.Tiny;
            Widgets.Label(labelRect, shield.IsApparel ? shield.parent.LabelCap : "ShieldInbuilt".Translate().Resolve());

            Rect barRect = inner;
            barRect.yMin = inner.y + inner.height / 2f;
            float fill = shield.Energy / Mathf.Max(1f, shield.parent.GetStatValue(StatDefOf.EnergyShieldEnergyMax));
            Widgets.FillableBar(barRect, fill, FullBarTex, EmptyBarTex, doBorder: false);

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(barRect, (shield.Energy * 100f).ToString("F0") + " / " + (shield.parent.GetStatValue(StatDefOf.EnergyShieldEnergyMax) * 100f).ToString("F0"));
            Text.Anchor = TextAnchor.UpperLeft;

            TooltipHandler.TipRegion(inner, "ShieldPersonalTip".Translate());
            return new GizmoResult(GizmoState.Clear);
        }
    }
}
