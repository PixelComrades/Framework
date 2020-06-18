﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PixelComrades {

    public static class StatExtensions {
        private const float Comparison = 0.001f;

        public static void SetupBasicCharacterStats(StatsContainer stats) {
            var owner = stats.GetEntity();
            for (int i = 0; i < Attributes.Count; i++) {
                stats.Add(new BaseStat(owner, Attributes.GetNameAt(i), Attributes.GetValue(i), 10));
            }
            for (int i = 0; i < Skills.Count; i++) {
                stats.Add(new BaseStat(owner, Skills.GetNameAt(i), Skills.GetValue(i), 0));
            }
            stats.Add(new BaseStat(owner, "Level", Stats.Level, 1));
            stats.Add(new BaseStat(owner, "Move Speed", Stats.MoveSpeed, 6));
            stats.Add(new BaseStat(owner, "Reach", Stats.Reach, 1));
            var weightStat = stats.Add(new BaseStat(owner, "Weight", Stats.Weight, 1));
            stats.Get<BaseStat>(Attributes.Strength).AddDerivedStat(10, weightStat);
            stats.Add(new DiceStat(owner, "Unarmed Damage", Stats.UnarmedAttackDamage, new DiceValue(1, DiceSides.D4)));
            stats.Add(new BaseStat(owner, "Unarmed Attack", Stats.UnarmedAttackAccuracy, 0));
        }

        public static void SetupVitalStats(StatsContainer stats) {
            var owner = stats.GetEntity();
            for (int i = 0; i < Vitals.Count; i++) {
                var vital = new VitalStat(owner, Vitals.GetNameAt(i), Vitals.GetValue(i), Vitals.GetStartingValue(i), Vitals.GetRecoveryValue(i));
                stats.Add(vital);
            }
            stats.SetMax();
        }

        public static void SetupDefenseStats(StatsContainer stats) {
            var owner = stats.GetEntity();
            for (int i = 0; i < Defenses.Count; i++) {
                var typeDef = new BaseStat(owner, Defenses.GetNameAt(i), Defenses.GetValue(i), 0);
                stats.Add(typeDef);
            }
        }

        public static BaseStat[] GetBasicCommandStats(StatsContainer stats) {
            var owner = stats.GetEntity();
            BaseStat[] newStats = new BaseStat[3];
            newStats[0] = new BaseStat(owner, Stats.Power, 0);
            newStats[1] = new BaseStat(owner, Stats.CriticalHit, 0);
            newStats[2] = new BaseStat(owner, Stats.CriticalMulti, GameOptions.Get(RpgSettings.DefaultCritMulti, 1f));
            return newStats;
        }

        public static void GetCharacterStatValues(this StatsContainer statsContainer, ref StringBuilder sb) {
            for (int i = 0; i < Attributes.Count; i++) {
                sb.AppendNewLine(statsContainer.Get(Attributes.GetValue(i)).ToString());
            }
            // var atkStats = GameData.Enums[Stats.AttackStats];
            // if (atkStats != null) {
            //     for (int i = 0; i < atkStats.Length; i++) {
            //         sb.AppendNewLine(statsContainer.Get(atkStats.IDs[i]).ToString());
            //     }
            // }
            for (int i = 0; i < Defenses.Count; i++) {
                sb.AppendNewLine(statsContainer.Get(Defenses.GetValue(i)).ToString());
            }
        }

        public static void AddStatList(Entity entity, DataList data, Equipment equipment) {
            if (data == null) {
                return;
            }
            var stats = entity.GetOrAdd<StatsContainer>();
            for (int i = 0; i < data.Count; i++) {
                var statEntry = data[i];
                var statName = statEntry.GetValue<string>(DatabaseFields.Stat);
                if (string.IsNullOrEmpty(statName)) {
                    continue;
                }
                var label = statEntry.TryGetValue("Label", statName);
                var amount = statEntry.GetValue<int>(DatabaseFields.Amount);
                var multiplier = statEntry.GetValue<float>(DatabaseFields.Multiplier);
                var addToEquip = statEntry.GetValue<bool>(DatabaseFields.AddToEquipList);
                if (equipment != null && addToEquip) {
                    if (!equipment.StatsToEquip.Contains(statName)) {
                        equipment.StatsToEquip.Add(statName);
                    }
                }
                if (Math.Abs(multiplier) < Comparison || Math.Abs(multiplier - 1) < Comparison) {
                    stats.GetOrAdd(statName, label).AddToBase(amount);
                    continue;
                }
                var stat = stats.Get(statName);
                string id = "";
                label = "";
                float adjustedAmount = amount;
                RangeStat rangeStat;
                if (stat != null) {
                    adjustedAmount = stat.BaseValue + amount;
                    rangeStat = stat as RangeStat;
                    if (rangeStat != null) {
                        rangeStat.SetValue(adjustedAmount, adjustedAmount * multiplier);
                        continue;
                    }
                    id = stat.ID;
                    label = stat.Label;
                    stats.Remove(stat);
                }
                if (string.IsNullOrEmpty(id)) {
                    var fakeEnum = GameData.Enums.GetEnumIndex(statName, out var index);
                    if (fakeEnum != null) {
                        id = fakeEnum.GetID(index);
                        label = fakeEnum.GetNameAt(index);
                    }
                    else {
                        id = label = statName;
                    }
                }
                rangeStat = new RangeStat(entity, label, id, adjustedAmount, adjustedAmount * multiplier);
                stats.Add(rangeStat);
            }
        }

        public static void AddStatList(Entity entity, DataList data, float multi) {
            if (data == null) {
                return;
            }
            var stats = entity.GetOrAdd<StatsContainer>();
            for (int i = 0; i < data.Count; i++) {
                var statEntry = data[i];
                var statName = statEntry.GetValue<string>(DatabaseFields.Stat);
                if (string.IsNullOrEmpty(statName)) {
                    continue;
                }
                var amount = statEntry.GetValue<int>(DatabaseFields.Amount);
                stats.GetOrAdd(statName).AddToBase(amount + (amount * multi));
            }
        }
    }
}
