using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;

namespace WuActivator.Util
{
    class DamageUtil
    {
        public static float GetSmiteDamage(AIHeroClient from = null)
        {
            var damage = new float(); //Arithmetic Progression OP :D

            if (from != null)
            {
                if (from.Level < 10) damage = 360 + (from.Level - 1) * 30;

                else if (from.Level < 15) damage = 280 + (from.Level - 1) * 40;

                else if (from.Level < 19) damage = 150 + (from.Level - 1) * 50;

                return damage;
            }

            if (Player.Instance.Level < 10) damage = 360 + (Player.Instance.Level - 1) * 30;

            else if (Player.Instance.Level < 15) damage = 280 + (Player.Instance.Level - 1) * 40;

            else if (Player.Instance.Level < 19) damage = 150 + (Player.Instance.Level - 1) * 50;

            return damage;
        }
    }
}
