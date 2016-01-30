using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Enumerations;
using SharpDX;
using WuAIO.Bases;
using WuAIO.Managers;
using Circle = EloBuddy.SDK.Rendering.Circle;
using ScalingTypes = WuAIO.Managers.DamageManager.ScalingTypes;

namespace WuAIO
{
    class Irelia : HeroBase
    {
        readonly AIHeroClient Player = EloBuddy.Player.Instance;

        float QSpeed;

        readonly Spell.Targeted Q = new Spell.Targeted(SpellSlot.Q, 625);//650
        readonly Spell.Active W = new Spell.Active(SpellSlot.W);
        readonly Spell.Targeted E = new Spell.Targeted(SpellSlot.E, 425);// \/ not 900 is 1000
        readonly Spell.Skillshot R = new Spell.Skillshot(SpellSlot.R, 900, SkillShotType.Linear, 250, 1600, 120);

        public override void CreateMenu()
        {
            base.CreateMenu();

            var menu = MenuManager.AddSubMenu("Drawings");
            {
                menu.NewCheckbox("disable", "Disable", false);
                menu.NewCheckbox("damageindicator", "Damage Indicator");
                menu.NewCheckbox("q", "Q");
                menu.NewCheckbox("e", "E");
                menu.NewCheckbox("r", "R");
            }

            menu = MenuManager.AddSubMenu("Combo");
            {
                menu.NewCheckbox("q", "Q");
                menu.NewCheckbox("q.smart", "SmartQ");
                menu.NewCheckbox("q.gapclose", "Q on units to gapclose");
                menu.NewCheckbox("w", "W", true, true);
                menu.NewCheckbox("w.beforeq", "W before Q");
                menu.NewCheckbox("e", "E", true, true);
                menu.NewCheckbox("e.jiws", "Just E if will stun");
                menu.NewCheckbox("r", "R", true, true);
                menu.NewCheckbox("r.jisa", "R just if self-actived");
                menu.NewCheckbox("r.gapclose", "R on units to gapclose");
            }

            menu = MenuManager.AddSubMenu("Harass");
            {
                menu.NewCheckbox("q", "Q");
                menu.NewCheckbox("w", "W", true, true);
                menu.NewCheckbox("w.beforeq", "W before Q");
                menu.NewCheckbox("e", "E", true, true);
                menu.NewCheckbox("e.jiws", "Just E if will stun");
                menu.NewSlider("mana%", "Min mana%", 30, 1, 99, true);
            }

            menu = MenuManager.AddSubMenu("Lane Clear");
            {
                menu.NewCheckbox("q", "Q");
                menu.NewCheckbox("w", "W");
                menu.NewSlider("mana%", "Min mana%", 30, 1, 99, true);
            }

            menu = MenuManager.AddSubMenu("Last Hit");
            {
                menu.NewCheckbox("q", "Q");
                menu.NewSlider("mana%", "Min mana%", 30, 1, 99, true);
            }

            menu = MenuManager.AddSubMenu("Jungle Clear");
            {
                menu.NewCheckbox("q", "Q");
                menu.NewCheckbox("w", "W");
                menu.NewCheckbox("e", "E");
                menu.NewSlider("mana%", "Min mana%", 30, 1, 99, true);
            }

            menu = MenuManager.AddSubMenu("Flee");
            {
                menu.NewCheckbox("q", "Q");
                menu.NewCheckbox("e", "E");
                menu.NewCheckbox("r", "R");
            }

            menu = MenuManager.AddSubMenu("Misc");
            {
                menu.NewCheckbox("ks", "KS");
                menu.NewCheckbox("interrupter", "Interrupter");
                menu.NewCheckbox("gapcloser", "Stun/Slow on enemy gapcloser");
                menu.NewCheckbox("eithe", "Auto E if tower hit enemy");
            }
        }

        public override void CreateVariables()
        {
            new SkinManager(6);

            QSpeed = Player.Spellbook.GetSpell(SpellSlot.Q).SData.MissileSpeed;

            R.AllowedCollisionCount = int.MaxValue;

            damageManager = new DamageManager
            (
                new List<Bases.Damage>()
                {
                    new Bases.Damage
                    (
                        Q, DamageType.Physical, new float[] { 0, 20, 50, 80, 110, 140 },

                        new List<Scale>()
                        {
                            new Scale(new float[] { 0, 1, 1, 1, 1, 1 }, ScalingTypes.AD)
                        },
                        true, true, true
                    ),

                    new Bases.Damage
                    (
                        W, DamageType.True, new float[] { 0, 15, 30, 45, 60, 75 }
                    ),

                    new Bases.Damage
                    (
                        E, DamageType.Magical, new float[] { 0, 80, 120, 160, 200, 240 },

                        new List<Scale>()
                        {
                            new Scale(new float[] { 0, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f }, ScalingTypes.AP)
                        },
                        true, true
                    ),

                    new Bases.Damage
                    (
                        R, DamageType.Physical, new float[] { 0, 80, 120, 160 },

                        new List<Scale>()
                        {
                            new Scale(new float[] { 0, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f }, ScalingTypes.AP),
                            new Scale(new float[] { 0, 0.6f, 0.6f, 0.6f, 0.6f, 0.6f }, ScalingTypes.ADBonus)
                        }
                    )
                }
            );
        }

        public override void PermaActive()
        {
            base.PermaActive();
        }

        public override void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead || draw.IsActive("disable")) return;

            if (draw.IsActive("q"))
                Circle.Draw(Q.IsReady() ? Color.Blue : Color.Red, Q.Range, Player.Position);

            if (draw.IsActive("e"))
                Circle.Draw(E.IsReady() ? Color.Blue : Color.Red, E.Range, Player.Position);

            if (draw.IsActive("r"))
                Circle.Draw(R.IsReady() ? Color.Blue : Color.Red, R.Range, Player.Position);

            return;
        }

        public override void KS()
        {
            if (!misc.IsActive("ks") || !EntityManager.Heroes.Enemies.Any(it => Q.IsInRange(it))) return;

            if (Q.IsReady())
            {
                var bye = EntityManager.Heroes.Enemies.FirstOrDefault(enemy => enemy.IsValidTarget(Q.Range) && damageManager.SpellDamage(enemy, SpellSlot.Q) >= enemy.Health);

                if (bye != null) { Q.Cast(bye); return; }

                else
                {
                    if (E.IsReady())
                    {
                        bye = EntityManager.Heroes.Enemies.FirstOrDefault(enemy => enemy.IsValidTarget(E.Range) && damageManager.SpellDamage(enemy, SpellSlot.Q) + damageManager.SpellDamage(enemy, SpellSlot.E) >= enemy.Health);
                        if (bye != null) { E.Cast(bye); return; }
                    }

                    if (W.IsReady())
                    {
                        bye = EntityManager.Heroes.Enemies.FirstOrDefault(enemy => enemy.IsValidTarget(Q.Range) && damageManager.SpellDamage(enemy, SpellSlot.Q) + damageManager.SpellDamage(enemy, SpellSlot.W) >= enemy.Health);
                        if (bye != null)
                        {
                            W.Cast();
                            Core.DelayAction(() => Q.Cast(bye), 100);
                            return;
                        }
                    }

                    if (E.IsReady() && W.IsReady())
                    {
                        bye = EntityManager.Heroes.Enemies.FirstOrDefault(enemy => enemy.IsValidTarget(E.Range) && damageManager.SpellDamage(enemy, SpellSlot.Q) + damageManager.SpellDamage(enemy, SpellSlot.W) + damageManager.SpellDamage(enemy, SpellSlot.E) >= enemy.Health);
                        if (bye != null)
                        {
                            E.Cast(bye);
                            Core.DelayAction(() => W.Cast(), 250);
                            Core.DelayAction(() => Q.Cast(bye), 350);
                            return;
                        }
                    }
                }
            }

            if (E.IsReady())
            {
                var bye = EntityManager.Heroes.Enemies.FirstOrDefault(enemy => enemy.IsValidTarget(E.Range) && damageManager.SpellDamage(enemy, SpellSlot.E) >= enemy.Health);
                if (bye != null) { E.Cast(bye); return; }
            }
        }

        public override void Flee()
        {
            if (Q.IsReady() && flee.IsActive("q"))
            {
                var EscapeTarget = EntityManager.MinionsAndMonsters.CombinedAttackable.FirstOrDefault(it => it.IsValidTarget(Q.Range) && damageManager.SpellDamage(it, SpellSlot.Q) >= it.Health && it.Distance(Game.CursorPos) <= 300);

                if (EscapeTarget != null) Q.Cast(EscapeTarget);

                else
                {
                    EscapeTarget = EntityManager.MinionsAndMonsters.CombinedAttackable.FirstOrDefault(it => it.IsValidTarget(Q.Range) && it.Distance(Game.CursorPos) <= 300);

                    if (EscapeTarget != null) Q.Cast(EscapeTarget);
                }
            }

            if (E.IsReady() && flee.IsActive("e"))
            {
                var ETarget = (from etarget in EntityManager.Heroes.Enemies.Where(it => it.IsValidTarget(E.Range)) orderby TargetSelector.GetPriority(etarget) descending select etarget).FirstOrDefault();

                if (ETarget != null) E.Cast(ETarget);
            }

            if (R.IsReady() && flee.IsActive("r"))
            {
                var RTarget = (from rtarget in EntityManager.Heroes.Enemies.Where(it => it.IsValidTarget(900)) orderby TargetSelector.GetPriority(rtarget) descending select rtarget).FirstOrDefault();

                if (RTarget != null) R.Cast(RTarget);
            }

            return;
        }

        public override void Combo()
        {
            var Target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            var GapCloseTarget = TargetSelector.GetTarget(1200, DamageType.Physical);

            if (GapCloseTarget == null || (Target == null && GapCloseTarget != null && !Q.IsReady())) return;

            if (EntityManager.MinionsAndMonsters.CombinedAttackable.Any(it => it.IsValidTarget(Q.Range) && damageManager.SpellDamage(it, SpellSlot.Q) >= it.Health))
            {
                if (TargetSelector.GetPriority(GapCloseTarget) > TargetSelector.GetPriority(Target)) Target = GapCloseTarget;
                else GapCloseTarget = Target;
            }

            if (Q.IsReady() && combo.IsActive("q"))
            {
                if (Target.IsValidTarget(Q.Range))
                {
                    if (combo.IsActive("q.smart")) QLogic(Target);

                    else if (W.IsReady() && combo.IsActive("w.beforeq"))
                    {
                        W.Cast();
                        Core.DelayAction(() => Q.Cast(Target), 100);
                    }

                    else Q.Cast(Target);
                }

                else if (combo.IsActive("q.gapclose"))
                {
                    var Minions = EntityManager.MinionsAndMonsters.CombinedAttackable.Where(it => it.IsValidTarget(Q.Range) && it.Distance(Game.CursorPos) <= 500 && damageManager.SpellDamage(it, SpellSlot.Q) >= it.Health);

                    if (Minions.Any())
                    {
                        var Minion = Minions.OrderBy(it => it.Distance(GapCloseTarget)).First();
                        Q.Cast(Minion);
                    }

                    else if (R.IsReady() && combo.IsActive("r.gapclose"))
                    {
                        Minions = EntityManager.MinionsAndMonsters.CombinedAttackable.Where(it => it.IsValidTarget(Q.Range) && it.Distance(Game.CursorPos) <= 500 && damageManager.SpellDamage(it, SpellSlot.Q) + damageManager.SpellDamage(it, SpellSlot.R) >= it.Health);

                        if (Minions.Any())
                        {
                            var Minion = Minions.OrderBy(it => it.Distance(GapCloseTarget)).First();

                            if (R.Cast(Minion)) Core.DelayAction(() => Q.Cast(Minion), R.CastDelay + 100);
                        }
                    }
                }
            }

            if (W.IsReady() && combo.IsActive("w") && (!Q.IsReady() || !combo.IsActive("w.beforeq")) && Player.Distance(Target) <= Player.GetAutoAttackRange(Target) - 50) W.Cast();

            if (R.IsReady() && Target.IsValidTarget(R.Range) && combo.IsActive("r"))
            {
                var RPred = R.GetPrediction(Target);

                if (RPred.HitChancePercent >= 75)
                {
                    if (combo.IsActive("r.jisa"))
                    {
                        if (Player.HasBuff("ireliatranscendentbladesspell")) R.Cast(RPred.CastPosition);
                    }
                    else R.Cast(RPred.CastPosition);
                }
            }

            if (E.IsReady() && Target.IsValidTarget(E.Range))
            {
                if (combo.IsActive("e.jiws"))
                {
                    if (Player.HealthPercent <= Target.HealthPercent) E.Cast(Target);
                }
                else E.Cast(Target);
            }

            return;
        }

        public override void Harass()
        {
            var Target = TargetSelector.GetTarget(900, DamageType.Physical);

            if (Target == null || !Target.IsValidTarget() || Player.ManaPercent < harass.Value("mana%")) return;

            if (Q.IsReady() && Q.IsInRange(Target) && Player.Distance(Target) >= Player.GetAutoAttackRange(Target) + 200 && harass.IsActive("q"))
            {
                if (W.IsReady() && harass.IsActive("w.beforeq"))
                {
                    if (W.Cast()) Core.DelayAction(() => Q.Cast(Target), 100);
                }
                else Q.Cast(Target);
            }

            if (W.IsReady() && harass.IsActive("w") && (!Q.IsReady() || !harass.IsActive("w.beforeq")) && Player.Distance(Target) <= Player.GetAutoAttackRange(Target) - 50) W.Cast();

            if (E.IsReady() && E.IsInRange(Target) && harass.IsActive("e"))
            {
                if (harass.IsActive("e.jiws"))
                {
                    if (Player.HealthPercent <= Target.HealthPercent) E.Cast(Target);
                }
                else E.Cast(Target);
            }

            return;
        }

        public override void LaneClear()
        {
            if (Player.ManaPercent < laneclear.Value("mana%")) return;

            var minions = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Player.Position, Q.Range);
            if (!minions.Any()) return;

            if (Q.IsReady() && laneclear.IsActive("q"))
            {
                //var Turret = ObjectManager.Get<Obj_AI_Turret>().FirstOrDefault(it => !it.IsDead && it.IsEnemy && it.Distance(Player) <= 1300);
                var Minion = EntityManager.MinionsAndMonsters.EnemyMinions.FirstOrDefault(it => it.IsValidTarget(Q.Range) && it.Distance(Player) >= Player.GetAutoAttackRange(it) && damageManager.SpellDamage(it, SpellSlot.Q) >= it.Health);

                if (Minion != null) Q.Cast(Minion);
            }

            if (W.IsReady() && laneclear.IsActive("w") && EntityManager.MinionsAndMonsters.EnemyMinions.Where(it => it.IsValidTarget() && Player.IsInAutoAttackRange(it)).Count() >= 3) W.Cast();

            return;
        }

        public override void JungleClear()
        {
            if (Player.ManaPercent >= jungleclear.Value("mana%"))
            {
                var monsters = EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Position, Q.Range);

                if (monsters.Any())
                {
                    if (Q.IsReady() && jungleclear.IsActive("q")) Q.Cast(monsters.First());

                    if (!Player.HasBuff("JaxCounterStrike") && E.IsReady() && monsters.Any(it => Player.IsInAutoAttackRange(it)) && jungleclear.IsActive("e")) E.Cast();
                }
            }

            return;
        }

        public override void LastHit()
        {
            if (Player.ManaPercent <= lasthit.Value("mana%")) return;

            var minions = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Player.Position, Q.Range);
            if (!minions.Any()) return;

            if (Q.IsReady() && lasthit.IsActive("q"))
            {
                //var Turret = ObjectManager.Get<Obj_AI_Turret>().FirstOrDefault(it => !it.IsDead && it.IsEnemy && it.Distance(Player) <= 1300);
                var Minion = EntityManager.MinionsAndMonsters.EnemyMinions.FirstOrDefault(it => it.IsValidTarget(Q.Range) && it.Distance(Player) > Player.GetAutoAttackRange(it) && damageManager.SpellDamage(it, SpellSlot.Q) >= it.Health);

                if (Minion != null) Q.Cast(Minion);
            }

            return;
        }

        public override void Obj_AI_Turret_OnBasicAttack(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (Player.IsDead || !(sender is Obj_AI_Turret)) return;

            if (sender.IsAlly && E.IsReady() && misc.IsActive("eithe"))
            {
                var target = EntityManager.Heroes.Enemies.FirstOrDefault(it => it.NetworkId == args.Target.NetworkId && it.IsValidTarget(E.Range));

                if (target != null && Player.HealthPercent <= target.HealthPercent) E.Cast(target);

                return;
            }

            return;
        }

        public override void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (Player.IsDead || !sender.IsEnemy || !misc.IsActive("gapcloser")) return;

            if (E.IsReady() && sender.IsValidTarget(E.Range)) E.Cast(sender);

            return;
        }

        public override void Interrupter_OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            if (Player.IsDead || !sender.IsEnemy || !misc.IsActive("interrupter")) return;

            if (E.IsReady())
            {
                if (sender.IsValidTarget(E.Range) && sender.HealthPercent >= Player.HealthPercent) { E.Cast(sender); return; }

                else if (Q.IsReady() && sender.IsValidTarget(Q.Range) && ((sender.Health - damageManager.SpellDamage(sender, SpellSlot.Q)) / sender.MaxHealth) * 100 >= Player.HealthPercent)
                {
                    if (Q.Cast(sender))
                    {
                        var delay = (int)(Game.Ping + Q.CastDelay + (Player.Distance(sender) / QSpeed * 1000));

                        Core.DelayAction(() => E.Cast(sender), delay);
                    }

                    return;
                }
            }

            return;
        }

        //------------------------------------|| Methods ||--------------------------------------

        //--------------------------------------QLogic()-----------------------------------------

        void QLogic(AIHeroClient Target)
        {
            if (combo.IsActive("w.beforeq") && W.IsReady())
            {
                if (Target.IsDashing()) { W.Cast(); Core.DelayAction(() => Q.Cast(Target), Game.Ping + W.CastDelay); }
                if (Player.Distance(Target) >= Player.GetAutoAttackRange(Target)) { W.Cast(); Core.DelayAction(() => Q.Cast(Target), Game.Ping + W.CastDelay); }
                if (Target.HealthPercent <= 30) { W.Cast(); Core.DelayAction(() => Q.Cast(Target), Game.Ping + W.CastDelay); }
                if (Player.HealthPercent <= 30) { W.Cast(); Core.DelayAction(() => Q.Cast(Target), Game.Ping + W.CastDelay); }
                if (damageManager.SpellDamage(Target, SpellSlot.Q) >= Target.Health) { W.Cast(); Core.DelayAction(() => Q.Cast(Target), Game.Ping + W.CastDelay); }
            }

            else
            {
                if (Target.IsDashing()) Q.Cast(Target);
                if (Player.Distance(Target) >= Player.GetAutoAttackRange(Target)) Q.Cast(Target);
                if (Target.HealthPercent <= 30) Q.Cast(Target);
                if (Player.HealthPercent <= 30) Q.Cast(Target);
                if (damageManager.SpellDamage(Target, SpellSlot.Q) >= Target.Health) Q.Cast(Target);
            }
        }
    }
}
