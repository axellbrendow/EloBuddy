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
using SharpDX;
using Color = System.Drawing.Color;
using Version = System.Version;
using System.Net;
using System.Text.RegularExpressions;

namespace WuYi
{
    static class Program
    {
        static Version AssVersion;//Kappa
        static readonly String CN = "MasterYi";
        static Spell.Targeted Smite = null;
        static Spell.Targeted Ignite = null;
        static AIHeroClient Player { get { return ObjectManager.Player; } }
        static ColorBGRA Green = new ColorBGRA(Color.Green.R, Color.Green.G, Color.Green.B, Color.Green.A);
        static ColorBGRA Red = new ColorBGRA(Color.Red.R, Color.Red.G, Color.Red.B, Color.Red.A);

        static Item BOTRK, Hextech, GhostBlade, Tiamat, Hydra, Bilgewater, Randuin, Scimitar, QSS;
        static Menu Menu;
        static Menu EOMenu;
        static AIHeroClient Target = null;
        static List<string> MenuSpells = new List<string>();
        static List<string> DodgeSpells = new List<string>() { "SorakaQ", "SorakaE", "TahmKenchW", "TahmKenchQ", "Bushwhack", "ForcePulse", "KarthusFallenOne", "KarthusWallOfPain", "KarthusLayWasteA1", "KarmaWMantra", "KarmaQMissileMantra", "KarmaSpiritBind", "KarmaQ", "JinxW", "JinxE", "JarvanIVGoldenAegis", "HowlingGaleSpell", "SowTheWind", "ReapTheWhirlwind", "IllaoiE", "HeimerdingerUltWDummySpell", "HeimerdingerUltEDummySpell", "HeimerdingerW", "HeimerdingerE", "HecarimUlt", "HecarimRampAttack", "GravesQLineSpell", "GravesQLineMis", "GravesClusterShot", "GravesSmokeGrenade", "GangplankR", "GalioIdolOfDurand", "GalioResoluteSmite", "FioraE", "EvelynnR", "EliseHumanE", "EkkoR", "EkkoW", "EkkoQ", "DravenDoubleShot", "InfectedCleaverMissileCast", "DariusExecute", "DariusAxeGrabCone", "DariusNoxianTacticsONH", "DariusCleave", "PhosphorusBomb", "MissileBarrage", "BraumQ", "BrandFissure", "BardR", "BardQ", "AatroxQ", "AatroxE", "AzirE", "AzirEWrapper", "AzirQWrapper", "AzirQ", "AzirR", "Pulverize", "AhriSeduce", "CurseoftheSadMummy", "InfernalGuardian", "Incinerate", "Volley", "EnchantedCrystalArrow", "BraumRWrapper", "CassiopeiaPetrifyingGaze", "FeralScream", "Rupture", "EzrealEssenceFlux", "EzrealMysticShot", "EzrealTrueshotBarrage", "FizzMarinerDoom", "GnarW", "GnarBigQMissile", "GnarQ", "GnarR", "GragasQ", "GragasE", "GragasR", "RiftWalk", "LeblancSlideM", "LeblancSlide", "LeonaSolarFlare", "UFSlash", "LuxMaliceCannon", "LuxLightStrikeKugel", "LuxLightBinding", "yasuoq3w", "VelkozE", "VeigarEventHorizon", "VeigarDarkMatter", "VarusR", "ThreshQ", "ThreshE", "ThreshRPenta", "SonaQ", "SonaR", "ShenShadowDash", "SejuaniGlacialPrisonCast", "RivenMartyr", "JavelinToss", "NautilusSplashZone", "NautilusAnchorDrag", "NamiR", "NamiQ", "DarkBindingMissile", "StaticField", "RocketGrab", "RocketGrabMissile", "timebombenemybuff", "karthusfallenonetarget", "NocturneUnspeakableHorror", "SyndraQ", "SyndraE", "SyndraR", "VayneCondemn", "Dazzle", "Overload", "AbsoluteZero", "IceBlast", "LeblancChaosOrb", "JudicatorReckoning", "KatarinaQ", "NullLance", "Crowstorm", "FiddlesticksDarkWind", "BrandWildfire", "Disintegrate", "FlashFrost", "Frostbite", "AkaliMota", "InfiniteDuress", "PantheonW", "blindingdart", "JayceToTheSkies", "IreliaEquilibriumStrike", "maokaiunstablegrowth", "nautilusgandline", "runeprison", "WildCards", "BlueCardAttack", "RedCardAttack", "GoldCardAttack", "AkaliShadowDance", "Headbutt", "PowerFist", "BrandConflagration", "CaitlynYordleTrap", "CaitlynAceintheHole", "CassiopeiaNoxiousBlast", "CassiopeiaMiasma", "CassiopeiaTwinFang", "Feast", "DianaArc", "DianaTeleport", "EliseHumanQ", "EvelynnE", "Terrify", "FizzPiercingStrike", "Parley", "GarenQAttack", "GarenR", "IreliaGatotsu", "IreliaEquilibriumStrike", "SowTheWind", "JarvanIVCataclysm", "JaxLeapStrike", "JaxEmpowerTwo", "JaxCounterStrike", "JayceThunderingBlow", "KarmaSpiritBind", "NetherBlade", "KatarinaR", "JudicatorRighteousFury", "KennenBringTheLight", "LeblancChaosOrbM", "BlindMonkRKick", "LeonaZenithBlade", "LeonaShieldOfDaybreak", "LissandraW", "LissandraQ", "LissandraR", "LuluQ", "LuluW", "LuluE", "LuluR", "SeismicShard", "AlZaharMaleficVisions", "AlZaharNetherGrasp", "MaokaiUnstableGrowth", "MordekaiserMaceOfSpades", "MordekaiserChildrenOfTheGrave", "SoulShackles", "NamiW", "NasusW", "NautilusGrandLine", "Takedown", "NocturneParanoia", "PoppyDevastatingBlow", "PoppyHeroicCharge", "QuinnE", "PuncturingTaunt", "RenektonPreExecute", "SpellFlux", "SejuaniWintersClaw", "TwoShivPoisen", "Fling", "SkarnerImpale", "SonaHymnofValor", "SwainTorment", "SwainDecrepify", "BlindingDart", "OrianaIzunaCommand", "OrianaDetonateCommand", "DetonatingShot", "BusterShot", "TrundleTrollSmash", "TrundlePain", "MockingShout", "Expunge", "UdyrBearStance", "UrgotHeatseekingLineMissile", "UrgotSwap2", "VeigarBalefulStrike", "VeigarPrimordialBurst", "ViR", "ViktorPowerTransfer", "VladimirTransfusion", "VolibearQ", "HungeringStrike", "XenZhaoComboTarget", "XenZhaoSweep", "YasuoQ3W", "YasuoQ3Mis", "YasuoQ3", "YasuoRKnockUpComboW" };
        static readonly Spell.Targeted Q = new Spell.Targeted(SpellSlot.Q, 600);
        static readonly Spell.Active W = new Spell.Active(SpellSlot.W);
        static readonly Spell.Active E = new Spell.Active(SpellSlot.E);
        static readonly Spell.Active R = new Spell.Active(SpellSlot.R);

        static void Main(string[] args) { Loading.OnLoadingComplete += OnLoadingComplete; }

        //---------------------------------------------OnLoadingComplete----------------------------------------

        static void OnLoadingComplete(EventArgs args)
        {
            if (Player.BaseSkinName != CN) { Chat.Print("Sorry, you didn't pick " + CN + ", addon disabled"); return; }

            AssVersion = Assembly.GetExecutingAssembly().GetName().Version;
            SearchVersion();

            //-------------------------------------------------Items--------------------------------------------------

            BOTRK = new Item(3153, 550);
            Hextech = new Item(3146, 700);
            Bilgewater = new Item(3144, 550);
            Hydra = new Item(3074, 400);
            Tiamat = new Item(3077, 400);
            GhostBlade = new Item(3142);
            Randuin = new Item(3143, 500);
            Scimitar = new Item(3139);
            QSS = new Item(3140);

            //-------------------------------------------------Smite--------------------------------------------------

            SpellDataInst smite = Player.Spellbook.Spells.Where(spell => spell.Name.Contains("smite")).Any() ? Player.Spellbook.Spells.Where(spell => spell.Name.Contains("smite")).First() : null;
            if (smite != null)
            {
                Smite = new Spell.Targeted(smite.Slot, 500);
            }

            //-------------------------------------------------Ignite--------------------------------------------------

            SpellDataInst dot = Player.Spellbook.Spells.Where(spell => spell.Name.Contains("dot")).Any() ? Player.Spellbook.Spells.Where(spell => spell.Name.Contains("dot")).First() : null;
            if (dot != null)
            {
                Ignite = new Spell.Targeted(dot.Slot, 600);
            }

            //---------------------------||   Menu   ||----------------------------

            Menu = MainMenu.AddMenu("Wu" + CN, "Wu" + CN);

            //------------------------------Combo-------------------------------

            Menu.AddGroupLabel("Combo");
            {
                Menu.Add("UseQCombo", new CheckBox("Use Q Combo"));
                Menu.Add("SmartQ", new CheckBox("SmartQ"));
                Menu.Add("SaveQDodge", new CheckBox("Save Q To Dodge Spells"));
                Menu.Add("UseWAARCombo", new CheckBox("Use W Auto Attack Reset"));
                Menu.Add("UseECombo", new CheckBox("Use E Combo"));
                Menu.Add("UseRCombo", new CheckBox("Use R Combo"));
            }
            Menu.AddSeparator();

            //------------------------------Harass-------------------------------

            Menu.AddGroupLabel("Harass");
            {
                Menu.Add("UseQHarass", new CheckBox("Use Q Harass"));
                Menu.Add("UseWAARHarass", new CheckBox("Use W Auto Attack Reset", false));
                Menu.Add("UseEHarass", new CheckBox("Use E Harass"));
                Menu.Add("Harass, Mana %", new Slider("Harass, Mana %", 30, 1, 100));
            }
            Menu.AddSeparator();

            //------------------------------LaneClear-------------------------------

            Menu.AddGroupLabel("LaneClear");
            {
                Menu.Add("UseQLaneClear", new CheckBox("Use Q LaneClear"));
                Menu.Add("JustQIMWD", new CheckBox("Just Q if minion will die"));
                Menu.Add("Min Minions Q", new Slider("Min Minions Q", 3, 1, 4));
                Menu.Add("LaneClear, Mana %", new Slider("LaneClear, Mana %", 30, 1, 100));
            }
            Menu.AddSeparator();

            //------------------------------Smite Usage-------------------------------

            Menu.AddGroupLabel("Smite Usage");
            {
                Menu.Add("Use Smite?", new CheckBox("Use Smite?"));
                Menu.AddSeparator();
                Menu.Add("Red?", new CheckBox("Red?"));
                Menu.Add("Blue?", new CheckBox("Blue?"));
                Menu.Add("Wolf?", new CheckBox("Wolf?"));
                Menu.Add("Gromp?", new CheckBox("Gromp?"));
                Menu.Add("Raptor?", new CheckBox("Raptor?"));
                Menu.Add("Krug?", new CheckBox("Krug?"));
            }
            Menu.AddSeparator();

            //------------------------------Drawings-------------------------------

            Menu.AddGroupLabel("Drawings");
            {
                Menu.Add("DrawQ", new CheckBox("Draw Q"));
                if (Smite != null) Menu.Add("DrawSmite", new CheckBox("DrawSmite"));
                Menu.Add("ComboDamage on HPBar", new CheckBox("ComboDamage on HPBar"));
            }
            Menu.AddSeparator();

            //------------------------------Other things-------------------------------

            Menu.AddGroupLabel("Other things");

            Menu.Add("KS", new CheckBox("KS"));
            Menu.Add("Gapcloser", new CheckBox("Gapcloser"));
            Menu.Add("Auto Ignite", new CheckBox("Auto Ignite"));

            Menu.AddSeparator();

            //---------------------------------------||   EOMenu   ||------------------------------------------

            EOMenu = Menu.AddSubMenu("Q/W Evade Options", "Q/W Evade Options");
            EOMenu.AddGroupLabel("0 = Don't evade / 1 = Q Evade / 2 = W Evade / 3 = Q/W Evade");
            EOMenu.AddSeparator();

            foreach (AIHeroClient hero in EntityManager.Heroes.Enemies)
            {
                EOMenu.AddGroupLabel(hero.BaseSkinName);
                {
                    foreach (SpellDataInst spell in hero.Spellbook.Spells)
                    {
                        if (DodgeSpells.Any(el => el == spell.SData.Name))
                        {
                            EOMenu.Add(spell.Name, new Slider(hero.BaseSkinName + " : " + spell.Slot.ToString() + " : " + spell.Name, 3, 0, 3));
                            MenuSpells.Add(spell.Name);
                        }
                    }
                }

                EOMenu.AddSeparator();
            }
            
            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnEndScene += Drawing_OnEndScene;
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
            Orbwalker.OnPostAttack += Orbwalker_OnPostAttack;
            AIHeroClient.OnProcessSpellCast += AIHeroClient_OnProcessSpellCast;

            Chat.Print("WuYi Loaded, [By WujuSan] , Version: " + AssVersion);
        }

        //--------------------------------------Orbwalker_OnPostAttack()------------------------------------------

        static void Orbwalker_OnPostAttack(AttackableUnit target, EventArgs args)
        {
            if (target != null)
            {
                if (W.IsReady() && Player.Distance(target) <= Player.GetAutoAttackRange() - 30)
                {
                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && Menu["UseWAARCombo"].Cast<CheckBox>().CurrentValue)
                    {
                        W.Cast();
                        Orbwalker.ResetAutoAttack();
                        EloBuddy.Player.IssueOrder(GameObjectOrder.AttackTo, target);
                        return;
                    }

                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass) && Menu["UseWAARHarass"].Cast<CheckBox>().CurrentValue)
                    {
                        W.Cast();
                        Orbwalker.ResetAutoAttack();
                        EloBuddy.Player.IssueOrder(GameObjectOrder.AttackTo, target);
                        return;
                    }
                }
            }
            
            return;
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
                        float FutureDamage = GetComboDamage(Target) > Target.Health ? -1 : GetComboDamage(Target) / Target.MaxHealth;

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
                        return;
                    }
                }

            }

            return;
        }

        //----------------------------------------------WaitAndBleed()----------------------------------------

        static void WaitAndBleed()
        {
            Obj_AI_Minion minion = ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(it => it.IsValidTarget(Q.Range) && !it.IsDead);
            if (minion != default(Obj_AI_Minion)) { Q.Cast(minion); }
            return;
        }

        //-------------------------------------AIHeroClient_OnProcessSpellCast--------------------------------------

        static void AIHeroClient_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsValidTarget() && sender.IsEnemy && MenuSpells.Any(el => el == args.SData.Name || el == args.SData.AlternateName) && Player.Distance(sender) <= args.SData.CastRange)
            {
                if (Q.IsReady() && (EOMenu[args.SData.Name].Cast<Slider>().CurrentValue == 1 || EOMenu[args.SData.Name].Cast<Slider>().CurrentValue == 3 || EOMenu[args.SData.AlternateName].Cast<Slider>().CurrentValue == 1 || EOMenu[args.SData.AlternateName].Cast<Slider>().CurrentValue == 3))
                {
                    if (args.SData.Name == "JaxCounterStrike") { Core.DelayAction(() => Q.Cast(sender), 2000 - Game.Ping - 100); return; }

                    if (args.SData.Name == "karthusfallenonetarget")
                    { Core.DelayAction(() => Q.Cast(Target), 250); Core.DelayAction(() => WaitAndBleed(), 250); return; }

                    else if (args.SData.Name == "ZedR")
                    {
                        var otherchamp = ObjectManager.Get<AIHeroClient>().LastOrDefault(it => it.IsValidTarget(Q.Range));

                        Core.DelayAction(() => WaitAndBleed(), 400);
                        if (otherchamp != null) Core.DelayAction(() => Q.Cast(otherchamp), 400);
                        return;
                    }

                    else if (args.SData.Name == "SoulShackles")
                    {
                        Core.DelayAction(delegate
                        {
                            if (Target != null) { Q.Cast(Target); return; }

                            var champ = EntityManager.Heroes.Enemies.FirstOrDefault(it => it.IsValidTarget(Q.Range));

                            if (champ != null) { Q.Cast(champ); return; }

                            var minion = EntityManager.MinionsAndMonsters.EnemyMinions.FirstOrDefault(it => it.IsValidTarget(Q.Range));

                            if (minion != null) { Q.Cast(minion); return; }

                            return;
                        }, 3000 - Game.Ping - 100);
                    }

                    else if (args.SData.Name == "AbsoluteZero")
                    {
                        Core.DelayAction(delegate
                        {
                            if (Player.Distance(args.Start) <= args.SData.CastRange - 10)
                            {
                                if (Target != null) { Q.Cast(Target); return; }

                                var champ = EntityManager.Heroes.Enemies.FirstOrDefault(it => it.IsValidTarget(Q.Range));

                                if (champ != null) { Q.Cast(champ); return; }

                                var minion = EntityManager.MinionsAndMonsters.EnemyMinions.FirstOrDefault(it => it.IsValidTarget(Q.Range));

                                if (minion != null) { Q.Cast(minion); return; }
                            }

                            return;
                        }, 3000 - Game.Ping - 100);
                    }

                    else if (sender.IsValidTarget(Q.Range))
                    {
                        Core.DelayAction( () => Q.Cast(Target), (int)args.SData.SpellCastTime - Game.Ping - 100 );
                        return;
                    }
                    return;
                }
                
                else if (W.IsReady() && Player.IsFacing(sender) && EOMenu[args.SData.Name].Cast<Slider>().CurrentValue > 1 && ( args.Target.IsMe || new Geometry.Polygon.Rectangle(args.Start, args.End, args.SData.LineWidth).IsInside(Player) || new Geometry.Polygon.Circle(args.End, args.SData.CastRadius).IsInside(Player)) )
                {
                    int delay = (int)(Player.Distance(sender) / ((args.SData.MissileMaxSpeed + args.SData.MissileMinSpeed) / 2) * 1000) - 150 + (int)args.SData.SpellCastTime;

                    if (args.SData.Name != "ZedR")
                    {
                        Core.DelayAction(() => W.Cast(), delay);
                        Core.DelayAction(() => EloBuddy.Player.IssueOrder(GameObjectOrder.AttackTo, Target), delay);
                    }
                    return;
                }
            }
            return;
        }

        //----------------------------------------------Gapcloser_OnGapcloser----------------------------------------

        static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (sender.IsEnemy && Q.IsReady() && sender.IsValidTarget(Q.Range))
            {
                Q.Cast(sender);
            }
            return;
        }

        //----------------------------------------------Drawing_OnDraw----------------------------------------
        
        static void Drawing_OnDraw(EventArgs args)
        {
            if (!Player.IsDead)
            {
                if (Menu["DrawQ"].Cast<CheckBox>().CurrentValue)
                    Circle.Draw(Q.IsReady() ? Green : Red, Q.Range, Player.Position);

                if (Smite != null)
                    if (Menu["DrawSmite"].Cast<CheckBox>().CurrentValue)
                        Circle.Draw(Smite.IsReady() ? Green : Red, Smite.Range, Player.Position);

            }

            return;
        }

        //-------------------------------------------Game_OnTick----------------------------------------------

        static void Game_OnTick(EventArgs args)
        {
            if (Player.IsDead) return;

            Target = TargetSelector.GetTarget(900, DamageType.Physical);

            //---------------------------------------------Smite Usage---------------------------------------------

            if (Smite != null)
            {
                if (Smite.IsReady() && Menu["Use Smite?"].Cast<CheckBox>().CurrentValue)
                {
                    Obj_AI_Minion Mob = EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Position, Smite.Range).FirstOrDefault();

                    if (Mob != default(Obj_AI_Minion))
                    {
                        bool kill = GetSmiteDamage() >= Mob.Health;

                        if (kill)
                        {
                            if ((Mob.Name.Contains("SRU_Dragon") || Mob.Name.Contains("SRU_Baron"))) Smite.Cast(Mob);
                            else if (Mob.Name.StartsWith("SRU_Red") && Menu["Red?"].Cast<CheckBox>().CurrentValue) Smite.Cast(Mob);
                            else if (Mob.Name.StartsWith("SRU_Blue") && Menu["Blue?"].Cast<CheckBox>().CurrentValue) Smite.Cast(Mob);
                            else if (Mob.Name.StartsWith("SRU_Murkwolf") && Menu["Wolf?"].Cast<CheckBox>().CurrentValue) Smite.Cast(Mob);
                            else if (Mob.Name.StartsWith("SRU_Krug") && Menu["Krug?"].Cast<CheckBox>().CurrentValue) Smite.Cast(Mob);
                            else if (Mob.Name.StartsWith("SRU_Gromp") && Menu["Gromp?"].Cast<CheckBox>().CurrentValue) Smite.Cast(Mob);
                            else if (Mob.Name.StartsWith("SRU_Razorbeak") && Menu["Raptor?"].Cast<CheckBox>().CurrentValue) Smite.Cast(Mob);
                        }
                    }
                }
            }

            //------------------------------------------------KS------------------------------------------------

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

            //-----------------------------------------------LaneClear---------------------------------------------

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) && Q.IsReady() && Player.ManaPercent >= Menu["LaneClear, Mana %"].Cast<Slider>().CurrentValue) Modes.LaneClear();

            return;
        }

        //---------------------------------------------class Modes-------------------------------------------------

        class Modes
        {
            static bool QIsReady;
            static bool EIsReady;

            static bool AARange;
            static bool QRange;

            //--------------------------------------------UpdateVariables()----------------------------------------

            public static void UpdateVariables()
            {
                QIsReady = Q.IsReady();
                EIsReady = E.IsReady();

                AARange = Player.IsInAutoAttackRange(Target);
                QRange = Q.IsInRange(Target);

                return;
            }

            //----------------------------------------------Combo()------------------------------------------------

            public static void Combo()
            {
                if ((Scimitar.IsReady() || QSS.IsReady()) && Player.HasBuffOfType(BuffType.Charm) || Player.HasBuffOfType(BuffType.Blind) || Player.HasBuffOfType(BuffType.Fear) || Player.HasBuffOfType(BuffType.Polymorph) || Player.HasBuffOfType(BuffType.Silence) || Player.HasBuffOfType(BuffType.Sleep) || Player.HasBuffOfType(BuffType.Snare) || Player.HasBuffOfType(BuffType.Stun) || Player.HasBuffOfType(BuffType.Suppression) || Player.HasBuffOfType(BuffType.Taunt)) { Scimitar.Cast(); QSS.Cast(); }

                if (Menu["UseQCombo"].Cast<CheckBox>().CurrentValue && QIsReady && QRange)
                {
                    if (Menu["SmartQ"].Cast<CheckBox>().CurrentValue) { QLogic(); }
                    else if (Menu["SaveQDodge"].Cast<CheckBox>().CurrentValue) { }
                    else { Q.Cast(Target); }
                }

                if (Smite != null)
                {
                    if (Target.IsValidTarget(Smite.Range) && Smite.IsReady())
                    {
                        if (Smite.Name.Contains("gank")) Smite.Cast(Target);
                        else if (Smite.Name.Contains("duel") && AARange) Smite.Cast(Target);
                    }
                }

                if (Menu["UseRCombo"].Cast<CheckBox>().CurrentValue && R.IsReady() && Player.Distance(Target) <= Player.GetAutoAttackRange(Target) + 300) { R.Cast(); }

                if (Menu["UseECombo"].Cast<CheckBox>().CurrentValue && EIsReady && AARange) E.Cast();

                if (QRange && GhostBlade.IsReady()) GhostBlade.Cast();

                if (Target.IsValidTarget(550) && BOTRK.IsReady()) BOTRK.Cast(Target);

                if (Target.IsValidTarget(550) && Bilgewater.IsReady()) Bilgewater.Cast(Target);

                if (Target.IsValidTarget(400) && Tiamat.IsReady()) Tiamat.Cast();

                if (Target.IsValidTarget(400) && Hydra.IsReady()) Hydra.Cast();

                if (Target.IsValidTarget(500) && Randuin.IsReady()) Randuin.Cast();

                if (Target.IsValidTarget(700) && Hextech.IsReady()) Hextech.Cast(Target);

                return;
            }

            //---------------------------------------------Harass()------------------------------------------------

            public static void Harass()
            {
                if (Menu["UseQHarass"].Cast<CheckBox>().CurrentValue && QIsReady && QRange) Q.Cast(Target);

                if (Menu["UseEHarass"].Cast<CheckBox>().CurrentValue && EIsReady && AARange) E.Cast();

                return;
            }

            //-------------------------------------------LaneClear()-----------------------------------------------

            public static void LaneClear()
            {
                IEnumerable<Obj_AI_Minion> ListMinions = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Player.ServerPosition, 1000).OrderBy(minion => minion.Distance(Player));
                int hits = new int();

                if (ListMinions.Any())
                {
                    if (!(ListMinions.First().Distance(Player) > Q.Range))
                    {
                        hits += 1;

                        for (int i = 0; i < ListMinions.Count(); i++)
                        {
                            if (i + 1 == ListMinions.Count()) break;
                            else if (ListMinions.ElementAt(i).Distance(ListMinions.ElementAt(i + 1)) <= 200) { hits += 1; }
                            else break;
                        }

                        if (Menu["UseQLaneClear"].Cast<CheckBox>().CurrentValue && hits >= Menu["Min Minions Q"].Cast<Slider>().CurrentValue)
                        {
                            if (Menu["JustQIMWD"].Cast<CheckBox>().CurrentValue)
                            {
                                if ((SpellDamage(ListMinions.First(), SpellSlot.Q) > ListMinions.First().Health || SpellDamage(ListMinions.ElementAt(1), SpellSlot.Q) > ListMinions.ElementAt(1).Health)) Q.Cast(ListMinions.First());
                            }
                            else { Q.Cast(ListMinions.First()); }
                        }
                    }
                }

                if (Tiamat.IsReady())
                {
                    bool UseItem = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Player.Position, Hydra.Range).Count() >= 3;
                    if (UseItem) Tiamat.Cast();
                    UseItem = EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Position, Hydra.Range).Count() >= 2;
                    if (UseItem) Tiamat.Cast();
                }

                if (Hydra.IsReady())
                {
                    bool UseItem = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Player.Position, Hydra.Range).Count() >= 3;
                    if (UseItem) Hydra.Cast();
                    UseItem = EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Position, Hydra.Range).Count() >= 2;
                    if (UseItem) Hydra.Cast();
                }

                return;
            }

            //-------------------------------------------------KS--------------------------------------------------

            public static void KS()
            {
                if (Q.IsReady())
                {
                    var bye = EntityManager.Heroes.Enemies.FirstOrDefault(enemy => enemy.IsValidTarget(Q.Range) && SpellDamage(enemy, SpellSlot.Q) >= enemy.Health);
                    if (bye != null) { Q.Cast(bye); return; }
                }

                if (Smite != null)
                {
                    if (Smite.Name.Contains("gank") && Smite.IsReady())
                    {
                        var bye = EntityManager.Heroes.Enemies.FirstOrDefault(enemy => enemy.IsValidTarget(Smite.Range) && DamageLibrary.GetSummonerSpellDamage(Player, enemy, DamageLibrary.SummonerSpells.Smite) >= enemy.Health);
                        if (bye != null) { Smite.Cast(bye); return; }
                    }
                }
            }

        }

        //---------------------------------------------QLogic()-------------------------------------------------

        static void QLogic()
        {
            if (Target.IsDashing()) Q.Cast(Target);
            if (Target.HealthPercent <= 30) Q.Cast(Target);
            if (Player.HealthPercent <= 30) Q.Cast(Target);
            if (SpellDamage(Target, SpellSlot.Q) >= Target.Health) Q.Cast(Target);
        }

        //-------------------------------------------SpellDamage()----------------------------------------------

        static float SpellDamage(Obj_AI_Base target, SpellSlot slot)
        {
            switch (slot)
            {
                case SpellSlot.Q:
                    return Damage.CalculateDamageOnUnit(Player, target, DamageType.Physical, new float[] { 25, 60, 95, 130, 165 }[Q.Level-1] + Player.TotalAttackDamage, true, true);

                case SpellSlot.E:
                    return Damage.CalculateDamageOnUnit(Player, target, DamageType.True, new float[] { 10, 15, 20, 25, 30 }[E.Level-1] + new float[] { 0.1f, 0.125f, 0.15f, 0.175f, 0.2f }[E.Level - 1] * Player.TotalAttackDamage, true, true);

                default:
                    return 0;
            }
        }

        //---------------------------------GetComboDamage(Obj_AI_Hero Target)-----------------------------------

        static float GetComboDamage(AIHeroClient Target)
        {
            if (Target != null)
            {
                float ComboDamage = new float();

                ComboDamage = Q.IsReady() ? SpellDamage(Target, SpellSlot.Q) : 0;
                ComboDamage = W.IsReady() ? Player.GetAutoAttackDamage(Target) : 0;
                ComboDamage += E.IsReady() ? SpellDamage(Target, SpellSlot.E)*3 : 0;
                ComboDamage += Player.GetAutoAttackDamage(Target) * 2;
                ComboDamage += Bilgewater.IsReady() ? DamageLibrary.GetItemDamage(Player, Target, ItemId.Bilgewater_Cutlass) : 0;
                ComboDamage += BOTRK.IsReady() ? DamageLibrary.GetItemDamage(Player, Target, ItemId.Blade_of_the_Ruined_King) : 0;
                ComboDamage += Hydra.IsReady() ? DamageLibrary.GetItemDamage(Player, Target, ItemId.Ravenous_Hydra_Melee_Only) : 0;
                ComboDamage += Hextech.IsReady() ? DamageLibrary.GetItemDamage(Player, Target, ItemId.Hextech_Gunblade) : 0;

                if (Ignite != null) ComboDamage += Convert.ToSingle( Ignite.IsReady() ? DamageLibrary.GetSummonerSpellDamage(Player, Target, DamageLibrary.SummonerSpells.Ignite) : 0);
                if (Smite != null) ComboDamage += Convert.ToSingle( Smite.IsReady() && Smite.Name.Contains("gank") ? DamageLibrary.GetSummonerSpellDamage(Player, Target, DamageLibrary.SummonerSpells.Smite) : 0);

                return ComboDamage;
            }
            return 0;
        }
        
        //------------------------------------------GetSmiteDamage()--------------------------------------------

        static float GetSmiteDamage()
        {
            float damage = new float(); //Arithmetic Progression OP :D

            if (Player.Level < 10) damage = 360 + (Player.Level - 1) * 30;

            else if (Player.Level < 15) damage = 280 + (Player.Level - 1) * 40;

            else if (Player.Level < 19) damage = 150 + (Player.Level - 1) * 50;

            return damage;
        }

        //--------------------------------------------SearchVersion()-------------------------------------------

        static void SearchVersion()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    string Text = new WebClient().DownloadString("https://raw.githubusercontent.com/WujuSan/EloBuddy/master/WuYi/WuYi/Properties/AssemblyInfo.cs");

                    var Match = new Regex(@"\[assembly\: AssemblyVersion\(""(\d{1,})\.(\d{1,})\.(\d{1,})\.(\d{1,})""\)\]").Match(Text);

                    if (Match.Success)
                    {
                        var CorrectVersion = new Version(string.Format("{0}.{1}.{2}.{3}", Match.Groups[1], Match.Groups[2], Match.Groups[3], Match.Groups[4]));

                        if (CorrectVersion > AssVersion)
                        {
                            Chat.Print("<font color='#FFFF00'>Your WuYi is </font><font color='#FF0000'>OUTDATED</font><font color='#FFFF00'>, The correct version is: " + CorrectVersion + "</font>");
                            Chat.Print("<font color='#FFFF00'>Your WuYi is </font><font color='#FF0000'>OUTDATED</font><font color='#FFFF00'>, The correct version is: " + CorrectVersion + "</font>");
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
