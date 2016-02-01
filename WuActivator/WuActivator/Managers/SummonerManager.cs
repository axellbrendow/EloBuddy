using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using WuActivator.Util;

namespace WuActivator.Managers
{
    class SummonerManager
    {
        private readonly AIHeroClient Player = EloBuddy.Player.Instance;

        private Menu _summoners { get { return MenuManager.Menus["WuActivator"]["Summoners"].Keys.First(); } }
        private SpellUtil _spellUtil;

        //<Summoners>
        public Spell.Active heal, barrier, ghost, cleanse;
        public Spell.Targeted ignite, exhaust, smite;
        public Spell.Skillshot flash;

        public SummonerManager()
        {
            //Initializing all summoner spells

            _spellUtil = new SpellUtil();

            flash = _spellUtil.GetSkillshotSpell(SpellUtil.Summoners.Flash);

            ignite = _spellUtil.GetTargettedSpell(SpellUtil.Summoners.Ignite);
            exhaust = _spellUtil.GetTargettedSpell(SpellUtil.Summoners.Exhaust);
            smite = _spellUtil.GetTargettedSpell(SpellUtil.Summoners.Smite);

            heal = _spellUtil.GetActiveSpell(SpellUtil.Summoners.Heal);
            barrier = _spellUtil.GetActiveSpell(SpellUtil.Summoners.Barrier);
            ghost = _spellUtil.GetActiveSpell(SpellUtil.Summoners.Ghost);
            cleanse = _spellUtil.GetActiveSpell(SpellUtil.Summoners.Cleanse);
        }

        #region Activator methods

        public void AutoExhaust()
        {
            if (exhaust == null || !exhaust.IsReady() || !_summoners.IsActive("summoners.exhaust") || Activator.Target == null || !Activator.Target.IsValidTarget(exhaust.Range) || TargetSelector.GetPriority(Activator.Target) < 3) return;

            exhaust.Cast(Activator.Target);

            return;
        }

        public void AutoIgnite()
        {
            if (ignite == null || !ignite.IsReady() || !_summoners.IsActive("summoners.ignite")) return;

            var igniteEnemy = EntityManager.Heroes.Enemies.FirstOrDefault(it => Player.GetSummonerSpellDamage(it, DamageLibrary.SummonerSpells.Ignite) >= it.Health + 28);

            if (igniteEnemy == null) return;

            if ((igniteEnemy.Distance(Player) >= 300 || Player.HealthPercent <= 40))
                ignite.Cast(igniteEnemy);

            return;
        }

        public void AutoSmite()
        {
            if (smite == null || !smite.IsReady()) return;

            if (_summoners.IsActive("summoners.smite") && _summoners.IsActive("summoners.smite.ks"))
            {
                var bye = EntityManager.Heroes.Enemies.FirstOrDefault(it => it.IsValidTarget(smite.Range) && DamageLibrary.GetSummonerSpellDamage(Player, it, DamageLibrary.SummonerSpells.Smite) >= it.Health);

                if (bye != null) { smite.Cast(bye); return; }
            }

            var Mob = EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Position, smite.Range).FirstOrDefault(it => (it.Name.Contains("SRU_Dragon") || it.Name.Contains("SRU_Baron")) && DamageUtil.GetSmiteDamage() >= it.Health);

            if (Mob != null) { smite.Cast(Mob); return; }
        }

        public void AutoSmiteCombo()
        {
            if (smite == null || !smite.IsReady() || Activator.Target == null || !Activator.Target.IsValidTarget(smite.Range) || !_summoners.IsActive("summoners.smite") || !_summoners.IsActive("summoners.smite.enemies")) return;

            if (smite.Name.Contains("gank")) smite.Cast(Activator.Target);
            else if (smite.Name.Contains("duel") && Player.IsInAutoAttackRange(Activator.Target)) smite.Cast(Activator.Target);
        }

        public void AutoSmiteFlee()
        {
            if (smite == null || !smite.IsReady() || Activator.Target == null || !Activator.Target.IsValidTarget(smite.Range) || !_summoners.IsActive("summoners.smite")) return;

            smite.Cast(Activator.Target);
        }

        public void AutoSmiteMob()
        {
            if (smite == null || !smite.IsReady() || !_summoners.IsActive("summoners.smite")) return;

            var Mob = EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Position, smite.Range).FirstOrDefault(it => DamageUtil.GetSmiteDamage() >= it.Health);

            if (Mob == null) return;

            if (Mob.Name.StartsWith("SRU_Red") && _summoners.IsActive("summoners.smite.red")) smite.Cast(Mob);
            else if (Mob.Name.StartsWith("SRU_Blue") && _summoners.IsActive("summoners.smite.blue")) smite.Cast(Mob);
            else if (Mob.Name.StartsWith("SRU_Murkwolf") && _summoners.IsActive("summoners.smite.wolf")) smite.Cast(Mob);
            else if (Mob.Name.StartsWith("SRU_Krug") && _summoners.IsActive("summoners.smite.krug")) smite.Cast(Mob);
            else if (Mob.Name.StartsWith("SRU_Gromp") && _summoners.IsActive("summoners.smite.gromp")) smite.Cast(Mob);
            else if (Mob.Name.StartsWith("SRU_Razorbeak") && _summoners.IsActive("summoners.smite.raptor")) smite.Cast(Mob);
        }

        public void AutoHeal()
        {
            if (heal == null || !heal.IsReady() || !_summoners.IsActive("summoners.heal")) return;

            var target = EntityManager.Heroes.Allies.FirstOrDefault(it => it.IsValidTarget(heal.Range) && it.HealthPercent <= _summoners.Value("summoners.heal.health%"));

            if (target != null)
            {
                if (EntityManager.Heroes.Enemies.Any(it => it.IsValidTarget() && it.Distance(target) <= it.GetAutoAttackRange())) heal.Cast();
            }
        }

        public void AutoHealOnDangerous(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!_summoners.IsActive("summoners.heal.dangerousspells")) return;

            if (args.Target != null && args.Target.IsMe) { heal.Cast(); return; }

            int delay = new int();

            if (Player.Distance(sender) >= 1000)
            {
                delay = (int)((((Player.Distance(sender)) / args.SData.MissileSpeed * 1000) + args.SData.SpellCastTime - Game.Ping) / 1.5);
            }
            else if (Player.Distance(sender) >= 400)
            {
                delay = (int)((((Player.Distance(sender)) / args.SData.MissileSpeed * 1000) + args.SData.SpellCastTime - Game.Ping) / 2);
            }

            Core.DelayAction(() => SummonersOnHit(sender, args, true), delay);

            return;
        }

        public void AutoBarrier()
        {
            if (barrier == null || !barrier.IsReady() || !_summoners.IsActive("summoners.barrier") || Player.HealthPercent > _summoners.Value("summoners.barrier.health%")) return;

            barrier.Cast();
        }

        public void AutoBarrierOnDangerous(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!_summoners.IsActive("summoners.barrier.dangerousspells")) return;

            if (args.Target != null && args.Target.IsMe) { barrier.Cast(); return; }

            int delay = new int();

            if (Player.Distance(sender) >= 1000)
            {
                delay = (int)((((Player.Distance(sender)) / args.SData.MissileSpeed * 1000) + args.SData.SpellCastTime - Game.Ping) / 1.5);
            }
            else if (Player.Distance(sender) >= 400)
            {
                delay = (int)((((Player.Distance(sender)) / args.SData.MissileSpeed * 1000) + args.SData.SpellCastTime - Game.Ping) / 2);
            }

            Core.DelayAction(() => SummonersOnHit(sender, args), delay);

            return;
        }

        public void AutoGhost()
        {
            if (ghost == null || !ghost.IsReady() || !_summoners.IsActive("summoners.ghost") || Activator.Target == null || !Activator.Target.IsValidTarget()) return;

            if (!Player.IsInAutoAttackRange(Activator.Target) && Player.Distance(Activator.Target) <= 400)
                ghost.Cast();
        }

        public void AutoCleanse()
        {
            if (cleanse == null || !cleanse.IsReady() || !_summoners.IsActive("summoners.cleanse")) return;

            if (_summoners.IsActive("summoners.cleanse.blind") && Player.HasBuffOfType(BuffType.Blind)) cleanse.Cast();
            if (_summoners.IsActive("summoners.cleanse.charm") && Player.HasBuffOfType(BuffType.Charm)) cleanse.Cast();
            if (_summoners.IsActive("summoners.cleanse.fear") && Player.HasBuffOfType(BuffType.Fear)) cleanse.Cast();
            if (_summoners.IsActive("summoners.cleanse.polymorph") && Player.HasBuffOfType(BuffType.Polymorph)) cleanse.Cast();
            if (_summoners.IsActive("summoners.cleanse.silence") && Player.HasBuffOfType(BuffType.Silence)) cleanse.Cast();
            if (_summoners.IsActive("summoners.cleanse.sleep") && Player.HasBuffOfType(BuffType.Sleep)) cleanse.Cast();
            if (_summoners.IsActive("summoners.cleanse.slow") && Player.HasBuffOfType(BuffType.Slow)) cleanse.Cast();
            if (_summoners.IsActive("summoners.cleanse.snare") && Player.HasBuffOfType(BuffType.Snare)) cleanse.Cast();
            if (_summoners.IsActive("summoners.cleanse.stun") && Player.HasBuffOfType(BuffType.Stun)) cleanse.Cast();
            if (_summoners.IsActive("summoners.cleanse.suppression") && Player.HasBuffOfType(BuffType.Suppression)) cleanse.Cast();
            if (_summoners.IsActive("summoners.cleanse.taunt") && Player.HasBuffOfType(BuffType.Taunt)) cleanse.Cast();
        }

        #endregion

        //------------------------------Methods--------------------------------

        private void SummonersOnHit(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args, bool useHeal = false)
        {
            var polygons = new Geometry.Polygon[] { new Geometry.Polygon.Rectangle(args.Start, args.End, args.SData.LineWidth), new Geometry.Polygon.Circle(args.End, args.SData.CastRadius) };

            if (polygons.Any(it => it.IsInside(Player)))
            {
                if (useHeal) heal.Cast();
                else barrier.Cast();
            }

            return;
        }
    }
}
