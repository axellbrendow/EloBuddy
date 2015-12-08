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

namespace WuJax
{
    static class Program
    {
        static Version AssVersion;//Kappa
        static readonly String CN = "Jax";
        static Spell.Targeted Smite = null;
        static Spell.Targeted Ignite = null;
        static AIHeroClient Player = EloBuddy.Player.Instance;
        static ColorBGRA Green = new ColorBGRA(Color.Green.R, Color.Green.G, Color.Green.B, Color.Green.A);
        static ColorBGRA Red = new ColorBGRA(Color.Red.R, Color.Red.G, Color.Red.B, Color.Red.A);

        static Item BOTRK, Hextech, GhostBlade, Tiamat, Hydra, Bilgewater, Randuin, Scimitar, QSS;
        static Menu Menu;
        static AIHeroClient Target = null;
        static readonly Spell.Targeted Q = new Spell.Targeted(SpellSlot.Q, 700);
        static readonly Spell.Active W = new Spell.Active(SpellSlot.W);
        static readonly Spell.Active E = new Spell.Active(SpellSlot.E, 187);
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
                Menu.Add("QOnDash", new CheckBox("Enemy AA range ? just Q on dash!"));
                Menu.Add("UseWCombo", new CheckBox("Use W Combo"));
                Menu.Add("UseWAARCombo", new CheckBox("W AA Reset"));
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
                Menu.Add("UseWAARHarass", new CheckBox("W AA Reset"));
                Menu.Add("UseEHarass", new CheckBox("Use E Harass"));
                Menu.Add("Harass, Mana %", new Slider("Harass, Mana %", 30, 1, 100));
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

            //------------------------------LaneClear-------------------------------

            Menu.AddGroupLabel("LaneClear");
            {
                Menu.Add("UseQLaneClear", new CheckBox("Use Q LaneClear"));
                Menu.Add("JustQIMWD", new CheckBox("Just Q if minion will die"));
                Menu.Add("UseWLaneClear", new CheckBox("Use W LaneClear"));
                Menu.Add("Min Minions E", new Slider("Min Minions E", 3, 1, 10));
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
                Menu.Add("DrawE", new CheckBox("Draw E"));
                if (Smite != null) Menu.Add("DrawSmite", new CheckBox("DrawSmite"));
                Menu.Add("ComboDamage on HPBar", new CheckBox("ComboDamage on HPBar"));
            }
            Menu.AddSeparator();

            //------------------------------Other things-------------------------------

            Menu.AddGroupLabel("Other things");

            Menu.Add("KS", new CheckBox("KS"));
            Menu.Add("Auto Ignite", new CheckBox("Auto Ignite"));
            Menu.Add("WardJump", new KeyBind("Ward Jump", false, KeyBind.BindTypes.HoldActive, 'T'));
            Menu.Add("Buffs", new KeyBind("Buffs", false, KeyBind.BindTypes.HoldActive, 'J'));

            Menu.AddSeparator();

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnEndScene += Drawing_OnEndScene;
            Orbwalker.OnPostAttack += Orbwalker_OnPostAttack;

            Chat.Print("Wu" + CN + " Loaded, [By WujuSan] , Version: " + AssVersion);
        }

        //--------------------------------------Orbwalker_OnPostAttack()------------------------------------------

        static void Orbwalker_OnPostAttack(AttackableUnit target, EventArgs args)
        {
            if (target.IsValidTarget())
            {
                if (W.IsReady() && Player.Distance(target) <= Player.GetAutoAttackRange() - 30)
                {
                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && Menu["UseWAARCombo"].Cast<CheckBox>().CurrentValue)
                    {
                        W.Cast();
                        Orbwalker.ResetAutoAttack();
                        EloBuddy.Player.IssueOrder(GameObjectOrder.AttackTo, target);
                    }

                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass) && Menu["UseWAARHarass"].Cast<CheckBox>().CurrentValue)
                    {
                        W.Cast();
                        Orbwalker.ResetAutoAttack();
                        EloBuddy.Player.IssueOrder(GameObjectOrder.AttackTo, target);
                    }

                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) && Menu["UseWLaneClear"].Cast<CheckBox>().CurrentValue && Player.ManaPercent >= Menu["LaneClear, Mana %"].Cast<Slider>().CurrentValue)
                    {
                        W.Cast();
                        Orbwalker.ResetAutoAttack();
                        EloBuddy.Player.IssueOrder(GameObjectOrder.AttackTo, target);
                    }

                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear) && Player.ManaPercent >= Menu["LaneClear, Mana %"].Cast<Slider>().CurrentValue)
                    {
                        W.Cast();
                        Orbwalker.ResetAutoAttack();
                        EloBuddy.Player.IssueOrder(GameObjectOrder.AttackTo, target);
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

            if (Menu["Buffs"].Cast<KeyBind>().CurrentValue)
            {
                foreach (BuffInstance buff in Player.Buffs)
                {
                    Chat.Print(buff.Name);
                }
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

            //---------------------------------------------Ward Jump---------------------------------------------

            if (Menu["WardJump"].Cast<KeyBind>().CurrentValue && Q.IsReady())
            {
                var CursorPos = Game.CursorPos;

                Obj_AI_Base JumpPlace = EntityManager.Heroes.Allies.FirstOrDefault( it => it.Distance(CursorPos) <= 150 && Q.IsInRange(it) );

                if (JumpPlace != default(Obj_AI_Base)) Q.Cast(JumpPlace);
                else
                {
                    JumpPlace = EntityManager.MinionsAndMonsters.Minions.FirstOrDefault( it => it.Distance(CursorPos) <= 150 && Q.IsInRange(it) );

                    if (JumpPlace != default(Obj_AI_Base)) Q.Cast(JumpPlace);
                    else if (JumpWard() != default(InventorySlot))
                    {
                        JumpWard().Cast( Player.Position.Extend(CursorPos, 600).To3D() );
                    }
                }

            }

            //--------------------------------------------Orbwalker Modes-------------------------------------------

            if (Target != null)
            {
                if (Target.IsValidTarget())
                {
                    bool QRange = Target.IsValidTarget(Q.Range);

                    //-----------------------------------------------KS----------------------------------------

                    if (Menu["KS"].Cast<CheckBox>().CurrentValue)
                    {
                        AIHeroClient bye = null;

                        if (Q.IsReady())
                        {
                            bye = EntityManager.Heroes.Enemies.FirstOrDefault(enemy => enemy.IsValidTarget(Q.Range) && SpellDamage(enemy, SpellSlot.Q) >= enemy.Health);
                            if (bye != null) Q.Cast(bye);
                        }

                        if (W.IsReady() && bye == null)
                        {
                            bye = EntityManager.Heroes.Enemies.FirstOrDefault(enemy => enemy.IsInAutoAttackRange(Player) && SpellDamage(enemy, SpellSlot.W) >= enemy.Health);
                            if (bye != null) { W.Cast(); EloBuddy.Player.IssueOrder(GameObjectOrder.AttackTo, bye); }

                            if (Smite != null && bye == null)
                            {
                                if (Smite.Name.Contains("gank") && Smite.IsReady())
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

                        //---------------------------------------------------Combo--------------------------------------------

                        if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)) Combo();

                        //---------------------------------------------------Harass--------------------------------------------

                        if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
                        {
                            if (Player.ManaPercent >= Menu["Harass, Mana %"].Cast<Slider>().CurrentValue)
                            {
                                if (E.IsReady() && !Player.HasBuff("jaxcounterstrike") && Target.IsValidTarget(745) && (Player.MoveSpeed - Target.MoveSpeed) >= 25) E.Cast();

                                if (QRange)
                                {
                                    if (Menu["UseEHarass"].Cast<CheckBox>().CurrentValue && E.IsReady() && !Player.HasBuff("jaxcounterstrike")) E.Cast();
                                    if (Menu["UseQHarass"].Cast<CheckBox>().CurrentValue && Q.IsReady()) Q.Cast(Target);
                                    if (Menu["UseWHarass"].Cast<CheckBox>().CurrentValue && W.IsReady() && !Menu["UseWAARHarass"].Cast<CheckBox>().CurrentValue) W.Cast();
                                }
                            }
                        }
                    }
                    else Target = null;
                }

                //---------------------------------------------------LastHit--------------------------------------------

                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
                {
                    if (Player.ManaPercent >= Menu["LastHit, Mana %"].Cast<Slider>().CurrentValue)
                    {
                        if (Menu["UseQLastHit"].Cast<CheckBox>().CurrentValue && Q.IsReady())
                        {
                            var QMinion = EntityManager.MinionsAndMonsters.EnemyMinions.FirstOrDefault( it => it.IsValidTarget(Q.Range) && SpellDamage(it, SpellSlot.Q) >= it.Health && !Player.IsInAutoAttackRange(it) );
                            if (QMinion != null) Q.Cast(QMinion);
                        }

                        if (Menu["UseWLastHit"].Cast<CheckBox>().CurrentValue && W.IsReady())
                        {
                            var WMinion = EntityManager.MinionsAndMonsters.EnemyMinions.FirstOrDefault(it => it.IsValidTarget(Player.GetAutoAttackRange()) && SpellDamage(it, SpellSlot.W) >= it.Health && it.Health > Player.GetAutoAttackDamage(it) );
                            if (WMinion != null) { W.Cast(); EloBuddy.Player.IssueOrder(GameObjectOrder.AttackTo, WMinion); }
                        }
                    }
                }

                //---------------------------------------------------LaneClear--------------------------------------------

                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
                {
                    if (Player.ManaPercent >= Menu["LaneClear, Mana %"].Cast<Slider>().CurrentValue) LaneClear();

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
        }

        //---------------------------------------------JumpWard()--------------------------------------------------

        static InventorySlot JumpWard()
        {
            var Inventory = Player.InventoryItems;

            if (Item.CanUseItem(3340)) return Inventory.First( it => it.Id == ItemId.Warding_Totem_Trinket );
            if (Item.CanUseItem(2049)) return Inventory.First( it => it.Id == ItemId.Sightstone );
            if (Item.CanUseItem(2045)) return Inventory.First( it => it.Id == ItemId.Ruby_Sightstone );
            if (Item.CanUseItem(2301)) return Inventory.First( it => (int)it.Id == 2301 );
            if (Item.CanUseItem(2302)) return Inventory.First( it => (int)it.Id == 2302 );
            if (Item.CanUseItem(2303)) return Inventory.First( it => (int)it.Id == 2303 );
            if (Item.CanUseItem(2043)) return Inventory.First( it => it.Id == ItemId.Vision_Ward );

            return default(InventorySlot);
        }

        //---------------------------------------------LaneClear()------------------------------------------------

        static void LaneClear()
        {
            if (Q.IsReady() && Menu["UseQLaneClear"].Cast<CheckBox>().CurrentValue)
            {
                Obj_AI_Minion QMinion = null;

                if (Menu["JustQIMWD"].Cast<CheckBox>().CurrentValue)
                {
                    QMinion = EntityManager.MinionsAndMonsters.EnemyMinions.FirstOrDefault(it => it.IsValidTarget(Q.Range) && SpellDamage(it, SpellSlot.Q) >= it.Health);
                    if (QMinion != null) Q.Cast(QMinion);
                }
                else
                {
                    QMinion = EntityManager.MinionsAndMonsters.EnemyMinions.FirstOrDefault( it => it.IsValidTarget(Q.Range) );
                    if (QMinion != null) Q.Cast(QMinion);
                }
            }

            if (E.IsReady() && Menu["UseELaneClear"].Cast<CheckBox>().CurrentValue)
            {
                var EMinions = EntityManager.MinionsAndMonsters.EnemyMinions.Where( it => it.IsValidTarget(E.Range + 100) );
                if (EMinions.Any())
                {
                    if (EMinions.Count() >= Menu["Min Minions E"].Cast<Slider>().CurrentValue) E.Cast();
                }
            }
        }

        //---------------------------------------------Combo()------------------------------------------------

        static void Combo()
        {
            bool AARange = Player.IsInAutoAttackRange(Target);

            if ((Scimitar.IsReady() || QSS.IsReady()) && Player.HasBuffOfType(BuffType.Charm) || Player.HasBuffOfType(BuffType.Blind) || Player.HasBuffOfType(BuffType.Fear) || Player.HasBuffOfType(BuffType.Polymorph) || Player.HasBuffOfType(BuffType.Silence) || Player.HasBuffOfType(BuffType.Sleep) || Player.HasBuffOfType(BuffType.Snare) || Player.HasBuffOfType(BuffType.Stun) || Player.HasBuffOfType(BuffType.Suppression) || Player.HasBuffOfType(BuffType.Taunt)) { Scimitar.Cast(); QSS.Cast(); }

            if (E.IsReady() && !Player.HasBuff("jaxcounterstrike"))
            {
                if (Target.IsValidTarget(745) && (Player.MoveSpeed - Target.MoveSpeed) >= 25) E.Cast();
                else if (Target.IsValidTarget(Q.Range)) E.Cast();
            }

            if (R.IsReady())
            {
                if (Player.Distance(Target) <= Target.GetAutoAttackRange() && (Target.HealthPercent >= 30 || Player.HealthPercent <= 50)) R.Cast();
                else if (Player.CountEnemiesInRange(650) >= Menu["Min Enemies R"].Cast<Slider>().CurrentValue) R.Cast();
            }
            
            if (Menu["UseQCombo"].Cast<CheckBox>().CurrentValue && Q.IsReady() && Target.IsValidTarget(Q.Range))
            {
                if (Menu["QOnDash"].Cast<CheckBox>().CurrentValue && Player.Distance(Target) <= Player.GetAutoAttackRange() + 100)
                {
                    if (Target.IsDashing()) Q.Cast(Target);
                }
                else Q.Cast(Target);
            }

            if (Smite != null)
            {
                if (Target.IsValidTarget(Smite.Range) && Smite.IsReady())
                {
                    if (Smite.Name.Contains("gank")) Smite.Cast(Target);
                    else if (Smite.Name.Contains("duel") && AARange) Smite.Cast(Target);
                }
            }

            if (Target.IsValidTarget(Q.Range) && GhostBlade.IsReady()) GhostBlade.Cast();

            if (Target.IsValidTarget(550) && BOTRK.IsReady()) BOTRK.Cast(Target);

            if (Target.IsValidTarget(550) && Bilgewater.IsReady()) Bilgewater.Cast(Target);

            if (Target.IsValidTarget(400) && Tiamat.IsReady()) Tiamat.Cast();

            if (Target.IsValidTarget(400) && Hydra.IsReady()) Hydra.Cast();

            if (Target.IsValidTarget(500) && Randuin.IsReady()) Randuin.Cast();

            if (Target.IsValidTarget(700) && Hextech.IsReady()) Hextech.Cast(Target);
        }

        //---------------------------------------------SpellDamage()-----------------------------------------------

        static float SpellDamage(Obj_AI_Base target, SpellSlot slot)
        {
            switch (slot)
            {
                case SpellSlot.Q:
                    return Player.CalculateDamageOnUnit(target, DamageType.Physical, new float[] { 70, 110, 150, 190, 230 }[Q.Level - 1] + Player.FlatPhysicalDamageMod + 0.6f * Player.TotalMagicalDamage, true, true);

                case SpellSlot.W:
                    return Player.CalculateDamageOnUnit(target, DamageType.Magical, new float[] { 40, 75, 110, 145, 180 }[W.Level - 1] + 0.6f * Player.TotalMagicalDamage, true, true);

                case SpellSlot.E:
                    return Player.CalculateDamageOnUnit(target, DamageType.Physical, new float[] { 50, 75, 100, 125, 150 }[E.Level - 1] + Player.FlatPhysicalDamageMod/2);

                default:
                    return 0;
            }
        }

        //---------------------------------GetComboDamage(Obj_AI_Hero Target)----------------------------------

        static float GetComboDamage(AIHeroClient target)
        {
            if (target != null)
            {
                float ComboDamage = new float();

                ComboDamage = Q.IsReady() ? SpellDamage(target, SpellSlot.Q) : 0;
                ComboDamage = W.IsReady() ? SpellDamage(target, SpellSlot.W) : 0;
                ComboDamage += E.IsReady() ? SpellDamage(target, SpellSlot.E) : 0;
                ComboDamage += Player.GetAutoAttackDamage(target) * 2;
                ComboDamage += Bilgewater.IsReady() ? DamageLibrary.GetItemDamage(Player, target, ItemId.Bilgewater_Cutlass) : 0;
                ComboDamage += BOTRK.IsReady() ? DamageLibrary.GetItemDamage(Player, target, ItemId.Blade_of_the_Ruined_King) : 0;
                ComboDamage += Hydra.IsReady() ? DamageLibrary.GetItemDamage(Player, target, ItemId.Ravenous_Hydra_Melee_Only) : 0;
                ComboDamage += Tiamat.IsReady() ? DamageLibrary.GetItemDamage(Player, target, ItemId.Tiamat_Melee_Only) : 0;
                ComboDamage += Hextech.IsReady() ? DamageLibrary.GetItemDamage(Player, target, ItemId.Hextech_Gunblade) : 0;

                if (Ignite != null) ComboDamage += Convert.ToSingle(Ignite.IsReady() ? DamageLibrary.GetSummonerSpellDamage(Player, target, DamageLibrary.SummonerSpells.Ignite) : 0);
                if (Smite != null) ComboDamage += Convert.ToSingle(Smite.IsReady() && Smite.Name.Contains("Gank") ? DamageLibrary.GetSummonerSpellDamage(Player, target, DamageLibrary.SummonerSpells.Smite) : 0);

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
                    string Text = new WebClient().DownloadString("https://raw.githubusercontent.com/WujuSan/EloBuddy/master/Wu" + CN + " /Wu" + CN + "/Properties/AssemblyInfo.cs");

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
