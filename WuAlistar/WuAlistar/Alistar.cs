using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu.Values;
using WuAIO.Extensions;
using WuAIO.Bases;
using WuAIO.Managers;
using SharpDX;
using Circle = EloBuddy.SDK.Rendering.Circle;
using Color = System.Drawing.Color;
using ScalingTypes = WuAIO.Managers.DamageManager.ScalingTypes;

namespace WuAIO
{
    class Alistar : HeroBase
    {
        int qwmana { get { return new[] { 0, 65, 70, 75, 80, 85 }[W.Level] + new[] { 0, 65, 70, 75, 80, 85 }[W.Level]; } }
        bool Insecing;
        bool Combing;
        Spell.Skillshot Flash;
        Vector3 WalkPos;
        List<string> DodgeSpells = new List<string>() { "LuxMaliceCannon", "LuxMaliceCannonMis", "EzrealtrueShotBarrage", "KatarinaR", "YasuoDashWrapper", "ViR", "NamiR", "ThreshQ", "AbsoluteZero", "xerathrmissilewrapper", "yasuoq3w", "UFSlash" };

        readonly Spell.Active Q = new Spell.Active(SpellSlot.Q, 365);
        readonly Spell.Targeted W = new Spell.Targeted(SpellSlot.W, 650);
        readonly Spell.Active E = new Spell.Active(SpellSlot.E, 575);
        readonly Spell.Active R = new Spell.Active(SpellSlot.R);

        public override void CreateVariables()
        {
            new SkinManager(9);

            damageManager = new DamageManager
            (
                new List<Bases.Damage>()
                {
                    new Bases.Damage
                    (
                        Q, DamageType.Magical, new float[]{ 0, 60 , 105 , 150 , 195 , 240 },

                        new List<Scale>()
                        {
                            new Scale(new float[] { 0, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f }, ScalingTypes.AP)
                        }
                    ),

                    new Bases.Damage
                    (
                        W, DamageType.Magical, new float[]{ 0, 55 , 110 , 165 , 220 , 275 },

                        new List<Scale>()
                        {
                            new Scale(new float[] { 0, 0.7f, 0.7f, 0.7f, 0.7f, 0.7f }, ScalingTypes.AP)
                        }
                    )
                }
            );
        }

        public override void CreateMenu()
        {
            base.CreateMenu();

            var menu = MenuManager.AddSubMenu("Drawings");
            {
                menu.NewCheckbox("disable", "Disable", false);
                menu.NewCheckbox("damageindicator", "Damage Indicator");
                menu.NewCheckbox("q", "Q");
                menu.NewCheckbox("w", "W");
                menu.NewCheckbox("e", "E");
            };

            menu = MenuManager.AddSubMenu("Combo");
            {
                menu.NewCheckbox("r", "R");
                menu.NewSlider("r.health%", "Health% to ult", 30, 1, 99);
                menu.NewSlider("r.minenemies", "Min Enemies to ult", 2, 1, 5);
            };

            menu = MenuManager.AddSubMenu("Misc");
            {
                menu.NewKeybind("insec", "Insec", false, KeyBind.BindTypes.HoldActive, 'J');
                menu.NewSlider("W/Q Delay", "W/Q Delay", 0, -200, 200, true);
                menu.NewCheckbox("heal", "Use E", true, true);
                menu.NewCheckbox("heal.myself", "Heal myself");
                menu.NewSlider("heal.health%", "[E]Heal when ally health% is at", 50, 1, 99, true);
                menu.NewSlider("heal.mana%", "[E]Heal min mana%", 50, 1, 99, true);
                menu.NewCheckbox("gapcloser", "W/Q on enemy gapcloser", true, true);
                menu.NewCheckbox("interrupter", "Interrupt enemy spells");
                menu.NewKeybind("hu3", "hu3HU3hu3", false, KeyBind.BindTypes.HoldActive, 'U', true);
                menu.NewSlider("hu3.mode", "hu3HU3hu3 mode, 1:joke, 2:taunt, 3:dance, 4:laugh", 3, 1, 4);
            };
        }

        public override void PermaActive()
        {
            base.PermaActive();

            //hu3HU3hu3

            if (misc.IsActive("hu3"))
            {
                switch (misc.Value("hu3.mode"))
                {
                    case 1:
                        Player.DoEmote(Emote.Joke);
                        Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                        break;

                    case 2:
                        Player.DoEmote(Emote.Taunt);
                        Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                        break;

                    case 3:
                        Player.DoEmote(Emote.Dance);
                        Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                        break;

                    case 4:
                        Player.DoEmote(Emote.Laugh);
                        Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                        break;

                    default:
                        break;
                }
            }

            //Insec

            if (misc.IsActive("insec"))
            {
                var Target = TargetSelector.GetTarget(1000, DamageType.Magical);

                var flashslot = Player.Instance.GetSpellSlotFromName("summonerflash");

                if (flashslot != SpellSlot.Unknown)
                {
                    Flash = new Spell.Skillshot(flashslot, 425, SkillShotType.Linear);
                }

                if (!Insecing && !Target.HasBuffOfType(BuffType.SpellImmunity) && !Target.HasBuffOfType(BuffType.Invulnerability))
                {
                    Player.IssueOrder(GameObjectOrder.MoveTo, Target);

                    if (W.IsReady())
                    {
                        if (W.IsReady() && (Target.IsValidTarget(W.Range - 130) || (Target.IsValidTarget(W.Range - 50) && !Target.CanMove())) && Player.Instance.Mana >= qwmana)
                        {
                            Insecing = true;
                            QWInsec(Target);
                        }
                        else if (Flash != null)
                        {
                            var WalkPos = Game.CursorPos.Extend(Target, Game.CursorPos.Distance(Target) + 100);

                            if ((Player.Instance.Distance(WalkPos) <= Flash.Range - 80 || (Target.IsValidTarget(Flash.Range - 50) && !Target.CanMove())) && Flash.IsReady() && Player.Instance.Mana >= new[] { 0, 65, 70, 75, 80, 85 }[W.Level])
                            {
                                Insecing = true;

                                if (Flash.Cast(WalkPos.To3D())) W.Cast(Target);

                                Insecing = false;
                            }

                            else if ((Target.IsValidTarget(Flash.Range + W.Range - 130) || (Target.IsValidTarget(Flash.Range + W.Range - 50) && !Target.CanMove())) && Flash.IsReady() && W.IsReady() && Player.Instance.Mana >= qwmana)
                            {
                                Insecing = true;
                                QWInsec(Target, true);
                            }
                        }
                    }
                }
            }

            //Heal

            if (E.IsReady() && !Player.HasBuff("recall") && misc.IsActive("heal") && Player.Instance.ManaPercent >= misc.Value("heal.mana%") && EntityManager.Heroes.Allies.Any(it => it.HealthPercent <= misc.Value("heal.health%") && E.IsInRange(it)))
            {
                if (!misc.IsActive("heal.myself") && Player.Instance.HealthPercent <= misc.Value("heal.health%")) { }
                else E.Cast();
            }

            return;
        }

        public override void Drawing_OnDraw(EventArgs args)
        {
            if (Player.Instance.IsDead || draw.IsActive("disable")) return;

            var Target = TargetSelector.GetTarget(750, DamageType.Magical);

            if (Target != null && W.IsReady())
            {
                if (W.IsReady() && (Target.IsValidTarget(600)) && Player.Instance.Mana >= qwmana)
                {
                    Drawing.DrawText(Target.Position.WorldToScreen().X - 30, Target.Position.WorldToScreen().Y - 180, Color.Yellow, "W/Q is possible !!");
                }

                if (W.IsReady() && (Target.IsValidTarget(W.Range - 130) || (Target.IsValidTarget(W.Range - 50) && !Target.CanMove()) && Player.Instance.Mana >= qwmana))
                {
                    Drawing.DrawText(Target.Position.WorldToScreen().X - 30, Target.Position.WorldToScreen().Y - 150, Color.Yellow, "Q/W Insec !!");
                    Drawing.DrawLine(Target.Position.WorldToScreen(), Game.CursorPos2D, 3, Color.Yellow);
                    Drawing.DrawCircle(WalkPos, 70, Color.BlueViolet);
                }
                else if (Flash != null)
                {
                    if (Flash.IsReady() && Player.Instance.Distance(WalkPos) <= Flash.Range - 100 && Player.Instance.Mana >= new[] { 0, 65, 70, 75, 80, 85 }[W.Level - 1])
                    {
                        Drawing.DrawText(Target.Position.WorldToScreen().X - 30, Target.Position.WorldToScreen().Y - 150, Color.Yellow, "Flash/W Insec !!");
                        Drawing.DrawLine(Target.Position.WorldToScreen(), Game.CursorPos2D, 3, Color.Yellow);
                        Drawing.DrawCircle(WalkPos, 70, Color.BlueViolet);
                    }

                    else if (Flash.IsReady() && W.IsReady() && Target.IsValidTarget(Flash.Range + W.Range - 40) && Player.Instance.Mana >= qwmana)
                    {
                        Drawing.DrawText(Target.Position.WorldToScreen().X - 30, Target.Position.WorldToScreen().Y - 150, Color.Yellow, "Flash/Q/W Insec !!");
                        Drawing.DrawLine(Target.Position.WorldToScreen(), Game.CursorPos2D, 3, Color.Yellow);
                        Drawing.DrawCircle(Player.Instance.Position.Extend(Target, Flash.Range).To3D(), 70, Color.Yellow);
                        Drawing.DrawCircle(WalkPos, 70, Color.BlueViolet);
                    }
                }
            }

            if (draw.IsActive("q"))
                Circle.Draw(W.IsReady() ? SharpDX.Color.Blue : SharpDX.Color.Red, W.Range, Player.Instance.Position);

            if (draw.IsActive("w"))
                Circle.Draw(W.IsReady() ? SharpDX.Color.Blue : SharpDX.Color.Red, W.Range, Player.Instance.Position);

            if (draw.IsActive("e"))
                Circle.Draw(E.IsReady() ? SharpDX.Color.Blue : SharpDX.Color.Red, E.Range, Player.Instance.Position);

        }

        public override void Combo()
        {
            if (!Combing)
            {
                var Target = TargetSelector.GetTarget(700, DamageType.Magical);

                if (Target == null || !Target.IsValidTarget()) return;

                if (W.IsReady() && Target.IsValidTarget(W.Range - 80) && !Player.Instance.IsDashing()) W.Cast();

                else if (W.IsReady() && W.IsReady() && Target.IsValidTarget(625) && Player.Instance.Mana >= qwmana) { WQ(Target); Combing = true; }

                if (R.IsReady() && combo.IsActive("r") && Player.Instance.CountEnemiesInRange(600) >= combo.Value("r.minenemies") && Player.Instance.HealthPercent <= combo.Value("r.health%")) R.Cast();
            }

            return;
        }

        public override void Harass()
        {
            if (!Combing)
            {
                var Target = TargetSelector.GetTarget(700, DamageType.Magical);

                if (Target == null || !Target.IsValidTarget()) return;

                if (W.IsReady() && Target.IsValidTarget(W.Range - 80) && !Player.Instance.IsDashing()) W.Cast();

                else if (W.IsReady() && W.IsReady() && Target.IsValidTarget(625) && Player.Instance.Mana >= qwmana) { WQ(Target); Combing = true; }
            }

            return;
        }

        public override void Interrupter_OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs args)
        {
            if (Player.Instance.IsDead || !sender.IsEnemy || !misc.IsActive("interrupter")) return;

            if (args.DangerLevel == DangerLevel.High)
            {
                if (W.IsReady() && sender.IsValidTarget(300)) W.Cast(sender);
                else if (W.IsReady() && sender.IsValidTarget(W.Range)) W.Cast();
            }

            return;
        }

        public override void AIHeroClient_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (Player.Instance.IsDead || !sender.IsEnemy || !(sender is AIHeroClient) || !misc.IsActive("interrupter")) return;

            if (DodgeSpells.Any(it => it == args.SData.Name))
            {
                if (args.SData.Name == "KatarinaR")
                {
                    if (W.IsReady() && W.IsInRange(sender)) W.Cast();
                    else if (W.IsReady() && W.IsInRange(sender)) W.Cast(sender);
                    return;
                }

                if (args.SData.Name == "AbsoluteZero")
                {
                    if (W.IsReady() && W.IsInRange(sender)) W.Cast();
                    else if (W.IsReady() && W.IsInRange(sender)) W.Cast(sender);
                    return;
                }

                if (args.SData.Name == "EzrealtrueShotBarrage")
                {
                    if (W.IsReady() && W.IsInRange(sender)) W.Cast();
                    else if (W.IsReady() && W.IsInRange(sender)) W.Cast(sender);
                    return;
                }

                if (W.IsReady() && W.IsInRange(sender)) { W.Cast(); return; }
                if (W.IsReady() && sender.Distance(Player.Instance) <= 300) { W.Cast(sender); return; }
            }

            return;
        }
        
        public override void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (Player.Instance.IsDead || !sender.IsEnemy || !misc.IsActive("gapcloser")) return;

            if (sender.IsValidTarget(W.Range)) W.Cast();
            else if (sender.IsValidTarget(W.Range)) W.Cast(sender);

            return;
        }

        //------------------------------------|| Extension ||--------------------------------------

        private void WQ(Obj_AI_Base target)
        {
            if (target != null && target.IsValidTarget())
            {
                Combing = true;
                int ADelay = misc.Value("W/Q Delay");
                int delay = (int)((150 * (Player.Instance.Distance(target))) / 650 + ADelay);

                if (Player.CastSpell(SpellSlot.W, target))
                {
                    Core.DelayAction(() => W.Cast(), delay);
                    Core.DelayAction(() => Combing = false, delay + 1000);
                }
                else Combing = false;
            }

            return;
        }

        private void CheckWDistance(Obj_AI_Base target)
        {
            if (Player.Instance.Distance(WalkPos) <= 70) W.Cast(target);
            else Insecing = false;

            return;
        }

        private void QWInsec(Obj_AI_Base target, bool flash = false)
        {
            if (flash)
            {
                var FlashPos = Player.Instance.Position.Extend(target, Flash.Range).To3D();

                var Flashed = Flash.Cast(FlashPos);

                if (Flashed)
                {
                    Core.DelayAction(delegate
                    {
                        if (W.Cast())
                        {
                            WalkPos = Game.CursorPos.Extend(target, Game.CursorPos.Distance(target) + 150).To3D();

                            int delay = (int)(Player.Instance.Distance(WalkPos) / Player.Instance.MoveSpeed * 1000) + 300 + W.CastDelay + 2 * Game.Ping;

                            Player.IssueOrder(GameObjectOrder.MoveTo, WalkPos);

                            Core.DelayAction(() => CheckWDistance(target), delay);
                            Core.DelayAction(() => Insecing = false, delay + 1000);
                        }
                        else Insecing = false;
                    }, Game.Ping + 70);
                }
                else Insecing = false;

                return;
            }

            else
            {
                if (W.Cast())
                {
                    WalkPos = Game.CursorPos.Extend(target, Game.CursorPos.Distance(target) + 150).To3D();

                    int delay = (int)(Player.Instance.Distance(WalkPos) / Player.Instance.MoveSpeed * 1000) + 300 + W.CastDelay + 2 * Game.Ping;

                    Player.IssueOrder(GameObjectOrder.MoveTo, WalkPos);
                    Core.DelayAction(() => CheckWDistance(target), delay);
                    Core.DelayAction(() => Insecing = false, delay + 1000);
                }
                else Insecing = false;

                return;
            }
        }
    }
}