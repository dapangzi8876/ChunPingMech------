using System.Collections.Generic;
using RimWorld;
using Verse;

namespace ChunPingMech
{
    public static class WasedaHeroUtility
    {
        public const float NearbyRadius = 12f;
        public const int ActiveDurationTicks = 900;

        public static bool IsFriendly(Pawn source, Pawn target, bool includeSelf = false)
        {
            return source != null && target != null && (includeSelf || source != target)
                && !target.Dead && target.Spawned && target.Map == source.Map
                && !target.HostileTo(source);
        }

        public static void GiveHediff(Pawn pawn, HediffDef def, int durationTicks)
        {
            if (pawn?.health == null || def == null || pawn.Dead)
            {
                return;
            }

            Hediff existing = pawn.health.hediffSet.GetFirstHediffOfDef(def);
            if (existing != null)
            {
                pawn.health.RemoveHediff(existing);
            }

            Hediff hediff = HediffMaker.MakeHediff(def, pawn);
            HediffComp_Disappears disappears = hediff.TryGetComp<HediffComp_Disappears>();
            if (disappears != null)
            {
                disappears.ticksToDisappear = durationTicks;
            }

            pawn.health.AddHediff(hediff);
        }

        public static void GiveHediffToNearbyAllies(Pawn source, Map map, IntVec3 center, HediffDef def)
        {
            if (map == null || def == null)
            {
                return;
            }

            IReadOnlyList<Pawn> pawns = map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn pawn = pawns[i];
                if (pawn != source && pawn.Position.DistanceTo(center) <= NearbyRadius
                    && IsFriendly(source, pawn))
                {
                    GiveHediff(pawn, def, ActiveDurationTicks);
                }
            }
        }

        public static bool TryFindCellNear(Pawn target, out IntVec3 result)
        {
            result = IntVec3.Invalid;
            if (target?.Map == null)
            {
                return false;
            }

            foreach (IntVec3 cell in GenRadial.RadialCellsAround(target.Position, 2f, true))
            {
                if (cell.InBounds(target.Map) && cell.Standable(target.Map)
                    && cell.GetFirstPawn(target.Map) == null)
                {
                    result = cell;
                    return true;
                }
            }

            return false;
        }
    }

    public class CompProperties_AbilityHeroRescue : CompProperties_AbilityEffect
    {
        public HediffDef protectedHediff;
        public HediffDef heroismHediff;

        public CompProperties_AbilityHeroRescue()
        {
            compClass = typeof(CompAbilityEffect_HeroRescue);
        }
    }

    public class CompAbilityEffect_HeroRescue : CompAbilityEffect
    {
        public new CompProperties_AbilityHeroRescue Props =>
            (CompProperties_AbilityHeroRescue)props;

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return Valid(target);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            Pawn caster = parent?.pawn;
            Pawn ally = target.Pawn;
            if (caster?.Map == null || !WasedaHeroUtility.IsFriendly(caster, ally))
            {
                if (throwMessages)
                {
                    Messages.Message("只能选择未倒地的友军。", MessageTypeDefOf.RejectInput, false);
                }

                return false;
            }

            return WasedaHeroUtility.TryFindCellNear(ally, out _);
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Pawn caster = parent?.pawn;
            Pawn ally = target.Pawn;
            if (caster == null || ally == null || !WasedaHeroUtility.IsFriendly(caster, ally)
                || !WasedaHeroUtility.TryFindCellNear(ally, out IntVec3 destination))
            {
                return;
            }

            caster.Position = destination;
            caster.Notify_Teleported();
            WasedaHeroUtility.GiveHediff(ally, Props.protectedHediff, WasedaHeroUtility.ActiveDurationTicks);
            WasedaHeroUtility.GiveHediff(caster, Props.heroismHediff, WasedaHeroUtility.ActiveDurationTicks);
        }
    }

    public class CompProperties_AbilityHeroHeal : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityHeroHeal()
        {
            compClass = typeof(CompAbilityEffect_HeroHeal);
        }
    }

    public class CompAbilityEffect_HeroHeal : CompAbilityEffect
    {
        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return Valid(target);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            Pawn caster = parent?.pawn;
            Pawn ally = target.Pawn;
            if (caster == null || !WasedaHeroUtility.IsFriendly(caster, ally))
            {
                return false;
            }

            for (int i = 0; i < ally.health.hediffSet.hediffs.Count; i++)
            {
                if (ally.health.hediffSet.hediffs[i].TendableNow())
                {
                    return true;
                }
            }

            return false;
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Pawn ally = target.Pawn;
            if (!Valid(target) || ally == null)
            {
                return;
            }

            int tended = 0;
            List<Hediff> hediffs = ally.health.hediffSet.hediffs;
            for (int i = hediffs.Count - 1; i >= 0; i--)
            {
                Hediff hediff = hediffs[i];
                if (hediff.TendableNow())
                {
                    hediff.Tended(1f, 1f, 1);
                    tended++;
                }
            }

            if (tended > 0 && ally.Map != null)
            {
                MoteMaker.ThrowText(ally.DrawPos, ally.Map, "NumWoundsTended".Translate(tended), 3.65f);
            }
        }
    }

    public class CompProperties_AbilityDamageTransfer : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityDamageTransfer()
        {
            compClass = typeof(CompAbilityEffect_DamageTransfer);
        }
    }

    public class CompAbilityEffect_DamageTransfer : CompAbilityEffect
    {
        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return Valid(target);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            Pawn caster = parent?.pawn;
            Pawn ally = target.Pawn;
            return caster != null && WasedaHeroUtility.IsFriendly(caster, ally) && HasInjury(ally);
        }

        private static bool HasInjury(Pawn pawn)
        {
            if (pawn?.health == null)
            {
                return false;
            }

            for (int i = 0; i < pawn.health.hediffSet.hediffs.Count; i++)
            {
                if (pawn.health.hediffSet.hediffs[i] is Hediff_Injury)
                {
                    return true;
                }
            }

            return false;
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Pawn caster = parent?.pawn;
            Pawn ally = target.Pawn;
            if (caster == null || ally == null || !WasedaHeroUtility.IsFriendly(caster, ally))
            {
                return;
            }

            List<Hediff_Injury> injuries = new List<Hediff_Injury>();
            List<Hediff> hediffs = ally.health.hediffSet.hediffs;
            for (int i = 0; i < hediffs.Count; i++)
            {
                if (hediffs[i] is Hediff_Injury injury)
                {
                    injuries.Add(injury);
                }
            }

            for (int i = 0; i < injuries.Count; i++)
            {
                Hediff_Injury injury = injuries[i];
                float severity = injury.Severity;
                BodyPartRecord part = FindMatchingPart(caster, injury.Part);
                ally.health.RemoveHediff(injury);

                if (severity > 0f && caster.health != null && !caster.Dead)
                {
                    Hediff transferred = HediffMaker.MakeHediff(injury.def, caster, part);
                    transferred.Severity = severity;
                    caster.health.AddHediff(transferred, part);
                }
            }
        }

        private static BodyPartRecord FindMatchingPart(Pawn pawn, BodyPartRecord sourcePart)
        {
            BodyPartRecord fallback = pawn.RaceProps.body.corePart;
            if (sourcePart == null)
            {
                return fallback;
            }

            List<BodyPartRecord> parts = pawn.RaceProps.body.AllParts;
            for (int i = 0; i < parts.Count; i++)
            {
                if (parts[i].def == sourcePart.def && !pawn.health.hediffSet.PartIsMissing(parts[i]))
                {
                    return parts[i];
                }
            }

            return fallback;
        }
    }

    public class CompProperties_AbilityWasedaSpirit : CompProperties_AbilityEffect
    {
        public HediffDef spiritHediff;

        public CompProperties_AbilityWasedaSpirit()
        {
            compClass = typeof(CompAbilityEffect_WasedaSpirit);
        }
    }

    public class CompAbilityEffect_WasedaSpirit : CompAbilityEffect
    {
        public new CompProperties_AbilityWasedaSpirit Props =>
            (CompProperties_AbilityWasedaSpirit)props;

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return parent?.pawn?.Map != null;
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Pawn caster = parent?.pawn;
            if (caster?.Map == null || Props.spiritHediff == null)
            {
                return;
            }

            IReadOnlyList<Pawn> pawns = caster.Map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < pawns.Count; i++)
            {
                if (WasedaHeroUtility.IsFriendly(caster, pawns[i], true))
                {
                    WasedaHeroUtility.GiveHediff(pawns[i], Props.spiritHediff, WasedaHeroUtility.ActiveDurationTicks);
                }
            }
        }
    }

    public class CompProperties_WasedaHeroPassive : CompProperties
    {
        public HediffDef lastStandHediff;
        public HediffDef deathTriggerHediff;
        public float criticalHealthThreshold = 0.3f;

        public CompProperties_WasedaHeroPassive()
        {
            compClass = typeof(CompWasedaHeroPassive);
        }
    }

    public class CompWasedaHeroPassive : ThingComp
    {
        private bool lastStandTriggered;

        public CompProperties_WasedaHeroPassive Props =>
            (CompProperties_WasedaHeroPassive)props;

        public override void CompTickRare()
        {
            base.CompTickRare();
            EnsureDeathTriggerHediff();
            TryTriggerLastStand();
        }

        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostPostApplyDamage(dinfo, totalDamageDealt);
            EnsureDeathTriggerHediff();
            TryTriggerLastStand();
        }

        public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            base.PostPreApplyDamage(ref dinfo, out absorbed);
            EnsureDeathTriggerHediff();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref lastStandTriggered, "lastStandTriggered", false);
        }

        private void TryTriggerLastStand()
        {
            Pawn pawn = parent as Pawn;
            if (pawn == null || pawn.Dead || !pawn.Spawned || lastStandTriggered
                || pawn.health?.summaryHealth == null
                || pawn.health.summaryHealth.SummaryHealthPercent > Props.criticalHealthThreshold)
            {
                return;
            }

            lastStandTriggered = true;
            WasedaHeroUtility.GiveHediffToNearbyAllies(pawn, pawn.Map, pawn.Position, Props.lastStandHediff);
        }

        private void EnsureDeathTriggerHediff()
        {
            Pawn pawn = parent as Pawn;
            if (pawn?.health == null || Props.deathTriggerHediff == null
                || pawn.health.hediffSet.GetFirstHediffOfDef(Props.deathTriggerHediff) != null)
            {
                return;
            }

            pawn.health.AddHediff(HediffMaker.MakeHediff(Props.deathTriggerHediff, pawn));
        }
    }

    public class HediffCompProperties_HeroDeathLegacy : HediffCompProperties
    {
        public HediffDef legacyHediff;

        public HediffCompProperties_HeroDeathLegacy()
        {
            compClass = typeof(HediffComp_HeroDeathLegacy);
        }
    }

    public class HediffComp_HeroDeathLegacy : HediffComp
    {
        private bool triggered;

        public HediffCompProperties_HeroDeathLegacy Props =>
            (HediffCompProperties_HeroDeathLegacy)props;

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit)
        {
            base.Notify_PawnDied(dinfo, culprit);
            if (triggered)
            {
                return;
            }

            triggered = true;
            Pawn pawn = parent.pawn;
            Map map = pawn.MapHeld;
            if (map != null)
            {
                WasedaHeroUtility.GiveHediffToNearbyAllies(pawn, map, pawn.PositionHeld, Props.legacyHediff);
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref triggered, "triggered", false);
        }
    }
}
