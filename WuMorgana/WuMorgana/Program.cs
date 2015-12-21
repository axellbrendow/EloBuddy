using System;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Generic;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using EloBuddy.SDK.Enumerations;
using SharpDX;
using Color = System.Drawing.Color;
using Version = System.Version;
using System.Net;
using System.Text.RegularExpressions;

namespace WuMorgana
{
    static class Program
    {
        static Version AssVersion;//Kappa
        static readonly String CN = "Morgana";
        static Spell.Active Heal;
        static Spell.Targeted Ignite, Exhaust;
        static Item Mikael, Glory, FOTMountain, Zhonya, Talisma;
        static AIHeroClient Player = EloBuddy.Player.Instance;
        static readonly ColorBGRA Green = new ColorBGRA(Color.Green.R, Color.Green.G, Color.Green.B, Color.Green.A);
        static readonly ColorBGRA Red = new ColorBGRA(Color.Red.R, Color.Red.G, Color.Red.B, Color.Red.A);
        
        static Menu Menu;
        static Menu EMenu;
        static AIHeroClient Target = null;
        static List<string> MenuSpells = new List<string>();
        static List<string> ESpells = new List<string>() { "SorakaQ", "SorakaE", "TahmKenchW", "TahmKenchQ", "Bushwhack", "ForcePulse", "KarthusFallenOne", "KarthusWallOfPain", "KarthusLayWasteA1", "KarmaWMantra", "KarmaQMissileMantra", "KarmaSpiritBind", "KarmaQ", "JinxW", "JinxE", "JarvanIVGoldenAegis", "HowlingGaleSpell", "SowTheWind", "ReapTheWhirlwind", "IllaoiE", "HeimerdingerUltWDummySpell", "HeimerdingerUltEDummySpell", "HeimerdingerW", "HeimerdingerE", "HecarimUlt", "HecarimRampAttack", "GravesQLineSpell", "GravesQLineMis", "GravesClusterShot", "GravesSmokeGrenade", "GangplankR", "GalioIdolOfDurand", "GalioResoluteSmite", "FioraE", "EvelynnR", "EliseHumanE", "EkkoR", "EkkoW", "EkkoQ", "DravenDoubleShot", "InfectedCleaverMissileCast", "DariusExecute", "DariusAxeGrabCone", "DariusNoxianTacticsONH", "DariusCleave", "PhosphorusBomb", "MissileBarrage", "BraumQ", "BrandFissure", "BardR", "BardQ", "AatroxQ", "AatroxE", "AzirE", "AzirEWrapper", "AzirQWrapper", "AzirQ", "AzirR", "Pulverize", "AhriSeduce", "CurseoftheSadMummy", "InfernalGuardian", "Incinerate", "Volley", "EnchantedCrystalArrow", "BraumRWrapper", "CassiopeiaPetrifyingGaze", "FeralScream", "Rupture", "EzrealEssenceFlux", "EzrealMysticShot", "EzrealTrueshotBarrage", "FizzMarinerDoom", "GnarW", "GnarBigQMissile", "GnarQ", "GnarR", "GragasQ", "GragasE", "GragasR", "RiftWalk", "LeblancSlideM", "LeblancSlide", "LeonaSolarFlare", "UFSlash", "LuxMaliceCannon", "LuxLightStrikeKugel", "LuxLightBinding", "yasuoq3w", "VelkozE", "VeigarEventHorizon", "VeigarDarkMatter", "VarusR", "ThreshQ", "ThreshE", "ThreshRPenta", "SonaQ", "SonaR", "ShenShadowDash", "SejuaniGlacialPrisonCast", "RivenMartyr", "JavelinToss", "NautilusSplashZone", "NautilusAnchorDrag", "NamiR", "NamiQ", "DarkBindingMissile", "StaticField", "RocketGrab", "RocketGrabMissile", "timebombenemybuff", "karthusfallenonetarget", "NocturneUnspeakableHorror", "SyndraQ", "SyndraE", "SyndraR", "VayneCondemn", "Dazzle", "Overload", "AbsoluteZero", "IceBlast", "LeblancChaosOrb", "JudicatorReckoning", "KatarinaQ", "NullLance", "Crowstorm", "FiddlesticksDarkWind", "BrandWildfire", "Disintegrate", "FlashFrost", "Frostbite", "AkaliMota", "InfiniteDuress", "PantheonW", "blindingdart", "JayceToTheSkies", "IreliaEquilibriumStrike", "maokaiunstablegrowth", "nautilusgandline", "runeprison", "WildCards", "BlueCardAttack", "RedCardAttack", "GoldCardAttack", "AkaliShadowDance", "Headbutt", "PowerFist", "BrandConflagration", "CaitlynYordleTrap", "CaitlynAceintheHole", "CassiopeiaNoxiousBlast", "CassiopeiaMiasma", "CassiopeiaTwinFang", "Feast", "DianaArc", "DianaTeleport", "EliseHumanQ", "EvelynnE", "Terrify", "FizzPiercingStrike", "Parley", "GarenQAttack", "GarenR", "IreliaGatotsu", "IreliaEquilibriumStrike", "SowTheWind", "JarvanIVCataclysm", "JaxLeapStrike", "JaxEmpowerTwo", "JaxCounterStrike", "JayceThunderingBlow", "KarmaSpiritBind", "NetherBlade", "KatarinaR", "JudicatorRighteousFury", "KennenBringTheLight", "LeblancChaosOrbM", "BlindMonkRKick", "LeonaZenithBlade", "LeonaShieldOfDaybreak", "LissandraW", "LissandraQ", "LissandraR", "LuluQ", "LuluW", "LuluE", "LuluR", "SeismicShard", "AlZaharMaleficVisions", "AlZaharNetherGrasp", "MaokaiUnstableGrowth", "MordekaiserMaceOfSpades", "MordekaiserChildrenOfTheGrave", "SoulShackles", "NamiW", "NasusW", "NautilusGrandLine", "Takedown", "NocturneParanoia", "PoppyDevastatingBlow", "PoppyHeroicCharge", "QuinnE", "PuncturingTaunt", "RenektonPreExecute", "SpellFlux", "SejuaniWintersClaw", "TwoShivPoisen", "Fling", "SkarnerImpale", "SonaHymnofValor", "SwainTorment", "SwainDecrepify", "BlindingDart", "OrianaIzunaCommand", "OrianaDetonateCommand", "DetonatingShot", "BusterShot", "TrundleTrollSmash", "TrundlePain", "MockingShout", "Expunge", "UdyrBearStance", "UrgotHeatseekingLineMissile", "UrgotSwap2", "VeigarBalefulStrike", "VeigarPrimordialBurst", "ViR", "ViktorPowerTransfer", "VladimirTransfusion", "VolibearQ", "HungeringStrike", "XenZhaoComboTarget", "XenZhaoSweep", "YasuoQ3W", "YasuoQ3Mis", "YasuoQ3", "YasuoRKnockUpComboW" };
        static readonly Spell.Skillshot Q = new Spell.Skillshot(SpellSlot.Q, 1100, SkillShotType.Linear, 250, 1200, 70);
        static readonly Spell.Skillshot W = new Spell.Skillshot(SpellSlot.W, 900, SkillShotType.Circular, 250, 2200, 280);
        static readonly Spell.Targeted E = new Spell.Targeted(SpellSlot.E, 750);
        static readonly Spell.Active R = new Spell.Active(SpellSlot.R, 625);

        static void Main(string[] args) { Loading.OnLoadingComplete += OnLoadingComplete; }

        //--------------------------------------------Game_OnGameLoad--------------------------------------------

        static void OnLoadingComplete(EventArgs args)
        {
            if (Player.BaseSkinName != CN) { Chat.Print("Sorry, you didn't pick " + CN + ", addon disabled"); return; }

            AssVersion = Assembly.GetExecutingAssembly().GetName().Version;
            SearchVersion();

            //-------------------------------------------------Items--------------------------------------------------

            Glory = new Item(3800);
            Mikael = new Item(3222, 750);
            FOTMountain = new Item(3401);
            Talisma = new Item(ItemId.Talisman_of_Ascension);//
            Zhonya = new Item(ItemId.Zhonyas_Hourglass);//

            Q.MinimumHitChance = HitChance.Medium;

            //-------------------------------------------------Ignite--------------------------------------------------

            SpellDataInst dot = Player.Spellbook.Spells.Where(spell => spell.Name.Contains("dot")).Any() ? Player.Spellbook.Spells.Where(spell => spell.Name.Contains("dot")).First() : null;
            if (dot != null)
            {
                Ignite = new Spell.Targeted(dot.Slot, 600);
            }

            //-------------------------------------------------Heal--------------------------------------------------

            SpellDataInst heal = Player.Spellbook.Spells.Where(spell => spell.Name.Contains("heal")).Any() ? Player.Spellbook.Spells.Where(spell => spell.Name.Contains("heal")).First() : null;
            if (heal != null)
            {
                Heal = new Spell.Active(heal.Slot, 850);//
            }

            //-------------------------------------------------Exhaust--------------------------------------------------

            SpellDataInst exhaust = Player.Spellbook.Spells.Where(spell => spell.Name.Contains("exhaust")).Any() ? Player.Spellbook.Spells.Where(spell => spell.Name.Contains("exhaust")).First() : null;
            if (exhaust != null)
            {
                Exhaust = new Spell.Targeted(exhaust.Slot, 650);//
            }

            //---------------------------||   Menu   ||----------------------------

            Menu = MainMenu.AddMenu("Wu" + CN, "Wu" + CN);

            //------------------------------Combo-------------------------------
            
            Menu.AddGroupLabel("Combo");
            {
                Menu.Add("UseQCombo", new CheckBox("Use Q Combo"));
                Menu.Add("QHitChanceCombo", new Slider("QHitChance:", 70, 0, 100));
                Menu.Add("UseWCombo", new CheckBox("Use W Combo"));
                Menu.Add("UseRCombo", new CheckBox("Use R Combo"));
                Menu.Add("Min Enemies R", new Slider("Min Enemies R", 2, 1, 5));
                Menu.Add("UseExhaust?", new CheckBox("Use Exhaust?"));
            }
            Menu.AddSeparator();

            //------------------------------Harass-------------------------------

            Menu.AddGroupLabel("Harass");
            {
                Menu.Add("UseQHarass", new CheckBox("Use Q Harass"));
                Menu.Add("QHitChanceHarass", new Slider("QHitChance:", 70, 0, 100));
                Menu.Add("UseWHarass", new CheckBox("Use W Harass"));
                Menu.Add("Harass, Mana %", new Slider("Harass, Mana %", 30, 1, 100));
            }
            Menu.AddSeparator();

            //------------------------------LaneClear-------------------------------

            Menu.AddGroupLabel("LaneClear");
            {
                Menu.Add("UseWLaneClear", new CheckBox("Use W LaneClear"));
                Menu.Add("Min Minions W", new Slider("Min Minions W", 3, 1, 7));
                Menu.Add("LaneClear, Mana %", new Slider("LaneClear, Mana %", 30, 1, 100));
            }
            Menu.AddSeparator();

            //------------------------------Drawings-------------------------------

            Menu.AddGroupLabel("Drawings");
            {
                Menu.Add("DrawQ", new CheckBox("Draw Q"));
                Menu.Add("DrawW", new CheckBox("Draw W", false));
                Menu.Add("DrawE", new CheckBox("Draw E"));
                Menu.Add("DrawR", new CheckBox("Draw R", false));
                Menu.Add("ComboDamage on HPBar", new CheckBox("ComboDamage on HPBar"));
            }
            Menu.AddSeparator();

            //------------------------------Other things-------------------------------

            Menu.AddGroupLabel("Other things");

            Menu.Add("KS", new CheckBox("KS", false));
            Menu.Add("AAMinions?", new CheckBox("AA minions when ally near?", false));
            Menu.Add("Gapcloser", new CheckBox("Gapcloser"));
            Menu.Add("Auto Ignite", new CheckBox("Auto Ignite"));
            Menu.Add("AutoQFlash", new CheckBox("Auto Q on flash"));
            Menu.Add("AutoQDash", new CheckBox("Auto Q on dashing"));
            Menu.Add("AutoQImmobile", new CheckBox("Auto Q on immobile"));
            Menu.Add("AutoWImmobile", new CheckBox("Auto W on immobile"));
            Menu.AddSeparator();
            Menu.Add("UseHeal?", new CheckBox("Use Heal?"));
            Menu.Add("HealHealth", new Slider("Auto Heal when Health% is at:", 20, 1, 100));
            Menu.AddSeparator();
            Menu.Add("UseZhonya?", new CheckBox("Use Zhonya?"));
            Menu.Add("ZhonyaUlt", new CheckBox("Just Zhonya when casting ultimate", false));
            Menu.Add("ZhonyaHealth", new Slider("Auto Zhonya when Health% is at:", 15, 1, 100));
            
            Menu.AddSeparator();

            //---------------------------------------||   EMenu   ||------------------------------------------

            EMenu = Menu.AddSubMenu("E Options", "E Options");
            EMenu.AddSeparator();

            foreach (var ally in EntityManager.Heroes.Allies)
            {
                EMenu.Add(ally.BaseSkinName, new Slider(string.Format("{0}'s Priority", ally.BaseSkinName), 3, 1, 5));
                EMenu.AddSeparator();
            }

            EMenu.AddSeparator();

            EMenu.Add("UseShield?", new CheckBox("Use Shield?"));

            EMenu.AddSeparator();

            EMenu.AddGroupLabel("Use shield for:");

            EMenu.AddSeparator();

            foreach (AIHeroClient hero in EntityManager.Heroes.Enemies)
            {
                EMenu.AddGroupLabel(hero.BaseSkinName);
                {
                    foreach (SpellDataInst spell in hero.Spellbook.Spells)
                    {
                        if (ESpells.Any(el => el == spell.SData.Name))
                        {
                            EMenu.Add(spell.Name, new CheckBox(hero.BaseSkinName + " : " + spell.Slot.ToString() + " : " + spell.Name));
                            MenuSpells.Add(spell.Name);
                        }
                    }
                }

                EMenu.AddSeparator();
            }

            Dash.OnDash += Dash_OnDash;
            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnEndScene += Drawing_OnEndScene;
            Orbwalker.OnPreAttack += Orbwalker_OnPreAttack;
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
            AIHeroClient.OnProcessSpellCast += AIHeroClient_OnProcessSpellCast;

            Chat.Print("Wu" + CN + " Loaded, [By WujuSan] , Version: " + AssVersion);
        }

        static void Orbwalker_OnPreAttack(AttackableUnit target, Orbwalker.PreAttackArgs args)
        {
            if (!Menu["AAMinions?"].Cast<CheckBox>().CurrentValue)
            {
                if ((Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass)) && CountAlliesInRange(2000) > 0) args.Process = false;
            }

            return;
        }

        //--------------------------------------------Dash_OnDash-------------------------------------------------

        static void Dash_OnDash(Obj_AI_Base sender, Dash.DashEventArgs e)
        {
            //Chat.Print(sender.BaseSkinName + " Dash");

            if (Menu["AutoQDash"].Cast<CheckBox>().CurrentValue && Q.IsReady() && sender.IsEnemy && e.EndPos.Distance(Player) <= Q.Range)
            {
                if (sender.BaseSkinName == "Yasuo")
                {
                    if (Player.Distance(e.EndPos) <= 200) Q.HitChanceCast(sender, 70);
                }
                //Chat.Print("Why you didn't Q");
                else Q.HitChanceCast(sender, 70);
            }

            return;
        }

        //------------------------------------AIHeroClient_OnProcessSpellCast-------------------------------------

        static void AIHeroClient_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsEnemy)
            {
                if (Q.IsReady() && args.SData.Name.ToLower() == "summonerflash" && args.End.Distance(Player) <= Q.Range && Menu["AutoQFlash"].Cast<CheckBox>().CurrentValue)
                {
                    //Chat.Print("{0} detected, Q on args.End", args.SData.Name);
                    var rectangle = new Geometry.Polygon.Rectangle(Player.Position, args.End, Q.Width+20);

                    if ( !EntityManager.MinionsAndMonsters.EnemyMinions.Any( it => rectangle.IsInside(it) ) ) Q.Cast(args.End);
                    return;
                }

                if (E.IsReady() && EMenu["UseShield?"].Cast<CheckBox>().CurrentValue && MenuSpells.Any(it => it == args.SData.Name))
                {
                    if (EMenu[args.SData.Name].Cast<CheckBox>().CurrentValue)
                    {
                        List<AIHeroClient> Allies = new List<AIHeroClient>();

                        //Division
                        if (args.Target != null)
                        {
                            if (args.Target.IsAlly || args.Target.IsMe)
                            {
                                var target = EntityManager.Heroes.Allies.FirstOrDefault(it => it.NetworkId == args.Target.NetworkId);

                                //Chat.Print(args.Target.Name);

                                if (target != null)
                                {
                                    int delay = (int)((sender.Distance(target) / ((args.SData.MissileMaxSpeed + args.SData.MissileMinSpeed) / 2)) * 1000 + args.SData.SpellCastTime - 150 - Game.Ping);

                                    Core.DelayAction(() => E.Cast(target), delay);
                                    //Chat.Print("Targetted detection");
                                }
                                return;
                            }
                        }

                        //Division

                        var rectangle = new Geometry.Polygon.Rectangle(args.Start, args.End, args.SData.LineWidth);

                        foreach (var ally in EntityManager.Heroes.Allies)
                        {
                            if (rectangle.IsInside(ally)) { Allies.Add(ally); continue; }

                            foreach (var point in rectangle.Points)
                            {
                                if (ally.Distance(point) <= 90)
                                {
                                    Allies.Add(ally);
                                }
                            }
                        }

                        if (Allies.Any())
                        {
                            //Chat.Print("Rectangle Detection");

                            PriorityCast(sender, args, Allies);
                            return;
                        }

                        //Division

                        var circle = new Geometry.Polygon.Circle(args.End, args.SData.CastRadius);

                        foreach (var ally in EntityManager.Heroes.Allies)
                        {
                            if (circle.IsInside(ally)) { Allies.Add(ally); continue; }

                            foreach (var point in circle.Points)
                            {
                                if (ally.Distance(point) <= 90)
                                {
                                    Allies.Add(ally);
                                }
                            }
                        }

                        if (Allies.Any())
                        {
                            //Chat.Print("Circle Detection");

                            PriorityCast(sender, args, Allies);
                            return;
                        }
                    }
                }
            }

            return;
        }

        //-----------------------------------------------PriorityCast---------------------------------------------

        static void PriorityCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args, List<AIHeroClient> Allies)
        {
            int delay = new int();
            var ally = Allies.First();

            if (Allies.Count == 1)
            {
                delay = (int)((sender.Distance(ally) / ((args.SData.MissileMaxSpeed + args.SData.MissileMinSpeed) / 2)) * 1000 + args.SData.SpellCastTime - 150 - Game.Ping);
                Core.DelayAction(() => E.Cast(ally), delay);
                //Chat.Print("Shield for {0} : {1}", sender.BaseSkinName, args.Slot.ToString());
                return;
            }
            else
            {
                for (byte i = 0; i < Allies.Count; i++)
                {
                    if (i == 0) continue;
                    else if (EMenu[Allies[i].BaseSkinName].Cast<Slider>().CurrentValue > EMenu[ally.BaseSkinName].Cast<Slider>().CurrentValue) ally = Allies[i];
                }

                delay = (int)((sender.Distance(ally) / ((args.SData.MissileMaxSpeed + args.SData.MissileMinSpeed) / 2)) * 1000 + args.SData.SpellCastTime - 150 - Game.Ping);
                Core.DelayAction(() => E.Cast(ally), delay);
                //Chat.Print("Shield for {0} : {1}", sender.BaseSkinName, args.Slot.ToString());
                return;
            }
        }

        //----------------------------------------------Drawing_OnEndScene----------------------------------------

        static void Drawing_OnEndScene(EventArgs args)
        {
            if (!Player.IsDead)
            {
                if (Target != null)
                {
                    if (Menu["ComboDamage on HPBar"].Cast<CheckBox>().CurrentValue)
                    {
                        float FutureDamage = GetComboDamage() > Target.Health ? -1 : GetComboDamage() / Target.MaxHealth;

                        if (FutureDamage == -1)
                        {
                            Drawing.DrawText(Target.Position.WorldToScreen().X - 30, Target.Position.WorldToScreen().Y - 150, Color.Yellow, "Killable");
                            return;
                        }

                        Line.DrawLine
                        (
                            Color.LightSkyBlue, 9f,
                            new Vector2(Target.HPBarPosition.X + 1, Target.HPBarPosition.Y + 9),
                            new Vector2(Target.HPBarPosition.X + 1 + FutureDamage * 104, Target.HPBarPosition.Y + 9)
                        );
                    }
                }

            }

            return;
        }

        //------------------------------------------Gapcloser_OnGapcloser----------------------------------------

        static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (Menu["GapCloser"].Cast<CheckBox>().CurrentValue && sender.IsEnemy)
            {
                if (Q.IsReady() && sender.IsValidTarget(Q.Range))
                {
                    Q.Cast(e.End);
                }
            }

            return;
        }

        //----------------------------------------------Drawing_OnDraw--------------------------------------------

        static void Drawing_OnDraw(EventArgs args)
        {
            if (!Player.IsDead)
            {
                if (Menu["DrawQ"].Cast<CheckBox>().CurrentValue)
                    Circle.Draw(Q.IsReady() ? Green : Red, Q.Range, Player.Position);

                if (Menu["DrawW"].Cast<CheckBox>().CurrentValue)
                    Circle.Draw(W.IsReady() ? Green : Red, W.Range, Player.Position);

                if (Menu["DrawE"].Cast<CheckBox>().CurrentValue)
                    Circle.Draw(E.IsReady() ? Green : Red, E.Range, Player.Position);

                if (Menu["DrawR"].Cast<CheckBox>().CurrentValue)
                    Circle.Draw(R.IsReady() ? Green : Red, R.Range, Player.Position);
                
            }

            return;
        }

        //---------------------------------------------Game_OnTick------------------------------------------------

        static void Game_OnTick(EventArgs args)
        {
            if (Player.IsDead) return;
            
            if (Player.CountEnemiesInRange(1000) > 0) Modes.SaveAlly();

            if (Zhonya.IsReady() && Menu["UseZhonya?"].Cast<CheckBox>().CurrentValue && Player.HealthPercent <= Menu["ZhonyaHealth"].Cast<Slider>().CurrentValue && EntityManager.Heroes.Enemies.Any(it => it.Distance(Player) <= it.GetAutoAttackRange() && it.IsValidTarget()))
            {
                if (!Menu["ZhonyaUlt"].Cast<CheckBox>().CurrentValue) Zhonya.Cast();
                else if (!R.IsReady()) Zhonya.Cast();
            }

            if (EntityManager.Heroes.Enemies.Any(it => it.IsValidTarget() && !CanMove(it))) Modes.Immobile();

            Target = TargetSelector.GetTarget(1100, DamageType.Magical);

            //-----------------------------------------------KS----------------------------------------

            if (Menu["KS"].Cast<CheckBox>().CurrentValue && EntityManager.Heroes.Enemies.Any(it => Q.IsInRange(it))) Modes.KS();

            //-----------------------------------------------Auto Ignite----------------------------------------

            if (Menu["Auto Ignite"].Cast<CheckBox>().CurrentValue && Ignite != null)
            {
                if (Ignite.IsReady())
                {
                    var IgniteEnemy = EntityManager.Heroes.Enemies.FirstOrDefault(it => DamageLibrary.GetSummonerSpellDamage(Player, it, DamageLibrary.SummonerSpells.Ignite) >= it.Health - 30);

                    if (IgniteEnemy != null)
                    {
                        if ((IgniteEnemy.Distance(Player) >= 300 || Player.HealthPercent <= 40))
                        {
                            Ignite.Cast(IgniteEnemy);
                        }
                    }
                }
            }

            //--------------------------------------------Orbwalker Modes-------------------------------------------

            if (Target != null)
            {
                if (Target.IsValidTarget())
                {
                    Modes.UpdateVariables();

                    //---------------------------------------------------Combo--------------------------------------------

                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)) Modes.Combo();

                    //---------------------------------------------------Mixed--------------------------------------------

                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass) && Player.ManaPercent >= Menu["Harass, Mana %"].Cast<Slider>().CurrentValue) Modes.Harass();
                }
                else Target = null;
            }

            //---------------------------------------------------LaneClear--------------------------------------------

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) && W.IsReady() && Menu["UseWLaneClear"].Cast<CheckBox>().CurrentValue && Player.ManaPercent >= Menu["LaneClear, Mana %"].Cast<Slider>().CurrentValue) Modes.LaneClear();

            return;
        }

        //-------------------------------------------class Modes--------------------------------------------------

        class Modes
        {
            static bool QIsReady;
            static bool WIsReady;

            static bool QRange;
            static bool WRange;

            //----------------------------------------UpdateVariables()------------------------------------------

            public static void UpdateVariables()
            {
                QIsReady = Q.IsReady();
                WIsReady = W.IsReady();

                QRange = Q.IsInRange(Target);
                WRange = W.IsInRange(Target);

                return;
            }

            //---------------------------------------------Combo()------------------------------------------------

            public static void Combo()
            {
                if (Menu["UseQCombo"].Cast<CheckBox>().CurrentValue && QRange && QIsReady) Q.HitChanceCast(Target, Menu["QHitChanceCombo"].Cast<Slider>().CurrentValue);

                if (Menu["UseWCombo"].Cast<CheckBox>().CurrentValue && WRange && WIsReady)
                {
                    var WPos = Prediction.Position.PredictUnitPosition(Target, 500).To3D();
                    W.Cast(WPos);
                }

                if (R.IsReady() && Menu["UseRCombo"].Cast<CheckBox>().CurrentValue && Player.CountEnemiesInRange(600) >= Menu["Min Enemies R"].Cast<Slider>().CurrentValue)
                {
                    if (Glory.IsReady() && CountAlliesInRange(650) > 0) Glory.Cast();
                    if (Talisma.IsReady() && CountAlliesInRange(650) > 0) Talisma.Cast();

                    R.Cast();
                }

                if (Exhaust != null && Menu["UseExhaust?"].Cast<CheckBox>().CurrentValue && TargetSelector.GetPriority(Target) > 3 && Target.IsValidTarget(Exhaust.Range)) Exhaust.Cast(Target);

                return;
            }

            //---------------------------------------------Harass()------------------------------------------------

            public static void Harass()
            {
                if (Menu["UseQHarass"].Cast<CheckBox>().CurrentValue && QIsReady && QRange) Q.HitChanceCast(Target, Menu["QHitChanceHarass"].Cast<Slider>().CurrentValue);

                if (Menu["UseWHarass"].Cast<CheckBox>().CurrentValue && WIsReady && WRange)
                {
                    var WPos = Prediction.Position.PredictUnitPosition(Target, 500).To3D();
                    W.Cast(WPos);
                }

                return;
            }

            //-------------------------------------------LaneClear()-----------------------------------------------

            public static void LaneClear()
            {
                var Minions = EntityManager.MinionsAndMonsters.EnemyMinions.Where(it => it.IsValidTarget(W.Range));
                if (Minions != null)
                {
                    var FL = EntityManager.MinionsAndMonsters.GetCircularFarmLocation(Minions, 280, (int)W.Range);
                    
                    if (FL.HitNumber >= Menu["Min Minions W"].Cast<Slider>().CurrentValue) W.Cast(FL.CastPosition);
                }

                return;
            }

            //------------------------------------------------KS()--------------------------------------------------

            public static void KS()
            {
                if (Q.IsReady())
                {
                    var bye = EntityManager.Heroes.Enemies.FirstOrDefault(enemy => enemy.IsValidTarget(Q.Range - 100) && SpellDamage(enemy, SpellSlot.Q) >= enemy.Health + 20);
                    if (bye != default(AIHeroClient)) { Q.HitChanceCast(bye, 70); return; }
                }

                if (R.IsReady())
                {
                    var bye = EntityManager.Heroes.Enemies.FirstOrDefault(enemy => enemy.IsValidTarget(R.Range) && SpellDamage(enemy, SpellSlot.R) >= enemy.Health + 20 && enemy.IsValidTarget(R.Range));
                    if (bye != default(AIHeroClient)) { R.Cast(); return; }
                }

                return;
            }

            //---------------------------------------------Immobile()-----------------------------------------------

            public static void Immobile()
            {
                if (Q.IsReady() && Menu["AutoQImmobile"].Cast<CheckBox>().CurrentValue)
                {
                    var QImmobile = EntityManager.Heroes.Enemies.FirstOrDefault(it => it.IsValidTarget(Q.Range) && !CanMove(it));
                    if (QImmobile != null) { Q.HitChanceCast(QImmobile, 50); return; }
                }

                if (W.IsReady() && Menu["AutoWImmobile"].Cast<CheckBox>().CurrentValue)
                {
                    var WImmobile = EntityManager.Heroes.Enemies.FirstOrDefault(it => it.IsValidTarget(W.Range) && !CanMove(it));
                    if (WImmobile != null) { W.HitChanceCast(WImmobile, 50); return; }
                }

                return;
            }

            //----------------------------------------------SaveAlly-----------------------------------------------

            public static void SaveAlly()
            {
                var Ally = EntityManager.Heroes.Allies.FirstOrDefault( ally => EntityManager.Heroes.Enemies.Any( enemy => ally.IsFacing(enemy) ) && ally.HealthPercent <= 30 && Player.Distance(ally) <= 750 );

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

        }

        //-----------HitChanceCast(this Spell.Skillshot spell, AIHeroClient target, float hitchance)--------------

        static void HitChanceCast(this Spell.Skillshot spell, Obj_AI_Base target, float hitchance)
        {
            var Pred = spell.GetPrediction(target);

            if (Pred.HitChancePercent >= hitchance && !Pred.CollisionObjects.Any()) spell.Cast(Pred.CastPosition);

            return;
        }
        
        //-----------------------------------CanMove(AIHeroClient target)------------------------------------------

        static bool CanMove(Obj_AI_Base target)
        {
            if (target.HasBuffOfType(BuffType.Charm) || target.HasBuffOfType(BuffType.Fear) || target.HasBuffOfType(BuffType.Knockback) ||
                target.HasBuffOfType(BuffType.Knockup) || target.HasBuffOfType(BuffType.Sleep) || target.HasBuffOfType(BuffType.Snare) ||
                target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Suppression) || target.HasBuffOfType(BuffType.Taunt)) return false;

            return true;
        }

        //--------------------------------------CountAlliesInRange(int range)--------------------------------------

        static int CountAlliesInRange(int range)
        {
            int allies = EntityManager.Heroes.Allies.Where(it => !it.IsMe && !it.IsDead && it.Distance(Player) <= range).Count();
            return allies;
        }

        //----------------------------SpellDamage(Obj_AI_Base target, SpellSlot slot)------------------------------

        static float SpellDamage(Obj_AI_Base target, SpellSlot slot)
        {
            switch (slot)
            {
                case SpellSlot.Q:
                    return Damage.CalculateDamageOnUnit(Player, target, DamageType.Magical, new float[] { 80, 135, 190, 245, 300 }[Q.Level - 1] + 0.9f * Player.TotalMagicalDamage)-30;
                case SpellSlot.W:
                    return Damage.CalculateDamageOnUnit(Player, target, DamageType.Magical, new float[] { 80, 160, 240, 320, 400 }[W.Level - 1] + 1.1f * Player.TotalMagicalDamage)*3/5;
                case SpellSlot.R:
                    return Damage.CalculateDamageOnUnit(Player, target, DamageType.Magical, new float[] { 300, 450, 600 }[R.Level - 1] + 1.4f * Player.TotalMagicalDamage)-30;
                default:
                    return 0;
            }
        }

        //-------------------------------------------GetComboDamage()----------------------------------------------

        static float GetComboDamage()
        {
            if (Target != null)
            {
                float ComboDamage = new float();

                ComboDamage = Q.IsReady() ? SpellDamage(Target, SpellSlot.Q) : 0;
                ComboDamage += W.IsReady() ? SpellDamage(Target, SpellSlot.W) : 0;
                ComboDamage += R.IsReady() ? SpellDamage(Target, SpellSlot.R) : 0;
                ComboDamage += Player.GetAutoAttackDamage(Target) * 2;

                if (Ignite != null) ComboDamage += Convert.ToSingle(Ignite.IsReady() ? DamageLibrary.GetSummonerSpellDamage(Player, Target, DamageLibrary.SummonerSpells.Ignite) : 0);

                return ComboDamage;
            }
            return 0;
        }

        //--------------------------------------------SearchVersion()----------------------------------------------

        static void SearchVersion()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    string Text = new WebClient().DownloadString("https://raw.githubusercontent.com/WujuSan/EloBuddy/master/Wu" + CN + "/Wu" + CN  + "/Properties/AssemblyInfo.cs");

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
