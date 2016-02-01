using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using WuActivator.Managers;

namespace WuActivator
{
    class Activator
    {
        public ItemManager itens;
        public SummonerManager summoners;

        public static AIHeroClient Target;

        private string[] _autoZhonyaSpells = new string[] { "zed.r", "veigar.r", "veigar.w", "malphite.r", "garen.r", "darius.r", "fizz.r", "lux.r", "ezreal.r", "leesin.r", "morgana.r", "chogath.r", "nunu.r" };

        private readonly AIHeroClient Player = EloBuddy.Player.Instance;
        private DamageType _damageType;
        private int _range;

        public Activator(DamageType damageType = DamageType.Physical, int range = 700)
        {
            InitMenu();

            itens = new ItemManager();
            summoners = new SummonerManager();

            _damageType = damageType;
            _range = range;

            Game.OnTick += Game_OnTick;
            Orbwalker.OnPostAttack += Orbwalker_OnPostAttack;
            AIHeroClient.OnProcessSpellCast += AIHeroClient_OnProcessSpellCast;
        }

        private void Orbwalker_OnPostAttack(AttackableUnit target, EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                itens.AutoTiamatHydra();
                itens.AutoTitanicHydra();
            }
        }

        void InitMenu()
        {
            MenuManager.Init("WuActivator");

            #region Summoners

            var summoners = MenuManager.AddSubMenu("Summoners");
            {
                summoners.NewCheckbox("summoners.ignite", "Use Ignite", true);

                summoners.NewCheckbox("summoners.exhaust", "Use Exhaust (Combo Mode, instantly)", true);

                summoners.NewCheckbox("summoners.heal", "Use Heal", true, true);
                summoners.NewCheckbox("summoners.heal.dangerousspells", "Use Heal on dangerous spells", true, true);
                summoners.NewSlider("summoners.heal.dangerousspells.safelife", "(Dangerous) Use Heal just if my health will be >=", 100, 10, 1500);
                summoners.NewSlider("summoners.heal.health%", "Use Heal when health% is at:", 30, 1, 99);

                summoners.NewCheckbox("summoners.barrier", "Use Barrier", true, true);
                summoners.NewCheckbox("summoners.barrier.dangerousspells", "Use Barrier on dangerous spells", true, true);
                summoners.NewSlider("summoners.barrier.dangerousspells.safelife", "(Dangerous) Use Barrier just if my health will be >=", 100, 10, 1500);
                summoners.NewSlider("summoners.barrier.health%", "Use Barrier when health% is at:", 20, 1, 99);

                summoners.NewCheckbox("summoners.ghost", "Use Ghost", true, true);

                summoners.NewCheckbox("summoners.smite", "Use Smite", true, true);
                summoners.NewCheckbox("summoners.smite.enemies", "Enemies - Use Smite", true, true);
                summoners.NewCheckbox("summoners.smite.ks", "KS - Use Smite", true);
                summoners.NewCheckbox("summoners.smite.red", "Red - Use Smite", true);
                summoners.NewCheckbox("summoners.smite.blue", "Blue - Use Smite", true);
                summoners.NewCheckbox("summoners.smite.wolf", "Wolf - Use Smite", true);
                summoners.NewCheckbox("summoners.smite.gromp", "Gromp - Use Smite", true);
                summoners.NewCheckbox("summoners.smite.raptor", "Raptor - Use Smite", true);
                summoners.NewCheckbox("summoners.smite.krug", "Krug - Use Smite", true);

                summoners.NewCheckbox("summoners.cleanse", "Use Cleanse", true, true);
                summoners.NewCheckbox("summoners.cleanse.stun", "Stun - Use Cleanse", true, true);
                summoners.NewCheckbox("summoners.cleanse.polymorph", "Polymorph - Use Cleanse", true);
                summoners.NewCheckbox("summoners.cleanse.slow", "Slow - Use Cleanse", true);
                summoners.NewCheckbox("summoners.cleanse.suppression", "Suppression - Use Cleanse", true);
                summoners.NewCheckbox("summoners.cleanse.taunt", "Taunt - Use Cleanse", true);
                summoners.NewCheckbox("summoners.cleanse.charm", "Charm - Use Cleanse", true);
                summoners.NewCheckbox("summoners.cleanse.fear", "Fear - Use Cleanse", true);
                summoners.NewCheckbox("summoners.cleanse.snare", "Snare - Use Cleanse", true);
                summoners.NewCheckbox("summoners.cleanse.blind", "Blind - Use Cleanse", true);
                summoners.NewCheckbox("summoners.cleanse.sleep", "Sleep - Use Cleanse", true);
                summoners.NewCheckbox("summoners.cleanse.silence", "Silence - Use Cleanse", true);
            }

            #endregion

            #region Offensives

            var offensives = MenuManager.AddSubMenu("Offensives");
            {
                offensives.NewCheckbox("offensives.hextech", "Use Hextech", true);

                offensives.NewCheckbox("offensives.botrk/bilgewater", "Use BOTRK/Bilgewater", true, true);
                offensives.NewSlider("offensives.botrk/bilgewater.health%", "Use BOTRK/Bilgewater when my health% is at:", 30, 1, 99);

                offensives.NewCheckbox("offensives.hydra/tiamat", "Use Hydra/Tiamat", true, true);

                offensives.NewCheckbox("offensives.titanic", "Use Titanic Hydra");

                offensives.NewCheckbox("offensives.ghostblade", "Use Ghost Blade", true);
            }

            #endregion

            #region Defensives

            var defensives = MenuManager.AddSubMenu("Defensives");
            {
                defensives.NewCheckbox("defensives.randuin", "Use Randuin", true);

                defensives.NewCheckbox("defensives.scimitar/qss", "Use Scimitar/QSS", true, true);
                defensives.NewCheckbox("defensives.scimitar/qss.stun", "Stun - Use Scimitar/Qss", true, true);
                defensives.NewCheckbox("defensives.scimitar/qss.polymorph", "Polymorph - Use Scimitar/Qss", true);
                defensives.NewCheckbox("defensives.scimitar/qss.slow", "Slow - Use Scimitar/Qss", true);
                defensives.NewCheckbox("defensives.scimitar/qss.suppression", "Suppression - Use Scimitar/Qss", true);
                defensives.NewCheckbox("defensives.scimitar/qss.taunt", "Taunt - Use Scimitar/Qss", true);
                defensives.NewCheckbox("defensives.scimitar/qss.charm", "Charm - Use Scimitar/Qss", true);
                defensives.NewCheckbox("defensives.scimitar/qss.fear", "Fear - Use Scimitar/Qss", true);
                defensives.NewCheckbox("defensives.scimitar/qss.snare", "Snare - Use Scimitar/Qss", true);
                defensives.NewCheckbox("defensives.scimitar/qss.blind", "Blind - Use Scimitar/Qss", true);
                defensives.NewCheckbox("defensives.scimitar/qss.sleep", "Sleep - Use Scimitar/Qss", true);
                defensives.NewCheckbox("defensives.scimitar/qss.silence", "Silence - Use Scimitar/Qss", true);

                defensives.NewCheckbox("defensives.fotmountain", "Use Face of the Mountain", true, true);
                defensives.NewSlider("defensives.fotmountain.health%", "Use Face of the Mountain when ally health% is at:", 30, 1, 99, true);

                defensives.NewCheckbox("defensives.mikael", "Use Mikael", true, true);
                defensives.NewSlider("defensives.mikael.health%", "Use Mikael when ally health% is at:", 30, 1, 99, true);

                defensives.NewCheckbox("defensives.solari", "Use Iron Solari", true, true);
                defensives.NewSlider("defensives.solari.health%", "Use Iron Solari when ally health% is at:", 30, 1, 99, true);

                defensives.NewCheckbox("defensives.seraph", "Use Seraph Embrance", true, true);
                defensives.NewSlider("defensives.seraph.health%", "Use Seraph Embrance when my health% is at:", 30, 1, 99, true);

                defensives.NewCheckbox("defensives.zhonya", "Use Zhonya", true, true);
                defensives.NewSlider("defensives.zhonya.health%", "Use Zhonya when my health% is at:", 20, 1, 99, true);
                defensives.NewCheckbox("defensives.zhonya.zed.r", "Use Zhonya on Zed R", true, true);
                defensives.NewCheckbox("defensives.zhonya.veigar.r", "Use Zhonya on Veigar R", true);
                defensives.NewCheckbox("defensives.zhonya.veigar.w", "Use Zhonya on Veigar W", true);
                defensives.NewCheckbox("defensives.zhonya.malphite.r", "Use Zhonya on Malphite R", true);
                defensives.NewCheckbox("defensives.zhonya.ezreal.r", "Use Zhonya on Ezreal R", true);
                defensives.NewCheckbox("defensives.zhonya.darius.r", "Use Zhonya on Darius R", true);
                defensives.NewCheckbox("defensives.zhonya.garen.r", "Use Zhonya on Garen R", true);
                defensives.NewCheckbox("defensives.zhonya.fizz.r", "Use Zhonya on Fizz R", true);
                defensives.NewCheckbox("defensives.zhonya.leesin.r", "Use Zhonya on Lee Sin R", true);
                defensives.NewCheckbox("defensives.zhonya.chogath.r", "Use Zhonya on ChoGath R", true);
                defensives.NewCheckbox("defensives.zhonya.lux.r", "Use Zhonya on Lux R", true);
                defensives.NewCheckbox("defensives.zhonya.nunu.r", "Use Zhonya on Nunu R", true);
                defensives.NewCheckbox("defensives.zhonya.morgana.r", "Use Zhonya on Morgana R (when stun)", true);
            }

            #endregion

            #region Potions

            var potions = MenuManager.AddSubMenu("Potions");
            {
                potions.NewCheckbox("potions.healthpotion", "Use Health Potion", true);
                potions.NewSlider("potions.healthpotion.health%", "Use Health Potion when my health% is at:", 60, 1, 99);

                potions.NewCheckbox("potions.biscuitpotion", "Use Biscuit Potion", true, true);
                potions.NewSlider("potions.biscuitpotion.health%", "Use Biscuit Potion when my health% is at:", 60, 1, 99);

                potions.NewCheckbox("potions.corruptingpotion", "Use Corrupting Potion", true, true);
                potions.NewSlider("potions.corruptingpotion.health%", "Use Corrupting Potion when my health% is at:", 60, 1, 99);

                potions.NewCheckbox("potions.hunterspotion", "Use Hunters Potion", true, true);
                potions.NewSlider("potions.hunterspotion.health%", "Use Hunters Potion when my health% is at:", 70, 1, 99);

                potions.NewCheckbox("potions.refillablepotion", "Use Refillable Potion", true, true);
                potions.NewSlider("potions.refillablepotion.health%", "Use Refillable Potion when my health% is at:", 70, 1, 99);
            }

            #endregion

            #region Speed

            var speed = MenuManager.AddSubMenu("Speed");

            speed.NewCheckbox("speed.talisma", "Use Talisma", true);

            speed.NewCheckbox("speed.glory", "Use Glory", true);

            #endregion

            #region Misc

            var misc = MenuManager.AddSubMenu("Misc");

            misc.NewCheckbox("misc.itemsonlaneclear", "Use Items on Lane Clear", true);
            misc.NewCheckbox("misc.itemsonjungleclear", "Use Items on Jungle Clear", true, true);

            #endregion

        }

        //------------------- (devil) -------------------

        private void Game_OnTick(EventArgs args)
        {
            if (Player.IsDead) return;

            Target = TargetSelector.GetTarget(_range, _damageType);

            //Dragon, Baron, KS
            summoners.AutoSmite();

            //Potions time
            if (!(Player.IsInShopRange() || Player.HasBuff("RegenerationPotion") || Player.HasBuff("HealthPotion") || Player.HasBuff("BiscuitPotion") || Player.HasBuff("ItemCrystalFlask") || Player.HasBuff("ItemCrystalFlaskJungle") || Player.HasBuff("CorruptingPotion")))
            {
                itens.AutoHealthPotion();
                itens.AutoBiscuitPotion();
                itens.AutoCorruptingPotion();
                itens.AutoHuntersPotion();
                itens.AutoRefillablePotion();
            }

            if (Target != null && Target.IsValidTarget() && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                //Combo mode 3:)

                //Summoners usage
                {
                    //Ghost usage
                    summoners.AutoGhost();

                    //Exhaust usage
                    summoners.AutoExhaust();

                    //Smite usage
                    summoners.AutoSmiteCombo();
                }

                //Offensives
                itens.AutoBilgeBtrk();
                itens.AutoHextechGunBlade();
                itens.AutoYoumuusGhostBlade();

                itens.AutoRanduin();

                itens.AutoRighteousGlory();
                itens.AutoTalisma();
            }

            if (Player.CountEnemiesInRange(1350) > 0)
            {
                //Defensives

                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
                {
                    summoners.AutoSmiteFlee();

                    itens.AutoFleeItens();
                }

                itens.AutoScimitarQSS();
                itens.AutoZhonya();
                itens.AutoSeraphEmbrace();
                itens.AutoSolari();
                itens.AutoMikael();
                itens.AutoFaceOfTheMountain();

                //Summoners
                summoners.AutoIgnite();
                summoners.AutoCleanse();
                summoners.AutoBarrier();
                summoners.AutoHeal();
            }

            //Lane Clear
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
                itens.AutoLaneItens(); //Items Usage

            //Jungle Clear
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                //Smiting mobs
                summoners.AutoSmiteMob();

                //Items usage
                itens.AutoJungleItens();
            }

            return;
        }

        private void AIHeroClient_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsAlly || sender.IsMe || Player.Distance(sender) > args.SData.CastRangeDisplayOverride || !(sender is AIHeroClient)) return;

            //Is dangerous spell ?
            var spell = _autoZhonyaSpells.FirstOrDefault(it => it == sender.BaseSkinName.ToLower() + "." + args.Slot.ToString().ToLower());

            //No ? return;
            if (spell == null) return;

            //Yes ? Let's go 3:)

            //Summoners on dangerous

            if (spell != "morgana.r" && spell != "nunu.r" && spell != "zed.r")
            {
                if (summoners.barrier != null && summoners.barrier.IsReady() && (Player.Health + (95 + (20 * Player.Level))) - ((AIHeroClient)sender).GetSpellDamage(Player, args.Slot) > 100 && (Player.Health + (95 + (20 * Player.Level))) - ((AIHeroClient)sender).GetSpellDamage(Player, args.Slot) <= Player.MaxHealth)
                    summoners.AutoBarrierOnDangerous(sender, args);

                else if (summoners.heal != null && summoners.heal.IsReady() && (Player.Health + (75 + (15 * Player.Level))) - ((AIHeroClient)sender).GetSpellDamage(Player, args.Slot) > 100 && (Player.Health + (75 + (15 * Player.Level))) - ((AIHeroClient)sender).GetSpellDamage(Player, args.Slot) <= Player.MaxHealth)
                    summoners.AutoHealOnDangerous(sender, args);
            }

            itens.AutoZhonyaOnDangerous(sender, args, spell);

            return;
        }
    }
}
