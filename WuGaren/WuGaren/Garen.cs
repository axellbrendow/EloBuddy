using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using SharpDX;
using WuAIO.Bases;
using WuAIO.Managers;
using Circle = EloBuddy.SDK.Rendering.Circle;
using ScalingTypes = WuAIO.Managers.DamageManager.ScalingTypes;

namespace WuAIO
{
    class Garen : HeroBase
    {
        readonly AIHeroClient Player = EloBuddy.Player.Instance;

        readonly List<string> buffs = new List<string>() { /*KayleR*/"JudicatorIntervention", /*SivirE*/"SpellShield", "FioraW", /*KindredR*/"kindredrnodeathbuff", "BansheesVeil", /*NocturneShield*/"NocturneShroudofDarknessShield", /*ZileanR*/"ChroneShift", /*YorickR*/"yorickrazombie", /*MordekaiserR*/"mordekaisercotgself", /*TryndamereR*/"UndyingRage", /*SionZombie*/"sionpassivezombie", /*KarthusPassive*/"KarthusDeathDefiedBuff", /*KogmawPassive*/"kogmawicathiansurprise", /*ZyraPassive*/"zyrapqueenofthorns" };

        readonly Spell.Active Q = new Spell.Active(SpellSlot.Q);
        readonly Spell.Active W = new Spell.Active(SpellSlot.W);
        readonly Spell.Active E = new Spell.Active(SpellSlot.E, 300);
        readonly Spell.Targeted R = new Spell.Targeted(SpellSlot.R, 400);

        public override void CreateMenu()
        {
            base.CreateMenu();

            var menu = MenuManager.AddSubMenu("Drawings");
            {
                menu.NewCheckbox("disable", "Disable");
                menu.NewCheckbox("damageindicator", "Damage Indicator");
                menu.NewCheckbox("e", "E");
                menu.NewCheckbox("r", "R");
            }

            menu = MenuManager.AddSubMenu("Combo");
            {
                menu.NewCheckbox("q", "Q");
                menu.NewCheckbox("q.afteraa", "Q after AA");
                menu.NewCheckbox("w", "W", true, true);
                menu.NewCheckbox("e", "E", true, true);
                menu.NewCheckbox("e.jaq", "E just after Q");
                menu.NewCheckbox("r", "R", true, true);
            }

            menu = MenuManager.AddSubMenu("Harass");
            {
                menu.NewCheckbox("q", "Q");
                menu.NewCheckbox("q.afteraa", "Q after AA");
                menu.NewCheckbox("w", "W", true, true);
                menu.NewCheckbox("e", "E", true, true);
                menu.NewCheckbox("e.jaq", "E just after Q");
            }

            menu = MenuManager.AddSubMenu("Lane Clear");
            {
                menu.NewCheckbox("e", "E");
                menu.NewCheckbox("e.minminions", "Min minions E");
            }

            menu = MenuManager.AddSubMenu("Jungle Clear");
            {
                menu.NewCheckbox("q", "Q");
                menu.NewCheckbox("w", "W");
                menu.NewCheckbox("e", "E");
            }

            menu = MenuManager.AddSubMenu("Flee");
            {
                menu.NewCheckbox("q", "Q");
                menu.NewCheckbox("e", "E");
            }

            menu = MenuManager.AddSubMenu("Misc");
            {
                menu.NewCheckbox("ks", "KS");
                menu.NewCheckbox("interrupter", "Interrupter");
            }
        }

        public override void CreateVariables()
        {
            new SkinManager(8);

            damageManager = new DamageManager
            (
                new List<Bases.Damage>()
                {
                    new Bases.Damage
                    (
                        Q, DamageType.Physical, new float[] { 0, 30, 55, 80, 105, 130 },

                        new List<Scale>()
                        {
                            new Scale(new float[] { 0, 1.4f, 1.4f, 1.4f, 1.4f, 1.4f }, ScalingTypes.AD)
                        },
                        true, true, true
                    ),

                    new Bases.Damage
                    (
                        E, DamageType.Magical, new float[] { 0, 15, 18.8f, 22.5f, 26.3f, 30 },

                        new List<Scale>()
                        {
                            new Scale(new float[] { 0, 0.345f, 0.353f, 0.36f, 0.368f, 0.375f }, ScalingTypes.AD)
                        }
                    ),

                    new Bases.Damage
                    (
                        R, DamageType.Physical, new float[] { 0, 175, 350, 525 },

                        new List<Scale>()
                        {
                            new Scale(new float[] { 0, 0.286f, 0.333f, 0.4f }, ScalingTypes.TargetLostLife)
                        },
                        true, true
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

            if (draw.IsActive("e"))
                Circle.Draw(E.IsReady() ? Color.Blue : Color.Red, E.Range, Player.Position);

            if (draw.IsActive("r"))
                Circle.Draw(R.IsReady() ? Color.Blue : Color.Red, R.Range, Player.Position);

            return;
        }

        public override void KS()
        {
            if (!misc.IsActive("ks") || !EntityManager.Heroes.Enemies.Any(it => Q.IsInRange(it))) return;

            if (R.IsReady())
            {
                var bye = EntityManager.Heroes.Enemies.FirstOrDefault(enemy => !enemy.Buffs.Any(it => buffs.Any(mybuffs => mybuffs == it.Name)) && damageManager.SpellDamage(enemy, SpellSlot.R) >= enemy.Health + 20 && enemy.IsValidTarget(R.Range));

                if (bye != null)
                {
                    if (Player.HasBuff("GarenE")) E.Cast();
                    Core.DelayAction(() => R.Cast(bye), 100);
                    return;
                }
            }

            if (Q.IsReady())
            {
                var bye = EntityManager.Heroes.Enemies.FirstOrDefault(enemy => !enemy.Buffs.Any(it => buffs.Any(mybuffs => mybuffs == it.Name)) && damageManager.SpellDamage(enemy, SpellSlot.Q) + Player.GetAutoAttackDamage(enemy) >= enemy.Health && enemy.IsValidTarget(Player.GetAutoAttackRange() - 50));

                if (bye != null)
                {
                    if (Player.HasBuff("GarenE")) return;

                    if (Q.Cast())
                    {
                        Orbwalker.ResetAutoAttack();
                        Core.DelayAction(() => EloBuddy.Player.IssueOrder(GameObjectOrder.AttackTo, bye), 100);
                    }
                    return;
                }
            }
        }

        public override void Flee()
        {
            if (Q.IsReady() && flee.IsActive("q")) Q.Cast();

            if (E.IsReady() && flee.IsActive("e") && EntityManager.Heroes.Enemies.Any(it => !it.IsDead && it.IsValidTarget(E.Range))) E.Cast();

            return;
        }

        public override void Combo()
        {
            var Target = TargetSelector.GetTarget(650, DamageType.Physical);

            if (Target == null || !Target.IsValidTarget() || Target.Buffs.Any(it => buffs.Any(mybuffs => mybuffs == it.Name))) return;

            if (Q.IsReady() && Target.IsValidTarget(Player.GetAutoAttackRange() + 300) && !combo.IsActive("q.afteraa") && combo.IsActive("q")) Q.Cast();

            if (W.IsReady() && Player.IsFacing(Target) && Target.IsValidTarget(Player.GetAutoAttackRange() + 300) && combo.IsActive("w")) W.Cast();

            if (E.IsReady() && E.IsInRange(Target) && !Player.HasBuff("GarenE") && combo.IsActive("e"))
            {
                if (combo.IsActive("e.jaq") && Q.IsReady())
                {
                    if (Target.HasBuffOfType(BuffType.Silence)) E.Cast();
                }
                else E.Cast();
            }

            if (R.IsReady() && R.IsInRange(Target) && damageManager.SpellDamage(Target, SpellSlot.R) >= Target.Health && combo.IsActive("r"))
            {
                if (Player.HasBuff("GarenE")) E.Cast();
                R.Cast(Target);
            }

            return;
        }

        public override void Harass()
        {
            var Target = TargetSelector.GetTarget(650, DamageType.Physical);

            if (Target == null || !Target.IsValidTarget() || Target.Buffs.Any(it => buffs.Any(mybuffs => mybuffs == it.Name))) return;

            if (Q.IsReady() && Target.IsValidTarget(Player.GetAutoAttackRange() + 300) && !harass.IsActive("q.afteraa") && harass.IsActive("q")) Q.Cast();

            if (W.IsReady() && Player.IsFacing(Target) && Target.IsValidTarget(Player.GetAutoAttackRange() + 300) && harass.IsActive("w")) W.Cast();

            if (E.IsReady() && E.IsInRange(Target) && !Player.HasBuff("GarenE") && harass.IsActive("e"))
            {
                if (harass.IsActive("e.jaq") && Q.IsReady())
                {
                    if (Target.HasBuffOfType(BuffType.Silence)) E.Cast();
                }
                else E.Cast();
            }

            return;
        }

        public override void LaneClear()
        {
            if (!E.IsReady() || !laneclear.IsActive("e") || Player.HasBuff("GarenE")) return;

            var minions = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Player.Position, E.Range);

            if (!minions.Any() || minions.Count() < laneclear.Value("e.minminions")) return;

            E.Cast();

            return;
        }

        public override void JungleClear()
        {
            var monsters = EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Position, 600).Where(it => it.Health >= 200);

            if (monsters.Count(it => it.IsValidTarget(E.Range)) == monsters.Count())
            {
                if (E.IsReady() && jungleclear.IsActive("e")) E.Cast();
                if (W.IsReady() && jungleclear.IsActive("w")) W.Cast();
            }

            return;
        }

        public override void Orbwalker_OnPostAttack(AttackableUnit target, EventArgs args)
        {
            if (Player.IsDead || Orbwalker.LastTarget != target) return;

            if (Q.IsReady() && target.IsValidTarget(Player.GetAutoAttackRange() + 200) && (((Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)) || (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass) && harass.IsActive("q.afteraa")))))
            {
                if (Q.Cast()) Orbwalker.ResetAutoAttack();
            }

            if (Q.IsReady() && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear) && jungleclear.IsActive("q"))
            {
                var monsters = EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Position, Player.GetAutoAttackRange());

                if (monsters != null && Player.IsInAutoAttackRange(monsters.First()))
                {
                    if (Q.Cast()) Orbwalker.ResetAutoAttack();
                }
            }

            return;
        }

        public override void Interrupter_OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            if (Player.IsDead || !sender.IsEnemy || !misc.IsActive("interrupter")) return;

            if (sender.IsValidTarget(Player.GetAutoAttackRange()) && Q.IsReady())
            {
                Q.Cast();
                Orbwalker.ResetAutoAttack();
                Core.DelayAction(() => EloBuddy.Player.IssueOrder(GameObjectOrder.AttackTo, sender), 100);
            }

            return;
        }

        //------------------------------------|| Methods ||--------------------------------------

        //--------------------------------------ESpins()-----------------------------------------

        byte Espins()
        {
            if (Player.Level < 3) return 5;
            else if (Player.Level < 6) return 6;
            else if (Player.Level < 9) return 7;
            else if (Player.Level < 12) return 8;
            else if (Player.Level < 15) return 9;
            else if (Player.Level < 19) return 10;
            else return 0;
        }
    }
}