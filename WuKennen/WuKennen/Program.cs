using System;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;
using Color = System.Drawing.Color;
using Version = System.Version;
using System.Net;
using System.Text.RegularExpressions;

namespace WuKennen
{
    static class Program
    {
        static Version AssVersion;//Kappa
        static readonly String CN = "Kennen";
        static Spell.Active Heal;
        static Spell.Targeted Smite, Ignite, Exhaust;
        static Spell.Skillshot Flash;
        static Item Mikael, Zhonya, Talisma, BOTRK, Hextech, GhostBlade, Bilgewater, Randuin, Scimitar, QSS;
        static readonly ColorBGRA Green = new ColorBGRA(Color.Green.R, Color.Green.G, Color.Green.B, Color.Green.A);
        static readonly ColorBGRA Red = new ColorBGRA(Color.Red.R, Color.Red.G, Color.Red.B, Color.Red.A);

        static AIHeroClient Target = null;
        static Menu Menu;
        static Spell.Skillshot Q = new Spell.Skillshot(SpellSlot.Q, 1000, SkillShotType.Linear, 125, 1700, 50);
        static Spell.Active W = new Spell.Active(SpellSlot.W, 900);
        static Spell.Active E = new Spell.Active(SpellSlot.E);
        static Spell.Active R = new Spell.Active(SpellSlot.R, 500);
        static AIHeroClient Player = EloBuddy.Player.Instance;

        static void Main(string[] args) { Loading.OnLoadingComplete += OnLoadingComplete; }

        //---------------------------------------OnLoadingComplete--------------------------------------------

        static void OnLoadingComplete(EventArgs args)
        {
            if (Player.BaseSkinName != CN) { Chat.Print("Sorry, you didn't pick " + CN + ", addon disabled..."); return; }

            AssVersion = Assembly.GetExecutingAssembly().GetName().Version;
            SearchVersion();

            //-------------------------------------------------Itens--------------------------------------------------
            
            Mikael = new Item(3222, 700);
            Zhonya = new Item(ItemId.Zhonyas_Hourglass);
            Talisma = new Item(ItemId.Talisman_of_Ascension);

            BOTRK = new Item(3153, 550);
            Hextech = new Item(3146, 700);
            Bilgewater = new Item(3144, 550);
            GhostBlade = new Item(3142);
            Randuin = new Item(3143, 500);
            Scimitar = new Item(3139);
            QSS = new Item(3140);

            Q.MinimumHitChance = HitChance.Medium;

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

            //--------------------------------------------------Flash---------------------------------------------------

            SpellDataInst flash = Player.Spellbook.Spells.Where(spell => spell.Name.Contains("flash")).Any() ? Player.Spellbook.Spells.Where(spell => spell.Name.Contains("flash")).First() : null;
            if (flash != null)
            {
                Flash = new Spell.Skillshot(flash.Slot, 425, SkillShotType.Linear);
            }

            //---------------------------||   Menus   ||----------------------------

            Menu = MainMenu.AddMenu("Wu" + CN, "Wu" + CN);

            //------------------------------Combo-------------------------------

            Menu.AddGroupLabel("Combo");
            {
                Menu.Add("UseQCombo", new CheckBox("Use Q Combo"));
                Menu.Add("UseWCombo", new CheckBox("Use W Combo"));
                Menu.Add("UseECombo", new CheckBox("Use E Combo"));
                Menu.Add("UseRCombo", new CheckBox("Use R Combo"));
                Menu.Add("Min Enemies R", new Slider("Min Enemies R", 2, 1, 5));
                Menu.Add("UseExhaust?", new CheckBox("Use Exhaust?"));
            }
            Menu.AddSeparator();

            //------------------------------Harass-------------------------------

            Menu.AddGroupLabel("Harass");
            {
                Menu.Add("UseQHarass", new CheckBox("Use Q Harass"));
                Menu.Add("UseWHarass", new CheckBox("Use W Harass"));
                Menu.Add("Harass, Mana %", new Slider("Harass, Mana %", 30, 1, 100));
            }
            Menu.AddSeparator();

            //------------------------------JungleClear-------------------------------

            Menu.AddGroupLabel("JungleClear");
            {
                Menu.Add("UseQJungleClear", new CheckBox("Use Q JungleClear"));
                Menu.Add("UseWJungleClear", new CheckBox("Use W JungleClear"));
                Menu.Add("UseEJungleClear", new CheckBox("Use E JungleClear"));
                Menu.Add("JungleClear, Mana %", new Slider("JungleClear, Mana %", 30, 1, 100));
            }
            Menu.AddSeparator();

            //------------------------------LaneClear-------------------------------

            Menu.AddGroupLabel("LaneClear");
            {
                Menu.Add("UseQLaneClear", new CheckBox("Use Q LaneClear"));
                Menu.Add("UseWLaneClear", new CheckBox("Use W LaneClear"));
                Menu.Add("Min Minions W", new Slider("Min Minions W", 4, 1, 15));
                Menu.Add("UseELaneClear", new CheckBox("Use E LaneClear"));
                Menu.Add("LaneClear, Mana %", new Slider("LaneClear, Mana %", 30, 1, 100));
            }
            Menu.AddSeparator();

            //------------------------------LastHit-------------------------------

            Menu.AddGroupLabel("LastHit");
            {
                Menu.Add("UseQLastHit", new CheckBox("Use Q LastHit"));
                Menu.Add("UseWLastHit", new CheckBox("Use W LastHit"));
                Menu.Add("LastHit, Mana %", new Slider("LastHit, Mana %", 30, 1, 100));
            }
            Menu.AddSeparator();

            //------------------------------Flee-------------------------------

            Menu.AddGroupLabel("Flee");
            {
                Menu.Add("UseQFlee", new CheckBox("Use Q Flee"));
                Menu.Add("UseWFlee", new CheckBox("Use W Flee"));
                Menu.Add("UseEFlee", new CheckBox("Use E Flee"));
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

            //------------------------------Drawings-------------------------------

            Menu.AddGroupLabel("Drawings");
            {
                Menu.Add("DrawQ", new CheckBox("Draw Q"));
                Menu.Add("DrawW", new CheckBox("Draw W"));
                Menu.Add("DrawR", new CheckBox("Draw R"));
                if (Smite != null) Menu.Add("DrawSmite", new CheckBox("DrawSmite"));
                Menu.Add("ComboDamage on HPBar", new CheckBox("ComboDamage on HPBar"));
            }
            Menu.AddSeparator();

            //------------------------------Others-------------------------------

            Menu.Add("KS", new CheckBox("KS"));
            Menu.Add("GapCloser", new CheckBox("Q on GapCloser"));
            Menu.Add("Auto Ignite", new CheckBox("Auto Ignite"));
            Menu.AddSeparator();
            Menu.Add("UseHeal?", new CheckBox("Use Heal?"));
            Menu.Add("HealHealth", new Slider("Auto Heal when Health% is at:", 20, 1, 100));
            Menu.AddSeparator();
            Menu.Add("UseZhonya?", new CheckBox("Use Zhonya?"));
            Menu.Add("ZhonyaHealth", new Slider("Auto Zhonya when Health% is at:", 15, 1, 100));

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnEndScene += Drawing_OnEndScene;
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Orbwalker.OnUnkillableMinion += Orbwalker_OnUnkillableMinion;

            Chat.Print("Wu" + CN + " Loaded, [By WujuSan] , Version: " + AssVersion);
        }

        //-----------------------------------Orbwalker_OnUnkillableMinion-------------------------------------

        static void Orbwalker_OnUnkillableMinion(Obj_AI_Base target, Orbwalker.UnkillableMinionArgs args)
        {
            if (W.IsReady() && target.IsValidTarget(W.Range) && target.HasBuff("kennenmarkofstorm") && SpellDamage(target, SpellSlot.W) >= target.Health)
            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) && !E.IsReady() && Player.ManaPercent >= Menu["LaneClear, Mana %"].Cast<Slider>().CurrentValue) { W.Cast(); return; }
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit) && Player.ManaPercent >= Menu["LastHit, Mana %"].Cast<Slider>().CurrentValue) { W.Cast(); return; }
            }

            return;
        }

        //---------------------------------------GapCloser_OnGapCloser----------------------------------------

        static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (!EBuff() && sender.IsEnemy && Player.Distance(e.End) <= Q.Range && Q.IsReady() && Menu["GapCloser"].Cast<CheckBox>().CurrentValue) Q.HitChanceCast(sender, 70);

            return;
        }

        //-----------------------------------Interrupter_OnInterruptableSpell---------------------------------

        static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            if (e.DangerLevel == DangerLevel.High && sender.IsValidTarget(Q.Range) && sender.GetBuffCount("kennenmarkofstorm") >= 2)
            {
                if (Q.IsReady()) Q.HitChanceCast(sender, 70);

                if (W.IsReady() && W.IsInRange(sender)) W.Cast();

                if (R.IsReady() && R.IsInRange(sender)) R.Cast();
            }

            return;
        }

        //-----------------------------------------Drawing_OnEndScene-----------------------------------------

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

                if (Menu["DrawW"].Cast<CheckBox>().CurrentValue)
                    Circle.Draw(W.IsReady() ? Green : Red, W.Range, Player.Position);

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

            if (EBuff()) Orbwalker.DisableAttacking = true;
            else Orbwalker.DisableAttacking = false;

            if (Zhonya.IsReady() && Menu["UseZhonya?"].Cast<CheckBox>().CurrentValue && Player.HealthPercent <= Menu["ZhonyaHealth"].Cast<Slider>().CurrentValue && EntityManager.Heroes.Enemies.Any(it => it.Distance(Player) <= it.GetAutoAttackRange() && it.IsValidTarget())) Zhonya.Cast();

            if (Player.CountEnemiesInRange(1000) > 0) Modes.SaveAlly();

            Target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);

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

            //----------------------------------------------------KS----------------------------------------------

            if (Menu["KS"].Cast<CheckBox>().CurrentValue && Player.CountEnemiesInRange(Q.Range) > 0) Modes.KS();

            //---------------------------------------------Flee Key-----------------------------------------------

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

            //---------------------------------------------------JungleClear--------------------------------------------

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear) && Player.ManaPercent >= Menu["JungleClear, Mana %"].Cast<Slider>().CurrentValue) Modes.JungleClear();

            //---------------------------------------------------LaneClear--------------------------------------------

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) && Player.ManaPercent >= Menu["LaneClear, Mana %"].Cast<Slider>().CurrentValue) Modes.LaneClear();

            //---------------------------------------------------LastHit--------------------------------------------

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit) && Player.ManaPercent >= Menu["LastHit, Mana %"].Cast<Slider>().CurrentValue) Modes.LastHit();

            return;
        }

        //------------------------------------------class Modes------------------------------------------------

        class Modes
        {
            static bool QIsReady;
            static bool WIsReady;
            static bool EIsReady;

            static bool QRange;
            static bool WRange;
            static bool ERange;

            //----------------------------------------UpdateVariables()-------------------------------------------

            public static void UpdateVariables()
            {
                QIsReady = Q.IsReady();
                WIsReady = W.IsReady();
                EIsReady = E.IsReady();

                QRange = Q.IsInRange(Target);
                WRange = W.IsInRange(Target);
                ERange = Player.Distance(Target) <= 700;

                return;
            }

            //---------------------------------------------Combo()------------------------------------------------

            public static void Combo()
            {
                if (!EBuff())
                {
                    if (Menu["UseECombo"].Cast<CheckBox>().CurrentValue && EIsReady && ERange) E.Cast();
                    if (Menu["UseQCombo"].Cast<CheckBox>().CurrentValue && QIsReady && QRange) Q.HitChanceCast(Target, 75);
                }

                if (Menu["UseRCombo"].Cast<CheckBox>().CurrentValue && Player.CountEnemiesInRange(R.Range) >= Menu["Min Enemies R"].Cast<Slider>().CurrentValue) R.Cast();

                if (Menu["UseWCombo"].Cast<CheckBox>().CurrentValue && WIsReady && WRange && Target.HasBuff("kennenmarkofstorm")) W.Cast();

                if (Smite != null)
                {
                    if (Smite.IsInRange(Target) && Smite.IsReady())
                    {
                        if (Smite.Name.Contains("gank")) Smite.Cast(Target);
                        else if (Smite.Name.Contains("duel") && Player.IsInAutoAttackRange(Target)) Smite.Cast(Target);
                    }
                }

                if (Talisma.IsReady() && CountAlliesInRange(650) > 0) Talisma.Cast();

                if (Exhaust != null && Menu["UseExhaust?"].Cast<CheckBox>().CurrentValue && TargetSelector.GetPriority(Target) > 3 && Target.IsValidTarget(Exhaust.Range)) Exhaust.Cast(Target);

                if (ERange && GhostBlade.IsReady()) GhostBlade.Cast();

                if (Target.IsValidTarget(550) && BOTRK.IsReady()) BOTRK.Cast(Target);

                if (Target.IsValidTarget(550) && Bilgewater.IsReady()) Bilgewater.Cast(Target);

                if (Target.IsValidTarget(500) && Randuin.IsReady()) Randuin.Cast();

                if (Target.IsValidTarget(700) && Hextech.IsReady()) Hextech.Cast(Target);

                return;
            }

            //---------------------------------------------Harass()-----------------------------------------------

            public static void Harass()
            {
                if (!EBuff())
                {
                    if (Menu["UseQHarass"].Cast<CheckBox>().CurrentValue && QIsReady && QRange) Q.HitChanceCast(Target, 75);
                }

                if (Menu["UseWHarass"].Cast<CheckBox>().CurrentValue && WIsReady && WRange && Target.HasBuff("kennenmarkofstorm")) W.Cast();

                return;
            }

            //-------------------------------------------JungleClear()-----------------------------------------------

            public static void JungleClear()
            {
                if (!EBuff())
                {
                    if (Menu["UseEJungleClear"].Cast<CheckBox>().CurrentValue && E.IsReady() && EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Position, 450).Where(it => it.Health >= 250).Any()) E.Cast();

                    if (Menu["UseQJungleClear"].Cast<CheckBox>().CurrentValue && Q.IsReady())
                    {
                        var QMinion = EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Position, Q.Range).FirstOrDefault(it => it.IsValidTarget(Q.Range));

                        if (QMinion != null) Q.HitChanceCast(QMinion, 40);
                    }

                    if (Menu["UseWJungleClear"].Cast<CheckBox>().CurrentValue && W.IsReady()) W.Cast();
                }

                return;
            }

            //-------------------------------------------LaneClear()-----------------------------------------------
            
            public static void LaneClear()
            {
                if (!EBuff())
                {
                    if (Menu["UseELaneClear"].Cast<CheckBox>().CurrentValue && E.IsReady() && EntityManager.MinionsAndMonsters.CombinedAttackable.Where(it => it.IsValidTarget(600)).Count() >= 4) E.Cast();

                    if (Menu["UseQLaneClear"].Cast<CheckBox>().CurrentValue && Q.IsReady())
                    {
                        var QMinion = EntityManager.MinionsAndMonsters.CombinedAttackable.FirstOrDefault(it => it.IsValidTarget(Q.Range) && SpellDamage(it, SpellSlot.Q) >= it.Health);

                        if (QMinion != null) Q.HitChanceCast(QMinion, 40);
                    }

                    if (Menu["UseWLaneClear"].Cast<CheckBox>().CurrentValue && W.IsReady())
                    {
                        var Minions = EntityManager.MinionsAndMonsters.CombinedAttackable.Where(it => it.IsValidTarget(W.Range) && it.HasBuff("kennenmarkofstorm"));

                        if (Minions.Count() >= Menu["Min Minions W"].Cast<Slider>().CurrentValue) W.Cast();
                    }
                }

                return;
            }

            //----------------------------------------------LastHit------------------------------------------------

            public static void LastHit()
            {
                if (Menu["UseQLastHit"].Cast<CheckBox>().CurrentValue && Q.IsReady())
                {
                    var QMinion = EntityManager.MinionsAndMonsters.CombinedAttackable.FirstOrDefault(it => it.IsValidTarget(Q.Range) && SpellDamage(it, SpellSlot.Q) >= it.Health);

                    if (QMinion != null) Q.HitChanceCast(QMinion, 40);
                }

                return;
            }

            //-------------------------------------------------KS--------------------------------------------------

            public static void KS()
            {
                if (Q.IsReady())
                {
                    var bye = EntityManager.Heroes.Enemies.FirstOrDefault(it => it.IsValidTarget(Q.Range) && SpellDamage(it, SpellSlot.Q) >= it.Health);
                    if (bye != null) Q.HitChanceCast(bye, 75);
                }

                if (W.IsReady())
                {
                    var bye = EntityManager.Heroes.Enemies.FirstOrDefault(it => it.IsValidTarget(W.Range) && SpellDamage(it, SpellSlot.W) >= it.Health && it.HasBuff("kennenmarkofstorm"));
                    if (bye != null) { W.Cast(); return; }
                }

                if (Q.IsReady() && !W.IsOnCooldown)
                {
                    var bye = EntityManager.Heroes.Enemies.FirstOrDefault(it => it.IsValidTarget(W.Range) && SpellDamage(it, SpellSlot.Q) + SpellDamage(it, SpellSlot.W) >= it.Health);
                    if (bye != null) { Q.HitChanceCast(bye, 75); return; }
                }

                if (R.IsReady())
                {
                    var bye = EntityManager.Heroes.Enemies.FirstOrDefault(it => it.IsValidTarget(R.Range - 150) && SpellDamage(it, SpellSlot.R) * 2 >= it.Health);
                    if (bye != null) { R.Cast(); return; }

                    bye = EntityManager.Heroes.Enemies.FirstOrDefault(it => it.IsValidTarget(R.Range) && SpellDamage(it, SpellSlot.R) >= it.Health);
                    if (bye != null) { R.Cast(); return; }
                }

                if (Smite != null)
                {
                    if (Smite.Name.Contains("gank") && Smite.IsReady())
                    {
                        var bye = EntityManager.Heroes.Enemies.FirstOrDefault(enemy => enemy.IsValidTarget(Smite.Range) && DamageLibrary.GetSummonerSpellDamage(Player, enemy, DamageLibrary.SummonerSpells.Smite) >= enemy.Health);
                        if (bye != null) { Smite.Cast(bye); return; }
                    }
                }

                return;
            }

            //----------------------------------------------Flee------------------------------------------------

            public static void Flee()
            {
                if (!EBuff())
                {
                    if (Menu["UseEFlee"].Cast<CheckBox>().CurrentValue && E.IsReady()) E.Cast();

                    if (Menu["UseQFlee"].Cast<CheckBox>().CurrentValue && Q.IsReady())
                    {
                        var QTarget = (from enemy in EntityManager.Heroes.Enemies.Where(it => it.IsValidTarget(Q.Range)) orderby TargetSelector.GetPriority(enemy) descending select enemy).FirstOrDefault();
                        if (QTarget != null) Q.HitChanceCast(QTarget, 75);
                    }
                }

                if (Menu["UseWFlee"].Cast<CheckBox>().CurrentValue && W.IsReady())
                {
                    var WTarget = EntityManager.Heroes.Enemies.FirstOrDefault(it => it.IsValidTarget(W.Range) && it.HasBuff("kennenmarkofstorm"));
                    if (WTarget != null) W.Cast();
                }

                return;
            }

            //----------------------------------------------SaveAlly-----------------------------------------------

            public static void SaveAlly()
            {
                if (Mikael.IsReady())
                {
                    var Ally = EntityManager.Heroes.Allies.FirstOrDefault(ally => EntityManager.Heroes.Enemies.Any(enemy => ally.IsFacing(enemy)) && ally.HealthPercent <= 30 && Player.Distance(ally) <= 750);

                    if (Ally != null)
                    {
                        if ((Ally.HasBuffOfType(BuffType.Charm) || Ally.HasBuffOfType(BuffType.Fear) || Ally.HasBuffOfType(BuffType.Poison) || Ally.HasBuffOfType(BuffType.Polymorph) || Ally.HasBuffOfType(BuffType.Silence) || Ally.HasBuffOfType(BuffType.Sleep) || Ally.HasBuffOfType(BuffType.Slow) || Ally.HasBuffOfType(BuffType.Snare) || Ally.HasBuffOfType(BuffType.Stun) || Ally.HasBuffOfType(BuffType.Taunt))) Mikael.Cast(Ally);
                    }
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

        //---------------------------------------EBuff()-------------------------------------------------------

        static bool EBuff() { return Player.HasBuff("KennenLightningRush"); }

        //----------------------------------CountAlliesInRange(float range)------------------------------------

        static int CountAlliesInRange(float range)
        {
            var allies = EntityManager.Heroes.Allies.Where(it => !it.IsMe && !it.IsDead && Player.Distance(it) <= range).Count();

            return allies;
        }

        //------------------------------------------HitChanceCast()--------------------------------------------

        static void HitChanceCast(this Spell.Skillshot spell, Obj_AI_Base target, float chance)
        {
            var Pred = spell.GetPrediction(target);

            if (!Pred.CollisionObjects.Any() && Pred.HitChancePercent >= chance) Q.Cast(Pred.CastPosition);
        }

        //------------------------SpellDamage(Obj_AI_Base target, SpellSlot slot)------------------------------

        static float SpellDamage(Obj_AI_Base target, SpellSlot slot)
        {
            switch (slot)
            {
                case SpellSlot.Q:
                    return Player.CalculateDamageOnUnit(target, DamageType.Magical, new int[] { 75, 115, 155, 195, 235 }[Q.Level - 1] + 0.75f * Player.TotalMagicalDamage);
                case SpellSlot.W:
                    return Player.CalculateDamageOnUnit(target, DamageType.Magical, new int[] { 65, 95, 125, 15, 185 }[W.Level - 1] + new float[] { 0.4f, 0.5f, 0.6f, 0.7f, 0.8f }[W.Level - 1] * Player.TotalAttackDamage + 0.55f * Player.TotalMagicalDamage, true, true);
                case SpellSlot.E:
                    return Player.CalculateDamageOnUnit(target, DamageType.Magical, new int[] { 85, 125, 165, 205, 245 }[E.Level - 1] + 0.6f * Player.TotalMagicalDamage);
                case SpellSlot.R:
                    return Player.CalculateDamageOnUnit(target, DamageType.Magical, new int[] { 80, 145, 210 }[R.Level - 1] + 0.4f * Player.TotalMagicalDamage);
                default:
                    return 0;
            }
        }

        //---------------------------------GetComboDamage(Obj_AI_Hero target)----------------------------------

        static float GetComboDamage(AIHeroClient target)
        {
            if (target != null)
            {
                float ComboDamage = new float();

                ComboDamage = Q.IsReady() ? SpellDamage(target, SpellSlot.Q) : 0;
                ComboDamage += W.IsReady() ? SpellDamage(target, SpellSlot.W) : 0;
                ComboDamage += E.IsReady() ? SpellDamage(target, SpellSlot.E) : 0;
                ComboDamage += R.IsReady() ? SpellDamage(target, SpellSlot.R) * 3: 0;
                ComboDamage += Player.TotalAttackDamage;
                if (Ignite != null) ComboDamage += Convert.ToSingle(Ignite.IsReady() && Ignite.IsInRange(target) ? DamageLibrary.GetSummonerSpellDamage(Player, target, DamageLibrary.SummonerSpells.Ignite) : 0);
                if (Smite != null) ComboDamage += Convert.ToSingle(Ignite.IsReady() && Smite.Name.Contains("gank") && Ignite.IsInRange(target) ? DamageLibrary.GetSummonerSpellDamage(Player, target, DamageLibrary.SummonerSpells.Smite) : 0);

                return ComboDamage;
            }
            return 0;
        }

        //------------------------------------------GetSmiteDamage()-------------------------------------------

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
