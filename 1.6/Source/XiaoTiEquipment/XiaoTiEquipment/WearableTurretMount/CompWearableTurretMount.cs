//using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace XiaoTiEquipment
{
    public class CompWearableTurretMount : ThingComp, IAttackTargetSearcher
    {
        public List<TurretGunSlot> slots = new List<TurretGunSlot>();

        private LocalTargetInfo lastAttackedTarget = LocalTargetInfo.Invalid;
        private int lastAttackTargetTick;

        public CompProperties_WearableTurretMount Props => (CompProperties_WearableTurretMount)props;

        public Pawn Wearer => (parent as Apparel)?.Wearer;

        private bool CanAct
        {
            get
            {
                if (Wearer == null || !Wearer.Spawned || Wearer.Dead || Wearer.Downed)
                    return false;
                if (!Props.attackUndrafted && Wearer.IsColonistPlayerControlled && !Wearer.Drafted)
                    return false;
                return true;
            }
        }

        // IAttackTargetSearcher
        public Thing Thing => Wearer;
        public Verb CurrentEffectiveVerb => slots.Count > 0 ? slots[0].AttackVerb : null;
        public LocalTargetInfo LastAttackedTarget => lastAttackedTarget;
        public int LastAttackTargetTick => lastAttackTargetTick;

        public override void PostPostMake()
        {
            base.PostPostMake();
            InitGuns();
        }

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            if (slots.Count == 0)
            {
                InitGuns();
            }
            else
            {
                UpdateAllVerbCasters();
            }
        }

        private void InitGuns()
        {
            if (Props.mountedGunDefs == null)
                return;

            foreach (ThingDef gunDef in Props.mountedGunDefs)
            {
                if (gunDef == null) continue;
                Thing gun = ThingMaker.MakeThing(gunDef);
                TurretGunSlot slot = new TurretGunSlot { gun = gun };
                slots.Add(slot);
            }
            UpdateAllVerbCasters();
        }

        private void UpdateAllVerbCasters()
        {
            if (Wearer == null) return;
            for (int i = 0; i < slots.Count; i++)
            {
                slots[i].UpdateVerbCaster(Wearer, () => OnSlotBurstComplete(i));
            }
        }

        private void OnSlotBurstComplete(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= slots.Count) return;
            Verb verb = slots[slotIndex].AttackVerb;
            if (verb == null) return;
            float cooldown = Props.burstCooldownTime >= 0f
                ? Props.burstCooldownTime
                : verb.verbProps.defaultCooldownTime;
            slots[slotIndex].burstCooldownTicksLeft = cooldown.SecondsToTicks();
        }

        public override void CompTick()
        {
            base.CompTick();
            if (!CanAct) return;

            for (int i = 0; i < slots.Count; i++)
            {
                TickSlot(i);
            }
        }

        private void TickSlot(int index)
        {
            TurretGunSlot slot = slots[index];
            Verb verb = slot.AttackVerb;
            if (verb == null) return;

            if (slot.burstCooldownTicksLeft > 0)
            {
                slot.burstCooldownTicksLeft--;
            }

            verb.VerbTick();
            if (verb.state == VerbState.Bursting) return;

            if (slot.IsWarmingUp)
            {
                slot.burstWarmupTicksLeft--;
                if (slot.burstWarmupTicksLeft <= 0)
                {
                    slot.burstWarmupTicksLeft = 0;
                    if (slot.currentTarget.IsValid && verb.CanHitTargetFrom(Wearer.Position, slot.currentTarget))
                    {
                        verb.TryStartCastOn(slot.currentTarget, surpriseAttack: false,
                            canHitNonTargetPawns: true, preventFriendlyFire: false,
                            nonInterruptingSelfCast: true);
                        lastAttackedTarget = slot.currentTarget;
                        lastAttackTargetTick = Find.TickManager.TicksGame;
                    }
                    else
                    {
                        slot.ResetCurrentTarget();
                    }
                }
                return;
            }

            if (slot.burstCooldownTicksLeft > 0) return;

            if (!Wearer.IsHashIntervalTick(15)) return;

            if (!slot.forcedTarget.IsValid || (slot.forcedTarget.HasThing && !slot.forcedTarget.Thing.Spawned))
            {
                slot.forcedTarget = LocalTargetInfo.Invalid;
            }

            if (slot.forcedTarget.IsValid)
            {
                slot.currentTarget = slot.forcedTarget;
            }
            else if (slot.fireAtWill)
            {
                slot.currentTarget = (Thing)AttackTargetFinder.BestShootTargetFromCurrentPosition(
                    this, TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable);
            }

            if (slot.currentTarget.IsValid)
            {
                if (!verb.CanHitTargetFrom(Wearer.Position, slot.currentTarget))
                {
                    slot.ResetCurrentTarget();
                    return;
                }
                slot.burstWarmupTicksLeft = Props.burstWarmupTime.SecondsToTicks();
            }
        }

        public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
        {
            foreach (Gizmo g in base.CompGetWornGizmosExtra())
            {
                yield return g;
            }

            if (Wearer == null || Wearer.Faction != Faction.OfPlayer) yield break;
            if (!Wearer.Drafted && !Props.attackUndrafted) yield break;

            for (int i = 0; i < slots.Count; i++)
            {
                TurretGunSlot slot = slots[i];
                if (slot.AttackVerb == null) continue;

                Verb verb = slot.AttackVerb;
                float range = verb.EffectiveRange;
                float minRange = verb.verbProps.EffectiveMinRange(allowAdjacentShot: true);

                // Force target button
                Command_Action forceTargetCmd = new Command_Action
                {
                    defaultLabel = $"{slot.gun.def.LabelCap}: " + "CommandForceAttack".Translate(),
                    defaultDesc = "CommandForceAttackDesc".Translate(),
                    icon = slot.gun.def.uiIcon ?? TexCommand.Attack,
                    action = delegate
                    {
                        TargetingParameters tp = new TargetingParameters
                        {
                            canTargetPawns = true,
                            canTargetBuildings = true,
                            canTargetLocations = true,
                            validator = (TargetInfo ti) =>
                            {
                                float dist = (ti.Cell - Wearer.Position).LengthHorizontal;
                                return dist <= range && dist >= minRange;
                            }
                        };
                        Find.Targeter.BeginTargeting(tp,
                            delegate (LocalTargetInfo target)
                            {
                                slot.forcedTarget = target;
                                slot.currentTarget = target;
                            });
                    }
                };
                yield return forceTargetCmd;

                // Fire at will toggle
                yield return new Command_Toggle
                {
                    defaultLabel = "CommandFireAtWill".Translate() + $" ({slot.gun.def.LabelCap})",
                    defaultDesc = "CommandFireAtWillDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/FireAtWill"),
                    isActive = () => slot.fireAtWill,
                    toggleAction = delegate { slot.fireAtWill = !slot.fireAtWill; }
                };

                if (slot.forcedTarget.IsValid)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "CommandStopForceAttack".Translate(),
                        defaultDesc = "CommandStopForceAttackDesc".Translate(),
                        icon = ContentFinder<Texture2D>.Get("UI/Commands/Halt"),
                        action = delegate
                        {
                            slot.forcedTarget = LocalTargetInfo.Invalid;
                            slot.ResetCurrentTarget();
                        }
                    };
                }
            }
        }

        public override void CompDrawWornExtras()
        {
            base.CompDrawWornExtras();
            if (Wearer == null || !Wearer.Spawned) return;

            for (int i = 0; i < slots.Count; i++)
            {
                TurretGunSlot slot = slots[i];
                if (!slot.currentTarget.IsValid) continue;
                if (!slot.currentTarget.HasThing || slot.currentTarget.Thing.Spawned)
                {
                    Vector3 a = Wearer.TrueCenter();
                    Vector3 b = slot.currentTarget.HasThing
                        ? slot.currentTarget.Thing.TrueCenter()
                        : slot.currentTarget.Cell.ToVector3Shifted();
                    b.y = AltitudeLayer.MetaOverlays.AltitudeFor();
                    a.y = b.y;
                    GenDraw.DrawLineBetween(a, b,
                        Building_TurretGun.ForcedTargetLineMat);
                }
            }
        }

        public override string CompInspectStringExtra()
        {
            if (slots.Count == 0) return null;
            string s = "";
            for (int i = 0; i < slots.Count; i++)
            {
                TurretGunSlot slot = slots[i];
                string label = slot.gun.def.LabelCap;
                if (slot.burstCooldownTicksLeft > 0)
                {
                    s += $"\n{label}: " + "CanFireIn".Translate() + ": " +
                         slot.burstCooldownTicksLeft.ToStringSecondsFromTicks();
                }
                else if (slot.IsWarmingUp)
                {
                    s += $"\n{label}: " + "Warming up...";
                }
                else
                {
                    s += $"\n{label}: " + "Ready".Translate();
                }
            }
            return s.TrimStart('\n');
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref slots, "turretSlots", LookMode.Deep);
            Scribe_TargetInfo.Look(ref lastAttackedTarget, "lastAttackedTarget");
            Scribe_Values.Look(ref lastAttackTargetTick, "lastAttackTargetTick", 0);

            if (slots == null)
            {
                slots = new List<TurretGunSlot>();
            }

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (slots.Count == 0 && Props.mountedGunDefs != null)
                {
                    Log.Warning($"CompWearableTurretMount had no slots after load. Reinitializing.");
                    InitGuns();
                }
                else
                {
                    UpdateAllVerbCasters();
                }
            }
        }
    }

    public class TurretGunSlot : IExposable
    {
        public Thing gun;
        public int burstCooldownTicksLeft;
        public int burstWarmupTicksLeft;
        public LocalTargetInfo currentTarget = LocalTargetInfo.Invalid;
        public LocalTargetInfo forcedTarget = LocalTargetInfo.Invalid;
        public bool fireAtWill = true;

        public CompEquippable GunCompEq => gun?.TryGetComp<CompEquippable>();
        public Verb AttackVerb => GunCompEq?.PrimaryVerb;
        public bool IsWarmingUp => burstWarmupTicksLeft > 0;

        public void UpdateVerbCaster(Pawn pawn, System.Action burstCompleteCallback)
        {
            if (gun == null || pawn == null) return;
            CompEquippable compEq = GunCompEq;
            if (compEq == null) return;
            List<Verb> allVerbs = compEq.AllVerbs;
            for (int i = 0; i < allVerbs.Count; i++)
            {
                allVerbs[i].caster = pawn;
                allVerbs[i].castCompleteCallback = burstCompleteCallback;
            }
        }

        public void ResetCurrentTarget()
        {
            currentTarget = LocalTargetInfo.Invalid;
            burstWarmupTicksLeft = 0;
        }

        public void ExposeData()
        {
            Scribe_Deep.Look(ref gun, "gun");
            Scribe_Values.Look(ref burstCooldownTicksLeft, "burstCooldownTicksLeft");
            Scribe_Values.Look(ref burstWarmupTicksLeft, "burstWarmupTicksLeft");
            Scribe_TargetInfo.Look(ref currentTarget, "currentTarget");
            Scribe_TargetInfo.Look(ref forcedTarget, "forcedTarget");
            Scribe_Values.Look(ref fireAtWill, "fireAtWill", defaultValue: true);
        }
    }
}
