using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;

namespace WuActivator.Util
{
    class SpellUtil
    {
        public enum Summoners
        {
            Heal, Barrier, Cleanse, Ghost, Exhaust, Smite, Ignite, Flash
        };

        public Spell.Active GetActiveSpell(string name, uint range = 0)
        {
            var slot = Player.Instance.GetSpellSlotFromName(name);

            if (slot != SpellSlot.Unknown)
            {
                return new Spell.Active(slot, range);
            }

            return null;
        }

        public Spell.Targeted GetTargettedSpell(string name, uint range)
        {
            var slot = Player.Instance.GetSpellSlotFromName(name);

            if (slot != SpellSlot.Unknown)
            {
                return new Spell.Targeted(slot, range);
            }

            return null;
        }

        public Spell.Skillshot GetSkillshotSpell(string name, uint range, SkillShotType type)
        {
            var slot = Player.Instance.GetSpellSlotFromName(name);

            if (slot != SpellSlot.Unknown)
            {
                return new Spell.Skillshot(slot, range, type);
            }

            return null;
        }

        public Spell.Active GetActiveSpell(Summoners summoner)
        {
            SpellSlot slot;

            switch (summoner)
            {
                case Summoners.Heal:
                    slot = Player.Instance.GetSpellSlotFromName("summonerheal");

                    if (slot != SpellSlot.Unknown) return new Spell.Active(slot);

                    return null;

                case Summoners.Barrier:
                    slot = Player.Instance.GetSpellSlotFromName("summonerbarrier");

                    if (slot != SpellSlot.Unknown) return new Spell.Active(slot);

                    return null;

                case Summoners.Cleanse:
                    slot = Player.Instance.GetSpellSlotFromName("summonercleanse");

                    if (slot != SpellSlot.Unknown) return new Spell.Active(slot);

                    return null;

                case Summoners.Ghost:
                    slot = Player.Instance.GetSpellSlotFromName("summonerghost");

                    if (slot != SpellSlot.Unknown) return new Spell.Active(slot);

                    return null;

                default:
                    return null;
            }
        }

        public Spell.Targeted GetTargettedSpell(Summoners summoner)
        {
            SpellSlot slot;

            switch (summoner)
            {
                case Summoners.Exhaust:
                    slot = Player.Instance.GetSpellSlotFromName("summonerexhaust");

                    if (slot != SpellSlot.Unknown) return new Spell.Targeted(slot, 650);

                    return null;

                case Summoners.Smite:
                    var spell = Player.Instance.Spellbook.Spells.FirstOrDefault(it => it.Name.Contains("summoner") && it.Name.Contains("smite"));

                    if (spell != null) return new Spell.Targeted(spell.Slot, 500);

                    return null;

                case Summoners.Ignite:
                    slot = Player.Instance.GetSpellSlotFromName("summonerdot");

                    if (slot != SpellSlot.Unknown) return new Spell.Targeted(slot, 600);

                    return null;

                default:
                    return null;
            }
        }

        public Spell.Skillshot GetSkillshotSpell(Summoners summoner)
        {

            if (summoner != Summoners.Flash) return null;

            var slot = Player.Instance.GetSpellSlotFromName("summonerflash");

            if (slot != SpellSlot.Unknown) return new Spell.Skillshot(slot, 425, SkillShotType.Linear);

            return null;
        }
    }
}
