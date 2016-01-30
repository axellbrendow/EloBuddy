using EloBuddy.SDK;
using EloBuddy;

namespace WuActivator.Extensions
{
    static class SpellsExtensions
    {
        public static bool HitChanceCast(this Spell.Skillshot spell, Obj_AI_Base target, float chance = 85)
        {
            var pred = spell.GetPrediction(target);

            if (pred.HitChancePercent >= chance)
                if (spell.Cast(pred.CastPosition))
                    return true;

            return false;
        }
    }
}
