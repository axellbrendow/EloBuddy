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

        static Item Bilgewater, Randuin, QSS, Glory, FOTMountain, Mikael;
        static Menu Menu;
        static bool Insecing = new bool();
        static AIHeroClient Target = null;
        static List<string> DodgeSpells = new List<string>() { "LuxMaliceCannon", "LuxMaliceCannonMis", "EzrealtrueShotBarrage", "KatarinaR", "YasuoDashWrapper", "ViR", "NamiR", "ThreshQ", "xerathrmissilewrapper", "yasuoq3w", "UFSlash" };
        static readonly Spell.Active Q = new Spell.Active(SpellSlot.Q, 365);
        static readonly Spell.Targeted W = new Spell.Targeted(SpellSlot.W, 650);
        static readonly Spell.Active E = new Spell.Active(SpellSlot.E, 575);

        static void Main(string[] args) { Loading.OnLoadingComplete += OnLoadingComplete; }

        //---------------------------------------------Game_OnGameLoad----------------------------------------

        static void OnLoadingComplete(EventArgs args)
        {
            if (Player.BaseSkinName != CN) { Chat.Print("Sorry, you didn't choose " + CN + ", addon disabled"); return; }

            AssVersion = Assembly.GetExecutingAssembly().GetName().Version;
            SearchVersion();

            //-------------------------------------------------Items--------------------------------------------------

            Bilgewater = new Item(3144, 550);
            Randuin = new Item(3143, 500);
            Glory = new Item(3800);
            QSS = new Item(3140);
            FOTMountain = new Item(3401);
            Mikael = new Item(3222, 750);

            //-------------------------------------------------Flash--------------------------------------------------

            SpellDataInst flash = Player.Spellbook.Spells.Where(spell => spell.Name.Contains("flash")).Any() ? Player.Spellbook.Spells.Where(spell => spell.Name.Contains("flash")).First() : null;
            if (flash != null)
            {
                Flash = new Spell.Skillshot(flash.Slot, 425, SkillShotType.Linear);
            }
            flash = null;

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

            Menu.Add("W/Q Delay", new Slider("W/Q Delay", 0, -500, 500));

            Menu.AddSeparator();

            Menu.Add("DrawW", new CheckBox("Draw W"));

            Menu.AddSeparator();

            Menu.Add("Insec", new KeyBind("Insec", false, KeyBind.BindTypes.HoldActive, 'T'));

            Menu.AddSeparator();

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            AIHeroClient.OnProcessSpellCast += AIHeroClient_OnProcessSpellCast;

            Chat.Print("Wu" + CN + " Loaded, [By WujuSan], Version: " + AssVersion);
        }
        //-------------------------------------Obj_AI_Base_OnProcessSpellCast--------------------------------------

        static void AIHeroClient_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (DodgeSpells.Any(el => el == args.SData.Name) && Menu[args.SData.Name].Cast<CheckBox>().CurrentValue)
            {
                if (Q.IsReady() && Q.IsInRange(sender)) Q.Cast();
                else if (W.IsReady() && W.IsInRange(sender)) W.Cast(sender);
            }
        }
        
        //----------------------------------------------Drawing_OnDraw----------------------------------------

        static void Drawing_OnDraw(EventArgs args)
        {
            if (!Player.IsDead)
            {
                if (Target != null && W.IsReady())
                {
                    var WalkPos = Game.CursorPos.Extend(Target, Game.CursorPos.Distance(Target) + 100).To3D();

                    if ((Target.IsValidTarget(Q.Range - 40) || (Target.IsValidTarget(Q.Range) && !Target.CanMove)) && (Q.IsReady() || !Target.CanMove))
                    {
                        Drawing.DrawText(Target.Position.WorldToScreen().X - 30, Target.Position.WorldToScreen().Y - 150, Color.Yellow, "Q/W Insec !!");
                        Drawing.DrawLine(Target.Position.WorldToScreen(), Game.CursorPos2D, 4, Color.Yellow);
                        Drawing.DrawCircle(WalkPos, 70, Color.Yellow);
                    }
                    else if (Target.IsValidTarget(Flash.Range) && Flash != null)
                    {
                        if (Flash.IsReady())
                        {
                            Drawing.DrawText(Target.Position.WorldToScreen().X - 30, Target.Position.WorldToScreen().Y - 150, Color.Yellow, "Flash Insec !!");
                            Drawing.DrawLine(Target.Position.WorldToScreen(), Game.CursorPos2D, 4, Color.Yellow);
                            Drawing.DrawCircle(WalkPos, 70, Color.Yellow);
                        }

                    }
                }

                if (Menu["DrawW"].Cast<CheckBox>().CurrentValue)
                    Drawing.DrawCircle(Player.Position, W.Range, W.IsReady() ? Color.Green : Color.Red);

            }
            return;
        }

        //-------------------------------------------Game_OnTick----------------------------------------------

        static void Game_OnTick(EventArgs args)
        {
            if (Player.IsDead) return;

            if (Player.CountEnemiesInRange(1000) > 0)
            {
                foreach (AIHeroClient enemy in EntityManager.Heroes.Enemies)
                {
                    foreach (AIHeroClient ally in EntityManager.Heroes.Allies)
                    {
                        if (ally.IsFacing(enemy) && ally.HealthPercent <= 30 && Player.Distance(ally) <= 750)
                        {
                            if (FOTMountain.IsReady()) FOTMountain.Cast(ally);

                            if ((ally.HasBuffOfType(BuffType.Charm) || ally.HasBuffOfType(BuffType.Fear) || ally.HasBuffOfType(BuffType.Poison) || ally.HasBuffOfType(BuffType.Polymorph) || ally.HasBuffOfType(BuffType.Silence) || ally.HasBuffOfType(BuffType.Sleep) || ally.HasBuffOfType(BuffType.Slow) || ally.HasBuffOfType(BuffType.Snare) || ally.HasBuffOfType(BuffType.Stun) || ally.HasBuffOfType(BuffType.Taunt)) && Mikael.IsReady()) Mikael.Cast(ally);
                        }
                    }
                }
            }

            Target = TargetSelector.GetTarget(700, DamageType.Magical);

            if (Target != null)
            {
                if (Target.IsValidTarget())
                {
                    //---------------------------------------------------Insec--------------------------------------------

                    if ( Menu["Insec"].Cast<KeyBind>().CurrentValue && W.IsReady() && !Insecing)
                    {
                        if ( (Target.IsValidTarget(Q.Range - 40) || (Target.IsValidTarget(Q.Range) && !Target.CanMove) ) && (Q.IsReady() || !Target.CanMove) )
                        {
                            QWInsec();
                        }
                        else if (Flash != null)
                        {
							if (Target.IsValidTarget(Flash.Range) && Flash.IsReady())
							{
								Flash.Cast(Target);
                                QWInsec();
							}
                        }
                    }

                    //---------------------------------------------------Combo--------------------------------------------

                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                    {
                        if (QSS.IsReady() && (Player.HasBuffOfType(BuffType.Charm) || Player.HasBuffOfType(BuffType.Blind) || Player.HasBuffOfType(BuffType.Fear) || Player.HasBuffOfType(BuffType.Polymorph) || Player.HasBuffOfType(BuffType.Silence) || Player.HasBuffOfType(BuffType.Sleep) || Player.HasBuffOfType(BuffType.Snare) || Player.HasBuffOfType(BuffType.Stun) || Player.HasBuffOfType(BuffType.Suppression) || Player.HasBuffOfType(BuffType.Taunt))) QSS.Cast();

                        if (Q.IsReady() && Target.IsValidTarget(Q.Range-30) && !Player.IsDashing()) Q.Cast();

                        else if (W.IsReady() && Q.IsReady() && Target.IsValidTarget(W.Range-30) && Player.Mana >= ( Player.Spellbook.GetSpell(SpellSlot.W).SData.ManaCostArray[W.Level - 1] + Player.Spellbook.GetSpell(SpellSlot.Q).SData.ManaCostArray[Q.Level - 1] ))
                        {
                            int delay = (int)(Player.Distance(Target) / Player.Spellbook.GetSpell(SpellSlot.W).SData.MissileSpeed)*1000 + Menu["W/Q Delay"].Cast<Slider>().CurrentValue;
                            
                            if (EntityManager.Heroes.Allies.Where(ally => ally != Player && ally.Distance(Player) <= 700).Count() > 0 && Glory.IsReady()) Glory.Cast();
                            Core.DelayAction(() => Q.Cast(), delay);
                            W.Cast(Target);
                        }

                        if (Target.IsValidTarget(Bilgewater.Range) && Bilgewater.IsReady()) Bilgewater.Cast(Target);

                        if (Target.IsValidTarget(Randuin.Range) && Randuin.IsReady()) Randuin.Cast();

                    }
                }
            }

            if (!Player.HasBuff("recall"))
            {
                if (E.IsReady() && EntityManager.Heroes.Allies.Where(ally => ally.HealthPercent <= Menu["LifeToE"].Cast<Slider>().CurrentValue && E.IsInRange(ally)).Any() && Player.ManaPercent >= Menu["ManaToE"].Cast<Slider>().CurrentValue)
                {
                    if (Player.HealthPercent <= Menu["LifeToE"].Cast<Slider>().CurrentValue && !Menu["EYourself"].Cast<CheckBox>().CurrentValue) { }
                    else E.Cast();
                }
            }

            return;
        }

        //----------------------------------------------QWInsec()----------------------------------------

        static void QWInsec()
        {
            var WalkPos = Game.CursorPos.Extend(Target, Game.CursorPos.Distance(Target) + 100).To3D();

            Insecing = true;
            Q.Cast();
            int delay = (int)(Player.Distance(WalkPos) / Player.MoveSpeed * 1000) + 200 + Q.CastDelay;
            EloBuddy.Player.IssueOrder(GameObjectOrder.MoveTo, WalkPos);
            Core.DelayAction(() => W.Cast(Target), delay);
            Core.DelayAction(() => Insecing = false, delay);

            return;
        }

        //--------------------------------------------SearchVersion()-------------------------------------------

        static void SearchVersion()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    string Text = new WebClient().DownloadString("https://raw.githubusercontent.com/WujuSan/EloBuddy/master/Wu" + CN + "/Properties/AssemblyInfo.cs");

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
