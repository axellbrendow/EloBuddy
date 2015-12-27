using System;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Enumerations;
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
        const String CN = "Irelia";
        static float QSpeed;
        static Spell.Targeted Smite, Ignite;
        static AIHeroClient Player = EloBuddy.Player.Instance;
        static ColorBGRA Green = new ColorBGRA(Color.Green.R, Color.Green.G, Color.Green.B, Color.Green.A);
        static ColorBGRA Red = new ColorBGRA(Color.Red.R, Color.Red.G, Color.Red.B, Color.Red.A);

        static Item Trinity, Sheen, BOTRK, Hextech, GhostBlade, Tiamat, Hydra, Bilgewater, Randuin, Scimitar, QSS;
        static Menu Menu;
        static AIHeroClient GapCloseTarget;
        static AIHeroClient Target;
        static readonly Spell.Targeted Q = new Spell.Targeted(SpellSlot.Q, 650);
        static readonly Spell.Active W = new Spell.Active(SpellSlot.W);
        static readonly Spell.Targeted E = new Spell.Targeted(SpellSlot.E, 425);
        static readonly Spell.Skillshot R = new Spell.Skillshot(SpellSlot.R, 1000, SkillShotType.Linear, 250, 1600, 120);

        static void Main(string[] args) { Loading.OnLoadingComplete += OnLoadingComplete; }

        //---------------------------------------------OnLoadingComplete----------------------------------------

        static void OnLoadingComplete(EventArgs args)
        {
            if (Player.BaseSkinName != CN) { Chat.Print("Sorry, you didn't pick " + CN + ", addon disabled"); return; }

            AssVersion = Assembly.GetExecutingAssembly().GetName().Version;
            SearchVersion();

            //-------------------------------------------------Items--------------------------------------------------

            QSpeed = Player.Spellbook.GetSpell(SpellSlot.Q).SData.MissileSpeed;

            BOTRK = new Item(3153, 550);
            Hextech = new Item(3146, 700);
            Bilgewater = new Item(3144, 550);
            Hydra = new Item(3074, 400);
            Tiamat = new Item(3077, 400);
            GhostBlade = new Item(3142);
            Randuin = new Item(3143, 500);
            Scimitar = new Item(3139);
            QSS = new Item(3140);
            Trinity = new Item(ItemId.Trinity_Force);
            Sheen = new Item(ItemId.Sheen);

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
                Menu.Add("SmartQ", new CheckBox("Use SmartQ"));
                Menu.Add("QGapCloserCombo", new CheckBox("Use Q on units to gapclose to enemy"));
                Menu.Add("UseWCombo", new CheckBox("Use W Combo"));
                Menu.Add("UseWBeforeQCombo", new CheckBox("Use W Before Q"));
                Menu.Add("UseECombo", new CheckBox("Use E Combo"));
                Menu.Add("OnlyEStunCombo", new CheckBox("Only E for stun"));
                Menu.Add("UseRCombo", new CheckBox("Use R Combo"));
                Menu.Add("UseRSelfActived", new CheckBox("Use R just if it was actived by yourself"));
                Menu.Add("UseRGapCloser", new CheckBox("Use R on units to gapclose with Q"));
            }
            Menu.AddSeparator();

            //------------------------------Harass-------------------------------

            Menu.AddGroupLabel("Harass");
            {
                Menu.Add("UseQHarass", new CheckBox("Use Q Harass"));
                Menu.Add("UseWHarass", new CheckBox("Use W Harass"));
                Menu.Add("UseWBeforeQHarass", new CheckBox("Use W Before Q"));
                Menu.Add("UseEHarass", new CheckBox("Use E Harass"));
                Menu.Add("OnlyEStunHarass", new CheckBox("Only E for stun"));
                Menu.Add("Harass, Mana %", new Slider("Harass, Mana %", 30, 1, 100));
            }
            Menu.AddSeparator();

            //------------------------------LaneClear-------------------------------

            Menu.AddGroupLabel("LaneClear");
            {
                Menu.Add("UseQLaneClear", new CheckBox("Use Q LaneClear"));
                Menu.Add("UseWLaneClear", new CheckBox("Use W LaneClear"));
                Menu.Add("LaneClear, Mana %", new Slider("LaneClear, Mana %", 30, 1, 100));
            }
            Menu.AddSeparator();

            //------------------------------LastHit-------------------------------

            Menu.AddGroupLabel("LastHit");
            {
                Menu.Add("UseQLastHit", new CheckBox("Use Q LastHit"));
                Menu.Add("LastHit, Mana %", new Slider("LastHit, Mana %", 40, 1, 100));
            }
            Menu.AddSeparator();

            //------------------------------Smite Usage-------------------------------

            if (Smite != null)
            {
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
            }

            //--------------------------------Flee---------------------------------

            Menu.AddGroupLabel("Flee");
            {
                Menu.Add("UseQFlee", new CheckBox("Use Q Flee"));
                Menu.Add("UseEFlee", new CheckBox("Use E Flee"));
                Menu.Add("UseRFlee", new CheckBox("Use R Flee"));
            }
            Menu.AddSeparator();

            //------------------------------Drawings-------------------------------

            Menu.AddGroupLabel("Drawings");
            {
                Menu.Add("DrawQ", new CheckBox("Draw Q"));
                Menu.Add("DrawE", new CheckBox("Draw E"));
                Menu.Add("DrawR", new CheckBox("Draw R"));
                if (Smite != null) Menu.Add("DrawSmite", new CheckBox("DrawSmite"));
                Menu.Add("ComboDamage on HPBar", new CheckBox("ComboDamage on HPBar"));
            }
            Menu.AddSeparator();

            //------------------------------Other things-------------------------------

            Menu.AddGroupLabel("Other things");

            Menu.Add("KS", new CheckBox("KS"));
            Menu.Add("Interrupter", new CheckBox("Interrupter"));
            Menu.Add("Auto Ignite", new CheckBox("Auto Ignite"));
            Menu.Add("EnemyGapCloser", new CheckBox("Stun/Slow on enemy gapcloser"));
            Menu.Add("s", new KeyBind("s", false, KeyBind.BindTypes.HoldActive, 'J'));

            Menu.AddSeparator();

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnEndScene += Drawing_OnEndScene;
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser1;
            Obj_AI_Turret.OnBasicAttack += Obj_AI_Turret_OnBasicAttack;
            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;

            Chat.Print("Wu" + CN + " Loaded, [By WujuSan] , Version: " + AssVersion);
        }

        //---------------------------------Obj_AI_Turret_OnBasicAttack----------------------------------------

        static void Obj_AI_Turret_OnBasicAttack(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.Distance(Player) <= 2000 && sender is Obj_AI_Turret)
            {
                if (sender.IsAlly && args.Target != null && E.IsReady())
                {
                    //Chat.Print("Ally tower shot target: {0}", args.Target.Name);

                    var target = EntityManager.Heroes.Enemies.FirstOrDefault(it => it.NetworkId == args.Target.NetworkId);

                    if (target != null && E.IsInRange(target) && Player.HealthPercent <= target.HealthPercent) E.Cast(target);

                    return;
                }
            }

            return;
        }

        //------------------------------------GapCloser_OnGapCloser1------------------------------------------

        static void Gapcloser_OnGapcloser1(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (Menu["EnemyGapCloser"].Cast<CheckBox>().CurrentValue && sender.IsEnemy && E.IsReady() && sender.IsValidTarget(E.Range)) E.Cast(sender);
            return;
        }

        //------------------------------------Interrupter_OnInterruptableSpell--------------------------------

        static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            if (sender.IsEnemy && E.IsReady() && Menu["Interrupter"].Cast<CheckBox>().CurrentValue)
            {
                if (sender.IsValidTarget(E.Range) && sender.HealthPercent >= Player.HealthPercent) { E.Cast(sender); return; }
                else if (Q.IsReady() && sender.IsValidTarget(Q.Range) && ((sender.Health - SpellDamage(sender, SpellSlot.Q)) / sender.MaxHealth) * 100 >= Player.HealthPercent)
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

        //---------------------------------------Drawing_OnEndScene-------------------------------------------

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

        //----------------------------------------------Drawing_OnDraw----------------------------------------

        static void Drawing_OnDraw(EventArgs args)
        {
            if (!Player.IsDead)
            {
                if (Menu["DrawQ"].Cast<CheckBox>().CurrentValue)
                    Circle.Draw(Q.IsReady() ? Green : Red, Q.Range, Player.Position);

                if (Menu["DrawE"].Cast<CheckBox>().CurrentValue)
                    Circle.Draw(E.IsReady() ? Green : Red, E.Range, Player.Position);

                if (Menu["DrawR"].Cast<CheckBox>().CurrentValue)
                    Circle.Draw(R.IsReady() ? Green : Red, R.Range, Player.Position);

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

            Target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            GapCloseTarget = TargetSelector.GetTarget(1200, DamageType.Physical);

            if (EntityManager.MinionsAndMonsters.CombinedAttackable.Any(it => it.IsValidTarget(Q.Range) && SpellDamage(it, SpellSlot.Q) >= it.Health))
            {
                if (TargetSelector.GetPriority(GapCloseTarget) > TargetSelector.GetPriority(Target)) Target = GapCloseTarget;
                else GapCloseTarget = Target;
            }

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

            if (Menu["KS"].Cast<CheckBox>().CurrentValue && EntityManager.Heroes.Enemies.Any(it => it.IsValidTarget(R.Range))) Modes.KS();

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

            //--------------------------------------------------Flee--------------------------------------------

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee)) Modes.Flee();

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

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) && Player.ManaPercent >= Menu["LaneClear, Mana %"].Cast<Slider>().CurrentValue) Modes.LaneClear();

            //-----------------------------------------------LastHit---------------------------------------------

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit) && Player.ManaPercent >= Menu["LastHit, Mana %"].Cast<Slider>().CurrentValue) Modes.LastHit();

            return;
        }

        //-------------------------------------------class Modes------------------------------------------------

        class Modes
        {
            static bool QIsReady;
            static bool WIsReady;
            static bool EIsReady;

            static bool ERange;
            static bool AARange;
            static bool QRange;

            //--------------------------------------------UpdateVariables()----------------------------------------

            public static void UpdateVariables()
            {
                QIsReady = Q.IsReady();
                WIsReady = W.IsReady();
                EIsReady = E.IsReady();

                AARange = Player.IsInAutoAttackRange(Target);
                QRange = Player.Distance(Target) <= 625;
                ERange = E.IsInRange(Target);

                return;
            }

            //----------------------------------------------Combo()------------------------------------------------

            public static void Combo()
            {
                if ((Scimitar.IsReady() || QSS.IsReady()) && Player.HasBuffOfType(BuffType.Charm) || Player.HasBuffOfType(BuffType.Blind) || Player.HasBuffOfType(BuffType.Fear) || Player.HasBuffOfType(BuffType.Polymorph) || Player.HasBuffOfType(BuffType.Silence) || Player.HasBuffOfType(BuffType.Sleep) || Player.HasBuffOfType(BuffType.Snare) || Player.HasBuffOfType(BuffType.Stun) || Player.HasBuffOfType(BuffType.Suppression) || Player.HasBuffOfType(BuffType.Taunt)) { Scimitar.Cast(); QSS.Cast(); }

                if (Menu["UseQCombo"].Cast<CheckBox>().CurrentValue && QIsReady)
                {
                    if (QRange)
                    {
                        if (Menu["SmartQ"].Cast<CheckBox>().CurrentValue) QLogic();
                        else if (Menu["UseWBeforeQCombo"].Cast<CheckBox>().CurrentValue && WIsReady)
                        {
                            W.Cast();
                            Core.DelayAction(() => Q.Cast(Target), Game.Ping + W.CastDelay);
                        }
                        else Q.Cast(Target);
                    }

                    else if (GapCloseTarget != null && Menu["QGapCloserCombo"].Cast<CheckBox>().CurrentValue)
                    {
                        if (Item.HasItem(ItemId.Trinity_Force))
                        {
                            var Minions = EntityManager.MinionsAndMonsters.CombinedAttackable.Where(it => it.IsValidTarget(Q.Range) && it.Distance(Game.CursorPos) <= 500 && SpellDamage(it, SpellSlot.Q) >= it.Health);

                            if (Minions.Any())
                            {
                                var Minion = Minions.OrderBy(it => it.Distance(GapCloseTarget)).First();
                                Q.Cast(Minion);
                            }

                            else if (R.IsReady() && Menu["UseRGapCloser"].Cast<CheckBox>().CurrentValue)
                            {
                                Minions = EntityManager.MinionsAndMonsters.CombinedAttackable.Where(it => it.IsValidTarget(Q.Range) && it.Distance(Game.CursorPos) <= 500 && SpellDamage(it, SpellSlot.Q) + SpellDamage(it, SpellSlot.R) >= it.Health);

                                if (Minions.Any())
                                {
                                    var Minion = Minions.OrderBy(it => it.Distance(GapCloseTarget)).First();

                                    if (R.Cast(Minion)) Core.DelayAction(() => Q.Cast(Minion), Game.Ping + R.CastDelay + 400);
                                }
                            }
                        }

                        else if (Item.HasItem(ItemId.Sheen))
                        {
                            var Minions = EntityManager.MinionsAndMonsters.CombinedAttackable.Where(it => it.IsValidTarget(Q.Range) && it.Distance(Game.CursorPos) <= 500 && SpellDamage(it, SpellSlot.Q) >= it.Health);

                            if (Minions.Any())
                            {
                                var Minion = Minions.OrderBy(it => it.Distance(GapCloseTarget)).First();
                                Q.Cast(Minion);
                            }

                            else if (R.IsReady() && Menu["UseRGapCloser"].Cast<CheckBox>().CurrentValue)
                            {
                                Minions = EntityManager.MinionsAndMonsters.CombinedAttackable.Where(it => it.IsValidTarget(Q.Range) && it.Distance(Game.CursorPos) <= 500 && SpellDamage(it, SpellSlot.Q) + SpellDamage(it, SpellSlot.R) >= it.Health);

                                if (Minions.Any())
                                {
                                    var Minion = Minions.OrderBy(it => it.Distance(GapCloseTarget)).First();

                                    if (R.Cast(Minion)) Core.DelayAction(() => Q.Cast(Minion), Game.Ping + R.CastDelay + 400);
                                }
                            }
                        }

                        else
                        {
                            var Minions = EntityManager.MinionsAndMonsters.CombinedAttackable.Where(it => it.IsValidTarget(Q.Range) && it.Distance(Game.CursorPos) <= 500 && SpellDamage(it, SpellSlot.Q) >= it.Health);

                            if (Minions.Any())
                            {
                                var Minion = Minions.OrderBy(it => it.Distance(GapCloseTarget)).First();
                                Q.Cast(Minion);
                            }
                        }
                    }
                    
                }

                if (Smite != null)
                {
                    if (Target.IsValidTarget(Smite.Range) && Smite.IsReady())
                    {
                        if (Smite.Name.Contains("gank")) Smite.Cast(Target);
                        else if (Smite.Name.Contains("duel") && AARange) Smite.Cast(Target);
                    }
                }

                if (Menu["UseWCombo"].Cast<CheckBox>().CurrentValue && WIsReady && (!QIsReady || !Menu["UseWBeforeQCombo"].Cast<CheckBox>().CurrentValue) && AARange) W.Cast();

                if (Menu["UseRCombo"].Cast<CheckBox>().CurrentValue && R.IsReady() && Player.Distance(Target) <= 900)
                {
                    var RPred = R.GetPrediction(Target);

                    if (RPred.HitChancePercent >= 75)
                    {
                        if (Menu["UseRSelfActived"].Cast<CheckBox>().CurrentValue)
                        {
                            if (Player.HasBuff("ireliatranscendentbladesspell")) R.Cast(RPred.CastPosition);
                        }
                        else R.Cast(RPred.CastPosition);
                    }
                }

                if (Menu["UseECombo"].Cast<CheckBox>().CurrentValue && EIsReady && E.IsInRange(Target))
                {
                    if (Menu["OnlyEStunCombo"].Cast<CheckBox>().CurrentValue)
                    {
                        if (Player.HealthPercent <= Target.HealthPercent) E.Cast(Target);
                    }
                    else E.Cast(Target);
                }

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
                if (Menu["UseQHarass"].Cast<CheckBox>().CurrentValue && QIsReady && QRange && Player.Distance(Target) >= Player.GetAutoAttackDamage(Target) + 200)
                {
                    if (WIsReady && Menu["UseWBeforeQHarass"].Cast<CheckBox>().CurrentValue)
                    {
                        if (W.Cast()) Core.DelayAction(() => Q.Cast(Target), Game.Ping + W.CastDelay);
                    }
                    else Q.Cast(Target);
                }

                if (Menu["UseWHarass"].Cast<CheckBox>().CurrentValue && WIsReady && (!QIsReady || !Menu["UseWBeforeQHarass"].Cast<CheckBox>().CurrentValue) && AARange) W.Cast();

                if (Menu["UseEHarass"].Cast<CheckBox>().CurrentValue && EIsReady && E.IsInRange(Target))
                {
                    if (Menu["OnlyEStunHarass"].Cast<CheckBox>().CurrentValue)
                    {
                        if (Player.HealthPercent <= Target.HealthPercent) E.Cast(Target);
                    }
                    else E.Cast(Target);
                }

                return;
            }

            //-------------------------------------------LaneClear()-----------------------------------------------

            public static void LaneClear()
            {
                if (Q.IsReady() && Menu["UseQLaneClear"].Cast<CheckBox>().CurrentValue)
                {
                    //var Turret = ObjectManager.Get<Obj_AI_Turret>().FirstOrDefault(it => !it.IsDead && it.IsEnemy && it.Distance(Player) <= 1300);
                    var Minion = EntityManager.MinionsAndMonsters.EnemyMinions.FirstOrDefault(it => it.IsValidTarget(Q.Range) && it.Distance(Player) >= Player.GetAutoAttackRange(it) && SpellDamage(it, SpellSlot.Q) >= it.Health);

                    if (Minion != null) Q.Cast(Minion);
                }

                if (W.IsReady() && Menu["UseWLaneClear"].Cast<CheckBox>().CurrentValue && EntityManager.MinionsAndMonsters.EnemyMinions.Where(it => it.IsValidTarget() && Player.IsInAutoAttackRange(it)).Count() >= 5) W.Cast();

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

            //-------------------------------------------LastHit()-----------------------------------------------

            public static void LastHit()
            {
                if (Q.IsReady() && Menu["UseQLastHit"].Cast<CheckBox>().CurrentValue)
                {
                    //var Turret = ObjectManager.Get<Obj_AI_Turret>().FirstOrDefault(it => !it.IsDead && it.IsEnemy && it.Distance(Player) <= 1300);
                    var Minion = EntityManager.MinionsAndMonsters.EnemyMinions.FirstOrDefault(it => it.IsValidTarget(Q.Range) && it.Distance(Player) >= Player.GetAutoAttackRange(it) && SpellDamage(it, SpellSlot.Q) >= it.Health);

                    if (Minion != null) Q.Cast(Minion);
                }

                return;
            }

            //-------------------------------------------------KS--------------------------------------------------

            public static void KS()
            {
                if (Q.IsReady() && E.IsReady())
                {
                    var bye = EntityManager.Heroes.Enemies.FirstOrDefault(enemy => enemy.IsValidTarget(Q.Range) && SpellDamage(enemy, SpellSlot.Q) + SpellDamage(enemy, SpellSlot.E) >= enemy.Health);
                    if (bye != null)
                    {
                        if (Q.Cast(bye))
                        {
                            var delay = (int)(Game.Ping + Q.CastDelay + (Player.Distance(bye) / QSpeed * 1000));

                            Core.DelayAction(() => E.Cast(bye), delay);

                            return;
                        }
                    }
                }

                if (Q.IsReady())
                {
                    var bye = EntityManager.Heroes.Enemies.FirstOrDefault(enemy => enemy.IsValidTarget(Q.Range) && SpellDamage(enemy, SpellSlot.Q) >= enemy.Health);
                    if (bye != null) { Q.Cast(bye); return; }
                }

                if (E.IsReady())
                {
                    var bye = EntityManager.Heroes.Enemies.FirstOrDefault(enemy => enemy.IsValidTarget(E.Range) && SpellDamage(enemy, SpellSlot.E) >= enemy.Health);
                    if (bye != null) { E.Cast(bye); return; }
                }

                if (R.IsReady())
                {
                    var bye = EntityManager.Heroes.Enemies.FirstOrDefault(enemy => enemy.IsValidTarget(900) && SpellDamage(enemy, SpellSlot.R) >= enemy.Health);
                    if (bye != null)
                    {
                        var RPred = R.GetPrediction(bye);

                        if (RPred.HitChancePercent >= 75) { R.Cast(RPred.CastPosition); return; }
                    }
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

            //------------------------------------------------Flee-------------------------------------------------

            public static void Flee()
            {
                if (Q.IsReady() && Menu["UseQFlee"].Cast<CheckBox>().CurrentValue)
                {
                    var EscapeTarget = EntityManager.MinionsAndMonsters.CombinedAttackable.FirstOrDefault(it => it.IsValidTarget(Q.Range) && SpellDamage(it, SpellSlot.Q) >= it.Health && it.Distance(Game.CursorPos) <= 150);

                    if (EscapeTarget != null) Q.Cast(EscapeTarget);
                    else { EscapeTarget = EntityManager.MinionsAndMonsters.CombinedAttackable.FirstOrDefault(it => it.IsValidTarget(Q.Range) && it.Distance(Game.CursorPos) <= 150); Q.Cast(EscapeTarget); }
                }

                if (E.IsReady() && Menu["UseEFlee"].Cast<CheckBox>().CurrentValue)
                {
                    var ETarget = (from etarget in EntityManager.Heroes.Enemies.Where(it => it.IsValidTarget(E.Range)) orderby TargetSelector.GetPriority(etarget) descending select etarget).FirstOrDefault();

                    if (ETarget != null) E.Cast(ETarget);
                }

                if (R.IsReady() && Menu["UseRFlee"].Cast<CheckBox>().CurrentValue)
                {
                    var RTarget = (from rtarget in EntityManager.Heroes.Enemies.Where(it => it.IsValidTarget(900)) orderby TargetSelector.GetPriority(rtarget) descending select rtarget).FirstOrDefault();

                    if (RTarget != null) R.Cast(RTarget);
                }

                return;
            }

        }

        //---------------------------------------------QLogic()-------------------------------------------------

        static void QLogic()
        {
            if (Menu["UseWBeforeQCombo"].Cast<CheckBox>().CurrentValue && W.IsReady())
            {
                if (Target.IsDashing()) { W.Cast(); Core.DelayAction(() => Q.Cast(Target), Game.Ping + W.CastDelay); }
                if (Player.Distance(Target) >= Player.GetAutoAttackRange(Target)) { W.Cast(); Core.DelayAction(() => Q.Cast(Target), Game.Ping + W.CastDelay); }
                if (Target.HealthPercent <= 30) { W.Cast(); Core.DelayAction(() => Q.Cast(Target), Game.Ping + W.CastDelay); }
                if (Player.HealthPercent <= 30) { W.Cast(); Core.DelayAction(() => Q.Cast(Target), Game.Ping + W.CastDelay); }
                if (SpellDamage(Target, SpellSlot.Q) >= Target.Health) { W.Cast(); Core.DelayAction(() => Q.Cast(Target), Game.Ping + W.CastDelay); }
            }

            else
            {
                if (Target.IsDashing()) Q.Cast(Target);
                if (Player.Distance(Target) >= Player.GetAutoAttackRange(Target)) Q.Cast(Target);
                if (Target.HealthPercent <= 30) Q.Cast(Target);
                if (Player.HealthPercent <= 30) Q.Cast(Target);
                if (SpellDamage(Target, SpellSlot.Q) >= Target.Health) Q.Cast(Target);
            }
        }

        //-------------------------------------------SpellDamage()----------------------------------------------

        static float SpellDamage(Obj_AI_Base target, SpellSlot slot)
        {
            switch (slot)
            {
                case SpellSlot.Q:
                    if (!Q.IsReady()) return 0;

                    if (Item.HasItem(ItemId.Trinity_Force) && ((Trinity.IsReady() && !Player.HasBuff("sheen")) || Player.HasBuff("sheen")) ) return Player.CalculateDamageOnUnit(target, DamageType.Physical, new float[] { 20, 50, 80, 110, 140 }[Q.Level - 1] + Player.TotalAttackDamage + 2 * Player.BaseAttackDamage, true, true);
                    if (Item.HasItem(ItemId.Sheen) && ((Sheen.IsReady() && !Player.HasBuff("sheen")) || Player.HasBuff("sheen")) ) return Player.CalculateDamageOnUnit(target, DamageType.Physical, new float[] { 20, 50, 80, 110, 140 }[Q.Level - 1] + Player.TotalAttackDamage + Player.BaseAttackDamage, true, true);
                    return Player.CalculateDamageOnUnit(target, DamageType.Physical, new float[] { 20, 50, 80, 110, 140 }[Q.Level - 1] + Player.TotalAttackDamage, true, true);

                case SpellSlot.W:
                    return Player.CalculateDamageOnUnit(target, DamageType.True, 15 * W.Level, true, true);

                case SpellSlot.E:
                    return Player.CalculateDamageOnUnit(target, DamageType.Magical, 40 * (E.Level + 1), true, true);

                case SpellSlot.R:
                    return Player.CalculateDamageOnUnit(target, DamageType.Physical, (40 * (R.Level + 1)) + Player.TotalMagicalDamage / 2 + Player.FlatPhysicalDamageMod * 0.6f);

                default:
                    return 0;
            }
        }

        //---------------------------------GetComboDamage(Obj_AI_Hero Target)-----------------------------------

        static float GetComboDamage(AIHeroClient target)
        {
            if (target != null)
            {
                float ComboDamage = new float();

                ComboDamage = Q.IsReady() ? SpellDamage(target, SpellSlot.Q) : 0;
                ComboDamage = W.IsReady() ? SpellDamage(target, SpellSlot.W) * 2 : 0;
                ComboDamage += E.IsReady() ? SpellDamage(target, SpellSlot.E) : 0;
                ComboDamage += R.IsReady() ? SpellDamage(target, SpellSlot.R) * 4 : 0;
                ComboDamage += Player.GetAutoAttackDamage(target) * 2;
                ComboDamage += Bilgewater.IsReady() ? DamageLibrary.GetItemDamage(Player, target, ItemId.Bilgewater_Cutlass) : 0;
                ComboDamage += BOTRK.IsReady() ? DamageLibrary.GetItemDamage(Player, target, ItemId.Blade_of_the_Ruined_King) : 0;
                ComboDamage += Hydra.IsReady() ? DamageLibrary.GetItemDamage(Player, target, ItemId.Ravenous_Hydra_Melee_Only) : 0;
                ComboDamage += Hextech.IsReady() ? DamageLibrary.GetItemDamage(Player, target, ItemId.Hextech_Gunblade) : 0;

                if (Ignite != null) ComboDamage += Convert.ToSingle(Ignite.IsReady() ? DamageLibrary.GetSummonerSpellDamage(Player, target, DamageLibrary.SummonerSpells.Ignite) : 0);
                if (Smite != null) ComboDamage += Convert.ToSingle(Smite.IsReady() && Smite.Name.Contains("gank") ? DamageLibrary.GetSummonerSpellDamage(Player, target, DamageLibrary.SummonerSpells.Smite) : 0);

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
