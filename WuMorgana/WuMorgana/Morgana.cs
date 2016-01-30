using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using SharpDX;
using WuAIO.Bases;
using WuAIO.Managers;
using WuAIO.Extensions;
using Circle = EloBuddy.SDK.Rendering.Circle;
using ScalingTypes = WuAIO.Managers.DamageManager.ScalingTypes;

namespace WuAIO
{
    class Morgana : HeroBase
    {
        readonly AIHeroClient Player = EloBuddy.Player.Instance;
        Menu EMenu;

        List<string> MenuSpells = new List<string>();
        List<string> CollisionSpells = new List<string>() { "TahmKenchQ", "JinxW", "IllaoiE", "HeimerdingerUltWDummySpell", "HeimerdingerW", "EliseHumanE", "InfectedMissileCleaverCast", "MissileBarrage", "BraumQ", "BardQ", "AhriSeduce", "EnchantedCrystalArrow", "EzrealEssenceFlux", "FizzMarinerDoom", "GnarBigQMissile", "GragasE", "LuxLightBinding", "VarusR", "ThreshQ", "SejuaniGlacialPrisonCast", "JavelinToss", "NautilusAnchorDrag", "DarkBindingMissile", "RocketGrab", "RocketGrabMissile", "LissandraQ" };
        List<string> ESpells = new List<string>() { "SorakaQ", "SorakaE", "TahmKenchW", "TahmKenchQ", "Bushwhack", "ForcePulse", "KarthusFallenOne", "KarthusWallOfPain", "KarthusLayWasteA1", "KarmaWMantra", "KarmaQMissileMantra", "KarmaSpiritBind", "KarmaQ", "JinxW", "JinxE", "JarvanIVGoldenAegis", "HowlingGaleSpell", "SowTheWind", "ReapTheWhirlwind", "IllaoiE", "HeimerdingerUltWDummySpell", "HeimerdingerUltEDummySpell", "HeimerdingerW", "HeimerdingerE", "HecarimUlt", "HecarimRampAttack", "GravesQLineSpell", "GravesQLineMis", "GravesClusterShot", "GravesSmokeGrenade", "GangplankR", "GalioIdolOfDurand", "GalioResoluteSmite", "FioraE", "EvelynnR", "EliseHumanE", "EkkoR", "EkkoW", "EkkoQ", "DravenDoubleShot", "InfectedCleaverMissileCast", "DariusExecute", "DariusAxeGrabCone", "DariusNoxianTacticsONH", "DariusCleave", "PhosphorusBomb", "MissileBarrage", "BraumQ", "BrandFissure", "BardR", "BardQ", "AatroxQ", "AatroxE", "AzirE", "AzirEWrapper", "AzirQWrapper", "AzirQ", "AzirR", "Pulverize", "AhriSeduce", "CurseoftheSadMummy", "InfernalGuardian", "Incinerate", "Volley", "EnchantedCrystalArrow", "BraumRWrapper", "CassiopeiaPetrifyingGaze", "FeralScream", "Rupture", "EzrealEssenceFlux", "EzrealMysticShot", "EzrealTrueshotBarrage", "FizzMarinerDoom", "GnarW", "GnarBigQMissile", "GnarQ", "GnarR", "GragasQ", "GragasE", "GragasR", "RiftWalk", "LeblancSlideM", "LeblancSlide", "LeonaSolarFlare", "UFSlash", "LuxMaliceCannon", "LuxLightStrikeKugel", "LuxLightBinding", "yasuoq3w", "VelkozE", "VeigarEventHorizon", "VeigarDarkMatter", "VarusR", "ThreshQ", "ThreshE", "ThreshRPenta", "SonaQ", "SonaR", "ShenShadowDash", "SejuaniGlacialPrisonCast", "RivenMartyr", "JavelinToss", "NautilusSplashZone", "NautilusAnchorDrag", "NamiR", "NamiQ", "DarkBindingMissile", "StaticField", "RocketGrab", "RocketGrabMissile", "timebombenemybuff", "karthusfallenonetarget", "NocturneUnspeakableHorror", "SyndraQ", "SyndraE", "SyndraR", "VayneCondemn", "Dazzle", "Overload", "AbsoluteZero", "IceBlast", "LeblancChaosOrb", "JudicatorReckoning", "KatarinaQ", "NullLance", "Crowstorm", "FiddlesticksDarkWind", "BrandWildfire", "Disintegrate", "FlashFrost", "Frostbite", "AkaliMota", "InfiniteDuress", "PantheonW", "blindingdart", "JayceToTheSkies", "IreliaEquilibriumStrike", "maokaiunstablegrowth", "nautilusgandline", "runeprison", "WildCards", "BlueCardAttack", "RedCardAttack", "GoldCardAttack", "AkaliShadowDance", "Headbutt", "PowerFist", "BrandConflagration", "CaitlynYordleTrap", "CaitlynAceintheHole", "CassiopeiaNoxiousBlast", "CassiopeiaMiasma", "CassiopeiaTwinFang", "Feast", "DianaArc", "DianaTeleport", "EliseHumanQ", "EvelynnE", "Terrify", "FizzPiercingStrike", "Parley", "GarenQAttack", "GarenR", "IreliaGatotsu", "IreliaEquilibriumStrike", "SowTheWind", "JarvanIVCataclysm", "JaxLeapStrike", "JaxEmpowerTwo", "JaxCounterStrike", "JayceThunderingBlow", "KarmaSpiritBind", "NetherBlade", "KatarinaR", "JudicatorRighteousFury", "KennenBringTheLight", "LeblancChaosOrbM", "BlindMonkRKick", "LeonaZenithBlade", "LeonaShieldOfDaybreak", "LissandraW", "LissandraQ", "LissandraR", "LuluQ", "LuluW", "LuluE", "LuluR", "SeismicShard", "AlZaharMaleficVisions", "AlZaharNetherGrasp", "MaokaiUnstableGrowth", "MordekaiserMaceOfSpades", "MordekaiserChildrenOfTheGrave", "SoulShackles", "NamiW", "NasusW", "NautilusGrandLine", "Takedown", "NocturneParanoia", "PoppyDevastatingBlow", "PoppyHeroicCharge", "QuinnE", "PuncturingTaunt", "RenektonPreExecute", "SpellFlux", "SejuaniWintersClaw", "TwoShivPoisen", "Fling", "SkarnerImpale", "SonaHymnofValor", "SwainTorment", "SwainDecrepify", "BlindingDart", "OrianaIzunaCommand", "OrianaDetonateCommand", "DetonatingShot", "BusterShot", "TrundleTrollSmash", "TrundlePain", "MockingShout", "Expunge", "UdyrBearStance", "UrgotHeatseekingLineMissile", "UrgotSwap2", "VeigarBalefulStrike", "VeigarPrimordialBurst", "ViR", "ViktorPowerTransfer", "VladimirTransfusion", "VolibearQ", "HungeringStrike", "XenZhaoComboTarget", "XenZhaoSweep", "YasuoQ3W", "YasuoQ3Mis", "YasuoQ3", "YasuoRKnockUpComboW" };

        Spell.Skillshot Q = new Spell.Skillshot(SpellSlot.Q, 1100, SkillShotType.Linear, 250, 1200, 70);
        Spell.Skillshot W = new Spell.Skillshot(SpellSlot.W, 900, SkillShotType.Circular, 250, 2200, 280);
        Spell.Targeted E = new Spell.Targeted(SpellSlot.E, 750);
        Spell.Active R = new Spell.Active(SpellSlot.R, 625);

        public override void CreateMenu()
        {
            base.CreateMenu();

            var menu = MenuManager.AddSubMenu("Drawings");
            {
                menu.NewCheckbox("disable", "Disable", false);
                menu.NewCheckbox("damageindicator", "Damage Indicator");
                menu.NewCheckbox("r", "R");
                menu.NewCheckbox("q", "Q");
                menu.NewCheckbox("e", "E");
                menu.NewCheckbox("w", "W", false);
            }

            menu = MenuManager.AddSubMenu("Combo");
            {
                menu.NewCheckbox("q", "Q");
                menu.NewSlider("q.hitchance%", "Q HitChance%", 85, 1, 100);
                menu.NewCheckbox("w", "W");
                menu.NewCheckbox("r", "R", true);
                menu.NewSlider("r.minenemies", "Min enemies R", 2, 1, 5);
            }

            menu = MenuManager.AddSubMenu("Harass");
            {
                menu.NewCheckbox("q", "Q");
                menu.NewSlider("q.hitchance%", "Q HitChance%", 85, 1, 100);
                menu.NewCheckbox("w", "W", true, true);
                menu.NewCheckbox("w.jitii", "Just W if target is immobile");
                menu.NewSlider("mana%", "Min mana%", 30, 1, 99, true);
            }

            menu = MenuManager.AddSubMenu("Lane Clear");
            {
                menu.NewCheckbox("w", "W");
                menu.NewSlider("w.minminions", "Min minions W", 3, 1, 7);
                menu.NewSlider("mana%", "Min mana%", 30, 1, 99, true);
            }

            menu = MenuManager.AddSubMenu("Jungle Clear");
            {
                menu.NewCheckbox("q", "Q");
                menu.NewCheckbox("w", "W");
                menu.NewSlider("mana%", "Min mana%", 30, 1, 99, true);
            }

            menu = MenuManager.AddSubMenu("Flee");
            {
                menu.NewCheckbox("q", "Q");
            }

            menu = MenuManager.AddSubMenu("Misc");
            {
                menu.NewCheckbox("ks", "KS");
                menu.NewCheckbox("aaminionswhenallynear", "AA minions when ally near?");
                menu.NewCheckbox("autoqonimmobile", "Auto Q on immobile enemies", true);
                menu.NewCheckbox("autoqonflash", "Auto Q on flash", true);
                menu.NewCheckbox("autoqondash", "Auto Q on dash", true);
                menu.NewCheckbox("autowonimmobile", "Auto W on immobile enemies", true);
                menu.NewCheckbox("gapcloser", "Q on enemy gapcloser", true);
            }

            //---------------------------------------||   EMenu   ||------------------------------------------

            EMenu = MenuManager.AddSubMenu("E Options");
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
        }

        public override void CreateVariables()
        {
            new SkinManager(8);

            Q.MinimumHitChance = HitChance.Medium;
            W.AllowedCollisionCount = int.MaxValue;
            
            damageManager = new DamageManager
            (
                new List<Bases.Damage>()
                {
                    new Bases.Damage
                    (
                        Q, DamageType.Magical, new float[] { 0, 80, 135, 190, 245, 300 },
                        
                        new List<Scale>()
                        {
                            new Scale(new float[] { 0, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f }, ScalingTypes.AP)
                        }
                    ),

                    new Bases.Damage
                    (
                        W, DamageType.Magical, new float[] { 0, 80, 160, 240, 320, 400 },

                        new List<Scale>()
                        {
                            //The true value is 1.1f and not 0.8f, really ? 5 seconds on a tormented soil ?
                            new Scale(new float[] { 0, 0.8f, 0.8f, 0.8f, 0.8f, 0.8f }, ScalingTypes.AP)
                        }
                    ),

                    new Bases.Damage
                    (
                        R, DamageType.Magical, new float[] { 0, 150, 225, 300 },

                        new List<Scale>()
                        {
                            new Scale(new float[] { 0, 0.7f, 0.7f, 0.7f, 0.7f, 0.7f }, ScalingTypes.AP)
                        }
                    )
                }
            );
        }

        public override void PermaActive()
        {
            base.PermaActive();

            //----Auto Q/W on immobile enemies
            
            var immobile = EntityManager.Heroes.Enemies.FirstOrDefault(it => !it.IsDead && it.IsValidTarget(Q.Range) && !it.CanMove());

            if (immobile != null)
            {
                if (misc.IsActive("autoqonimmobile") && Q.IsReady()) Q.HitChanceCast(immobile);
                if (misc.IsActive("autowonimmobile") && W.IsReady() && W.IsInRange(immobile)) W.Cast(immobile.ServerPosition);
            }
        }

        public override void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead || draw.IsActive("disable")) return;

            if (draw.IsActive("r"))
                Circle.Draw(R.IsReady() ? Color.Blue : Color.Red, R.Range, Player.Position);

            if (draw.IsActive("q"))
                Circle.Draw(Q.IsReady() ? Color.Blue : Color.Red, Q.Range, Player.Position);

            if (draw.IsActive("e"))
                Circle.Draw(E.IsReady() ? Color.Blue : Color.Red, E.Range, Player.Position);

            if (draw.IsActive("w"))
                Circle.Draw(W.IsReady() ? Color.Blue : Color.Red, W.Range, Player.Position);

        }

        public override void Combo()
        {
            var Target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);

            if (Target == null || !Target.IsValidTarget()) return;

            if (R.IsReady() && Player.CountEnemiesInRange(R.Range) >= combo.Value("r.minenemies"))
                R.Cast();

            if (Q.IsReady() && Target.IsValidTarget(Q.Range) && combo.IsActive("q"))
                Q.HitChanceCast(Target, combo.Value("q.hitchance%"));

            if (W.IsReady() && !Target.CanMove() && combo.IsActive("w")) W.Cast(Target.ServerPosition);

            return;
        }

        public override void Harass()
        {
            var Target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);

            if (Target == null || !Target.IsValidTarget()) return;

            if (Player.ManaPercent < harass.Value("mana%")) return;

            if (Q.IsReady() && Target.IsValidTarget(Q.Range) && harass.IsActive("q"))
                Q.HitChanceCast(Target, harass.Value("q.hitchance%"));

            if (W.IsReady() && harass.IsActive("w"))
            {
                if (harass.IsActive("w.jitii"))
                {
                    if (!Target.CanMove()) W.Cast(Target.ServerPosition);
                }
                else
                {
                    W.HitChanceCast(Target);
                }
            }

            return;
        }

        public override void LaneClear()
        {
            if (!W.IsReady() || !laneclear.IsActive("w") || Player.ManaPercent <= laneclear.Value("mana%")) return;

            var Minions = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Player.Position, W.Range);

            if (Minions != null)
            {
                var FL = EntityManager.MinionsAndMonsters.GetCircularFarmLocation(Minions, 280, (int)W.Range);

                if (FL.HitNumber >= laneclear.Value("w.minminions")) W.Cast(FL.CastPosition);
            }

            return;
        }

        public override void Flee()
        {
            if (!Q.IsReady() || !flee.IsActive("q")) return;

            var Target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);

            if (Target != null && Target.IsValidTarget(Q.Range)) Q.HitChanceCast(Target);
        }

        public override void KS()
        {
            if (!EntityManager.Heroes.Enemies.Any(it => !it.IsDead && it.IsValidTarget(Q.Range)) || !misc.IsActive("ks")) return;

            if (Q.IsReady())
            {
                var bye = EntityManager.Heroes.Enemies.FirstOrDefault(enemy => enemy.IsValidTarget(Q.Range) && damageManager.SpellDamage(enemy, SpellSlot.Q) >= enemy.Health);
                if (bye != null) { Q.HitChanceCast(bye); return; }
            }

            if (R.IsReady())
            {
                var bye = EntityManager.Heroes.Enemies.FirstOrDefault(enemy => enemy.IsValidTarget(R.Range) && damageManager.SpellDamage(enemy, SpellSlot.R) >= enemy.Health);
                if (bye != null) { R.Cast(); return; }
            }

            if (Q.IsReady() && R.IsReady())
            {
                var bye = EntityManager.Heroes.Enemies.FirstOrDefault(enemy => enemy.IsValidTarget(R.Range - 50) && damageManager.SpellDamage(enemy, SpellSlot.R) + damageManager.SpellDamage(enemy, SpellSlot.Q) >= enemy.Health);
                if (bye != null) { Q.HitChanceCast(bye); return; }
            }
        }

        public override void Dash_OnDash(Obj_AI_Base sender, Dash.DashEventArgs e)
        {
            if (!Q.IsReady() || !(sender.BaseSkinName == "Yasuo" && Player.Distance(e.EndPos) <= 200) || !sender.IsEnemy || Player.Distance(e.EndPos) > Q.Range || !misc.IsActive("autoqondash")) return;

            var rectangle = new Geometry.Polygon.Rectangle(Player.Position, e.EndPos, Q.Width);

            if (!EntityManager.MinionsAndMonsters.EnemyMinions.Any(it => !it.IsDead && it.IsValidTarget() && rectangle.IsInside(it)) || !(EntityManager.Heroes.Enemies.Count(enemy => !enemy.IsDead && enemy.IsValidTarget() && rectangle.IsInside(enemy)) > 0))
            {
                Q.Cast(e.EndPos);
            }
        }

        public override void Orbwalker_OnPostAttack(AttackableUnit target, EventArgs args)
        {
            if (Player.IsDead || Orbwalker.LastTarget != target) return;

            if (Player.ManaPercent < jungleclear.Value("mana%")) return;

            var minion = EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Position, W.Range).FirstOrDefault(it => it.Health > 2 * Player.GetAutoAttackDamage(it));

            if (minion == null) return;

            if (Q.IsReady() && jungleclear.IsActive("q"))
                Q.HitChanceCast(minion, 1);

            if (W.IsReady() && jungleclear.IsActive("w"))
                W.HitChanceCast(minion);
        }

        public override void AIHeroClient_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (Player.IsDead || !sender.IsEnemy || !(sender is AIHeroClient)) return;

            if (Q.IsReady() && args.SData.Name.ToLower() == "summonerflash" && args.End.Distance(Player) <= Q.Range && MenuManager.Menus[HERO_MENU]["Misc"].Keys.Last().IsActive("autoqonflash"))
            {
                //Chat.Print("{0} detected, Q on args.End", args.SData.Name);
                var rectangle = new Geometry.Polygon.Rectangle(Player.Position, args.End, Q.Width + 10);

                if (!EntityManager.MinionsAndMonsters.EnemyMinions.Any(it => rectangle.IsInside(it))) Q.Cast(args.End);
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

                            if (target != null) E.Cast(target);

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

                        PriorityCast(sender, args, Allies, rectangle);
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

                        PriorityCast(sender, args, Allies, circle);
                        return;
                    }
                }
            }

            return;
        }

        public override void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (Player.IsDead || !sender.IsEnemy || !Q.IsReady() || !sender.IsEnemy || !sender.IsValidTarget(Q.Range) || !misc.IsActive("gapcloser")) return;

            Q.HitChanceCast(sender);

            return;
        }

        public override void Orbwalker_OnPreAttack(AttackableUnit target, Orbwalker.PreAttackArgs args)
        {
            if (Player.CountAlliesInRange(2000) > 1 && !misc.IsActive("aaminionswhenallynear"))
                args.Process = false;
        }

        //------------------------------------|| Methods ||--------------------------------------

        //-------------------------------------PriorityCast-----------------------------------

        void PriorityCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args, List<AIHeroClient> Allies, Geometry.Polygon polygon)
        {
            int delay = new int();

            Allies.OrderBy(it => it.Distance(args.Start));

            var ally = Allies.First();

            if (Allies.Count == 1)
            {
                delay = (int)((uint)(((sender.Distance(ally) - 300) / args.SData.MissileMaxSpeed * 1000) + args.SData.SpellCastTime - 300 - Game.Ping));

                Core.DelayAction(delegate
                {
                    if (polygon.IsInside(ally) && ally.IsValidTarget(E.Range)) E.Cast(ally);
                    return;
                }, delay);

                //Chat.Print("Shield for {0} : {1}", sender.BaseSkinName, args.Slot.ToString());
                return;
            }
            else
            {
                if (CollisionSpells.Any(it => it == args.SData.Name))
                {
                    delay = (int)((uint)(((sender.Distance(ally) - 300) / args.SData.MissileMaxSpeed * 1000) + args.SData.SpellCastTime - Game.Ping));

                    Core.DelayAction(delegate
                    {
                        foreach (var Ally in Allies)
                        {
                            if (polygon.IsInside(Ally) && E.IsInRange(Ally)) { E.Cast(Ally); return; }
                        }
                        return;
                    }, delay);

                    //Chat.Print("Shield for {0} : {1}", sender.BaseSkinName, args.Slot.ToString());

                    return;
                }

                else
                {
                    var priorities = from aliado in EntityManager.Heroes.Allies orderby EMenu[aliado.BaseSkinName].Cast<Slider>().CurrentValue descending select aliado;

                    delay = (int)((uint)(((sender.Distance(ally) - 300) / args.SData.MissileMaxSpeed * 1000) + args.SData.SpellCastTime - 200 - Game.Ping));

                    Core.DelayAction(delegate
                    {
                        foreach (var Ally in priorities)
                        {
                            if (polygon.IsInside(Ally) && E.IsInRange(Ally)) { E.Cast(Ally); return; }
                        }
                        return;
                    }, delay);

                    //Chat.Print("Shield for {0} : {1}", sender.BaseSkinName, args.Slot.ToString());
                    return;
                }
            }
        }
    }
}