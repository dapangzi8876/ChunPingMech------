using RimWorld;
using Verse;

namespace ChunPingMech
{
    public class CompProperties_AbilityExecution : CompProperties_AbilityEffect
    {
        public float damageAmount = 70f;
        public float armorPenetration = 1.5f;
        public float executionHealthThreshold = 0.1f;

        public CompProperties_AbilityExecution()
        {
            compClass = typeof(CompAbilityEffect_Execution);
        }
    }

    public class CompAbilityEffect_Execution : CompAbilityEffect
    {
        public new CompProperties_AbilityExecution Props =>
            (CompProperties_AbilityExecution)props;

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return Valid(target);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            Pawn caster = parent?.pawn;
            Pawn victim = target.Pawn;
            return caster != null && victim != null && victim != caster
                && !victim.Dead && victim.Spawned && victim.Map == caster.Map
                && victim.HostileTo(caster);
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Pawn caster = parent?.pawn;
            Pawn victim = target.Pawn;
            if (!Valid(target))
            {
                return;
            }

            if (victim.health.summaryHealth.SummaryHealthPercent < Props.executionHealthThreshold)
            {
                victim.Kill(new DamageInfo(
                    DamageDefOf.ExecutionCut,
                    99999f,
                    armorPenetration: 999f,
                    instigator: caster));
                return;
            }

            BodyPartRecord hitPart = victim.RaceProps.body.corePart;
            victim.TakeDamage(new DamageInfo(
                DamageDefOf.Cut,
                Props.damageAmount,
                armorPenetration: Props.armorPenetration,
                instigator: caster,
                hitPart: hitPart));
        }
    }
}
