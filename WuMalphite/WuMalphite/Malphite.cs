using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using SharpDX;
using WuAIO.Bases;
using WuAIO.Managers;
using Circle = EloBuddy.SDK.Rendering.Circle;
using ScalingTypes = WuAIO.Managers.DamageManager.ScalingTypes;

namespace WuAIO
{
    class Malphite : HeroBase
    {
        readonly AIHeroClient Player = EloBuddy.Player.Instance;

        Spell.Targeted Q = new Spell.Targeted(SpellSlot.Q, 625);
        Spell.Active W = new Spell.Active(SpellSlot.W);
        Spell.Active E = new Spell.Active(SpellSlot.E, 400);
        Spell.Skillshot R = new Spell.Skillshot(SpellSlot.R, 1000, SkillShotType.Circular, 250, 700, 270);

        public override void CreateMenu()
        {
            base.CreateMenu();

            var menu = MenuManager.AddSubMenu("Drawings");
            {
                menu.NewCheckbox("disable", "Disable", false);
                menu.NewCheckbox("damageindicator", "Damage Indicator");
                menu.NewCheckbox("r", "R");
                menu.NewCheckbox("q", "Q");
                menu.NewCheckbox("e", "E");
                menu.NewCheckbox("ultpos&hits", "Draw R position and hits");
            }

            menu = MenuManager.AddSubMenu("Combo");
            {
                menu.NewCheckbox("q", "Q");
                menu.NewCheckbox("w", "W");
                menu.NewCheckbox("e", "E");
                menu.NewCheckbox("r", "R", true);
                menu.NewSlider("r.minenemies", "Min enemies R", 2, 1, 5);
            }

            menu = MenuManager.AddSubMenu("Harass");
            {
                menu.NewCheckbox("q", "Q");
                menu.NewCheckbox("w", "W");
                menu.NewCheckbox("e", "E");
                menu.NewSlider("mana%", "Min mana%", 30, 1, 99, true);
            }

            menu = MenuManager.AddSubMenu("Last Hit");
            {
                menu.NewCheckbox("q", "Q");
                menu.NewSlider("mana%", "Min mana%", 30, 1, 99, true);
            }

            menu = MenuManager.AddSubMenu("Lane Clear");
            {
                menu.NewCheckbox("q", "Q");
                menu.NewCheckbox("w", "W");
                menu.NewCheckbox("e", "E");
                menu.NewSlider("e.minminions", "Min minions E", 3, 1, 7);
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
            }

            menu = MenuManager.AddSubMenu("Misc");
            {
                menu.NewCheckbox("ks", "KS");
                menu.NewCheckbox("interrupter", "R to interrupt spells");
                menu.NewCheckbox("gapcloser", "Q on enemy gapcloser", true);
                menu.NewKeybind("ult", "Auto R (ignore min enemies slider)", false, KeyBind.BindTypes.HoldActive, 'J', true);
            }
        }

        public override void CreateVariables()
        {
            new SkinManager(8);

            R.MinimumHitChance = HitChance.High;
            R.AllowedCollisionCount = int.MaxValue;

            damageManager = new DamageManager
            (
                new List<Bases.Damage>()
                {
                    new Bases.Damage
                    (
                        Q, DamageType.Magical, new float[] { 0, 70, 120, 170, 220, 270 },

                        new List<Scale>()
                        {
                            new Scale(new float[] { 0, 0.6f, 0.6f, 0.6f, 0.6f, 0.6f }, ScalingTypes.AP)
                        },

                        true, true
                    ),

                    new Bases.Damage
                    (
                        W, DamageType.Physical, new float[] { 0, 15, 30, 45, 60, 75 },

                        new List<Scale>()
                        {
                            new Scale(new float[] { 0, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f }, ScalingTypes.AP),
                            new Scale(new float[] { 0, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f }, ScalingTypes.Armor),
                        }
                    ),

                    new Bases.Damage
                    (
                        E, DamageType.Magical, new float[] { 0, 60, 100, 140, 180, 220 },

                        new List<Scale>()
                        {
                            new Scale(new float[] { 0, 0.2f, 0.2f, 0.2f, 0.2f, 0.2f }, ScalingTypes.AP),
                            new Scale(new float[] { 0, 0.3f, 0.3f, 0.3f, 0.3f, 0.3f }, ScalingTypes.Armor),
                        }
                    ),

                    new Bases.Damage
                    (
                        R, DamageType.Magical, new float[] { 0, 200, 300, 400 },

                        new List<Scale>()
                        {
                            new Scale(new float[] { 0, 1, 1, 1, 1, 1 }, ScalingTypes.AP)
                        }
                    )
                }
            );
        }

        public override void PermaActive()
        {
            base.PermaActive();

            //----Auto R

            if (misc.IsActive("ult") && R.IsReady())
            {
                var Target = TargetSelector.GetTarget(R.Range, DamageType.Magical);

                if (Target != null) R.Cast(Target);
            }
        }

        public override void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead || draw.IsActive("disable")) return;

            if (R.IsReady() && draw.IsActive("ultpos&hits"))
            {
                var Target = TargetSelector.GetTarget(R.Range + 300, DamageType.Magical);

                if (Target != null)
                {
                    var PosAndHits = GetBestRPos(Target.ServerPosition.To2D());

                    if (PosAndHits.First().Value >= combo.Value("r.minenemies"))
                    {
                        Drawing.DrawCircle(PosAndHits.First().Key.To3D(), 70, System.Drawing.Color.Yellow);
                        Drawing.DrawText(Drawing.WorldToScreen(Player.Position).X, Drawing.WorldToScreen(Player.Position).Y - 200, System.Drawing.Color.Yellow, string.Format("R WILL HIT {0} ENEMIES", PosAndHits.First().Value));
                    }
                }
            }

            if (draw.IsActive("r"))
                Circle.Draw(R.IsReady() ? Color.Blue : Color.Red, R.Range, Player.Position);

            if (draw.IsActive("q"))
                Circle.Draw(Q.IsReady() ? Color.Blue : Color.Red, Q.Range, Player.Position);

            if (draw.IsActive("e"))
                Circle.Draw(E.IsReady() ? Color.Blue : Color.Red, E.Range, Player.Position);

            return;
        }

        public override void Combo()
        {
            var Target = TargetSelector.GetTarget(R.Range, DamageType.Magical);

            if (Target == null || !Target.IsValidTarget()) return;

            if (R.IsInRange(Target) && R.IsReady())
            {
                var PosAndHits = GetBestRPos(Target.ServerPosition.To2D());

                if (combo.IsActive("r") && PosAndHits.First().Value >= combo.Value("r.minenemies"))
                {
                    R.Cast(PosAndHits.First().Key.To3D());
                }
            }

            if (Q.IsReady() && Q.IsInRange(Target) && combo.IsActive("q")) Q.Cast(Target);

            if (E.IsReady() && E.IsInRange(Target) && combo.IsActive("e")) E.Cast();

            if (W.IsReady() && Player.Distance(Target) <= Player.GetAutoAttackRange() - 50 && combo.IsActive("w")) W.Cast();

            return;

            return;
        }

        public override void Harass()
        {
            var Target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);

            if (Target == null || !Target.IsValidTarget() || Player.ManaPercent < harass.Value("mana%")) return;

            if (Q.IsReady() && Q.IsInRange(Target) && combo.IsActive("q")) Q.Cast(Target);

            if (E.IsReady() && E.IsInRange(Target) && combo.IsActive("e")) E.Cast();

            if (W.IsReady() && Player.Distance(Target) <= Player.GetAutoAttackRange() - 50 && combo.IsActive("w")) W.Cast();

            return;
        }

        public override void LaneClear()
        {
            if (Player.ManaPercent <= laneclear.Value("mana%")) return;

            var Minions = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Player.Position, Q.Range);

            if (Q.IsReady() && laneclear.IsActive("q"))
            {
                var minion = Minions.FirstOrDefault(it => !Player.IsInAutoAttackRange(it) && damageManager.SpellDamage(it, SpellSlot.Q) >= it.Health);
                if (minion != null) Q.Cast(minion);
            }

            if (E.IsReady() && laneclear.IsActive("e") && laneclear.Value("e.minminions") >= Minions.Count(it => it.IsValidTarget(E.Range)))
                E.Cast();

            if (W.IsReady() && laneclear.IsActive("w") && 4 >= Minions.Count(it => it.IsValidTarget(E.Range)))

            return;
        }

        public override void LastHit()
        {
            if (Player.ManaPercent <= lasthit.Value("mana%")) return;

            var Minions = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Player.Position, Q.Range);

            if (Q.IsReady() && lasthit.IsActive("q"))
            {
                var minion = Minions.FirstOrDefault(it => !Player.IsInAutoAttackRange(it) && damageManager.SpellDamage(it, SpellSlot.Q) >= it.Health);
                if (minion != null) Q.Cast(minion);
            }
        }

        public override void Flee()
        {
            if (!Q.IsReady() || !flee.IsActive("q")) return;

            var Target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);

            if (Target != null && Target.IsValidTarget(Q.Range)) Q.Cast(Target);
        }

        public override void KS()
        {
            if (!EntityManager.Heroes.Enemies.Any(it => !it.IsDead && it.IsValidTarget(Q.Range)) || !misc.IsActive("ks")) return;

            if (Q.IsReady())
            {
                var bye = EntityManager.Heroes.Enemies.FirstOrDefault(enemy => enemy.IsValidTarget(Q.Range) && damageManager.SpellDamage(enemy, SpellSlot.Q) >= enemy.Health);
                if (bye != null) { Q.Cast(bye); return; }
            }

            if (E.IsReady())
            {
                var bye = EntityManager.Heroes.Enemies.FirstOrDefault(enemy => enemy.IsValidTarget(E.Range) && damageManager.SpellDamage(enemy, SpellSlot.E) >= enemy.Health);
                if (bye != null) { R.Cast(); return; }
            }

            if (Q.IsReady() && E.IsReady())
            {
                var bye = EntityManager.Heroes.Enemies.FirstOrDefault(enemy => enemy.IsValidTarget(E.Range) && damageManager.SpellDamage(enemy, SpellSlot.E) + damageManager.SpellDamage(enemy, SpellSlot.Q) >= enemy.Health);
                if (bye != null) { E.Cast(); return; }
            }
        }

        public override void Orbwalker_OnPostAttack(AttackableUnit target, EventArgs args)
        {
            if (Player.IsDead || Orbwalker.LastTarget != target) return;

            if (Player.ManaPercent < jungleclear.Value("mana%")) return;

            var minion = EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Position, Q.Range).FirstOrDefault(it => it.Health > 2 * Player.GetAutoAttackDamage(it));

            if (minion == null) return;

            if (Q.IsReady() && jungleclear.IsActive("q"))
                Q.Cast(minion);

            if (E.IsReady() && jungleclear.IsActive("e"))
                E.Cast();

            if (W.IsReady() && jungleclear.IsActive("w") && Player.IsInAutoAttackRange(minion))
                W.Cast();
        }

        public override void Interrupter_OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            if (Player.IsDead || !sender.IsEnemy || !misc.IsActive("interrupter")) return;

            if (e.DangerLevel == DangerLevel.High && R.IsReady())
                R.Cast(sender);
        }

        public override void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (Player.IsDead || !sender.IsEnemy || !misc.IsActive("gapcloser")) return;

            if (!Q.IsReady() || !sender.IsValidTarget(Q.Range)) return;

            Q.Cast(sender);

            return;
        }

        //------------------------------------|| Methods ||--------------------------------------

        //----------------------------CountRHits(Vector2 CastPosition)---------------------------

        int CountRHits(Vector2 CastPosition)
        {
            int Hits = new int();

            foreach (Vector3 EnemyPos in GetEnemiesPosition())
            {
                if (CastPosition.Distance(EnemyPos) <= 260) Hits += 1;
            }

            return Hits;
        }

        //---------------------------GetBestRPos(Vector2 TargetPosition)-------------------------

        Dictionary<Vector2, int> GetBestRPos(Vector2 TargetPosition)
        {
            Dictionary<Vector2, int> PosAndHits = new Dictionary<Vector2, int>();

            List<Vector2> RPos = new List<Vector2>
            {
                new Vector2(TargetPosition.X - 250, TargetPosition.Y + 100),
                new Vector2(TargetPosition.X - 250, TargetPosition.Y),

                new Vector2(TargetPosition.X - 200, TargetPosition.Y + 300),
                new Vector2(TargetPosition.X - 200, TargetPosition.Y + 200),
                new Vector2(TargetPosition.X - 200, TargetPosition.Y + 100),
                new Vector2(TargetPosition.X - 200, TargetPosition.Y - 100),
                new Vector2(TargetPosition.X - 200, TargetPosition.Y),

                new Vector2(TargetPosition.X - 160, TargetPosition.Y - 160),

                new Vector2(TargetPosition.X - 100, TargetPosition.Y + 300),
                new Vector2(TargetPosition.X - 100, TargetPosition.Y + 200),
                new Vector2(TargetPosition.X - 100, TargetPosition.Y + 100),
                new Vector2(TargetPosition.X - 100, TargetPosition.Y + 250),
                new Vector2(TargetPosition.X - 100, TargetPosition.Y - 200),
                new Vector2(TargetPosition.X - 100, TargetPosition.Y - 100),
                new Vector2(TargetPosition.X - 100, TargetPosition.Y),

                new Vector2(TargetPosition.X, TargetPosition.Y + 300),
                new Vector2(TargetPosition.X, TargetPosition.Y + 270),
                new Vector2(TargetPosition.X, TargetPosition.Y + 200),
                new Vector2(TargetPosition.X, TargetPosition.Y + 100),

                new Vector2(TargetPosition.X, TargetPosition.Y),

                new Vector2(TargetPosition.X, TargetPosition.Y - 100),
                new Vector2(TargetPosition.X, TargetPosition.Y - 200),

                new Vector2(TargetPosition.X + 100, TargetPosition.Y),
                new Vector2(TargetPosition.X + 100, TargetPosition.Y - 100),
                new Vector2(TargetPosition.X + 100, TargetPosition.Y - 200),
                new Vector2(TargetPosition.X + 100, TargetPosition.Y + 100),
                new Vector2(TargetPosition.X + 100, TargetPosition.Y + 200),
                new Vector2(TargetPosition.X + 100, TargetPosition.Y + 250),
                new Vector2(TargetPosition.X + 100, TargetPosition.Y + 300),

                new Vector2(TargetPosition.X + 160, TargetPosition.Y - 160),

                new Vector2(TargetPosition.X + 200, TargetPosition.Y),
                new Vector2(TargetPosition.X + 200, TargetPosition.Y - 100),
                new Vector2(TargetPosition.X + 200, TargetPosition.Y + 100),
                new Vector2(TargetPosition.X + 200, TargetPosition.Y + 200),
                new Vector2(TargetPosition.X + 200, TargetPosition.Y + 300),

                new Vector2(TargetPosition.X + 250, TargetPosition.Y),
                new Vector2(TargetPosition.X + 250, TargetPosition.Y + 100),
            };

            foreach (Vector2 pos in RPos)
            {
                PosAndHits.Add(pos, CountRHits(pos));
            }

            Vector2 PosToGG = PosAndHits.First(pos => pos.Value == PosAndHits.Values.Max()).Key;
            int Hits = PosAndHits.First(pos => pos.Key == PosToGG).Value;

            return new Dictionary<Vector2, int>() { { PosToGG, Hits } };
        }

        //---------------------------------GetEnemiesPosition()----------------------------------

        List<Vector3> GetEnemiesPosition()
        {
            List<Vector3> Positions = new List<Vector3>();

            foreach (AIHeroClient Hero in EntityManager.Heroes.Enemies.Where(hero => !hero.IsDead && Player.Distance(hero) <= R.Range + E.Range))
            {
                Positions.Add(Prediction.Position.PredictUnitPosition(Hero, 500).To3D());
            }

            return Positions;
        }
    }
}