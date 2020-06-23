using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace PixelComrades {
    public class EquipmentConfig : ItemConfig {
        [ValueDropdown("SkillSlotList")] public string Skill;
        public int RequiredSkillRank = 0;
        [ValueDropdown("EquipmentSlotList")] public string EquipmentSlot;
        public int[] AttributeBonuses = new int[0];
        public int[] DefenseBonuses = new int[0];
        public override string ItemType { get { return ItemTypes.Equipment; } }

        private ValueDropdownList<string> EquipmentSlotList() {
<<<<<<< HEAD
            return EquipmentSlotType.GetDropdownList();
=======
            return EquipmentSlotTypes.GetDropdownList();
>>>>>>> FirstPersonAction
        }

        private ValueDropdownList<string> SkillSlotList() {
            return Skills.GetDropdownList();
        }
        public override void AddComponents(Entity entity) {
            base.AddComponents(entity);
            entity.Add(new SkillRequirement(Skill, RequiredSkillRank));
            var equipment = entity.Add(new Equipment(EquipmentSlot));
            var stats = entity.GetOrAdd<StatsContainer>();
            for (int i = 0; i < AttributeBonuses.Length; i++) {
                var attributeBonus = AttributeBonuses[i];
                if (attributeBonus == 0) {
                    continue;
                }
<<<<<<< HEAD
                equipment.StatsToEquip.Add(Attributes.GetID(i));
                stats.Add(new BaseStat(entity, Attributes.GetNameAt(i), Attributes.GetID(i), AttributeBonuses[i]));
=======
                equipment.StatsToEquip.Add(Attributes.GetValue(i));
                stats.Add(new BaseStat(entity, Attributes.GetNameAt(i), Attributes.GetValue(i), AttributeBonuses[i]));
>>>>>>> FirstPersonAction
            }
            for (int i = 0; i < DefenseBonuses.Length; i++) {
                var defBonus = DefenseBonuses[i];
                if (defBonus == 0) {
                    continue;
                }
<<<<<<< HEAD
                equipment.StatsToEquip.Add(Defenses.GetID(i));
                stats.Add(new BaseStat(entity, Defenses.GetNameAt(i), Defenses.GetID(i), DefenseBonuses[i]));
=======
                equipment.StatsToEquip.Add(Defenses.GetValue(i));
                stats.Add(new BaseStat(entity, Defenses.GetNameAt(i), Defenses.GetValue(i), DefenseBonuses[i]));
>>>>>>> FirstPersonAction
            }
        }
    }
}
