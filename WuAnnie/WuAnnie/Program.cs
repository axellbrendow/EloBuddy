using System;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Generic;
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

namespace WuAnnie
{
    static class Program
    {
        static Version AssVersion;//Kappa
        static readonly String CN = "Annie";
        static Spell.Active Heal;
        static Spell.Targeted Smite, Ignite, Exhaust;
        static Spell.Skillshot Flash;
        static Item Mikael, Zhonya, Talisma;
        const float Angle = 5 * (float)Math.PI / 18;
        static readonly ColorBGRA Green = new ColorBGRA(Color.Green.R, Color.Green.G, Color.Green.B, Color.Green.A);
        static readonly ColorBGRA Red = new ColorBGRA(Color.Red.R, Color.Red.G, Color.Red.B, Color.Red.A);

        static Vector2 Player2D = new Vector2();
        static Dictionary<Vector2, int> PosAndHits;
        static AIHeroClient Target = null;
        static Menu Menu;
        static Spell.Targeted Q = new Spell.Targeted(SpellSlot.Q, 625);
        static Spell.Skillshot W = new Spell.Skillshot(SpellSlot.W, 625, SkillShotType.Cone, 250, int.MaxValue, 210);
        static Spell.Active E = new Spell.Active(SpellSlot.E);
        static Spell.Skillshot R = new Spell.Skillshot(SpellSlot.R, 600, SkillShotType.Circular, 50, int.MaxValue, 250);
        static AIHeroClient Player = EloBuddy.Player.Instance;

        static void Main(string[] args) { Loading.OnLoadingComplete += OnLoadingComplete; }

        //---------------------------------------OnLoadingComplete--------------------------------------------

        static void OnLoadingComplete(EventArgs args)
        {
            if (Player.BaseSkinName != CN) { Chat.Print("Sorry, you didn't choose " + CN + ", closing..."); return; }

            AssVersion = Assembly.GetExecutingAssembly().GetName().Version;
            SearchVersion();

            //-------------------------------------------------Itens--------------------------------------------------

            Mikael = new Item(3222, 700);
            Zhonya = new Item(ItemId.Zhonyas_Hourglass);
            Talisma = new Item(ItemId.Talisman_of_Ascension);

            R.MinimumHitChance = HitChance.High;
            W.MinimumHitChance = HitChance.Medium;
            W.ConeAngleDegrees = 50;

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
                Menu.Add("UseRCombo", new CheckBox("Use R Combo"));
                Menu.Add("Min Enemies R", new Slider("Min Enemies R", 2, 1, 5));
                Menu.Add("UseExhaust?", new CheckBox("Use Exhaust?"));
                Menu.Add("AAMaxRange?", new CheckBox("AA when max range?"));
                Menu.Add("AA?", new CheckBox("Auto Attack?"));
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

            //------------------------------LastHit-------------------------------

            Menu.AddGroupLabel("LastHit");
            {
                Menu.Add("UseQLastHit", new CheckBox("Use Q LastHit"));
                Menu.Add("ModeQLastHit", new Slider("Q Mode -> Always / AlwaysIfNoEnemiesAround / NoWhenStun", 0, 0, 2));
            }
            Menu.AddSeparator();

            //------------------------------LaneClear-------------------------------

            Menu.AddGroupLabel("LaneClear");
            {
                Menu.Add("UseQLaneClear", new CheckBox("Use Q LaneClear"));
                Menu.Add("ModeQLaneClear", new Slider("Q Mode -> Always / AlwaysIfNoEnemiesAround / NoWhenStun", 0, 0, 2));
                Menu.Add("UseWLaneClear", new CheckBox("Use W LaneClear"));
                Menu.Add("Min Minions W", new Slider("Min Minions W", 4, 1, 7));
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
                Menu.Add("DrawQ/W/R", new CheckBox("Draw Q/W/R"));
                if (Smite != null) Menu.Add("DrawSmite", new CheckBox("DrawSmite"));
                Menu.Add("UltiPos && Hits", new CheckBox("UltiPos && Hits"));
                Menu.Add("ComboDamage on HPBar", new CheckBox("ComboDamage on HPBar"));
            }
            Menu.AddSeparator();

            Menu.Add("KS", new CheckBox("KS"));
            Menu.Add("StackStun", new CheckBox("StackStun"));
            Menu.Add("Auto Ignite", new CheckBox("Auto Ignite"));
            Menu.Add("RWithStun", new CheckBox("Just ult if stun is up", false));
            Menu.Add("Ult on Target", new KeyBind("Ult on Target", false, KeyBind.BindTypes.HoldActive, 'T'));
            Menu.Add("Flash+R", new KeyBind("Flash + R", false, KeyBind.BindTypes.HoldActive, 'T'));
            Menu.AddSeparator();
            Menu.Add("UseHeal?", new CheckBox("Use Heal?"));
            Menu.Add("HealHealth", new Slider("Auto Heal when Health% is at:", 20, 1, 100));
            Menu.AddSeparator();
            Menu.Add("UseZhonya?", new CheckBox("Use Zhonya?"));
            Menu.Add("ZhonyaHealth", new Slider("Auto Zhonya when Health% is at:", 15, 1, 100));

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnEndScene += Drawing_OnEndScene;
            Orbwalker.OnPreAttack += Orbwalker_OnPreAttack;
            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;

            Chat.Print("Wu" + CN + " Loaded, [By WujuSan] , Version: " + AssVersion);
        }

        //-----------------------------------Interrupter_OnInterruptableSpell---------------------------------

        static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            if (e.DangerLevel == DangerLevel.High && sender.IsValidTarget(R.Range))
            {
                if (W.IsReady()) { W.Cast(sender); return; }
                if (Q.IsReady() && sender.Distance(Player) <= 200) { Q.Cast(sender); return; }
                if (R.IsReady()) { R.Cast(sender); return; }
            }

            return;
        }

        //---------------------------------------Orbwalker_OnPreAttack----------------------------------------

        static void Orbwalker_OnPreAttack(AttackableUnit target, Orbwalker.PreAttackArgs args)
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                if (!Menu["AAMaxRange?"].Cast<CheckBox>().CurrentValue && target.Distance(Player) >= Q.Range - 30) args.Process = false;
                if (!Menu["AA?"].Cast<CheckBox>().CurrentValue) args.Process = false;
            }
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
                if (Target != null)
                {
                    if (Target.IsValidTarget(1500))
                    {
                        if (Menu["UltiPos && Hits"].Cast<CheckBox>().CurrentValue && R.IsReady())
                        {
                            PosAndHits = GetBestRPos(Target.ServerPosition.To2D());

                            if (PosAndHits.First().Value >= Menu["Min Enemies R"].Cast<Slider>().CurrentValue)
                            {
                                Drawing.DrawCircle(PosAndHits.First().Key.To3D(), 70, Color.Yellow);
                                Drawing.DrawText(Drawing.WorldToScreen(Player.Position).X, Drawing.WorldToScreen(Player.Position).Y - 200, Color.Yellow, string.Format("R WILL HIT {0} ENEMIES", PosAndHits.First().Value));
                            }
                        }
                    }
                }

                if (Menu["DrawQ/W/R"].Cast<CheckBox>().CurrentValue)
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

            if (Zhonya.IsReady() && Menu["UseZhonya?"].Cast<CheckBox>().CurrentValue && Player.HealthPercent <= Menu["ZhonyaHealth"].Cast<Slider>().CurrentValue && EntityManager.Heroes.Enemies.Any(it => it.Distance(Player) <= it.GetAutoAttackRange() && it.IsValidTarget())) Zhonya.Cast();

            if (Player.CountEnemiesInRange(1000) > 0) Modes.SaveAlly();

            Target = TargetSelector.GetTarget(1300, DamageType.Magical);
            Player2D = Player.ServerPosition.To2D();

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

            //--------------------------------------------Stack Stun-------------------------------------------

            if (Menu["StackStun"].Cast<CheckBox>().CurrentValue && !Player.HasBuff("recall") && !Player.HasBuff("pyromania_particle"))
            {
                if (!(Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit)) && E.IsReady()) E.Cast();
                if (Player.IsInShopRange() && W.IsReady()) W.Cast(Player.Position);
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

            //--------------------------------------------Orbwalker Modes-------------------------------------------

            if (Target != null)
            {
                if (Target.IsValidTarget())
                {
                    Modes.UpdateVariables();

                    if (Player.HasBuff("infernalguardiantime")) { EloBuddy.Player.IssueOrder(GameObjectOrder.MovePet, Target); EloBuddy.Player.IssueOrder(GameObjectOrder.AutoAttackPet, Target); }

                    //---------------------------------------------Flee Key-----------------------------------------------

                    if (R.IsReady())
                    {
                        if (Menu["Ult on Target"].Cast<KeyBind>().CurrentValue && Target.IsValidTarget(R.Range)) R.Cast(R.GetPrediction(Target).CastPosition);

                        else if (Menu["Flash+R"].Cast<KeyBind>().CurrentValue && Target.IsValidTarget(R.Range + Flash.Range - 50) && Flash.IsReady())
                        {
                            var RPos = GetBestRPos(Target.ServerPosition.To2D());
                            if (RPos.First().Value > 0)
                            {
                                var FlashPos = Player.Position.Extend(RPos.First().Key, Flash.Range).To3D();
                                Flash.Cast(FlashPos);
                                Core.DelayAction(() => R.Cast(RPos.First().Key.To3D()), Game.Ping + 20);
                            }
                        }
                    }

                    //---------------------------------------------------Combo--------------------------------------------

                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)) Modes.Combo();

                    //---------------------------------------------------Mixed--------------------------------------------

                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass) && Player.ManaPercent >= Menu["Harass, Mana %"].Cast<Slider>().CurrentValue) Modes.Harass();
                }

                else Target = null;
            }

            //---------------------------------------------------LaneClear--------------------------------------------

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear)) Modes.LaneClear();

            //---------------------------------------------------LastHit--------------------------------------------

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit) && Q.IsReady()) Modes.LastHit();

            return;
        }

        //-------------------------------------------class Modes-------------------------------------------------
        
        class Modes
        {
            static bool RRange;

            static bool QIsReady;
            static bool WIsReady;

            //----------------------------------------UpdateVariables()-------------------------------------------

            public static void UpdateVariables()
            {
                RRange = R.IsInRange(Target);

                QIsReady = Q.IsReady();
                WIsReady = W.IsReady();

                return;
            }

            //---------------------------------------------Combo()------------------------------------------------

            public static void Combo()
            {
                if (RRange && R.IsReady())
                {
                    PosAndHits = GetBestRPos(Target.ServerPosition.To2D());

                    if (Menu["UseRCombo"].Cast<CheckBox>().CurrentValue && PosAndHits.First().Value >= Menu["Min Enemies R"].Cast<Slider>().CurrentValue)
                    {
                        if (Menu["RWithStun"].Cast<CheckBox>().CurrentValue)
                        {
                            if (Player.HasBuff("pyromania_particle")) R.Cast(PosAndHits.First().Key.To3D());
                        }
                        else R.Cast(PosAndHits.First().Key.To3D());
                    }

                }

                if (Menu["UseQCombo"].Cast<CheckBox>().CurrentValue && QIsReady && RRange) Q.Cast(Target);

                if (Menu["UseWCombo"].Cast<CheckBox>().CurrentValue && WIsReady && RRange)
                {
                    var WPos = GetBestWPos();
                    if (WPos != default(Vector3)) W.Cast(WPos);
                }

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

                return;
            }

            //---------------------------------------------Harass()-----------------------------------------------

            public static void Harass()
            {
                if (Menu["UseQHarass"].Cast<CheckBox>().CurrentValue && QIsReady && RRange) Q.Cast(Target);

                if (Menu["UseWHarass"].Cast<CheckBox>().CurrentValue && WIsReady && RRange) W.Cast(Target);

                return;
            }

            //-------------------------------------------LaneClear()-----------------------------------------------

            public static void LaneClear()
            {
                if (Menu["UseQLaneClear"].Cast<CheckBox>().CurrentValue && Q.IsReady())
                {
                    IEnumerable<Obj_AI_Base> IEMinions = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Player.ServerPosition, Q.Range).Where(minion => minion.Health <= SpellDamage(minion, SpellSlot.Q)).OrderBy(minion => minion.Distance(Player2D));

                    if (IEMinions.Any())
                    {
                        switch (Menu["ModeQLaneClear"].Cast<Slider>().CurrentValue)
                        {
                            case 0:
                                Q.Cast(IEMinions.First());
                                break;
                            case 1:
                                if (Player.CountEnemiesInRange(700) == 0) Q.Cast(IEMinions.First());
                                break;
                            case 2:
                                if (!Player.HasBuff("pyromania_particle")) Q.Cast(IEMinions.First());
                                break;
                        }
                    }
                }

                if (Menu["UseWLaneClear"].Cast<CheckBox>().CurrentValue && W.IsReady() && Player.ManaPercent >= Menu["LaneClear, Mana %"].Cast<Slider>().CurrentValue)
                {
                    var WPos = GetBestWPos(true);
                    if (WPos != default(Vector3)) W.Cast(WPos);
                }

                return;
            }

            //----------------------------------------------LastHit------------------------------------------------

            public static void LastHit()
            {
                IEnumerable<Obj_AI_Base> IEMinions = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Player.ServerPosition, Q.Range).Where(minion => minion.Health <= SpellDamage(minion, SpellSlot.Q)).OrderBy(minion => minion.Distance(Player2D));

                if (IEMinions.Any())
                {
                    if (Menu["UseQLastHit"].Cast<CheckBox>().CurrentValue)
                        switch (Menu["ModeQLastHit"].Cast<Slider>().CurrentValue)
                        {
                            case 0:
                                Q.Cast(IEMinions.First());
                                break;
                            case 1:
                                if (Player.CountEnemiesInRange(700) == 0) Q.Cast(IEMinions.First());
                                break;
                            case 2:
                                if (!Player.HasBuff("pyromania_particle")) Q.Cast(IEMinions.First());
                                break;
                        }
                }

                return;
            }

            //-------------------------------------------------KS--------------------------------------------------

            public static void KS()
            {
                if (Q.IsReady())
                {
                    var bye = EntityManager.Heroes.Enemies.FirstOrDefault(it => it.IsValidTarget(Q.Range) && SpellDamage(it, SpellSlot.Q) >= it.Health);
                    if (bye != null) { Q.Cast(bye); return; }
                }

                if (W.IsReady())
                {
                    var bye = EntityManager.Heroes.Enemies.FirstOrDefault(it => it.IsValidTarget(W.Range) && SpellDamage(it, SpellSlot.W) >= it.Health);
                    if (bye != null) { W.Cast(bye); return; }
                }

                if (Q.IsReady() && W.IsReady())
                {
                    var bye = EntityManager.Heroes.Enemies.FirstOrDefault(it => it.IsValidTarget(Q.Range) && SpellDamage(it, SpellSlot.Q) + SpellDamage(it, SpellSlot.W) >= it.Health);
                    if (bye != null) { W.Cast(bye); Core.DelayAction(() => Q.Cast(bye), 100); return; }
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

        //--------------------------------------CountAlliesInRange(int range)--------------------------------------

        static int CountAlliesInRange(int range)
        {
            return EntityManager.Heroes.Allies.Where(it => !it.IsMe && !it.IsDead && it.Distance(Player) <= range).Count();
        }

        //-----------------------------------------------CountRHits(Vector2 CastPosition)-------------------------------------------

        static int CountRHits(Vector2 CastPosition)
        {
            int Hits = new int();

            foreach (Vector2 EnemyPos in GetEnemiesPosition())
            {
                if (CastPosition.Distance(EnemyPos) <= 250) Hits += 1;
            }

            return Hits;
        }

        //----------------------------------GetBestRPos(Vector2 TargetPosition)---------------------------------

        static Dictionary<Vector2, int> GetBestRPos(Vector2 TargetPosition)
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

        //----------------------------------------GetEnemiesPosition()------------------------------------------

        static List<Vector3> GetEnemiesPosition()
        {
            List<Vector3> Positions = new List<Vector3>();

            foreach (AIHeroClient Hero in EntityManager.Heroes.Enemies.Where(hero => !hero.IsDead && Player.Distance(hero) <= 1200))
            {
                Positions.Add(Prediction.Position.PredictUnitPosition(Hero, 500).To3D());
            }

            return Positions;
        }

        //--------------------------------------------GetBestWPos()---------------------------------------------

        static Vector3 GetBestWPos(bool minions = false)
        {
            if (minions)
            {
                var CS = new List<Geometry.Polygon.Sector>();

                var Minion = EntityManager.MinionsAndMonsters.EnemyMinions.Where(it => it.IsValidTarget(W.Range)).OrderByDescending(it => it.Distance(Player)).FirstOrDefault();

                if (Minion == null) return default(Vector3);

                var Vectors = new List<Vector3>() { new Vector3(Minion.ServerPosition.X + 550, Minion.ServerPosition.Y, Minion.ServerPosition.Z), new Vector3(Minion.ServerPosition.X - 550, Minion.ServerPosition.Y, Minion.ServerPosition.Z), new Vector3(Minion.ServerPosition.X, Minion.ServerPosition.Y + 550, Minion.ServerPosition.Z), new Vector3(Minion.ServerPosition.X, Minion.ServerPosition.Y - 550, Minion.ServerPosition.Z), new Vector3(Minion.ServerPosition.X + 230, Minion.ServerPosition.Y, Minion.ServerPosition.Z), new Vector3(Minion.ServerPosition.X - 230, Minion.ServerPosition.Y, Minion.ServerPosition.Z), new Vector3(Minion.ServerPosition.X, Minion.ServerPosition.Y + 230, Minion.ServerPosition.Z), new Vector3(Minion.ServerPosition.X, Minion.ServerPosition.Y - 230, Minion.ServerPosition.Z), Minion.ServerPosition };

                var CS1 = new Geometry.Polygon.Sector(Player.Position, Vectors[0], Angle, 600);
                var CS2 = new Geometry.Polygon.Sector(Player.Position, Vectors[1], Angle, 600);
                var CS3 = new Geometry.Polygon.Sector(Player.Position, Vectors[2], Angle, 600);
                var CS4 = new Geometry.Polygon.Sector(Player.Position, Vectors[3], Angle, 600);
                var CS5 = new Geometry.Polygon.Sector(Player.Position, Vectors[4], Angle, 600);
                var CS6 = new Geometry.Polygon.Sector(Player.Position, Vectors[5], Angle, 600);
                var CS7 = new Geometry.Polygon.Sector(Player.Position, Vectors[6], Angle, 600);
                var CS8 = new Geometry.Polygon.Sector(Player.Position, Vectors[7], Angle, 600);
                var CS9 = new Geometry.Polygon.Sector(Player.Position, Vectors[8], Angle, 600);

                CS.Add(CS1);
                CS.Add(CS2);
                CS.Add(CS3);
                CS.Add(CS4);
                CS.Add(CS5);
                CS.Add(CS6);
                CS.Add(CS7);
                CS.Add(CS8);
                CS.Add(CS9);

                var CSHits = new List<byte>() { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

                for (byte j = 0; j < 9; j++)
                {
                    foreach ( Obj_AI_Base minion in EntityManager.MinionsAndMonsters.EnemyMinions.Where(it => it.IsValidTarget(W.Range)) )
                    {
                        if (CS.ElementAt(j).IsInside(minion)) CSHits[j]++;
                    }
                }

                int i = CSHits.Select((value, index) => new { Value = value, Index = index }).Aggregate((a, b) => (a.Value > b.Value) ? a : b).Index;

                if (CSHits[i] < Menu["Min Minions W"].Cast<Slider>().CurrentValue) return default(Vector3);

                return Vectors[i];
            }
            else
            {
                var CS = new List<Geometry.Polygon.Sector>();
                var Vectors = new List<Vector3>() { new Vector3(Target.ServerPosition.X + 550, Target.ServerPosition.Y, Target.ServerPosition.Z), new Vector3(Target.ServerPosition.X - 550, Target.ServerPosition.Y, Target.ServerPosition.Z), new Vector3(Target.ServerPosition.X, Target.ServerPosition.Y + 550, Target.ServerPosition.Z), new Vector3(Target.ServerPosition.X, Target.ServerPosition.Y - 550, Target.ServerPosition.Z), new Vector3(Target.ServerPosition.X + 230, Target.ServerPosition.Y, Target.ServerPosition.Z), new Vector3(Target.ServerPosition.X - 230, Target.ServerPosition.Y, Target.ServerPosition.Z), new Vector3(Target.ServerPosition.X, Target.ServerPosition.Y + 230, Target.ServerPosition.Z), new Vector3(Target.ServerPosition.X, Target.ServerPosition.Y - 230, Target.ServerPosition.Z), Target.ServerPosition };

                var CS1 = new Geometry.Polygon.Sector(Player.Position, Vectors[0], Angle, 600);
                var CS2 = new Geometry.Polygon.Sector(Player.Position, Vectors[1], Angle, 600);
                var CS3 = new Geometry.Polygon.Sector(Player.Position, Vectors[2], Angle, 600);
                var CS4 = new Geometry.Polygon.Sector(Player.Position, Vectors[3], Angle, 600);
                var CS5 = new Geometry.Polygon.Sector(Player.Position, Vectors[4], Angle, 600);
                var CS6 = new Geometry.Polygon.Sector(Player.Position, Vectors[5], Angle, 600);
                var CS7 = new Geometry.Polygon.Sector(Player.Position, Vectors[6], Angle, 600);
                var CS8 = new Geometry.Polygon.Sector(Player.Position, Vectors[7], Angle, 600);
                var CS9 = new Geometry.Polygon.Sector(Player.Position, Vectors[8], Angle, 600);

                CS.Add(CS1);
                CS.Add(CS2);
                CS.Add(CS3);
                CS.Add(CS4);
                CS.Add(CS5);
                CS.Add(CS6);
                CS.Add(CS7);
                CS.Add(CS8);
                CS.Add(CS9);

                var CSHits = new List<byte>() { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

                for (byte j = 0; j < 9; j++)
                {
                    foreach (AIHeroClient hero in EntityManager.Heroes.Enemies.Where(enemy => !enemy.IsDead && enemy.IsValidTarget(W.Range)))
                    {
                        if (CS.ElementAt(j).IsInside(hero)) CSHits[j]++;
                        if (hero == Target) CSHits[j] += 10;
                    }
                }

                byte i = (byte)CSHits.Select((value, index) => new { Value = value, Index = index }).Aggregate((a, b) => (a.Value > b.Value) ? a : b).Index;

                if (CSHits[i] == 0) return default(Vector3);

                return Vectors[i];
            }
        }

        //------------------------SpellDamage(Obj_AI_Base target, SpellSlot slot)-------------------------------

        static float SpellDamage(Obj_AI_Base target, SpellSlot slot)
        {
            switch (slot)
            {
                case SpellSlot.Q:
                    return Damage.CalculateDamageOnUnit(Player, target, DamageType.Magical, new float[] { 80, 115, 150, 185, 220 }[Q.Level - 1] + 0.8f * Player.TotalMagicalDamage, true, true);
                case SpellSlot.W:
                    return Damage.CalculateDamageOnUnit(Player, target, DamageType.Magical, new float[] { 70, 115, 160, 205, 250 }[W.Level - 1] + 0.85f * Player.TotalMagicalDamage);
                case SpellSlot.R:
                    return Damage.CalculateDamageOnUnit(Player, target, DamageType.Magical, new float[] { 175, 300, 425 }[R.Level - 1] + 0.8f * Player.TotalMagicalDamage);
                default:
                    return 0;
            }
        }

        //---------------------------------GetComboDamage(Obj_AI_Hero Target)----------------------------------

        static float GetComboDamage(AIHeroClient Target)
        {
            if (Target != null)
            {
                float ComboDamage = new float();

                ComboDamage = Q.IsReady() ? SpellDamage(Target, SpellSlot.Q) : 0;
                ComboDamage += W.IsReady() ? SpellDamage(Target, SpellSlot.W) : 0;
                ComboDamage += R.IsReady() ? SpellDamage(Target, SpellSlot.R) : 0;
                ComboDamage += Player.TotalAttackDamage;
                if (Ignite != null) ComboDamage += Convert.ToSingle(Ignite.IsReady() && Ignite.IsInRange(Target) ? DamageLibrary.GetSummonerSpellDamage(Player, Target, DamageLibrary.SummonerSpells.Ignite) : 0);

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
