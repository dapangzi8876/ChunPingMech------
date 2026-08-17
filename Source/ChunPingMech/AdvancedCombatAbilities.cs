using System.Collections.Generic;
using RimWorld;
using Verse;

namespace ChunPingMech
{
    public static class AdvancedCombatAbilityUtility
    {
        public static bool IsValidCell(Pawn caster, LocalTargetInfo target)
        {
            return caster?.Map != null && target.IsValid && target.Cell.InBounds(caster.Map)
                && target.Cell.Standable(caster.Map);
        }

        public static bool TryFindSpawnCell(Pawn caster, IntVec3 center, out IntVec3 result)
        {
            result = IntVec3.Invalid;
            if (caster?.Map == null)
            {
                return false;
            }

            foreach (IntVec3 cell in GenRadial.RadialCellsAround(center, 2.9f, true))
            {
                if (cell.InBounds(caster.Map) && cell.Standable(caster.Map)
                    && cell.GetFirstPawn(caster.Map) == null)
                {
                    result = cell;
                    return true;
                }
            }

            return false;
        }
    }

    public class CompProperties_AbilityCellRecombination : CompProperties_AbilityEffect
    {
        public HediffDef healingHediff;
        public int durationTicks = 900;

        public CompProperties_AbilityCellRecombination()
        {
            compClass = typeof(CompAbilityEffect_CellRecombination);
        }
    }

    public class CompAbilityEffect_CellRecombination : CompAbilityEffect
    {
        public new CompProperties_AbilityCellRecombination Props =>
            (CompProperties_AbilityCellRecombination)props;

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return Valid(target);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            Pawn caster = parent?.pawn;
            Pawn patient = target.Pawn;
            bool valid = caster != null && WasedaHeroUtility.IsFriendly(caster, patient)
                && patient.health?.hediffSet != null && HasInjury(patient);
            if (!valid && throwMessages)
            {
                Messages.Message("只能选择有伤势的友军。", MessageTypeDefOf.RejectInput, false);
            }

            return valid;
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Pawn patient = target.Pawn;
            if (patient != null && Valid(target))
            {
                WasedaHeroUtility.GiveHediff(patient, Props.healingHediff, Props.durationTicks);
            }
        }

        private static bool HasInjury(Pawn pawn)
        {
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (int i = 0; i < hediffs.Count; i++)
            {
                if (hediffs[i] is Hediff_Injury)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class HediffCompProperties_CellRecombination : HediffCompProperties
    {
        public int pulseIntervalTicks = 60;
        public float healPerInjury = 2f;

        public HediffCompProperties_CellRecombination()
        {
            compClass = typeof(HediffComp_CellRecombination);
        }
    }

    public class HediffComp_CellRecombination : HediffComp
    {
        private int ticksUntilHeal;

        public HediffCompProperties_CellRecombination Props =>
            (HediffCompProperties_CellRecombination)props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            ticksUntilHeal--;
            if (ticksUntilHeal > 0)
            {
                return;
            }

            ticksUntilHeal = Props.pulseIntervalTicks;
            Pawn pawn = parent.pawn;
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (int i = hediffs.Count - 1; i >= 0; i--)
            {
                if (hediffs[i] is Hediff_Injury injury)
                {
                    injury.Heal(Props.healPerInjury);
                }
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref ticksUntilHeal, "ticksUntilHeal", 0);
        }
    }

    public class CompProperties_AbilitySummonMech : CompProperties_AbilityEffect
    {
        public PawnKindDef pawnKind;

        public CompProperties_AbilitySummonMech()
        {
            compClass = typeof(CompAbilityEffect_SummonMech);
        }
    }

    public class CompAbilityEffect_SummonMech : CompAbilityEffect
    {
        public new CompProperties_AbilitySummonMech Props =>
            (CompProperties_AbilitySummonMech)props;

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return Valid(target);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            Pawn caster = parent?.pawn;
            bool valid = caster?.Map != null && target.IsValid && target.Cell.InBounds(caster.Map)
                && AdvancedCombatAbilityUtility.TryFindSpawnCell(caster, target.Cell, out _);
            if (!valid && throwMessages)
            {
                Messages.Message("目标附近没有可供机械体出现的位置。", MessageTypeDefOf.RejectInput, false);
            }

            return valid;
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Pawn caster = parent?.pawn;
            if (caster == null || Props.pawnKind == null
                || !AdvancedCombatAbilityUtility.TryFindSpawnCell(caster, target.Cell, out IntVec3 cell))
            {
                return;
            }

            Pawn summoned = PawnGenerator.GeneratePawn(Props.pawnKind, caster.Faction);
            GenSpawn.Spawn(summoned, cell, caster.Map);
        }
    }

    public class CompProperties_SummonedLifetime : CompProperties
    {
        public int lifespanTicks = 7200;

        public CompProperties_SummonedLifetime()
        {
            compClass = typeof(CompSummonedLifetime);
        }
    }

    public class CompSummonedLifetime : ThingComp
    {
        private int ageTicks;

        public CompProperties_SummonedLifetime Props =>
            (CompProperties_SummonedLifetime)props;

        public override void CompTick()
        {
            base.CompTick();
            ageTicks++;
            if (ageTicks >= Props.lifespanTicks && parent is Pawn pawn && !pawn.Dead)
            {
                pawn.Kill(null);
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref ageTicks, "ageTicks", 0);
        }
    }

    public class CompProperties_AbilityAreaEmp : CompProperties_AbilityEffect
    {
        public float radius = 8f;
        public int damageAmount = 45;

        public CompProperties_AbilityAreaEmp()
        {
            compClass = typeof(CompAbilityEffect_AreaEmp);
        }
    }

    public class CompAbilityEffect_AreaEmp : CompAbilityEffect
    {
        public new CompProperties_AbilityAreaEmp Props =>
            (CompProperties_AbilityAreaEmp)props;

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return Valid(target);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            Pawn caster = parent?.pawn;
            bool valid = caster?.Map != null && target.IsValid && target.Cell.InBounds(caster.Map);
            if (!valid && throwMessages)
            {
                Messages.Message("无法在该位置释放 EMC。", MessageTypeDefOf.RejectInput, false);
            }

            return valid;
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Pawn caster = parent?.pawn;
            if (caster?.Map != null && Valid(target))
            {
                GenExplosion.DoExplosion(target.Cell, caster.Map, Props.radius, DamageDefOf.EMP,
                    caster, Props.damageAmount, 0f);
            }
        }
    }

    public class CompProperties_AbilityDeployTrap : CompProperties_AbilityEffect
    {
        public ThingDef trapDef;

        public CompProperties_AbilityDeployTrap()
        {
            compClass = typeof(CompAbilityEffect_DeployTrap);
        }
    }

    public class CompAbilityEffect_DeployTrap : CompAbilityEffect
    {
        public new CompProperties_AbilityDeployTrap Props =>
            (CompProperties_AbilityDeployTrap)props;

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return Valid(target);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            Pawn caster = parent?.pawn;
            bool valid = AdvancedCombatAbilityUtility.IsValidCell(caster, target)
                && target.Cell.GetFirstBuilding(caster.Map) == null;
            if (!valid && throwMessages)
            {
                Messages.Message("该位置无法部署陷阱。", MessageTypeDefOf.RejectInput, false);
            }

            return valid;
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Pawn caster = parent?.pawn;
            if (caster?.Map == null || Props.trapDef == null || !Valid(target))
            {
                return;
            }

            Thing trap = ThingMaker.MakeThing(Props.trapDef);
            trap.SetFactionDirect(caster.Faction);
            GenSpawn.Spawn(trap, target.Cell, caster.Map);
        }
    }

    public class CompProperties_ProximityTrap : CompProperties
    {
        public float triggerRadius = 3.5f;
        public float explosionRadius = 4f;
        public int damageAmount = 45;

        public CompProperties_ProximityTrap()
        {
            compClass = typeof(CompProximityTrap);
        }
    }

    public class CompProximityTrap : ThingComp
    {
        public CompProperties_ProximityTrap Props =>
            (CompProperties_ProximityTrap)props;

        public override void CompTickRare()
        {
            base.CompTickRare();
            if (!parent.Spawned || parent.Map == null)
            {
                return;
            }

            IReadOnlyList<Pawn> pawns = parent.Map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn pawn = pawns[i];
                if (!pawn.Dead && pawn.HostileTo(parent)
                    && pawn.Position.DistanceTo(parent.Position) <= Props.triggerRadius)
                {
                    Trigger();
                    return;
                }
            }
        }

        private void Trigger()
        {
            Map map = parent.Map;
            IntVec3 position = parent.Position;
            GenExplosion.DoExplosion(position, map, Props.explosionRadius, DamageDefOf.Bomb,
                parent, Props.damageAmount, 0.5f);
            if (!parent.Destroyed)
            {
                parent.Destroy(DestroyMode.KillFinalize);
            }
        }
    }

    public class CompProperties_HighAggroDecoy : CompProperties
    {
        public float aggroRadius = 24f;

        public CompProperties_HighAggroDecoy()
        {
            compClass = typeof(CompHighAggroDecoy);
        }
    }

    public class CompHighAggroDecoy : ThingComp
    {
        private int ticksUntilAggroPulse;

        public CompProperties_HighAggroDecoy Props =>
            (CompProperties_HighAggroDecoy)props;

        public override void CompTick()
        {
            base.CompTick();
            ticksUntilAggroPulse--;
            if (ticksUntilAggroPulse > 0)
            {
                return;
            }

            ticksUntilAggroPulse = 60;
            Pawn decoy = parent as Pawn;
            if (decoy?.Map == null || decoy.Dead)
            {
                return;
            }

            IReadOnlyList<Pawn> pawns = decoy.Map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn enemy = pawns[i];
                if (enemy != decoy && !enemy.Dead && !enemy.Downed && enemy.HostileTo(decoy)
                    && enemy.Position.DistanceTo(decoy.Position) <= Props.aggroRadius
                    && enemy.mindState != null)
                {
                    enemy.mindState.enemyTarget = decoy;
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref ticksUntilAggroPulse, "ticksUntilAggroPulse", 0);
        }
    }

    public class CompProperties_AbilityEnemyDebuff : CompProperties_AbilityEffect
    {
        public HediffDef hediffDef;
        public int durationTicks = 1200;

        public CompProperties_AbilityEnemyDebuff()
        {
            compClass = typeof(CompAbilityEffect_EnemyDebuff);
        }
    }

    public class CompAbilityEffect_EnemyDebuff : CompAbilityEffect
    {
        public new CompProperties_AbilityEnemyDebuff Props =>
            (CompProperties_AbilityEnemyDebuff)props;

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return Valid(target);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            Pawn caster = parent?.pawn;
            Pawn enemy = target.Pawn;
            bool valid = caster != null && enemy != null && enemy != caster && !enemy.Dead
                && enemy.Spawned && enemy.Map == caster.Map && enemy.HostileTo(caster);
            if (!valid && throwMessages)
            {
                Messages.Message("只能选择敌方单位。", MessageTypeDefOf.RejectInput, false);
            }

            return valid;
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Pawn enemy = target.Pawn;
            if (enemy != null && Valid(target))
            {
                WasedaHeroUtility.GiveHediff(enemy, Props.hediffDef, Props.durationTicks);
            }
        }
    }
}
