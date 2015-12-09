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

namespace WuMalphite
{
    static class Program
    {
        static Version AssVersion;//Kappa
        static readonly String CN = "Malphite";
        static Spell.Targeted Smite = null;
        static Spell.Targeted Ignite = null;
        static AIHeroClient Player { get { return ObjectManager.Player; } }
        static ColorBGRA Green = new ColorBGRA(Color.Green.R, Color.Green.G, Color.Green.B, Color.Green.A);
        static ColorBGRA Red = new ColorBGRA(Color.Red.R, Color.Red.G, Color.Red.B, Color.Red.A);
        
        static Item BOTRK, Randuin, Bilgewater, Tiamat, Hydra, Glory, FOTMountain, Mikael;
        static Menu Menu;
        static Vector2 Player2D = new Vector2();
        static Dictionary<Vector2, int> PosAndHits;
        static AIHeroClient Target = null;
        static Spell.Targeted Q = new Spell.Targeted(SpellSlot.Q, 625);
        static Spell.Active W = new Spell.Active(SpellSlot.W);
        static Spell.Active E = new Spell.Active(SpellSlot.E, 400);
        static Spell.Skillshot R = new Spell.Skillshot(SpellSlot.R, 1000, SkillShotType.Circular, 250, 700, 270);

        static void Main(string[] args) { Loading.OnLoadingComplete += OnLoadingComplete; }

        //---------------------------------------------Game_OnGameLoad----------------------------------------

        static void OnLoadingComplete(EventArgs args)
        {
            if (Player.BaseSkinName != CN) { Chat.Print("You didn't choose " + CN + ", disabling addon..."); return; }

            AssVersion = Assembly.GetExecutingAssembly().GetName().Version;
            SearchVersion();

            R.MinimumHitChance = HitChance.High;

            //-------------------------------------------------Itens--------------------------------------------------

            BOTRK = new Item(3153, 550);
            Bilgewater = new Item(3144, 550);
            Randuin = new Item(3143, 500);
            Tiamat = new Item(3077, 400);
            Hydra = new Item(3074, 400);
            Glory = new Item(3800);
            FOTMountain = new Item(3401);
            Mikael = new Item(3222, 750);

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
            }
            Menu.AddSeparator();

            //------------------------------Harass-------------------------------

            Menu.AddGroupLabel("Harass");
            {
                Menu.Add("UseQHarass", new CheckBox("Use Q Harass"));
                Menu.Add("UseWHarass", new CheckBox("Use W Harass"));
                Menu.Add("UseEHarass", new CheckBox("Use E Harass"));
                Menu.Add("Harass, Mana %", new Slider("Harass, Mana %", 30, 1, 100));
            }
            Menu.AddSeparator();

            //------------------------------LaneClear-------------------------------

            Menu.AddGroupLabel("LaneClear");
            {
                Menu.Add("UseELaneClear", new CheckBox("Use E LaneClear"));
                Menu.Add("Min Minions E", new Slider("Min Minions E", 3, 1, 10));
                Menu.Add("UseQLaneClear", new CheckBox("Use Q LaneClear"));
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
                Menu.Add("DrawR", new CheckBox("Draw R"));
                if (Smite != null) Menu.Add("DrawSmite", new CheckBox("DrawSmite"));
                Menu.Add("UltiPos && Hits", new CheckBox("UltiPos && Hits"));
                Menu.Add("ComboDamage on HPBar", new CheckBox("ComboDamage on HPBar"));
            }
            Menu.AddSeparator();

            Menu.Add("KS", new CheckBox("KS"));
            Menu.Add("Auto Ignite", new CheckBox("Auto Ignite"));
            Menu.Add("Gapcloser", new CheckBox("Gapcloser"));
            Menu.Add("Interrupt Dangerous Spells", new CheckBox("Interrupt Dangerous Spells"));
            Menu.Add("Ult on Target", new KeyBind("Ult on Target", false, KeyBind.BindTypes.HoldActive, 'T'));

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnEndScene += Drawing_OnEndScene;
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;

            Chat.Print("Wu" + CN + " Loaded, [By WujuSan] , Version: " + AssVersion);
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
                        float FutureDamage = GetComboDamage(Target) > Target.Health ? 1 : GetComboDamage(Target) / Target.MaxHealth;

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

        //--------------------------------------Interrupter_OnInterruptableSpell------------------------------------

        static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            if (e.DangerLevel == DangerLevel.High && R.IsReady() && sender.IsEnemy && Menu["Interrupt Spells"].Cast<CheckBox>().CurrentValue)
            {
                if (R.GetPrediction(sender).HitChance == HitChance.High)
                {
                    R.Cast(sender);
                }
            }
        }

        //----------------------------------------------Gapcloser_OnGapcloser----------------------------------------

        static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (sender.IsEnemy && Q.IsReady() && sender.IsValidTarget(Q.Range))
            {
                Q.Cast(sender);
            }
        }

        //----------------------------------------------Drawing_OnDraw----------------------------------------

        static void Drawing_OnDraw(EventArgs args)
        {
            if (!Player.IsDead)
            {
                if (Target != null)
                {
                    if (Target.IsValidTarget(1700))
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

                if (Menu["DrawQ"].Cast<CheckBox>().CurrentValue)
                    Circle.Draw(Q.IsReady() ? Green : Red, Q.Range, Player.Position);

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

            if (Player.CountEnemiesInRange(1000) > 0)
            {
                foreach (AIHeroClient enemy in EntityManager.Heroes.Enemies)
                {
                    foreach (AIHeroClient ally in EntityManager.Heroes.Allies)
                    {
                        if (ally.IsFacing(enemy) && ally.HealthPercent <= 30)
                        {
                            FOTMountain.Cast(ally);

                            if ((ally.HasBuffOfType(BuffType.Charm) || ally.HasBuffOfType(BuffType.Fear) || ally.HasBuffOfType(BuffType.Poison) || ally.HasBuffOfType(BuffType.Polymorph) || ally.HasBuffOfType(BuffType.Silence) || ally.HasBuffOfType(BuffType.Sleep) || ally.HasBuffOfType(BuffType.Slow) || ally.HasBuffOfType(BuffType.Snare) || ally.HasBuffOfType(BuffType.Stun) || ally.HasBuffOfType(BuffType.Taunt)) && Mikael.IsInRange(ally)) Mikael.Cast(ally);
                        }
                    }
                }
            }

            Target = TargetSelector.GetTarget(R.Range + E.Range, DamageType.Magical);
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

            //----------------------------------------------------KS----------------------------------------------

            if (Menu["KS"].Cast<CheckBox>().CurrentValue && Player.CountEnemiesInRange(Q.Range) > 0)
            {
                AIHeroClient bye = null;

                if (Q.IsReady())
                {
                    bye = EntityManager.Heroes.Enemies.FirstOrDefault(it => it.IsValidTarget(Q.Range) && SpellDamage(it, SpellSlot.Q) >= it.Health);
					if (bye != null) Q.Cast(bye);
                }

                if (E.IsReady() && bye == null)
                {
                    bye = EntityManager.Heroes.Enemies.FirstOrDefault(it => it.IsValidTarget(E.Range - 40) && SpellDamage(it, SpellSlot.E) >= it.Health);
                    if (bye != null) E.Cast();
                }

                if (Q.IsReady() && E.IsReady() && bye == null)
                {
                    bye = EntityManager.Heroes.Enemies.FirstOrDefault(it => it.IsValidTarget(E.Range - 40) && SpellDamage(it, SpellSlot.Q) + SpellDamage(it, SpellSlot.E) >= it.Health);
                    if (bye != null) { E.Cast(); Core.DelayAction( () => Q.Cast(bye), E.CastDelay + Game.Ping ); }
                }

                else if (Smite != null && bye == null)
                {
                    if (Smite.Name.Contains("Gank") && Smite.IsReady())
                    {
                        bye = EntityManager.Heroes.Enemies.FirstOrDefault(enemy => enemy.IsValidTarget(Smite.Range) && DamageLibrary.GetSummonerSpellDamage(Player, enemy, DamageLibrary.SummonerSpells.Smite) >= enemy.Health);
                        if (bye != null) Smite.Cast(bye);
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

            //--------------------------------------------Orbwalker Modes-------------------------------------------

            if (Target != null)
            {
                if (Target.IsValidTarget())
                {
                    bool QRange = Q.IsInRange(Target);
                    bool WRange = Target.IsValidTarget(Player.GetAutoAttackRange());
                    bool ERange = Target.Distance(Player2D) <= E.Range - 20;
                    bool RRange = R.IsInRange(Target);

                    bool QIsReady = Q.IsReady();
                    bool EIsReady = E.IsReady();
                    
                    //-----------------------------------------------Ult On Target----------------------------------------

                    if (Menu["Ult on Target"].Cast<KeyBind>().CurrentValue) R.Cast(Target);

                    //---------------------------------------------------Combo--------------------------------------------
                    
                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                    {
                        if (RRange && R.IsReady())
                        {
                            PosAndHits = GetBestRPos(Target.ServerPosition.To2D());

                            if (Menu["UseRCombo"].Cast<CheckBox>().CurrentValue && PosAndHits.First().Value >= Menu["Min Enemies R"].Cast<Slider>().CurrentValue)
                            {
                                if (EntityManager.Heroes.Allies.Where(ally => ally != Player && ally.Distance(Player) <= 700).Count() > 0 && Glory.IsReady()) Glory.Cast();
                                R.Cast(PosAndHits.First().Key.To3D());
                            }
                        }

                        if (Smite != null)
                        {
                            if (Target.IsValidTarget(Smite.Range) && Smite.IsReady())
                            {
                                if (Smite.Name.Contains("Gank")) Smite.Cast(Target);
                                else if (Smite.Name.Contains("Duel") && Player.IsInAutoAttackRange(Target)) Smite.Cast(Target);
                            }
                        }

                        if (Menu["UseQCombo"].Cast<CheckBox>().CurrentValue && QIsReady && QRange) Q.Cast(Target);

                        if (Menu["UseECombo"].Cast<CheckBox>().CurrentValue && EIsReady && ERange) E.Cast();

                        if (Menu["UseWCombo"].Cast<CheckBox>().CurrentValue && W.IsReady() && WRange) W.Cast();

                        if (Randuin.IsReady() && Target.IsValidTarget(500)) Randuin.Cast();

                        if (Bilgewater.IsReady() && Target.IsValidTarget(550)) Bilgewater.Cast(Target);

                        if (BOTRK.IsReady() && Target.IsValidTarget(550)) BOTRK.Cast(Target);

                        if (Tiamat.IsReady() && Target.IsValidTarget(400)) Tiamat.Cast();

                        if (Hydra.IsReady() && Target.IsValidTarget(400)) Hydra.Cast();
                    }

                    //---------------------------------------------------Mixed--------------------------------------------

                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
                    {
                        if (Player.ManaPercent >= Menu["Harass, Mana %"].Cast<Slider>().CurrentValue)
                        {
                            if (Menu["UseQHarass"].Cast<CheckBox>().CurrentValue && QIsReady && QRange) Q.Cast(Target);

                            if (Menu["UseEHarass"].Cast<CheckBox>().CurrentValue && EIsReady && ERange) E.Cast();

                            if (Menu["UseWHarass"].Cast<CheckBox>().CurrentValue && W.IsReady() && WRange) W.Cast();
                        }
                    }
                }
                else Target = null;
            }

            //---------------------------------------------------LaneClear--------------------------------------------

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                if (Player.ManaPercent >= Menu["LaneClear, Mana %"].Cast<Slider>().CurrentValue)
                {
                    IEnumerable<Obj_AI_Minion> ListMinions = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Player.ServerPosition, E.Range);

                    if (ListMinions.Any() && E.IsReady())
                    {
                        if (Menu["UseELaneClear"].Cast<CheckBox>().CurrentValue && ListMinions.Count() >= Menu["Min Minions E"].Cast<Slider>().CurrentValue) E.Cast();
                    }

                    IEnumerable<Obj_AI_Minion> IEMinions = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Player.ServerPosition, Q.Range).Where(minion => minion.Health <= SpellDamage(minion, SpellSlot.Q)).OrderByDescending(minion => minion.Distance(Player2D));

                    if (IEMinions.Any() && Q.IsReady())
                    {
                        if (Menu["UseQLaneClear"].Cast<CheckBox>().CurrentValue)
                            Q.Cast(IEMinions.First());
                    }
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
            }

            return;
        }

        //-----------------------------------------------CountRHits(Vector2 CastPosition)-------------------------------------------

        static int CountRHits(Vector2 CastPosition)
        {
            int Hits = new int();

            foreach (Vector3 EnemyPos in GetEnemiesPosition())
            {
                if (CastPosition.Distance(EnemyPos) <= 260) Hits += 1;
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
            
            foreach (AIHeroClient Hero in EntityManager.Heroes.Enemies.Where(hero => !hero.IsDead && Player.Distance(hero) <= R.Range + E.Range))
            {
                Positions.Add(Prediction.Position.PredictUnitPosition(Hero, 500).To3D());
            }
            
            return Positions;
        }

        //------------------------SpellDamage(Obj_AI_Base target, SpellSlot slot)-------------------------------

        static float SpellDamage(Obj_AI_Base target, SpellSlot slot)
        {
            switch (slot)
            {
                case SpellSlot.Q:
                    return Damage.CalculateDamageOnUnit(Player, target, DamageType.Magical, new float[] { 70, 120, 170, 220, 270 }[Q.Level - 1] + 0.6f * Player.TotalMagicalDamage, true, true);
                case SpellSlot.W:
                    return Damage.CalculateDamageOnUnit(Player, target, DamageType.Physical, 15 * W.Level + 0.1f * Player.Armor + 0.1f * Player.TotalMagicalDamage);
                case SpellSlot.E:
                    return Damage.CalculateDamageOnUnit(Player, target, DamageType.Magical, new float[] { 60, 100, 140, 180, 220 }[E.Level - 1] + 0.3f * Player.Armor + 0.2f * Player.TotalMagicalDamage);
                case SpellSlot.R:
                    return Damage.CalculateDamageOnUnit(Player, target, DamageType.Magical, new float[] { 200, 300, 400 }[R.Level - 1] + Player.TotalMagicalDamage);
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
                ComboDamage += W.IsReady() ? SpellDamage(Target, SpellSlot.W) * 3 : 0;
                ComboDamage += E.IsReady() ? SpellDamage(Target, SpellSlot.E) : 0;
                ComboDamage += R.IsReady() ? SpellDamage(Target, SpellSlot.R) : 0;
                ComboDamage += Player.GetAutoAttackDamage(Target);
                ComboDamage += Item.CanUseItem(3144) && Player.Distance(Target) <= 550 ? DamageLibrary.GetItemDamage(Player, Target, ItemId.Bilgewater_Cutlass) : 0;
                ComboDamage += Item.CanUseItem(3153) && Player.Distance(Target) <= 550 ? DamageLibrary.GetItemDamage(Player, Target, ItemId.Blade_of_the_Ruined_King) : 0;
                if (Ignite != null) ComboDamage += Convert.ToSingle(Ignite.IsReady() && Ignite.IsInRange(Target) ? DamageLibrary.GetSummonerSpellDamage(Player, Target, DamageLibrary.SummonerSpells.Ignite) : 0);

                return ComboDamage;
            }
            return 0;
        }

        //------------------------------------------GetSmiteDamage()--------------------------------------------

        static float GetSmiteDamage()
        {
            float damage = new float(); //Arithmetic Progression OP, General Term OP :D

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
