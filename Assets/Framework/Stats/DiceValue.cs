using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [System.Serializable]
    public class DiceValue {
        public int DiceRolls;
        public int DiceSides;
        public int Bonus;

        public override string ToString() {
            return string.Format("{0}d{1}+{2}", DiceRolls, DiceSides, Bonus);
        }
    }
}