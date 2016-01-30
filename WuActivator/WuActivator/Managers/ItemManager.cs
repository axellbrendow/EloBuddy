using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using WuActivator.Util;

namespace WuActivator.Managers
{
    class ItemManager
    {
        private readonly AIHeroClient Player = EloBuddy.Player.Instance;
        private readonly List<AIHeroClient> Allies = EntityManager.Heroes.Allies;
        private readonly List<AIHeroClient> Enemies = EntityManager.Heroes.Enemies;

        private Menu _offensives { get { return MenuManager.Menus["WuActivator"]["Offensives"].Keys.First(); } }
        private Menu _defensives { get { return MenuManager.Menus["WuActivator"]["Defensives"].Keys.First(); } }
        private Menu _speed { get { return MenuManager.Menus["WuActivator"]["Speed"].Keys.First(); } }
        private Menu _potions { get { return MenuManager.Menus["WuActivator"]["Potions"].Keys.First(); } }
        private Menu _misc { get { return MenuManager.Menus["WuActivator"]["Misc"].Keys.First(); } }

        private ItemUtil _itemUtil;

        //<Itens>
        public Item
        //Offensives
        hextech, botrk, bilgewater, tiamat, hydra, titanic, youmuus,

        //Defensives
        faceMountain, mikael, solari, randuin, scimitar, qss, seraph, zhonya,

        //Speed
        talisma, righteousGlory,

        //Potions
        healthPotion, biscuitPotion, corruptingPotion, huntersPotion, refillablePotion;

        /*
        Wards
        _wardingTotem, _stealthWard, VisionWard _pinkWard, _sightstone, _rubySightstone,
        _tracersKnife, _eyeWatchers, _eyeOasis, _eyeEquinox,
        */

        public ItemManager()
        {
            //Initializing all items

            _itemUtil = new ItemUtil();
            
            #region Offensives

            hextech = _itemUtil.GetItem(ItemId.Hextech_Gunblade, 700);
            botrk = _itemUtil.GetItem(ItemId.Blade_of_the_Ruined_King, 550);
            bilgewater = _itemUtil.GetItem(ItemId.Bilgewater_Cutlass, 550);
            tiamat = _itemUtil.GetItem(ItemId.Tiamat_Melee_Only, 325);//range = 400
            hydra = _itemUtil.GetItem(ItemId.Ravenous_Hydra_Melee_Only, 325);//range = 400
            titanic = _itemUtil.GetItem(3748, 75);//range = 150 (3053)
            youmuus = _itemUtil.GetItem(ItemId.Youmuus_Ghostblade);

            #endregion

            #region Defensives

            faceMountain = _itemUtil.GetItem(ItemId.Face_of_the_Mountain, 600);
            mikael = _itemUtil.GetItem(ItemId.Mikaels_Crucible, 600);
            solari = _itemUtil.GetItem(ItemId.Locket_of_the_Iron_Solari, 600);
            randuin = _itemUtil.GetItem(ItemId.Randuins_Omen, 450);//range = 500
            scimitar = _itemUtil.GetItem(ItemId.Mercurial_Scimitar);
            qss = _itemUtil.GetItem(ItemId.Quicksilver_Sash);
            seraph = _itemUtil.GetItem(3040);
            zhonya = _itemUtil.GetItem(ItemId.Zhonyas_Hourglass);

            #endregion

            #region Speed

            talisma = _itemUtil.GetItem(ItemId.Talisman_of_Ascension);
            righteousGlory = _itemUtil.GetItem(ItemId.Righteous_Glory, 600);

            #endregion

            #region Potions

            healthPotion = _itemUtil.GetItem(ItemId.Health_Potion);
            biscuitPotion = _itemUtil.GetItem(2010);
            corruptingPotion = _itemUtil.GetItem(2033);
            huntersPotion = _itemUtil.GetItem(2032);
            refillablePotion = _itemUtil.GetItem(2031);

            #endregion
        }

        #region Activator methods

        #region Offensive Itens

        public void AutoHextechGunBlade()
        {
            if (!hextech.IsOwned() || !hextech.IsReady() || Activator.Target == null || !Activator.Target.IsValidTarget() || !_offensives.IsActive("offensives.hextech")) return;

            if (hextech.IsInRange(Activator.Target))
                hextech.Cast(Activator.Target);

            return;
        }

        public void AutoTiamatHydra()
        {
            if (!Player.IsMelee) return;

            if (tiamat.IsOwned() && tiamat.IsReady() && !_offensives.IsActive("offensives.hydra/tiamat") && tiamat.IsInRange(Activator.Target) && tiamat.Cast()) return;

            if (hydra.IsOwned() && hydra.IsReady() && !_offensives.IsActive("offensives.hydra/tiamat") && hydra.IsInRange(Activator.Target) && hydra.Cast()) return;

            return;
        }

        public void AutoTitanicHydra()
        {
            if (!titanic.IsOwned() || !titanic.IsReady() || !_offensives.IsActive("offensives.titanic")) return;

            if (titanic.IsInRange(Activator.Target) && titanic.Cast()) Orbwalker.ResetAutoAttack();

            return;
        }

        public void AutoYoumuusGhostBlade()
        {
            if (youmuus.IsOwned() && youmuus.IsReady() && _offensives.IsActive("offensives.ghostblade") && Player.IsMelee ? Player.Distance(Activator.Target) <= 400 : Player.IsInAutoAttackRange(Activator.Target) && youmuus.Cast()) return;

            return;
        }

        public void AutoBilgeBtrk()
        {
            if (!botrk.IsInRange(Activator.Target) && !_offensives.IsActive("offensives.botrk/bilgewater")) return;

            if (botrk.IsOwned() && botrk.IsReady() && botrk.Cast(Activator.Target)) return;

            if (bilgewater.IsOwned() && bilgewater.IsReady() && bilgewater.Cast(Activator.Target)) return;

            return;
        }

        #endregion

        #region Defensive Itens

        private AIHeroClient GetPrioritedProtectionTarget(int healthpercent, int range = 600)
        {
            var target = Allies.FirstOrDefault(ally => ally.HealthPercent <= healthpercent && Enemies.Any(enemy => enemy.IsValidTarget() && !enemy.IsDead && enemy.IsInAutoAttackRange(ally)));

            return target;
        }

        public void AutoFaceOfTheMountain()
        {
            if (!faceMountain.IsOwned() || !faceMountain.IsReady() || !_defensives.IsActive("defensives.fotmountain")) return;

            var target = GetPrioritedProtectionTarget(_defensives.Value("defensives.fotmountain.health%"));

            if (target == null || !target.IsValidTarget() || !faceMountain.IsInRange(target)) return;

            faceMountain.Cast(target);

            return;
        }

        public void AutoMikael()
        {
            if (!mikael.IsOwned() || !mikael.IsReady() || !_defensives.IsActive("defensives.mikael")) return;

            var target = GetPrioritedProtectionTarget(_defensives.Value("defensives.mikael.health%"));

            if (target == null || !target.IsValidTarget() || !mikael.IsInRange(target)) return;

            mikael.Cast(target);

            return;
        }

        public void AutoSolari()
        {
            if (!solari.IsOwned() || !solari.IsReady() || !_defensives.IsActive("defensives.solari")) return;

            var target = GetPrioritedProtectionTarget(_defensives.Value("defensives.solari.health%"));

            if (target == null || !target.IsValidTarget() || !solari.IsInRange(target)) return;

            solari.Cast(target);

            return;
        }

        public void AutoScimitarQSS()
        {
            if (!_defensives.IsActive("defensives.scimitar/qss")) return;

            if (qss.IsOwned() && qss.IsReady())
            {
                if (_defensives.IsActive("defensives.scimitar/qss.blind") && Player.HasBuffOfType(BuffType.Blind)) qss.Cast();
                if (_defensives.IsActive("defensives.scimitar/qss.charm") && Player.HasBuffOfType(BuffType.Charm)) qss.Cast();
                if (_defensives.IsActive("defensives.scimitar/qss.fear") && Player.HasBuffOfType(BuffType.Fear)) qss.Cast();
                if (_defensives.IsActive("defensives.scimitar/qss.polymorph") && Player.HasBuffOfType(BuffType.Polymorph)) qss.Cast();
                if (_defensives.IsActive("defensives.scimitar/qss.silence") && Player.HasBuffOfType(BuffType.Silence)) qss.Cast();
                if (_defensives.IsActive("defensives.scimitar/qss.sleep") && Player.HasBuffOfType(BuffType.Sleep)) qss.Cast();
                if (_defensives.IsActive("defensives.scimitar/qss.slow") && Player.HasBuffOfType(BuffType.Slow)) qss.Cast();
                if (_defensives.IsActive("defensives.scimitar/qss.snare") && Player.HasBuffOfType(BuffType.Snare)) qss.Cast();
                if (_defensives.IsActive("defensives.scimitar/qss.stun") && Player.HasBuffOfType(BuffType.Stun)) qss.Cast();
                if (_defensives.IsActive("defensives.scimitar/qss.suppression") && Player.HasBuffOfType(BuffType.Suppression)) qss.Cast();
                if (_defensives.IsActive("defensives.scimitar/qss.taunt") && Player.HasBuffOfType(BuffType.Taunt)) qss.Cast();
            }
            else if (scimitar.IsOwned() && scimitar.IsReady())
            {
                if (_defensives.IsActive("defensives.scimitar/qss.blind") && Player.HasBuffOfType(BuffType.Blind)) scimitar.Cast();
                if (_defensives.IsActive("defensives.scimitar/qss.charm") && Player.HasBuffOfType(BuffType.Charm)) scimitar.Cast();
                if (_defensives.IsActive("defensives.scimitar/qss.fear") && Player.HasBuffOfType(BuffType.Fear)) scimitar.Cast();
                if (_defensives.IsActive("defensives.scimitar/qss.polymorph") && Player.HasBuffOfType(BuffType.Polymorph)) scimitar.Cast();
                if (_defensives.IsActive("defensives.scimitar/qss.silence") && Player.HasBuffOfType(BuffType.Silence)) scimitar.Cast();
                if (_defensives.IsActive("defensives.scimitar/qss.sleep") && Player.HasBuffOfType(BuffType.Sleep)) scimitar.Cast();
                if (_defensives.IsActive("defensives.scimitar/qss.slow") && Player.HasBuffOfType(BuffType.Slow)) scimitar.Cast();
                if (_defensives.IsActive("defensives.scimitar/qss.snare") && Player.HasBuffOfType(BuffType.Snare)) scimitar.Cast();
                if (_defensives.IsActive("defensives.scimitar/qss.stun") && Player.HasBuffOfType(BuffType.Stun)) scimitar.Cast();
                if (_defensives.IsActive("defensives.scimitar/qss.suppression") && Player.HasBuffOfType(BuffType.Suppression)) scimitar.Cast();
                if (_defensives.IsActive("defensives.scimitar/qss.taunt") && Player.HasBuffOfType(BuffType.Taunt)) scimitar.Cast();
            }

            return;
        }

        public void AutoZhonya()
        {
            if (!zhonya.IsOwned() || !zhonya.IsReady() || !_defensives.IsActive("defensives.zhonya") || Player.HealthPercent > _defensives.Value("defensives.zhonya.health%")) return;

            if (Enemies.Any(it => it.IsInAutoAttackRange(Player))) zhonya.Cast();

            return;
        }

        public void AutoZhonyaOnDangerous(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args, string spell)
        {
            //This spells isn't checked on menu ? return;
            if (!_defensives.IsActive("defensives.zhonya." + spell)) return;

            //Zhonya on dangerous
            else if (zhonya.IsOwned() && zhonya.IsReady())
            {
                if (spell == "morgana.r")
                {
                    Core.DelayAction(delegate
                    {
                        if (Player.Distance(sender) < args.SData.CastRangeDisplayOverride) zhonya.Cast();
                    }, 2700 - Game.Ping);
                }

                else if (spell == "nunu.r")
                {
                    Core.DelayAction(delegate
                    {
                        if (Player.Distance(sender) < args.SData.CastRangeDisplayOverride) zhonya.Cast();
                    }, 600 - Game.Ping);
                }

                else if (spell == "zed.r")
                {
                    Core.DelayAction(() => ZhonyaOnHit(sender, args), 450 - Game.Ping);
                }

                else
                {
                    var delay = (int)(((Player.Distance(sender) - 300) / args.SData.MissileSpeed * 1000) + args.SData.SpellCastTime - 50 - Game.Ping);

                    Core.DelayAction(() => ZhonyaOnHit(sender, args), delay);
                }
            }

            return;
        }

        public void AutoRanduin()
        {
            if (randuin.IsOwned() && randuin.IsReady() && randuin.IsInRange(Activator.Target) && randuin.Cast()) return;

            return;
        }

        public void AutoSeraphEmbrace()
        {
            if (!seraph.IsOwned() || !seraph.IsReady() || !_defensives.IsActive("defensives.seraph") || Player.HealthPercent > _defensives.Value("defensives.seraph.health%")) return;

            if (Enemies.Any(it => it.IsInAutoAttackRange(Player))) seraph.Cast();

            return;
        }

        #endregion

        #region Speed Itens

        public void AutoTalisma()
        {
            if (!talisma.IsOwned() || !talisma.IsReady() || !_speed.IsActive("speed.talisma") || Player.CountAlliesInRange(600) <= 0) return;

            if (Player.Distance(Activator.Target) > 400) talisma.Cast();

            return;
        }

        public void AutoRighteousGlory()
        {
            if (!righteousGlory.IsOwned() || !righteousGlory.IsReady() || !_speed.IsActive("speed.righteousGlory") || Player.CountAlliesInRange(600) <= 0) return;

            if (Player.Distance(Activator.Target) > 400) righteousGlory.Cast();

            return;
        }

        #endregion

        #region Potion Itens

        private bool CanUsePotion(string name)
        {
            if (_potions.IsActive("potions." + name + "potion") && Player.HealthPercent <= _potions.Value("potions." + name + "potion.health%")) return true;

            return false;
        }

        public void AutoHealthPotion()
        {
            if (!healthPotion.IsOwned() || !healthPotion.IsReady() || !CanUsePotion("health")) return;

            healthPotion.Cast();

            return;
        }

        public void AutoBiscuitPotion()
        {
            if (!biscuitPotion.IsOwned() || !biscuitPotion.IsReady() || !CanUsePotion("biscuit")) return;

            biscuitPotion.Cast();

            return;
        }

        public void AutoCorruptingPotion()
        {
            if (!corruptingPotion.IsOwned() || !corruptingPotion.IsReady() || !CanUsePotion("corrupting")) return;

            corruptingPotion.Cast();

            return;
        }

        public void AutoHuntersPotion()
        {
            if (!huntersPotion.IsOwned() || !huntersPotion.IsReady() || !CanUsePotion("hunters")) return;

            huntersPotion.Cast();

            return;
        }

        public void AutoRefillablePotion()
        {
            if (!refillablePotion.IsOwned() || !refillablePotion.IsReady() || !CanUsePotion("refillable")) return;

            refillablePotion.Cast();

            return;
        }

        #endregion

        #region Orbwalker modes

        //Lane Clear
        public void AutoLaneItens()
        {
            if (_misc.IsActive("misc.itemsonlaneclear"))
            {
                var minions = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Player.Position, 400);

                if (minions.Count() >= 3 && minions.Any(it => it.Health > 150))
                {
                    if (tiamat.IsOwned() && tiamat.IsReady()) tiamat.Cast();
                    if (hydra.IsOwned() && hydra.IsReady()) hydra.Cast();
                }
            }
        }

        //Jungle Clear
        public void AutoJungleItens()
        {
            if (_misc.IsActive("misc.itemsonjungleclear"))
            {
                var minions = EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Position, 650);

                if (minions.Count(it => it.Distance(Player) <= 400) == minions.Count() && minions.Any(it => it.Health > 150))
                {
                    if (tiamat.IsOwned() && tiamat.IsReady()) tiamat.Cast();
                    if (hydra.IsOwned() && hydra.IsReady()) hydra.Cast();
                    if (minions.Any(it => it.Health >= 200) && titanic.IsOwned() && titanic.IsReady()) titanic.Cast();
                }
            }
        }

        //Flee
        public void AutoFleeItens()
        {
            if (randuin.IsOwned() && randuin.IsReady() && EntityManager.Heroes.Enemies.Any(it => !it.IsDead && it.IsValidTarget(randuin.Range))) randuin.Cast();

            if (righteousGlory.IsOwned() && righteousGlory.IsReady())
                righteousGlory.Cast();

            if (talisma.IsOwned() && talisma.IsReady())
                talisma.Cast();
        }

        #endregion

        #endregion

        //---------------------------------------Methods-----------------------------------

        private void ZhonyaOnHit(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (args.Target != null)
            {
                if (args.Target.IsMe) zhonya.Cast();
                return;
            }

            var polygons = new Geometry.Polygon[] { new Geometry.Polygon.Rectangle(args.Start, args.End, args.SData.LineWidth), new Geometry.Polygon.Circle(args.End, args.SData.CastRadius) };

            if (polygons.Any(it => it.IsInside(Player))) zhonya.Cast();

            return;
        }
    }
}
