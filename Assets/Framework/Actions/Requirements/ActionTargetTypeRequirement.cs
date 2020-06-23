﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public sealed class ActionTargetTypeRequirement : IActionRequirement {
        private TargetType _targetType;

        public ActionTargetTypeRequirement(TargetType targetType) {
            _targetType = targetType;
        }

<<<<<<< HEAD
        public string Description(BaseActionTemplate template, CharacterTemplate character) {
            return string.Format("Requires: {0}", _targetType);
        }

        public bool CanTarget(BaseActionTemplate template, CharacterTemplate character, CharacterTemplate target) {
=======
        public string Description(ActionTemplate template, CharacterTemplate character) {
            return string.Format("Requires: {0}", _targetType);
        }

        public bool CanTarget(ActionTemplate template, CharacterTemplate character, CharacterTemplate target) {
>>>>>>> FirstPersonAction
            switch (_targetType) {
                default:
                case TargetType.Any:
                    return true;
                case TargetType.Self:
                    return character == target;
                case TargetType.Enemy:
                    return World.Get<FactionSystem>().AreEnemies(character, target);
                case TargetType.Friendly:
                    return World.Get<FactionSystem>().AreFriends(character, target);
            }
        }

<<<<<<< HEAD
        public bool CanEffect(BaseActionTemplate template, CharacterTemplate character, CharacterTemplate target) {
=======
        public bool CanEffect(ActionTemplate template, CharacterTemplate character, CharacterTemplate target) {
>>>>>>> FirstPersonAction
            switch (_targetType) {
                default:
                case TargetType.Any:
                    return true;
                case TargetType.Self:
                    return character == target;
                case TargetType.Enemy:
                    return World.Get<FactionSystem>().AreEnemies(character, target);
                case TargetType.Friendly:
                    return World.Get<FactionSystem>().AreFriends(character, target);
            }
        }
    }
}
