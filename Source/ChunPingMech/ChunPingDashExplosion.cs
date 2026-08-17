using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace ChunPingMech
{
    public class CompProperties_AbilityDashExplosion : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityDashExplosion()
        {
            compClass = typeof(CompAbilityEffect_DashExplosion);
        }
    }

    public class CompAbilityEffect_DashExplosion : CompAbilityEffect
    {
        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Pawn caster = parent?.pawn;
            if (caster?.Map == null || !target.IsValid || !target.Cell.InBounds(caster.Map))
            {
                return false;
            }

            return caster.CanReach(target.Cell, PathEndMode.OnCell, Danger.Deadly);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            Pawn caster = parent?.pawn;
            if (caster?.Map == null || !target.IsValid || !target.Cell.InBounds(caster.Map))
            {
                if (throwMessages)
                {
                    Messages.Message("Invalid target location.", MessageTypeDefOf.RejectInput, false);
                }

                return false;
            }

            if (!caster.CanReach(target.Cell, PathEndMode.OnCell, Danger.Deadly))
            {
                if (throwMessages)
                {
                    Messages.Message("Unable to reach that location.", MessageTypeDefOf.RejectInput, false);
                }

                return false;
            }

            return true;
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Pawn caster = parent?.pawn;
            if (caster == null || caster.Map == null || !target.IsValid)
            {
                return;
            }

            Job job = JobMaker.MakeJob(ChunPingJobDefOf.ChunPing_DashExplosion);
            job.targetA = target;

            caster.jobs?.TryTakeOrderedJob(job, JobTag.Misc);
        }
    }

    [DefOf]
    public static class ChunPingJobDefOf
    {
        public static JobDef ChunPing_DashExplosion;

        static ChunPingJobDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ChunPingJobDefOf));
        }
    }

    public class JobDriver_ChunPingDashExplosion : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);

            yield return Toils_General.Do(delegate
            {
                if (pawn?.Spawned == true && pawn.Map != null)
                {
                    GenExplosion.DoExplosion(pawn.Position, pawn.Map, 3.0f, DamageDefOf.Bomb, pawn, 55, 0f);
                }
            });
        }
    }
}
