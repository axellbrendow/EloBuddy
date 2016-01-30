using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu.Values;
using SharpDX;
using WuAIO.Bases;
using WuAIO.Managers;
using Circle = EloBuddy.SDK.Rendering.Circle;
using ScalingTypes = WuAIO.Managers.DamageManager.ScalingTypes;

namespace WuAIO
{
    class Jax : HeroBase
    {
        readonly AIHeroClient Player = EloBuddy.Player.Instance;

        float ETime;
        float WardTick;

        readonly Spell.Targeted Q = new Spell.Targeted(SpellSlot.Q, 700);
        readonly Spell.Active W = new Spell.Active(SpellSlot.W);
        readonly Spell.Active E = new Spell.Active(SpellSlot.E, 187);
        readonly Spell.Active R = new Spell.Active(SpellSlot.R);

        public override void CreateMenu()
        {
            base.CreateMenu();

            var menu = MenuManager.AddSubMenu("Drawings");
            {
                menu.NewCheckbox("disable", "Disable");
                menu.NewCheckbox("damageindicator", "Damage Indicator");
                menu.NewCheckbox("q", "Q");
            }

            menu = MenuManager.AddSubMenu("Combo");
            {
                menu.NewCheckbox("q", "Q");
                menu.NewCheckbox("q.aarange?qondash", "Enemy AARange ? Just Q on dash!");
                menu.NewSlider("q.delay", "Use E and after some milliseconds use Q (1000ms = 1sec):", 1500, 0, 1900);
                menu.NewCheckbox("w", "W", true, true);
                menu.NewCheckbox("w.aareset", "Use W AA Reset");
                menu.NewCheckbox("e", "E", true, true);
                menu.NewCheckbox("r", "R", true, true);
                menu.NewCheckbox("r.1v1logic", "Use 1v1 R Logic");
                menu.NewSlider("r.minenemies", "Min enemies R", 2, 1, 5, true);
            }

            menu = MenuManager.AddSubMenu("Harass");
            {
                menu.NewCheckbox("q", "Q");
                menu.NewCheckbox("q.aarange?justqondash", "Enemy AARange ? Just Q on dash!");
                menu.NewSlider("q.delay", "Use E and after some milliseconds use Q (1000ms = 1sec):", 1500, 0, 1900, true);
                menu.NewCheckbox("w", "W", true, true);
                menu.NewCheckbox("w.aareset", "Use W AA Reset");
                menu.NewCheckbox("e", "E", true, true);
                menu.NewSlider("mana%", "Min mana%", 30, 1, 99, true);
            }

            menu = MenuManager.AddSubMenu("Lane Clear");
            {
                menu.NewCheckbox("q", "Q");
                menu.NewCheckbox("q.jimwd", "Just Q if minion will die");
                menu.NewCheckbox("w", "W", true, true);
                menu.NewCheckbox("e", "E", true, true);
                menu.NewSlider("e.minminions", "Min minions E", 3, 1, 7);
                menu.NewSlider("mana%", "Min mana%", 30, 1, 99, true);
            }

            menu = MenuManager.AddSubMenu("Last Hit");
            {
                menu.NewCheckbox("q", "Q");
                menu.NewCheckbox("w", "W");
                menu.NewSlider("mana%", "Min mana%", 30, 1, 99, true);
            }

            menu = MenuManager.AddSubMenu("Jungle Clear");
            {
                menu.NewCheckbox("q", "Q");
                menu.NewCheckbox("w", "W");
                menu.NewCheckbox("e", "E");
                menu.NewSlider("mana%", "Min mana%", 30, 1, 99, true);
            }

            menu = MenuManager.AddSubMenu("Misc");
            {
                menu.NewCheckbox("ks", "KS");
                menu.NewKeybind("wardjump", "Ward Jump", false, KeyBind.BindTypes.HoldActive, 'T');
            }
        }

        public override void CreateVariables()
        {
            new SkinManager(11);

            damageManager = new DamageManager
            (
                new List<Bases.Damage>()
                {
                    new Bases.Damage
                    (
                        Q, DamageType.Physical, new float[] { 0, 70, 110, 150, 190, 230 },

                        new List<Scale>()
                        {
                            new Scale(new float[] { 0, 0.6f, 0.6f, 0.6f, 0.6f, 0.6f }, ScalingTypes.AD),
                            new Scale(new float[] { 0, 1, 1, 1, 1, 1 }, ScalingTypes.AP)
                        },
                        true, true
                    ),

                    new Bases.Damage
                    (
                        W, DamageType.Magical, new float[] { 0, 40, 75, 110, 145, 180 },

                        new List<Scale>()
                        {
                            new Scale(new float[] { 0, 0.6f, 0.6f, 0.6f, 0.6f, 0.6f }, ScalingTypes.AP)
                        },
                        true, true
                    ),

                    new Bases.Damage
                    (
                        E, DamageType.Physical, new float[] { 0, 50, 75, 100, 125, 150 },

                        new List<Scale>()
                        {
                            new Scale(new float[] { 0, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f }, ScalingTypes.APBonus)
                        }
                    )
                }
            );
        }

        public override void PermaActive()
        {
            base.PermaActive();

            //----------------------------------------------Ward Jump---------------------------------------

            if (Q.IsReady() && misc.IsActive("wardjump") && Environment.TickCount - WardTick >= 2000)
            {
                var CursorPos = Game.CursorPos;

                Obj_AI_Base JumpPlace = EntityManager.Heroes.Allies.FirstOrDefault(it => it.Distance(CursorPos) <= 250 && Q.IsInRange(it));

                if (JumpPlace != default(Obj_AI_Base)) Q.Cast(JumpPlace);
                else
                {
                    JumpPlace = EntityManager.MinionsAndMonsters.Minions.FirstOrDefault(it => it.Distance(CursorPos) <= 250 && Q.IsInRange(it));

                    if (JumpPlace != default(Obj_AI_Base)) Q.Cast(JumpPlace);
                    else if (JumpWard() != default(InventorySlot))
                    {
                        var Ward = JumpWard();
                        CursorPos = Player.Position.Extend(CursorPos, 600).To3D();
                        Ward.Cast(CursorPos);
                        WardTick = Environment.TickCount;
                        Core.DelayAction(() => WardJump(CursorPos), Game.Ping + 100);
                    }
                }

            }
        }

        public override void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead || draw.IsActive("disable")) return;

            if (draw.IsActive("q"))
                Circle.Draw(Q.IsReady() ? Color.Blue : Color.Red, Q.Range, Player.Position);

            return;
        }

        public override void KS()
        {
            if (!misc.IsActive("ks") || !EntityManager.Heroes.Enemies.Any(it => Q.IsInRange(it))) return;

            if (Q.IsReady())
            {
                var bye = EntityManager.Heroes.Enemies.FirstOrDefault(enemy => enemy.IsValidTarget(Q.Range) && damageManager.SpellDamage(enemy, SpellSlot.Q) >= enemy.Health);
                if (bye != null) { Q.Cast(bye); return; }
            }

            if (W.IsReady())
            {
                var bye = EntityManager.Heroes.Enemies.FirstOrDefault(enemy => enemy.IsValidTarget(W.Range) && damageManager.SpellDamage(enemy, SpellSlot.W) + Player.GetAutoAttackDamage(enemy) >= enemy.Health);
                if (bye != null) { W.Cast(); EloBuddy.Player.IssueOrder(GameObjectOrder.AttackTo, bye); return; }
            }

            if (Q.IsReady() && W.IsReady())
            {
                var bye = EntityManager.Heroes.Enemies.FirstOrDefault(enemy => enemy.IsValidTarget(W.Range) && damageManager.SpellDamage(enemy, SpellSlot.Q) + damageManager.SpellDamage(enemy, SpellSlot.W) >= enemy.Health);
                if (bye != null) { W.Cast(); Core.DelayAction(() => Q.Cast(bye), 100); return; }
            }
        }

        public override void Combo()
        {
            var Target = TargetSelector.GetTarget(900, DamageType.Physical);

            if (Target == null || !Target.IsValidTarget()) return;

            if (Q.IsInRange(Target))
            {
                if (!Player.HasBuff("JaxCounterStrike") && E.IsReady() && combo.IsActive("e") && E.Cast()) ETime = Environment.TickCount;

                if (Q.IsReady() && combo.IsActive("q"))
                {
                    if (Player.Distance(Target) <= Player.GetAutoAttackRange(Target) + 100 && Target.IsDashing() && combo.IsActive("q.aarange?qondash")) Q.Cast(Target);
                    else if (Environment.TickCount - ETime >= combo.Value("q.delay")) Q.Cast(Target);
                }

                if (W.IsReady() && !combo.IsActive("w.aareset") && (Q.IsReady() || Player.IsInAutoAttackRange(Target)) && W.Cast()) Orbwalker.ResetAutoAttack();

                if (R.IsReady() && combo.IsActive("r"))
                {
                    if (Player.CountEnemiesInRange(650) >= combo.Value("r.minenemies")) R.Cast();
                    else if (combo.IsActive("r.1v1logic") && (Player.HealthPercent <= 42 || Target.HealthPercent > 40)) R.Cast();
                }
            }

            return;
        }

        public override void Harass()
        {
            var Target = TargetSelector.GetTarget(900, DamageType.Physical);

            if (Target == null || !Target.IsValidTarget() || Player.ManaPercent < harass.Value("mana%")) return;

            if (Q.IsInRange(Target))
            {
                if (!Player.HasBuff("JaxCounterStrike") && E.IsReady() && harass.IsActive("e") && E.Cast()) ETime = Environment.TickCount;

                if (Q.IsReady() && harass.IsActive("q") && Environment.TickCount - ETime >= harass.Value("q.delay")) Q.Cast(Target);

                if (W.IsReady() && !harass.IsActive("w.aareset") && (Q.IsReady() || Player.IsInAutoAttackRange(Target)) && W.Cast()) Orbwalker.ResetAutoAttack();
            }

            return;
        }

        public override void LaneClear()
        {
            var minions = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Player.Position, Q.Range);
            if (!minions.Any()) return;

            if (Player.ManaPercent < laneclear.Value("mana%")) return;

            if (Q.IsReady() && laneclear.IsActive("q"))
            {
                if (laneclear.IsActive("q.jimwd"))
                {
                    var QMinions = minions.Where(it => !Player.IsInAutoAttackRange(it) && damageManager.SpellDamage(it, SpellSlot.Q) >= it.Health).OrderBy(it => it.Health);
                    if (QMinions.Any()) Q.Cast(QMinions.First());
                }
                else Q.Cast(minions.First());
            }

            if (!Player.HasBuff("JaxCounterStrike") && E.IsReady() && laneclear.IsActive("e") && minions.Count(it => it.IsValidTarget(E.Range)) >= laneclear.Value("e.minminions")) E.Cast();

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
            var minions = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Player.Position, Q.Range);
            if (!minions.Any()) return;

            if (Player.ManaPercent <= lasthit.Value("mana%")) return;

            if (Q.IsReady() && lasthit.IsActive("q"))
            {
                var QMinion = minions.FirstOrDefault(it => !Player.IsInAutoAttackRange(it) && damageManager.SpellDamage(it, SpellSlot.Q) > it.Health);
                if (QMinion != null) Q.Cast(QMinion);
            }

            if (W.IsReady() && lasthit.IsActive("w"))
            {
                var WMinion = minions.FirstOrDefault(it => Player.IsInAutoAttackRange(it) && it.Health > Player.GetAutoAttackDamage(it));
                if (WMinion != null)
                {
                    W.Cast();
                    EloBuddy.Player.IssueOrder(GameObjectOrder.AttackTo, WMinion);
                }
            }

            return;
        }

        public override void Orbwalker_OnPostAttack(AttackableUnit target, EventArgs args)
        {
            if (W.IsReady())
            {
                if (Player.Distance(target) <= Player.GetAutoAttackRange())
                {
                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && combo.IsActive("w") && combo.IsActive("w.aareset"))
                    {
                        if (W.Cast())
                        {
                            Orbwalker.ResetAutoAttack();
                            EloBuddy.Player.IssueOrder(GameObjectOrder.AttackTo, target);
                        }

                        return;
                    }

                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass) && harass.IsActive("w") && harass.IsActive("w.aareset") && Player.ManaPercent >= harass.Value("mana%"))
                    {
                        if (W.Cast())
                        {
                            Orbwalker.ResetAutoAttack();
                            EloBuddy.Player.IssueOrder(GameObjectOrder.AttackTo, target);
                        }

                        return;
                    }

                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) && laneclear.IsActive("w") && Player.ManaPercent >= laneclear.Value("mana%"))
                    {
                        if (target.Health > Player.GetAutoAttackDamage((Obj_AI_Base)target) && W.Cast())
                        {
                            Orbwalker.ResetAutoAttack();
                            EloBuddy.Player.IssueOrder(GameObjectOrder.AttackTo, target);
                        }

                        return;
                    }

                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear) && jungleclear.IsActive("w") && Player.ManaPercent >= jungleclear.Value("mana%"))
                    {
                        if (target.Health > Player.GetAutoAttackDamage((Obj_AI_Base)target) && W.Cast())
                        {
                            Orbwalker.ResetAutoAttack();
                            EloBuddy.Player.IssueOrder(GameObjectOrder.AttackTo, target);
                        }

                        return;
                    }
                }
            }

            return;
        }

        //------------------------------------|| Methods ||--------------------------------------

        //---------------------------------------------WardJump()-------------------------------------------------

        void WardJump(Vector3 cursorpos)
        {
            var Ward = ObjectManager.Get<Obj_AI_Base>().FirstOrDefault(it => it.IsValidTarget(Q.Range) && it.Distance(cursorpos) <= 250);
            if (Ward != null) Q.Cast(Ward);
        }

        //---------------------------------------------JumpWard()--------------------------------------------------

        InventorySlot JumpWard()
        {
            var Inventory = Player.InventoryItems;

            if (Item.CanUseItem(3340)) return Inventory.First(it => it.Id == ItemId.Warding_Totem_Trinket);
            if (Item.CanUseItem(2049)) return Inventory.First(it => it.Id == ItemId.Sightstone);
            if (Item.CanUseItem(2045)) return Inventory.First(it => it.Id == ItemId.Ruby_Sightstone);
            if (Item.CanUseItem(3711)) return Inventory.First(it => (int)it.Id == 3711); //Tracker's Knife
            if (Item.CanUseItem(2301)) return Inventory.First(it => (int)it.Id == 2301); //Eye of the Watchers
            if (Item.CanUseItem(2302)) return Inventory.First(it => (int)it.Id == 2302); //Eye of the Oasis
            if (Item.CanUseItem(2303)) return Inventory.First(it => (int)it.Id == 2303); //Eye of the Equinox
            if (Item.CanUseItem(2043)) return Inventory.First(it => it.Id == ItemId.Vision_Ward);

            return default(InventorySlot);
        }
    }
}