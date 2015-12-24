using System;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Generic;
using SharpDX;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Rendering;
using Color = System.Drawing.Color;
using Version = System.Version;
using System.Net;
using System.Text.RegularExpressions;

namespace WuAlistar
{
    static class Program
    {
        static Version AssVersion;//Kappa
        static readonly String CN = "Alistar";
        static AIHeroClient Player { get { return ObjectManager.Player; } }
        static Spell.Skillshot Flash;
        static Spell.Active Heal;
        static Spell.Targeted Exhaust;
        static ColorBGRA Green = new ColorBGRA(Color.Green.R, Color.Green.G, Color.Green.B, Color.Green.A);
        static ColorBGRA Red = new ColorBGRA(Color.Red.R, Color.Red.G, Color.Red.B, Color.Red.A);

        static Item Bilgewater, Randuin, QSS, Glory, FOTMountain, Mikael, Talisma;
        static Menu Menu;
        static Vector2 WalkPos;
        static bool Insecing = new bool();
        static bool Combing = new bool(); //Kappa
        static AIHeroClient Target = null;
        static List<string> DodgeSpells = new List<string>() { "LuxMaliceCannon", "LuxMaliceCannonMis", "EzrealtrueShotBarrage", "KatarinaR", "YasuoDashWrapper", "ViR", "NamiR", "ThreshQ", "xerathrmissilewrapper", "yasuoq3w", "UFSlash" };
        static readonly Spell.Active Q = new Spell.Active(SpellSlot.Q, 365);
        static readonly Spell.Targeted W = new Spell.Targeted(SpellSlot.W, 650);
        static readonly Spell.Active E = new Spell.Active(SpellSlot.E, 575);

        static void Main(string[] args) { Loading.OnLoadingComplete += OnLoadingComplete; }

        //---------------------------------------------Game_OnGameLoad----------------------------------------

        static void OnLoadingComplete(EventArgs args)
        {
            if (Player.BaseSkinName != CN) { Chat.Print("Sorry, you didn't pick " + CN + ", addon disabled"); return; }

            AssVersion = Assembly.GetExecutingAssembly().GetName().Version;
            SearchVersion();

            //-------------------------------------------------Items--------------------------------------------------

            Bilgewater = new Item(3144, 550);
            Randuin = new Item(3143, 500);
            Glory = new Item(3800);
            QSS = new Item(3140);
            FOTMountain = new Item(3401);
            Mikael = new Item(3222, 750);
            Talisma = new Item(ItemId.Talisman_of_Ascension);

            //-------------------------------------------------Flash--------------------------------------------------

            SpellDataInst flash = Player.Spellbook.Spells.Where(spell => spell.Name.Contains("flash")).Any() ? Player.Spellbook.Spells.Where(spell => spell.Name.Contains("flash")).First() : null;
            if (flash != null)
            {
                Flash = new Spell.Skillshot(flash.Slot, 425, SkillShotType.Linear);
            }

            //-------------------------------------------------Heal--------------------------------------------------

            SpellDataInst heal = Player.Spellbook.Spells.Where(spell => spell.Name.Contains("heal")).Any() ? Player.Spellbook.Spells.Where(spell => spell.Name.Contains("heal")).First() : null;
            if (heal != null)
            {
                Heal = new Spell.Active(heal.Slot, 850);
            }

            //-------------------------------------------------Exhaust--------------------------------------------------

            SpellDataInst exhaust = Player.Spellbook.Spells.Where(spell => spell.Name.Contains("exhaust")).Any() ? Player.Spellbook.Spells.Where(spell => spell.Name.Contains("exhaust")).First() : null;
            if (exhaust != null)
            {
                Exhaust = new Spell.Targeted(exhaust.Slot, 650);
            }

            //-----------------------------||   Menu   ||------------------------------

            Menu = MainMenu.AddMenu("Wu" + CN, "Wu" + CN);
            
            string slot = "";//H3U3UH3UH3U3HU3HUH3UH3U3U
            string champ = "";//H3UH3UH3U3HU3H3U3H3UH3UH3U

            foreach (string spell in DodgeSpells)
            {
                if (EntityManager.Heroes.Enemies.Where(enemy => enemy.Spellbook.Spells.Where(it => it.SData.Name == spell && (slot = it.Slot.ToString()) == it.Slot.ToString() && (champ = enemy.BaseSkinName) == enemy.BaseSkinName).Any()).Any())
                {
                    Menu.Add(spell, new CheckBox("Interrupt " + champ + slot + " ?"));
                }
            }

            Menu.AddSeparator();

            Menu.Add("LifeToE", new Slider("[E] Heal ally when health percent is lower or equals to:", 50, 1, 100));
            Menu.Add("ManaToE", new Slider("Just [E] when mana % is greater or equals to:", 30, 1, 100));
            Menu.Add("EYourself", new CheckBox("Heal yourself"));

            Menu.AddSeparator();

            Menu.Add("W/Q Delay", new Slider("W/Q Delay", 0, -400, 400));

            Menu.AddSeparator();

            Menu.Add("DrawW", new CheckBox("Draw W"));

            Menu.AddSeparator();

            Menu.Add("Insec", new KeyBind("Insec", false, KeyBind.BindTypes.HoldActive, 'J'));

            Menu.AddSeparator();

            Menu.Add("UseExhaust?", new CheckBox("Use Exhaust?"));

            Menu.AddSeparator();

            Menu.Add("UseHeal?", new CheckBox("Use Heal?"));
            Menu.Add("HealHealth", new Slider("Auto Heal when Health% is at:", 20, 1, 100));

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            AIHeroClient.OnProcessSpellCast += AIHeroClient_OnProcessSpellCast;
            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;

            Chat.Print("Wu" + CN + " Loaded, [By WujuSan], Version: " + AssVersion);
            if (Exhaust != null) Chat.Print("Use Harass Key in lane for not lose your Exhaust");
        }

        //-------------------------------------Interrupter_OnInterruptableSpell--------------------------------------

        static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            if (e.DangerLevel == DangerLevel.High)
            {
                if (W.IsReady() && sender.IsValidTarget(200)) W.Cast(sender);
                else if (Q.IsReady() && sender.IsValidTarget(Q.Range)) Q.Cast();
            }

            return;
        }

        //-------------------------------------Obj_AI_Base_OnProcessSpellCast--------------------------------------

        static void AIHeroClient_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (DodgeSpells.Any(el => el == args.SData.Name) && Menu[args.SData.Name].Cast<CheckBox>().CurrentValue)
            {
                if (args.SData.Name == "KatarinaR")
                {
                    if (Q.IsReady() && Q.IsInRange(sender)) Q.Cast();
                    else if (W.IsReady() && W.IsInRange(sender)) W.Cast(sender);
                    return;
                }

                if (Q.IsReady() && Q.IsInRange(sender)) { Q.Cast(); return; }
                if (W.IsReady() && sender.Distance(Player) <= 200) { W.Cast(sender); return; }
            }

            return;
        }
        
        //----------------------------------------------Drawing_OnDraw----------------------------------------

        static void Drawing_OnDraw(EventArgs args)
        {
            if (!Player.IsDead)
            {
                if (Target != null && W.IsReady())
                {
                    if (!Menu["Insec"].Cast<KeyBind>().CurrentValue && !Insecing) WalkPos = Game.CursorPos.Extend(Target, Game.CursorPos.Distance(Target) + 150);

                    if (Q.IsReady() && (Target.IsValidTarget(Q.Range - 50) || (Target.IsValidTarget(Q.Range) && !Target.CanMove)) && Player.Mana >= (Player.Spellbook.GetSpell(SpellSlot.W).SData.ManaCostArray[W.Level - 1] + Player.Spellbook.GetSpell(SpellSlot.Q).SData.ManaCostArray[Q.Level - 1]))
                    {
                        Drawing.DrawText(Target.Position.WorldToScreen().X - 30, Target.Position.WorldToScreen().Y - 150, Color.Yellow, "Q/W Insec !!");
                        Drawing.DrawLine(Target.Position.WorldToScreen(), Game.CursorPos2D, 3, Color.Yellow);
                        Drawing.DrawCircle(WalkPos.To3D(), 70, Color.BlueViolet);
                    }
                    else if (Flash != null)
                    {
                        if (Flash.IsReady() && Player.Distance(WalkPos) <= Flash.Range - 40 && Player.Mana >= Player.Spellbook.GetSpell(SpellSlot.W).SData.ManaCostArray[W.Level - 1])
                        {
                            Drawing.DrawText(Target.Position.WorldToScreen().X - 30, Target.Position.WorldToScreen().Y - 150, Color.Yellow, "Flash/W Insec !!");
                            Drawing.DrawLine(Target.Position.WorldToScreen(), Game.CursorPos2D, 3, Color.Yellow);
                            Drawing.DrawCircle(WalkPos.To3D(), 70, Color.BlueViolet);
                        }

                        else if (Flash.IsReady() && Q.IsReady() && Target.IsValidTarget(Flash.Range + Q.Range - 40) && Player.Mana >= (Player.Spellbook.GetSpell(SpellSlot.W).SData.ManaCostArray[W.Level - 1] + Player.Spellbook.GetSpell(SpellSlot.Q).SData.ManaCostArray[Q.Level - 1]))
                        {
                            Drawing.DrawText(Target.Position.WorldToScreen().X - 30, Target.Position.WorldToScreen().Y - 150, Color.Yellow, "Flash/Q/W Insec !!");
                            Drawing.DrawLine(Target.Position.WorldToScreen(), Game.CursorPos2D, 3, Color.Yellow);
                            Drawing.DrawCircle(Player.Position.Extend(Target, Flash.Range).To3D(), 70, Color.Yellow);
                            Drawing.DrawCircle(WalkPos.To3D(), 70, Color.BlueViolet);
                        }
                    }
                }

                if (Menu["DrawW"].Cast<CheckBox>().CurrentValue)
                    Circle.Draw(W.IsReady() ? Green : Red, W.Range, Player.Position);

            }
            return;
        }

        //-------------------------------------------Game_OnTick----------------------------------------------

        static void Game_OnTick(EventArgs args)
        {
            if (Player.IsDead) return;

            if (Player.CountEnemiesInRange(1000) > 0) Modes.SaveAlly();

            Target = TargetSelector.GetTarget(1200, DamageType.Magical);

            if (Target != null)
            {
                if (Target.IsValidTarget())
                {
                    //---------------------------------------------------Insec--------------------------------------------

                    if (Menu["Insec"].Cast<KeyBind>().CurrentValue && !Insecing && !Target.HasBuffOfType(BuffType.SpellImmunity) && !Target.HasBuffOfType(BuffType.Invulnerability))
                    {
                        EloBuddy.Player.IssueOrder(GameObjectOrder.MoveTo, Target);

                        if (W.IsReady())
                        {
                            if (Q.IsReady() && (Target.IsValidTarget(Q.Range - 50) || (Target.IsValidTarget(Q.Range) && !Target.CanMove)) && Player.Mana >= (Player.Spellbook.GetSpell(SpellSlot.W).SData.ManaCostArray[W.Level - 1] + Player.Spellbook.GetSpell(SpellSlot.Q).SData.ManaCostArray[Q.Level - 1]))
                            {
                                Insecing = true;
                                QWInsec();
                            }
                            else if (Flash != null)
                            {
                                WalkPos = Game.CursorPos.Extend(Target, Game.CursorPos.Distance(Target) + 100);

                                if (Player.Distance(WalkPos) <= Flash.Range - 40 && Flash.IsReady() && Player.Mana >= Player.Spellbook.GetSpell(SpellSlot.W).SData.ManaCostArray[W.Level-1])
                                {
                                    Insecing = true;
                                    Flash.Cast(WalkPos.To3D());
                                    W.Cast(Target);
                                    Insecing = false;
                                }

                                else if (Target.IsValidTarget(Flash.Range + Q.Range - 50) && Flash.IsReady() && Q.IsReady() && Player.Mana >= (Player.Spellbook.GetSpell(SpellSlot.W).SData.ManaCostArray[W.Level - 1] + Player.Spellbook.GetSpell(SpellSlot.Q).SData.ManaCostArray[Q.Level - 1]))
                                {
                                    Insecing = true;
                                    QWInsec(true);
                                }
                            }
                        }
                    }

                    //---------------------------------------------------Combo--------------------------------------------

                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)) Modes.Combo();
                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass)) Modes.Harass();
                }
            }

            if (!Player.HasBuff("recall") && !Insecing && E.IsReady()) Modes.GoHeal();

            return;
        }

        //----------------------------------------class Modes---------------------------------------

        class Modes
        {
            public static void Combo()
            {
                if (QSS.IsReady() && (Player.HasBuffOfType(BuffType.Charm) || Player.HasBuffOfType(BuffType.Blind) || Player.HasBuffOfType(BuffType.Fear) || Player.HasBuffOfType(BuffType.Polymorph) || Player.HasBuffOfType(BuffType.Silence) || Player.HasBuffOfType(BuffType.Sleep) || Player.HasBuffOfType(BuffType.Snare) || Player.HasBuffOfType(BuffType.Stun) || Player.HasBuffOfType(BuffType.Suppression) || Player.HasBuffOfType(BuffType.Taunt))) QSS.Cast();

                if (Q.IsReady() && Target.IsValidTarget(Q.Range - 40) && !Player.IsDashing()) Q.Cast();

                else if (!Combing && W.IsReady() && Q.IsReady() && Target.IsValidTarget(W.Range - 50) && Player.Mana >= (Player.Spellbook.GetSpell(SpellSlot.W).SData.ManaCostArray[W.Level - 1] + Player.Spellbook.GetSpell(SpellSlot.Q).SData.ManaCostArray[Q.Level - 1])) { WQ(); Combing = true; }

                if (Exhaust != null && Menu["UseExhaust?"].Cast<CheckBox>().CurrentValue && TargetSelector.GetPriority(Target) > 3 && Target.IsValidTarget(Exhaust.Range) && Exhaust.IsReady()) Exhaust.Cast(Target);

                if (Target.IsValidTarget(Bilgewater.Range) && Bilgewater.IsReady()) Bilgewater.Cast(Target);

                if (Target.IsValidTarget(Randuin.Range) && Randuin.IsReady()) Randuin.Cast();

                return;
            }

            public static void Harass()
            {
                if (Q.IsReady() && Target.IsValidTarget(Q.Range - 50) && !Player.IsDashing()) Q.Cast();

                else if (!Combing && W.IsReady() && Q.IsReady() && Target.IsValidTarget(W.Range - 50) && Player.Mana >= (Player.Spellbook.GetSpell(SpellSlot.W).SData.ManaCostArray[W.Level - 1] + Player.Spellbook.GetSpell(SpellSlot.Q).SData.ManaCostArray[Q.Level - 1])) { WQ(); Combing = true; }

                return;
            }

            public static void SaveAlly()
            {
                var Ally = EntityManager.Heroes.Allies.FirstOrDefault(ally => EntityManager.Heroes.Enemies.Any(enemy => ally.IsFacing(enemy)) && ally.HealthPercent <= 30 && Player.Distance(ally) <= 750);

                if (Ally != null)
                {
                    if (FOTMountain.IsReady()) FOTMountain.Cast(Ally);

                    if (Mikael.IsReady() && (Ally.HasBuffOfType(BuffType.Charm) || Ally.HasBuffOfType(BuffType.Fear) || Ally.HasBuffOfType(BuffType.Poison) || Ally.HasBuffOfType(BuffType.Polymorph) || Ally.HasBuffOfType(BuffType.Silence) || Ally.HasBuffOfType(BuffType.Sleep) || Ally.HasBuffOfType(BuffType.Slow) || Ally.HasBuffOfType(BuffType.Snare) || Ally.HasBuffOfType(BuffType.Stun) || Ally.HasBuffOfType(BuffType.Taunt))) Mikael.Cast(Ally);
                }

                if (Heal != null && Menu["UseHeal?"].Cast<CheckBox>().CurrentValue)
                {
                    var healtarget = EntityManager.Heroes.Allies.FirstOrDefault(it => it.IsValidTarget(Heal.Range) && it.HealthPercent <= Menu["HealHealth"].Cast<Slider>().CurrentValue);

                    if (healtarget != null)
                    {
                        if (EntityManager.Heroes.Enemies.Any(it => it.IsValidTarget() && it.Distance(healtarget) <= it.GetAutoAttackRange())) Heal.Cast();
                    }
                }

                return;
            }

            public static void GoHeal()
            {
                if (EntityManager.Heroes.Allies.Where(ally => ally.HealthPercent <= Menu["LifeToE"].Cast<Slider>().CurrentValue && E.IsInRange(ally)).Any() && Player.ManaPercent >= Menu["ManaToE"].Cast<Slider>().CurrentValue)
                {
                    if (Player.HealthPercent <= Menu["LifeToE"].Cast<Slider>().CurrentValue && !Menu["EYourself"].Cast<CheckBox>().CurrentValue) { }
                    else E.Cast();
                }

                return;
            }
        }

        //----------------------------------------------WQ()----------------------------------------

        static void WQ()
        {
            if (EntityManager.Heroes.Allies.Where(ally => !ally.IsMe && ally.Distance(Player) <= 600).Count() > 0)
            {
                if (Glory.IsReady()) Glory.Cast();
                if (Talisma.IsReady()) Talisma.Cast();
            }

            int delay = (int)( ( Player.Distance(Target) / Player.Spellbook.GetSpell(SpellSlot.W).SData.MissileSpeed * 1000) + W.CastDelay - Q.CastDelay + Menu["W/Q Delay"].Cast<Slider>().CurrentValue);

            if (W.Cast(Target))
            {
                Core.DelayAction(() => Q.Cast(), delay);
                Core.DelayAction(() => Combing = false, delay + 1000);
            }
            else Combing = false;

            return;
        }

        //----------------------------------------------QWInsec(bool flash)----------------------------------------

        static void QWInsec(bool flash = false)
        {
            if (EntityManager.Heroes.Allies.Where(ally => !ally.IsMe && ally.Distance(Player) <= 600).Count() > 0)
            {
                if (Glory.IsReady()) Glory.Cast();
                if (Talisma.IsReady()) Talisma.Cast();
            }

            if (flash)
            {
                var FlashPos = Player.Position.Extend(Target, Flash.Range).To3D();

                var Flashed = Flash.Cast(FlashPos);

                if (Flashed)
                {
                    Core.DelayAction(delegate
                    {
                        int delay = (int)(Player.Distance(WalkPos) / Player.MoveSpeed * 1000) + 400 + Q.CastDelay + 2 * Game.Ping;

                        Q.Cast();

                        WalkPos = Game.CursorPos.Extend(Target, Game.CursorPos.Distance(Target) + 150);
                        var WalkPos3D = WalkPos.To3D();

                        EloBuddy.Player.IssueOrder(GameObjectOrder.MoveTo, WalkPos3D);
                        Core.DelayAction(() => CheckWDistance(), delay);
                        Core.DelayAction(() => Insecing = false, delay + 1000);
                    }, Game.Ping + 70);
                }
                else Insecing = false;

                return;
            }

            else
            {
                Q.Cast();

                WalkPos = Game.CursorPos.Extend(Target, Game.CursorPos.Distance(Target) + 150);

                int delay = (int)(Player.Distance(WalkPos) / Player.MoveSpeed * 1000) + 400 + Q.CastDelay + 2 * Game.Ping;

                EloBuddy.Player.IssueOrder(GameObjectOrder.MoveTo, WalkPos.To3D());
                Core.DelayAction(() => CheckWDistance(), delay);
                Core.DelayAction(() => Insecing = false, delay + 1000);

                return;
            }
        }

        //----------------------------------------------CheckWDistance()----------------------------------------

        static void CheckWDistance()
        {
            if (Player.Distance(WalkPos) <= 70) W.Cast(Target);
            return;
        }

        //--------------------------------------------SearchVersion()-------------------------------------------

        static void SearchVersion()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    string Text = new WebClient().DownloadString("https://raw.githubusercontent.com/WujuSan/EloBuddy/master/Wu" + CN + "/Wu" + CN + "/Properties/AssemblyInfo.cs");

                    var Match = new Regex(@"\[assembly\: AssemblyVersion\(""(\d{1,})\.(\d{1,})\.(\d{1,})\.(\d{1,})""\)\]").Match(Text);

                    if (Match.Success)
                    {
                        var CorrectVersion = new Version(string.Format("{0}.{1}.{2}.{3}", Match.Groups[1], Match.Groups[2], Match.Groups[3], Match.Groups[4]));

                        if (CorrectVersion > AssVersion)
                        {
                            Chat.Print("<font color='#FFFF00'>Your Wu" + CN + " is </font><font color='#FF0000'>OUTDATED</font><font color='#FFFF00'>, The correct version is: " + CorrectVersion + "</font>");
                            Chat.Print("<font color='#FFFF00'>Your Wu" + CN + " is </font><font color='#FF0000'>OUTDATED</font><font color='#FFFF00'>, The correct version is: " + CorrectVersion + "</font>");
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e + "\n [ [RIP] Search ]");
                }
            });
        }

    }//Class End
}
