using System;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
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

namespace WuGaren
{
    static class Program
    {
        static Version AssVersion;//Kappa
        const String CN = "Garen";
        static Spell.Targeted Smite = null;
        static Spell.Targeted Ignite = null;
        static AIHeroClient Player = EloBuddy.Player.Instance;
        static ColorBGRA Green = new ColorBGRA(Color.Green.R, Color.Green.G, Color.Green.B, Color.Green.A);
        static ColorBGRA Red = new ColorBGRA(Color.Red.R, Color.Red.G, Color.Red.B, Color.Red.A);

        static Item BOTRK, GhostBlade, Tiamat, Hydra, Bilgewater, Randuin, Scimitar, QSS, FOTMountain, Mikael;
        static Menu Menu;
        static AIHeroClient Target = null;
        static readonly Spell.Active Q = new Spell.Active(SpellSlot.Q);
        static readonly Spell.Active W = new Spell.Active(SpellSlot.W);
        static readonly Spell.Active E = new Spell.Active(SpellSlot.E, 300);
        static readonly Spell.Targeted R = new Spell.Targeted(SpellSlot.R, 400);
        static readonly int[] QDamages = new int[] { 30, 55, 80, 105, 130 };
        static readonly float[] EDamages = new float[] { 15, 18.8f, 25.5f, 26.3f, 30 };
        static readonly float[] EPercentADSpin = new float[] { 0.345f, 0.353f, 0.36f, 0.368f, 0.375f };
        static readonly int[] RDamages = new int[] { 175, 350, 525 };
        static readonly float[] RMissingHealth = new float[] { 0.286f, 0.333f, 0.4f };

        static void Main(string[] args) { Loading.OnLoadingComplete += OnLoadingComplete; }

        //---------------------------------------------Game_OnGameLoad----------------------------------------

        static void OnLoadingComplete(EventArgs args)
        {
            if (Player.BaseSkinName != CN) { Chat.Print("Sorry, you didn't pick " + CN + ", addon disabled"); return; }

            AssVersion = Assembly.GetExecutingAssembly().GetName().Version;
            SearchVersion();

            //-------------------------------------------------Items--------------------------------------------------

            BOTRK = new Item(3153, 550);
            Bilgewater = new Item(3144, 550);
            Hydra = new Item(3074, 400);
            Tiamat = new Item(3077, 400);
            GhostBlade = new Item(3142);
            Randuin = new Item(3143, 500);
            Scimitar = new Item(3139);
            QSS = new Item(3140);
            FOTMountain = new Item(3401);
            Mikael = new Item(3222, 750);

            //-------------------------------------------------Smite--------------------------------------------------

            SpellDataInst smite = Player.Spellbook.Spells.Where(spell => spell.Name.Contains("smite")).Any() ? Player.Spellbook.Spells.Where(spell => spell.Name.Contains("smite")).First() : null;
            if (smite != null)
            {
                Smite = new Spell.Targeted(smite.Slot, 500);
            }
            smite = null;

            //-------------------------------------------------Ignite--------------------------------------------------

            SpellDataInst dot = Player.Spellbook.Spells.Where(spell => spell.Name.Contains("dot")).Any() ? Player.Spellbook.Spells.Where(spell => spell.Name.Contains("dot")).First() : null;
            if (dot != null)
            {
                Ignite = new Spell.Targeted(dot.Slot, 600);
            }
            dot = null;

            //---------------------------||   Menu   ||----------------------------

            Menu = MainMenu.AddMenu("Wu" + CN, "Wu" + CN);

            //------------------------------Combo-------------------------------

            Menu.AddGroupLabel("Combo");
            {
                Menu.Add("UseQCombo", new CheckBox("Use Q Combo"));
                Menu.Add("UseWCombo", new CheckBox("Use W Combo"));
                Menu.Add("UseECombo", new CheckBox("Use E Combo"));
                Menu.Add("UseRCombo", new CheckBox("Use R Combo"));
            }
            Menu.AddSeparator();

            //------------------------------Harass-------------------------------

            Menu.AddGroupLabel("Harass");
            {
                Menu.Add("UseQHarass", new CheckBox("Use Q Harass"));
                Menu.Add("UseWHarass", new CheckBox("Use W Harass"));
                Menu.Add("UseEHarass", new CheckBox("Use E Harass"));
            }
            Menu.AddSeparator();

            //------------------------------LaneClear-------------------------------

            Menu.AddGroupLabel("LaneClear");
            {
                Menu.Add("UseELaneClear", new CheckBox("Use E LaneClear"));
                Menu.Add("Min Minions E", new Slider("Min Minions E", 3, 1, 7));
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
                Menu.Add("DrawE", new CheckBox("Draw E"));
                Menu.Add("DrawR", new CheckBox("Draw R"));
                if (Smite != null) Menu.Add("DrawSmite", new CheckBox("DrawSmite"));
                Menu.Add("ComboDamage on HPBar", new CheckBox("ComboDamage on HPBar"));
            }
            Menu.AddSeparator();

            //------------------------------Other things-------------------------------

            Menu.AddGroupLabel("Other things");

            Menu.Add("KS", new CheckBox("KS"));
            Menu.Add("QAfterAA", new CheckBox("QAfterAA"));
            Menu.Add("JEBQ", new CheckBox("Just E Before Q", false));
            Menu.Add("Interrupter", new CheckBox("Try to interrupt spells [Q]"));
            Menu.Add("Auto Ignite", new CheckBox("Auto Ignite"));

            Menu.AddSeparator();

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnEndScene += Drawing_OnEndScene;
            Orbwalker.OnPostAttack += Orbwalker_OnPostAttack;
            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;

            Chat.Print("Wu" + CN + " Loaded, [By WujuSan] , Version: " + AssVersion);
        }

        //-------------------------------------------Interrupter_OnInterruptableSpell-------------------------------------

        static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            if (Menu["Interrupter"].Cast<CheckBox>().CurrentValue)
            {
                if (!sender.IsAlly && !sender.IsMe && sender.IsValidTarget(Player.GetAutoAttackRange()) && Q.IsReady())
                {
                    Q.Cast();
                    Orbwalker.ResetAutoAttack();
                    EloBuddy.Player.IssueOrder(GameObjectOrder.AttackTo, sender);
                }
            }

            return;
        }

        //--------------------------------------------Orbwalker_OnPostAttack----------------------------------------------

        static void Orbwalker_OnPostAttack(AttackableUnit target, EventArgs args)
        {
            if (Menu["QAfterAA"].Cast<CheckBox>().CurrentValue)
            {
                if (Q.IsReady() && target.IsValidTarget(Player.GetAutoAttackRange() + 200) && (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass)))
                {
                    Q.Cast();
                    Orbwalker.ResetAutoAttack();
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

        //----------------------------------------------Drawing_OnDraw----------------------------------------

        static void Drawing_OnDraw(EventArgs args)
        {
            if (!Player.IsDead)
            {
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

            if (Player.CountEnemiesInRange(1000) > 0) Modes.SaveAlly();

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

            //-----------------------------------------------KS----------------------------------------

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

            //--------------------------------------------Orbwalker Modes-------------------------------------------

            if (Target != null)
            {
                if (Target.IsValidTarget())
                {
                    //---------------------------------------------------Combo--------------------------------------------

                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)) Modes.Combo();

                    //---------------------------------------------------Mixed--------------------------------------------

                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass)) Modes.Harass();
                }

                else Target = null;
            }

            //---------------------------------------------------LaneClear--------------------------------------------

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear)) Modes.LaneClear();

            return;
        }

        //-------------------------------------------class Modes-------------------------------------------------

        class Modes
        {
            //---------------------------------------------Combo()------------------------------------------------

            public static void Combo()
            {
                if ((Scimitar.IsReady() || QSS.IsReady()) && Player.HasBuffOfType(BuffType.Charm) || Player.HasBuffOfType(BuffType.Blind) || Player.HasBuffOfType(BuffType.Fear) || Player.HasBuffOfType(BuffType.Polymorph) || Player.HasBuffOfType(BuffType.Silence) || Player.HasBuffOfType(BuffType.Sleep) || Player.HasBuffOfType(BuffType.Snare) || Player.HasBuffOfType(BuffType.Stun) || Player.HasBuffOfType(BuffType.Suppression) || Player.HasBuffOfType(BuffType.Taunt)) { Scimitar.Cast(); QSS.Cast(); }

                if (Smite != null)
                {
                    if (Target.IsValidTarget(Smite.Range) && Smite.IsReady())
                    {
                        if (Smite.Name.Contains("gank")) Smite.Cast(Target);
                        else if (Smite.Name.Contains("duel") && Player.IsInAutoAttackRange(Target)) Smite.Cast(Target);
                    }
                }

                if (Menu["UseQCombo"].Cast<CheckBox>().CurrentValue && Q.IsReady() && Target.IsValidTarget(Player.GetAutoAttackRange() + 300) && !Menu["QAfterAA"].Cast<CheckBox>().CurrentValue) Q.Cast();

                if (Menu["UseWCombo"].Cast<CheckBox>().CurrentValue && W.IsReady() && Player.IsFacing(Target) && Target.IsValidTarget(Player.GetAutoAttackRange() + 300)) W.Cast();

                if (Menu["UseECombo"].Cast<CheckBox>().CurrentValue && E.IsReady() && Target.IsValidTarget(E.Range) && !Player.HasBuff("GarenE"))
                {
                    if (Menu["JEBQ"].Cast<CheckBox>().CurrentValue && Q.IsReady())
                    {
                        if (Target.HasBuffOfType(BuffType.Silence)) E.Cast();
                    }
                    else E.Cast();
                }

                if (R.IsReady())
                {
                    if (Menu["UseRCombo"].Cast<CheckBox>().CurrentValue && Target.IsValidTarget(R.Range) && SpellDamage(Target, SpellSlot.R) > Target.Health + 30)
                    {
                        if (Player.HasBuff("GarenE")) E.Cast();
                        R.Cast(Target);
                    }
                }

                if (Target.IsValidTarget(Player.GetAutoAttackRange() + 300) && GhostBlade.IsReady()) GhostBlade.Cast();

                if (Target.IsValidTarget(550) && BOTRK.IsReady()) BOTRK.Cast(Target);

                if (Target.IsValidTarget(550) && Bilgewater.IsReady()) Bilgewater.Cast(Target);

                if (Target.IsValidTarget(400) && Tiamat.IsReady()) Tiamat.Cast();

                if (Target.IsValidTarget(400) && Hydra.IsReady()) Hydra.Cast();

                if (Target.IsValidTarget(500) && Randuin.IsReady()) Randuin.Cast();

                return;
            }

            //--------------------------------------------Harass()-------------------------------------------------

            public static void Harass()
            {
                if (Menu["UseQHarass"].Cast<CheckBox>().CurrentValue && Q.IsReady() && Target.IsValidTarget(Player.GetAutoAttackRange() + 300)) Q.Cast();

                if (Menu["UseWHarass"].Cast<CheckBox>().CurrentValue && W.IsReady() && Target.IsAttackingPlayer && Target.IsValidTarget(Player.GetAutoAttackRange() + 200)) W.Cast();

                if (Menu["UseEHarass"].Cast<CheckBox>().CurrentValue && E.IsReady() && Target.IsValidTarget(E.Range) && !Player.HasBuff("GarenE"))
                {
                    if (Menu["JEBQ"].Cast<CheckBox>().CurrentValue)
                    {
                        if (Target.HasBuffOfType(BuffType.Silence)) E.Cast();
                    }
                    else E.Cast();
                }

                return;
            }

            //-------------------------------------------LaneClear()-----------------------------------------------

            public static void LaneClear()
            {
                if (E.IsReady())
                {
                    if (EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Player.Position, E.Range).Count() >= Menu["Min Minions E"].Cast<Slider>().CurrentValue && Menu["UseELaneClear"].Cast<CheckBox>().CurrentValue && !Player.HasBuff("GarenE")) E.Cast();
                    else if (EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.ServerPosition, E.Range).Where(it => Player.GetSpellDamage(it, SpellSlot.Q) < it.Health).Any() && !Player.HasBuff("GarenE")) E.Cast();
                }

                if (Tiamat != null)
                {
                    if (Tiamat.IsReady())
                    {
                        bool UseItem = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Player.Position, Hydra.Range).Count() >= 3;
                        if (UseItem) Tiamat.Cast();
                        UseItem = EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Position, Hydra.Range).Count() >= 2;
                        if (UseItem) Tiamat.Cast();
                    }
                }

                if (Hydra != null)
                {
                    if (Hydra.IsReady())
                    {
                        bool UseItem = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Player.Position, Hydra.Range).Count() >= 3;
                        if (UseItem) Hydra.Cast();
                        UseItem = EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Position, Hydra.Range).Count() >= 2;
                        if (UseItem) Hydra.Cast();
                    }
                }

                return;
            }

            //----------------------------------------------SaveAlly------------------------------------------------

            public static void SaveAlly()
            {
                var Ally = EntityManager.Heroes.Allies.FirstOrDefault(ally => EntityManager.Heroes.Enemies.Any(enemy => ally.IsFacing(enemy)) && ally.HealthPercent <= 30 && Player.Distance(ally) <= 750);

                if (Ally != null)
                {
                    if (FOTMountain.IsReady()) FOTMountain.Cast(Ally);

                    if (Mikael.IsReady() && (Ally.HasBuffOfType(BuffType.Charm) || Ally.HasBuffOfType(BuffType.Fear) || Ally.HasBuffOfType(BuffType.Poison) || Ally.HasBuffOfType(BuffType.Polymorph) || Ally.HasBuffOfType(BuffType.Silence) || Ally.HasBuffOfType(BuffType.Sleep) || Ally.HasBuffOfType(BuffType.Slow) || Ally.HasBuffOfType(BuffType.Snare) || Ally.HasBuffOfType(BuffType.Stun) || Ally.HasBuffOfType(BuffType.Taunt))) Mikael.Cast(Ally);
                }

                return;
            }

            //-------------------------------------------------KS--------------------------------------------------

            public static void KS()
            {
                if (R.IsReady())
                {
                    var bye = EntityManager.Heroes.Enemies.FirstOrDefault(enemy => SpellDamage(enemy, SpellSlot.R) >= enemy.Health + 30 && enemy.IsValidTarget(R.Range));
                    if (bye != default(AIHeroClient))
                    {
                        if (Player.HasBuff("GarenE")) E.Cast();
                        R.Cast(bye);
                        return;
                    }
                }

                if (Q.IsReady())
                {
                    var bye = EntityManager.Heroes.Enemies.FirstOrDefault(enemy => SpellDamage(enemy, SpellSlot.Q) >= enemy.Health && Target.IsValidTarget(Player.GetAutoAttackRange()));
                    if (bye != default(AIHeroClient))
                    {
                        if (Player.HasBuff("GarenE")) E.Cast();
                        Q.Cast();
                        EloBuddy.Player.IssueOrder(GameObjectOrder.AttackTo, bye);
                        return;
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

        }

        //-------------------------------------------GetComboDamage()------------------------------------------

        static float GetComboDamage()
        {
            if (Target != null)
            {
                float ComboDamage = new float();

                ComboDamage = Q.IsReady() ? SpellDamage(Target, SpellSlot.Q) : 0;
                ComboDamage += E.IsReady() ? SpellDamage(Target, SpellSlot.E) : 0;
                ComboDamage += R.IsReady() ? SpellDamage(Target, SpellSlot.R) : 0;
                ComboDamage += Player.GetAutoAttackDamage(Target) * 2;
                ComboDamage += Bilgewater.IsReady() ? DamageLibrary.GetItemDamage(Player, Target, ItemId.Bilgewater_Cutlass) : 0;
                ComboDamage += BOTRK.IsReady() ? DamageLibrary.GetItemDamage(Player, Target, ItemId.Blade_of_the_Ruined_King) : 0;
                ComboDamage += Hydra.IsReady() ? DamageLibrary.GetItemDamage(Player, Target, ItemId.Ravenous_Hydra_Melee_Only) : 0;

                if (Ignite != null) ComboDamage += Convert.ToSingle(Ignite.IsReady() ? DamageLibrary.GetSummonerSpellDamage(Player, Target, DamageLibrary.SummonerSpells.Ignite) : 0);
                if (Smite != null) ComboDamage += Convert.ToSingle(Smite.IsReady() && Smite.Name.Contains("gank") ? DamageLibrary.GetSummonerSpellDamage(Player, Target, DamageLibrary.SummonerSpells.Smite) : 0);

                return ComboDamage;
            }
            return 0;
        }

        //----------------------------------------------ESpins()-----------------------------------------------

        static byte Espins()
        {
            if (Player.Level < 3) return 5;
            else if (Player.Level < 6) return 6;
            else if (Player.Level < 9) return 7;
            else if (Player.Level < 12) return 8;
            else if (Player.Level < 15) return 9;
            else if (Player.Level < 19) return 10;
            else return 0;
        }

        //-------------------------------------SpellDamage(SpellSlot slot)--------------------------------------

        static float SpellDamage(Obj_AI_Base target, SpellSlot slot)
        {
            float damage = new float();

            if (slot == SpellSlot.R && target.HasBuff("garenpassiveenemytarget")) return EloBuddy.Player.Instance.CalculateDamageOnUnit(target, DamageType.True, RDamages[R.Level - 1] + (target.MaxHealth - target.Health) * RMissingHealth[R.Level - 1], true, true) - 180;

            switch (slot)
            {
                case SpellSlot.Q:
                    damage = Player.CalculateDamageOnUnit(target, DamageType.Physical, QDamages[Q.Level - 1] + 1.4f * Player.TotalAttackDamage, true, true) - 100;
                    break;
                case SpellSlot.E:
                    damage = Player.CalculateDamageOnUnit(target, DamageType.Physical, (EDamages[E.Level-1] + EPercentADSpin[E.Level-1] * Player.TotalAttackDamage) * Espins()) - 100;
                    break;
                case SpellSlot.R:
                    damage = EloBuddy.Player.Instance.CalculateDamageOnUnit(target, DamageType.Magical, RDamages[R.Level - 1] + (target.MaxHealth - target.Health) * RMissingHealth[R.Level - 1], true, true) - 180;
                    break;
            }

            return damage;
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
