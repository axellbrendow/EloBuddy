using System;
using EloBuddy;
using EloBuddy.SDK.Events;
using WuAIO.Managers;

namespace WuAIO
{
    static class Program
    {
        static void Main(string[] args) { Loading.OnLoadingComplete += OnLoadingComplete; }

        static void OnLoadingComplete(EventArgs args)
        {
            VersionManager.CheckVersion();

            try
            {
                Activator.CreateInstance(null, "WuAIO." + Player.Instance.ChampionName);
                Chat.Print("Wu{0} Loaded, [By WujuSan], Version: {1}", Player.Instance.ChampionName == "MasterYi" ? "Yi" : Player.Instance.ChampionName, VersionManager.AssVersion);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
<<<<<<< HEAD
=======
            Menu.AddSeparator();

            //------------------------------JungleClear-------------------------------

            Menu.AddGroupLabel("JungleClear");
            {
                Menu.Add("UseQJungleClear", new CheckBox("Use Q JungleClear"));
                Menu.Add("UseEJungleClear", new CheckBox("Use E JungleClear"));
                Menu.Add("JungleClear, Mana %", new Slider("JungleClear, Mana %", 30, 1, 100));
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
            EOMenu.Add("Q/WOnlyCombo", new CheckBox("Just evade on combo ?", false));
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

        //------------------------------------Orbwalker_OnPostAttack()----------------------------------------

        static void Orbwalker_OnPostAttack(AttackableUnit target, EventArgs args)
        {
            if (target != null && target.IsValidTarget())
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

                if (Q.IsReady() && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear) && Menu["UseQJungleClear"].Cast<CheckBox>().CurrentValue)
                {
                    if (Player.ManaPercent < Menu["JungleClear, Mana %"].Cast<Slider>().CurrentValue) return;

                    var monster = EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Position, Q.Range).FirstOrDefault();

                    if (monster != null) Q.Cast(monster);

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

        //------------------------------------------WaitAndBleed()--------------------------------------------

        static void WaitAndBleed()
        {
            Obj_AI_Minion minion = ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(it => it.IsValidTarget(Q.Range) && !it.IsDead);
            if (minion != default(Obj_AI_Minion)) { Q.Cast(minion); }
            return;
        }

        //----------------------------------------------Dodge()-----------------------------------------------

        static void Dodge()
        {
            if (Target != null)
            {
                if (Q.IsInRange(Target))
                {
                    Q.Cast(Target);
                    return;
                }
            }

            var champ = EntityManager.Heroes.Enemies.FirstOrDefault(it => it.IsValidTarget(Q.Range));

            if (champ != null) { Q.Cast(champ); return; }

            var minion = EntityManager.MinionsAndMonsters.EnemyMinions.FirstOrDefault(it => it.IsValidTarget(Q.Range));

            if (minion != null) { Q.Cast(minion); return; }

            return;
        }

        //---------------------------------AIHeroClient_OnProcessSpellCast------------------------------------

        static void AIHeroClient_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (EOMenu["Q/WOnlyCombo"].Cast<CheckBox>().CurrentValue && !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)) return;

            if (sender.IsValidTarget() && sender.IsEnemy && MenuSpells.Any(el => el == args.SData.Name) && Player.Distance(sender) <= args.SData.CastRange)
            {
                if (Q.IsReady() && (EOMenu[args.SData.Name].Cast<Slider>().CurrentValue == 1 || EOMenu[args.SData.Name].Cast<Slider>().CurrentValue == 3))
                {
                    if (args.SData.Name == "JaxCounterStrike") { Core.DelayAction(() => Dodge(), 2000 - Game.Ping - 100); return; }

                    if (args.SData.Name == "KarthusFallenOne") { Core.DelayAction(() => Dodge(), 3000 - Game.Ping - 100); return; }

                    if (args.SData.Name == "ZedR" && args.Target.IsMe) { Core.DelayAction(() => Dodge(), 750 - Game.Ping - 100); return; }

                    if (args.SData.Name == "SoulShackles") { Core.DelayAction(() => Dodge(), 3000 - Game.Ping - 100); return; }

                    if (args.SData.Name == "AbsoluteZero") { Core.DelayAction(() => Dodge(), 3000 - Game.Ping - 100); return; }

                    if (args.SData.Name == "NocturneUnspeakableHorror" && args.Target.IsMe) { Core.DelayAction(() => Dodge(), 2000 - Game.Ping - 100); return; }

                    Core.DelayAction(delegate
                    {
                        if (Target != null && Target.IsValidTarget(Q.Range)) Q.Cast(Target);

                    }, (int)args.SData.SpellCastTime - Game.Ping - 100);

                    return;
                }
                
                else if (W.IsReady() && Player.IsFacing(sender) && EOMenu[args.SData.Name].Cast<Slider>().CurrentValue > 1 && ( args.Target.IsMe || new Geometry.Polygon.Rectangle(args.Start, args.End, args.SData.LineWidth).IsInside(Player) || new Geometry.Polygon.Circle(args.End, args.SData.CastRadius).IsInside(Player)) )
                {
                    int delay = (int)(Player.Distance(sender) / ((args.SData.MissileMaxSpeed + args.SData.MissileMinSpeed) / 2) * 1000) - 150 + (int)args.SData.SpellCastTime;

                    if (args.SData.Name != "ZedR" && args.SData.Name != "NocturneUnpeakableHorror")
                    {
                        Core.DelayAction(() => W.Cast(), delay);
                        if (Target != null) Core.DelayAction(() => EloBuddy.Player.IssueOrder(GameObjectOrder.AttackTo, Target), delay + 100);
                    }
                    return;
                }
            }
            return;
        }

        //---------------------------------------Gapcloser_OnGapcloser----------------------------------------

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

        //-------------------------------------------class Modes------------------------------------------------

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

                if (Target.IsValidTarget(350) && Tiamat.IsReady()) Tiamat.Cast();

                if (Target.IsValidTarget(100) && Titanic.IsReady() && Titanic.Cast()) Orbwalker.ResetAutoAttack();

                if (Target.IsValidTarget(350) && Hydra.IsReady()) Hydra.Cast();

                if (Target.IsValidTarget(450) && Randuin.IsReady()) Randuin.Cast();

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
                }

                if (Hydra.IsReady())
                {
                    bool UseItem = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Player.Position, Hydra.Range).Count() >= 3;
                    if (UseItem) Hydra.Cast();
                }

                return;
            }

            //-------------------------------------------LaneClear()-----------------------------------------------

            public static void JungleClear()
            {
                if (E.IsReady() && Menu["UseEJungleClear"].Cast<CheckBox>().CurrentValue && EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Position, Player.GetAutoAttackRange()).Any()) E.Cast();

                if (Tiamat.IsReady())
                {
                    bool UseItem = EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Position, Hydra.Range).Count() >= 2;
                    if (UseItem) Tiamat.Cast();
                }

                if (Hydra.IsReady())
                {
                    bool UseItem = EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Position, Hydra.Range).Count() >= 2;
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
                    if (target.IsMinion)
                        return Player.CalculateDamageOnUnit(target, DamageType.Physical, new[] { 0, 100, 160, 220, 280, 340 }[Q.Level] + Player.TotalAttackDamage, true, true);
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
>>>>>>> 1b8d27d181f94585e47e58f0f7e61f7f8a3f0352
        }

    }
}
