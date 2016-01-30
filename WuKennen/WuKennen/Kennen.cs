using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using SharpDX;
using WuAIO.Bases;
using WuAIO.Managers;
using WuAIO.Extensions;
using Circle = EloBuddy.SDK.Rendering.Circle;
using ScalingTypes = WuAIO.Managers.DamageManager.ScalingTypes;

namespace WuAIO
{
    class Kennen : HeroBase
    {
        readonly AIHeroClient Player = EloBuddy.Player.Instance;

        Spell.Skillshot Q = new Spell.Skillshot(SpellSlot.Q, 1000, SkillShotType.Linear, 125, 1700, 50);
        Spell.Active W = new Spell.Active(SpellSlot.W, 900);
        Spell.Active E = new Spell.Active(SpellSlot.E, 700);//Kappa
        Spell.Active R = new Spell.Active(SpellSlot.R, 500);

        public override void CreateMenu()
        {
            base.CreateMenu();

            var menu = MenuManager.AddSubMenu("Drawings");
            {
                menu.NewCheckbox("disable", "Disable", false);
                menu.NewCheckbox("damageindicator", "Damage Indicator");
                menu.NewCheckbox("r", "R");
                menu.NewCheckbox("q", "Q");
                menu.NewCheckbox("w", "W");
            }

            menu = MenuManager.AddSubMenu("Combo");
            {
                menu.NewCheckbox("q", "Q");
                menu.NewSlider("q.hitchance%", "Q HitChance%", 85, 1, 100);
                menu.NewCheckbox("w", "W", true, true);
                menu.NewCheckbox("e", "E");
                menu.NewCheckbox("r", "R", true);
                menu.NewSlider("r.minenemies", "Min enemies R", 2, 1, 5);
            }

            menu = MenuManager.AddSubMenu("Harass");
            {
                menu.NewCheckbox("q", "Q");
                menu.NewSlider("q.hitchance%", "Q HitChance%", 85, 1, 100);
                menu.NewCheckbox("w", "W", true, true);
                menu.NewSlider("mana%", "Min mana%", 30, 1, 99, true);
            }

            menu = MenuManager.AddSubMenu("Last Hit");
            {
                menu.NewCheckbox("q", "Q");
                menu.NewCheckbox("w", "W");
                menu.NewSlider("mana%", "Min mana%", 30, 1, 99, true);
            }

            menu = MenuManager.AddSubMenu("Lane Clear");
            {
                menu.NewCheckbox("q", "Q");
                menu.NewCheckbox("w", "W");
                menu.NewCheckbox("e", "E");
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
                menu.NewCheckbox("w", "W");
                menu.NewCheckbox("e", "E");
            }

            menu = MenuManager.AddSubMenu("Misc");
            {
                menu.NewCheckbox("ks", "KS");
                menu.NewCheckbox("interrupter", "Interrupt spells");
                menu.NewCheckbox("gapcloser", "Q on enemy gapcloser", true);
            }
        }

        public override void CreateVariables()
        {
            new SkinManager(8);

            Q.MinimumHitChance = HitChance.Medium;

            damageManager = new DamageManager
            (
                new List<Bases.Damage>()
                {
                    new Bases.Damage
                    (
                        Q, DamageType.Magical, new float[] { 0, 75, 115, 155, 195, 235 },

                        new List<Scale>()
                        {
                            new Scale(new float[] { 0, 0.75f, 0.75f, 0.75f, 0.75f, 0.75f }, ScalingTypes.AP)
                        }
                    ),

                    new Bases.Damage
                    (
                        W, DamageType.Magical, new float[] { 0, 65, 95, 125, 15, 185 },

                        new List<Scale>()
                        {
                            new Scale(new float[] { 0, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f }, ScalingTypes.AD),
                            new Scale(new float[] { 0, 0.55f, 0.55f, 0.55f, 0.55f, 0.55f }, ScalingTypes.AP)
                        }
                    ),

                    new Bases.Damage
                    (
                        E, DamageType.Magical, new float[] { 0, 85, 125, 165, 205, 245 },

                        new List<Scale>()
                        {
                            new Scale(new float[] { 0, 0.6f, 0.6f, 0.6f, 0.6f, 0.6f }, ScalingTypes.AP)
                        }
                    ),

                    new Bases.Damage
                    (
                        R, DamageType.Magical, new float[] { 0, 80, 145, 210 },

                        new List<Scale>()
                        {
                            new Scale(new float[] { 0, 0.4f, 0.4f, 0.4f, 0.4f, 0.4f }, ScalingTypes.AP)
                        }
                    )
                }
            );
        }

        public override void PermaActive()
        {
            base.PermaActive();

            if (EBuff()) Orbwalker.DisableAttacking = true;
            else Orbwalker.DisableAttacking = false;
        }

        public override void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead || draw.IsActive("disable")) return;

            if (draw.IsActive("r"))
                Circle.Draw(R.IsReady() ? Color.Blue : Color.Red, R.Range, Player.Position);

            if (draw.IsActive("q"))
                Circle.Draw(Q.IsReady() ? Color.Blue : Color.Red, Q.Range, Player.Position);

            if (draw.IsActive("w"))
                Circle.Draw(W.IsReady() ? Color.Blue : Color.Red, W.Range, Player.Position);

        }

        public override void Combo()
        {
            var Target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);

            if (Target == null || !Target.IsValidTarget()) return;

            if (!EBuff())
            {
                if (E.IsReady() && E.IsInRange(Target) && combo.IsActive("e")) E.Cast();
                if (Q.IsReady() && Q.IsInRange(Target) && combo.IsActive("q")) Q.HitChanceCast(Target, combo.Value("q.hitchance%"));
            }

            if (R.IsReady() && Player.CountEnemiesInRange(R.Range) >= combo.Value("r.minenemies") && combo.IsActive("r")) R.Cast();

            if (W.IsReady() && W.IsInRange(Target) && Target.HasBuff("kennenmarkofstorm") && combo.IsActive("w")) W.Cast();

            return;
        }

        public override void Harass()
        {
            var Target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);

            if (Target == null || !Target.IsValidTarget() || Player.ManaPercent < harass.Value("mana%")) return;

            if (!EBuff())
            {
                if (Q.IsReady() && Q.IsInRange(Target) && harass.IsActive("q")) Q.HitChanceCast(Target, harass.Value("q.hitchance%"));
            }

            if (W.IsReady() && W.IsInRange(Target) && Target.HasBuff("kennenmarkofstorm") && harass.IsActive("w")) W.Cast();

            return;
        }

        public override void LaneClear()
        {
            if (Player.ManaPercent <= laneclear.Value("mana%")) return;

            if (!EBuff())
            {
                if (E.IsReady() && EntityManager.MinionsAndMonsters.CombinedAttackable.Where(it => it.IsValidTarget(500)).Count() >= 4 && laneclear.IsActive("e")) E.Cast();

                if (Q.IsReady() && laneclear.IsActive("q"))
                {
                    var QMinion = EntityManager.MinionsAndMonsters.CombinedAttackable.FirstOrDefault(it => it.IsValidTarget(Q.Range) && damageManager.SpellDamage(it, SpellSlot.Q) >= it.Health);

                    if (QMinion != null) Q.HitChanceCast(QMinion, 40);
                }

                if (W.IsReady() && laneclear.IsActive("w"))
                {
                    var Minions = EntityManager.MinionsAndMonsters.CombinedAttackable.Where(it => it.IsValidTarget(W.Range) && it.HasBuff("kennenmarkofstorm"));

                    if (Minions.Count() >= laneclear.Value("w.minminions")) W.Cast();
                }
            }

            return;
        }

        public override void JungleClear()
        {
            if (!EBuff())
            {
                if (E.IsReady() && jungleclear.IsActive("e") && EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Position, 450).Where(it => it.Health >= 250).Any()) E.Cast();

                if (Q.IsReady() && jungleclear.IsActive("q"))
                {
                    var QMinion = EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Position, Q.Range).FirstOrDefault(it => it.IsValidTarget(Q.Range));

                    if (QMinion != null) Q.HitChanceCast(QMinion, 40);
                }

                if (W.IsReady() && jungleclear.IsActive("w")) W.Cast();
            }

            return;
        }

        public override void Flee()
        {
            if (!Q.IsReady() || !flee.IsActive("q")) return;

            if (!EBuff())
            {
                if (E.IsReady() && misc.IsActive("e")) E.Cast();

                if (Q.IsReady() && misc.IsActive("q"))
                {
                    var QTarget = (from enemy in EntityManager.Heroes.Enemies.Where(it => it.IsValidTarget(Q.Range)) orderby TargetSelector.GetPriority(enemy) descending select enemy).FirstOrDefault();
                    if (QTarget != null) Q.HitChanceCast(QTarget, 75);
                }
            }

            if (W.IsReady() && misc.IsActive("w"))
            {
                var WTarget = EntityManager.Heroes.Enemies.FirstOrDefault(it => it.IsValidTarget(W.Range) && it.HasBuff("kennenmarkofstorm"));
                if (WTarget != null) W.Cast();
            }
        }

        public override void KS()
        {
            if (!EntityManager.Heroes.Enemies.Any(it => !it.IsDead && it.IsValidTarget(Q.Range)) || !misc.IsActive("ks")) return;

            if (Q.IsReady())
            {
                var bye = EntityManager.Heroes.Enemies.FirstOrDefault(it => it.IsValidTarget(Q.Range) && damageManager.SpellDamage(it, SpellSlot.Q) >= it.Health);
                if (bye != null) Q.HitChanceCast(bye, 75);
            }

            if (W.IsReady())
            {
                var bye = EntityManager.Heroes.Enemies.FirstOrDefault(it => it.IsValidTarget(W.Range) && damageManager.SpellDamage(it, SpellSlot.W) >= it.Health && it.HasBuff("kennenmarkofstorm"));
                if (bye != null) { W.Cast(); return; }
            }

            if (Q.IsReady() && !W.IsOnCooldown)
            {
                var bye = EntityManager.Heroes.Enemies.FirstOrDefault(it => it.IsValidTarget(W.Range) && damageManager.SpellDamage(it, SpellSlot.Q) + damageManager.SpellDamage(it, SpellSlot.W) >= it.Health);
                if (bye != null) { Q.HitChanceCast(bye, 75); return; }
            }

            if (R.IsReady())
            {
                var bye = EntityManager.Heroes.Enemies.FirstOrDefault(it => it.IsValidTarget(R.Range - 150) && damageManager.SpellDamage(it, SpellSlot.R) * 2 >= it.Health);
                if (bye != null) { R.Cast(); return; }

                bye = EntityManager.Heroes.Enemies.FirstOrDefault(it => it.IsValidTarget(R.Range) && damageManager.SpellDamage(it, SpellSlot.R) >= it.Health);
                if (bye != null) { R.Cast(); return; }
            }
        }

        public override void Orbwalker_OnUnkillableMinion(Obj_AI_Base target, Orbwalker.UnkillableMinionArgs args)
        {
            if ((Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit) && Player.ManaPercent >= lasthit.Value("mana%")) || (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) && !E.IsReady() && Player.ManaPercent >= laneclear.Value("mana%")))
            {
                if (Q.IsReady() && target.IsValidTarget(Q.Range) && damageManager.SpellDamage(target, SpellSlot.Q) >= target.Health) Q.HitChanceCast(target);
                if (W.IsReady() && target.IsValidTarget(W.Range) && target.HasBuff("kennenmarkofstorm") && damageManager.SpellDamage(target, SpellSlot.W) >= target.Health) W.Cast();
            }

            return;
        }

        public override void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (Player.IsDead || !sender.IsEnemy || !Q.IsReady() || Player.Distance(e.End) > Q.Range || EBuff() || !sender.IsValidTarget(Q.Range) || !misc.IsActive("gapcloser")) return;

            Q.HitChanceCast(sender);

            return;
        }

        public override void Interrupter_OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            if (e.DangerLevel == DangerLevel.High && sender.IsValidTarget(Q.Range) && sender.GetBuffCount("kennenmarkofstorm") >= 1)
            {
                if (Q.IsReady()) Q.HitChanceCast(sender, 75);

                if (!W.IsOnCooldown && W.IsInRange(sender)) W.Cast();

                if (R.IsReady() && R.IsInRange(sender)) R.Cast();
            }

            return;
        }

        //------------------------------------|| Methods ||--------------------------------------
        
        //---------------------------------------EBuff()-------------------------------------------------------

        bool EBuff() { return Player.HasBuff("KennenLightningRush"); }
    }
}