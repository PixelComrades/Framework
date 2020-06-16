using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine.AddressableAssets;

namespace PixelComrades {
//    [CustomPropertyDrawer(typeof(RandomObjectHolder))]
//    public class RandomObjectHolderDrawer : PropertyDrawer {
//        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
//            TargetAnimator animator = PropertyDrawerUtility.GetActualObjectForSerializedProperty<TargetAnimator>(fieldInfo, property);
//        }
//    }


    [CustomPropertyDrawer (typeof (EnumLabelArrayAttribute), true)]
    public class EnumLabelArrayAttributeDrawer : PropertyDrawer {

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            // Properly configure height for expanded contents.
            return EditorGUI.GetPropertyHeight(property, label, property.isExpanded);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            try {
                var config = (EnumLabelArrayAttribute) attribute;
                var enumNames = Enum.GetNames(config.Labels);
                if (property.isArray && property.arraySize != enumNames.Length) {
                    property.arraySize = enumNames.Length;
                    while (property.arraySize < enumNames.Length) {
                        property.InsertArrayElementAtIndex(0);
                    }
                }

                var match = Regex.Match(property.propertyPath, "\\[(\\d)\\]", RegexOptions.RightToLeft);
                int pos = int.Parse(match.Groups[1].Value);

                // Make names nicer to read (but won't exactly match enum definition).
                var enumLabel = ObjectNames.NicifyVariableName(enumNames[pos].ToLower());
                label = new GUIContent(enumLabel);
            }
            catch {
                // keep default label
            }
            if (property.isArray) {
                for (int i = 0; i < property.arraySize; i++) {
                    var arrayProperty = property.GetArrayElementAtIndex(i);
                    if (property.GetTargetObjectOfProperty() is AssetReferenceEntry arrayTarget) {
                        AssetEntryDrawer.Display(position, property, label, arrayTarget);
                    }
                    else {
                        EditorGUI.PropertyField(position, arrayProperty, label);
                    }
                    position.y += 20;
                }
                return;
            }
            if (property.GetTargetObjectOfProperty() is AssetReferenceEntry target) {
                AssetEntryDrawer.Display(position, property, label, target);
            }
            else {
                EditorGUI.PropertyField(position, property, label);
            }
        }
    }
    
    [CustomPropertyDrawer (typeof (DropdownListAttribute), true)]
    public class StringInListDrawer : PropertyDrawer {
        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var stringInList = (DropdownListAttribute) attribute;
            var list = stringInList.List;
            if (list == null) {
                return;
            }
            if (property.propertyType == SerializedPropertyType.String) {
                int index = Mathf.Max(0, Array.IndexOf(list, property.stringValue));
                index = EditorGUI.Popup(position, property.displayName, index, list);
                property.stringValue = list[index];
            }
            else if (property.propertyType == SerializedPropertyType.Integer) {
                property.intValue = EditorGUI.Popup(position, property.displayName, property.intValue, list);
            }
            // else {
            //     base.OnGUI(position, property, label);
            // }
        }
    }

    [CustomPropertyDrawer(typeof(AssetReferenceEntry), true)]
    public class AssetEntryDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            if (property.isArray) {
                for (int i = 0; i < property.arraySize; i++) {
                    var arrayProperty = property.GetArrayElementAtIndex(i);
                    Display(position, arrayProperty, label, arrayProperty.GetTargetObjectOfProperty() as AssetReferenceEntry);
                    position.y += 20;
                }
                return;
            }
            Display(position, property, label, property.GetTargetObjectOfProperty() as AssetReferenceEntry);
        }

        public static void Display(Rect position, SerializedProperty property, GUIContent label, AssetReferenceEntry target) {
            if (target == null) {
                EditorGUI.LabelField(position, property.type);
                return;
            }
            if (target.Asset == null) {
                if (!string.IsNullOrEmpty(target.Path)) {
                    ApplyModified(property, target, AssetReferenceUtilities.LoadAsset(target));
                }
            }
            // else {
            //     var path = AssetDatabase.GetAssetPath(target.AssetReference);
            //     if (target.Path != path) {
            //         target.Path = path;
            //         property.serializedObject.ApplyModifiedProperties();
            //     }
            // }
            bool multiEntry = false;
            string[] split = null;
            if (target.Path != null) {
                label.tooltip = target.Path.Replace("Assets/", "");
                split = target.Path.SplitFromEntryBreak();
                if (split.Length > 1) {
                    multiEntry = true;
                    position.height *= 0.5f;
                    label.tooltip = split[0].Replace("Assets/", "") + split[1];
                }
                else {
                    label.tooltip = split[0].Replace("Assets/", "");
                }
            }
            // var subAsset = target as SubAssetReferenceEntry;
            // if (subAsset != null) {
            //     position.height *= 0.5f;
            // }
            var obj = EditorGUI.ObjectField(position, label, target.Asset, target.Type, false);
            if (obj != target.Asset) {
                ApplyModified(property, target, obj);
            }
            if (multiEntry) {
                position.position = new Vector2(position.x + EditorGUIUtility.labelWidth, position.y + position.height);
                EditorGUI.LabelField(position, split[1]);
            }
        }

        private static void ApplyModified(SerializedProperty property, AssetReferenceEntry entry, UnityEngine.Object obj) {
            if (obj != null) {
                AddressableAssetEditorUtility.GetOrCreateEntry(obj);
            }
            entry.AssetReference.SetEditorAsset(obj);
            AssetReferenceUtilities.SetPath(entry);
            property.serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(property.serializedObject.targetObject);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            var target = property.GetTargetObjectOfProperty() as AssetReferenceEntry;
            if (target != null) {
                var path = target.Path.SplitFromEntryBreak();
                if (path != null && path.Length > 1) {
                    return base.GetPropertyHeight(property, label) * 2;
                }
                //if (target is SubAssetReferenceEntry) {
            }
            return base.GetPropertyHeight(property, label);
        }
    }

    [CustomPropertyDrawer(typeof(FloatRange))]
    public class FloatRangeDrawer : PropertyDrawer {
        private const float MinPercent = 0;
        private const float MaxPercent = 100;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            label = EditorGUI.BeginProperty(position, label, property);
            Rect contentPosition = EditorGUI.PrefixLabel(position, label);
            EditorGUI.indentLevel = 0;
            contentPosition.height *= 0.5f;
            contentPosition.width *= 0.5f;
            SerializedProperty min = property.FindPropertyRelative("Min");
            SerializedProperty max = property.FindPropertyRelative("Max");
            min.floatValue = EditorGUI.FloatField(contentPosition, min.floatValue);
            contentPosition.x += contentPosition.width;
            max.floatValue = EditorGUI.FloatField(contentPosition, max.floatValue);
            contentPosition.x -= contentPosition.width;
            contentPosition.width *= 2f;
            contentPosition.y += contentPosition.height;
            float minfloat = (float)System.Math.Round((decimal)min.floatValue);
            float maxfloat = (float)System.Math.Round((decimal)max.floatValue);
            EditorGUI.MinMaxSlider(contentPosition, ref minfloat, ref maxfloat, MinPercent, MaxPercent);
            min.floatValue = minfloat;
            max.floatValue = maxfloat;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return base.GetPropertyHeight(property, label) * 2f + 5f;
        }
    }

    [CustomPropertyDrawer(typeof(IntRange))]
    public class IntRangeDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            label = EditorGUI.BeginProperty(position, label, property);
            Rect contentPosition = EditorGUI.PrefixLabel(position, label);
            EditorGUI.indentLevel = 0;
            // contentPosition.height *= 0.5f;
            contentPosition.width *= 0.5f;
            SerializedProperty min = property.FindPropertyRelative("Min");
            SerializedProperty max = property.FindPropertyRelative("Max");
            min.intValue = EditorGUI.IntField(contentPosition, min.intValue);
            contentPosition.x += contentPosition.width;
            max.intValue = EditorGUI.IntField(contentPosition, max.intValue);
            contentPosition.x -= contentPosition.width;
            contentPosition.width *= 2f;
            // contentPosition.y += contentPosition.height;
            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(DiceValue))]
    public class DiceValueDrawer : PropertyDrawer {
        
        private static GUIStyle _labelStyle = new GUIStyle() {
            alignment = TextAnchor.MiddleCenter,
        };

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            label = EditorGUI.BeginProperty(position, label, property);
            Rect contentPosition = EditorGUI.PrefixLabel(position, label);
            EditorGUI.indentLevel = 0;
            contentPosition.width *= 0.25f;
            
            SerializedProperty diceRolls = property.FindPropertyRelative("DiceRolls");
            SerializedProperty diceSides = property.FindPropertyRelative("DiceSides");
            SerializedProperty bonus = property.FindPropertyRelative("Bonus");
            
            diceRolls.intValue = EditorGUI.IntField(contentPosition, diceRolls.intValue);
            
            contentPosition.x += contentPosition.width;
            diceSides.intValue = (int) (DiceSides) EditorGUI.EnumPopup(contentPosition, (DiceSides)  diceSides.intValue );

            contentPosition.x += contentPosition.width;
            EditorGUI.LabelField(contentPosition, "+", _labelStyle);
            
            contentPosition.x += contentPosition.width;
            bonus.intValue = EditorGUI.IntField(contentPosition, bonus.intValue);
            
            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(GenericKeyedValue), true)]
    public class GenericValueDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            label = EditorGUI.BeginProperty(position, label, property);
            Rect contentPosition = EditorGUI.PrefixLabel(position, label);
            // EditorGUI.indentLevel = 0;
            EditorGUIUtility.labelWidth *= 0.35f;
            contentPosition.width *= 0.5f;
            SerializedProperty key = property.FindPropertyRelative("Key");
            SerializedProperty value = property.FindPropertyRelative("Value");
            EditorGUI.PropertyField(contentPosition, key);
            contentPosition.x += contentPosition.width * 1.05f;
            contentPosition.width *= 0.95f;
            EditorGUI.PropertyField(contentPosition, value);
            EditorGUI.EndProperty();
            EditorGUIUtility.labelWidth = 0;
        }
    }

    [CustomPropertyDrawer(typeof(NormalizedFloatRange))]
    public class NormalizedFloatRangeDrawer : PropertyDrawer {
        private const float MinPercent = 0;
        private const float MaxPercent = 1;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            label = EditorGUI.BeginProperty(position, label, property);
            Rect contentPosition = EditorGUI.PrefixLabel(position, label);
            EditorGUI.indentLevel = 0;
            contentPosition.height *= 0.5f;
            contentPosition.width *= 0.5f;
            SerializedProperty min = property.FindPropertyRelative("Min");
            SerializedProperty max = property.FindPropertyRelative("Max");
            min.floatValue = EditorGUI.FloatField(contentPosition, min.floatValue);
            contentPosition.x += contentPosition.width;
            max.floatValue = EditorGUI.FloatField(contentPosition, max.floatValue);
            contentPosition.x -= contentPosition.width;
            contentPosition.width *= 2f;
            contentPosition.y += contentPosition.height;
            float minfloat = min.floatValue;
            float maxfloat = max.floatValue;
            EditorGUI.MinMaxSlider(contentPosition, ref minfloat, ref maxfloat, MinPercent, MaxPercent);
            min.floatValue = minfloat;
            max.floatValue = maxfloat;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return base.GetPropertyHeight(property, label) * 2f + 5f;
        }
    }

    [CustomPropertyDrawer(typeof(TargetAnimator), true)]
    public class TargetAnimatorDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            label = EditorGUI.BeginProperty(position, label, property);
            TargetAnimator animator = PropertyDrawerUtility.GetActualObjectForSerializedProperty<TargetAnimator>(fieldInfo, property);
            if (animator != null) {
                var contentPosition = EditorGUI.PrefixLabel(position, label);
                var width = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth *= 0.5f;
                EditorGUI.PropertyField(contentPosition, property, new GUIContent(animator.Description));
                EditorGUIUtility.labelWidth = width;
            }
            else {
                EditorGUI.PropertyField(position, property, label);
            }
            //EditorGUI.PropertyField(position, property, animator != null ? new GUIContent(animator.Description) : label);
            EditorGUI.EndProperty();
        }

        //protected override void DrawPropertyRect(Rect position, IPropertyValueEntry<T> entry, GUIContent label) {

        //    //var property = entry.SmartValue[ as SerializedProperty;
        //    //label = EditorGUI.BeginProperty(position, label, property);

        //    //TargetAnimator animator = PropertyDrawerUtility.GetActualObjectForSerializedProperty<TargetAnimator>(fieldInfo, property);
        //    var animator = entry.SmartValue;
        //    if (animator != null) {
        //        //var contentPosition = EditorGUI.PrefixLabel(position, label);
        //        var width = EditorGUIUtility.labelWidth;
        //        EditorGUIUtility.labelWidth *= 0.5f;
        //        InspectorUtilities.DrawProperty(entry.Property, new GUIContent(animator.Description));
        //        //EditorGUI.PropertyField(contentPosition, property, new GUIContent(animator.Description));
        //        EditorGUIUtility.labelWidth = width;
        //    }
        //    else {
        //        InspectorUtilities.DrawProperty(entry.Property, label);
        //        //EditorGUI.PropertyField(position, property, label);
        //    }
        //    //EditorGUI.PropertyField(position, property, animator != null ? new GUIContent(animator.Description) : label);
        //    EditorGUI.EndProperty();
        //    base.DrawPropertyRect(position, entry, label);
        //}
    }

    //[CustomPropertyDrawer(typeof(ImpactHolder), true)]
    //public class ImpactDrawer : PropertyDrawer {
    //    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
    //        label = EditorGUI.BeginProperty(position, label, property);
    //        ImpactHolder target = PropertyDrawerUtility.GetActualObjectForSerializedProperty<ImpactHolder>(fieldInfo, property);
    //        if (target != null) {
    //            var contentPosition = EditorGUI.PrefixLabel(position, label);
    //            var width = EditorGUIUtility.labelWidth;
    //            EditorGUIUtility.labelWidth *= 0.5f;
    //            EditorGUI.PropertyField(contentPosition, property, new GUIContent(target.Description.ToString()));
    //            EditorGUIUtility.labelWidth = width;
    //        }
    //        else {
    //            EditorGUI.PropertyField(position, property, label);
    //        }
    //        //EditorGUI.PropertyField(position, property, animator != null ? new GUIContent(animator.Description) : label);
    //        EditorGUI.EndProperty();
    //    }
    //}

    public class PropertyDrawerUtility {
        public static T GetActualObjectForSerializedProperty<T>(FieldInfo fieldInfo, SerializedProperty property) where T : class {
            var obj = fieldInfo.GetValue(property.serializedObject.targetObject);
            if (obj == null) { return null; }

            T actualObject = null;
            if (obj.GetType().IsArray) {
                var index = Convert.ToInt32(new string(property.propertyPath.Where(c => char.IsDigit(c)).ToArray()));
                if (index < ((T[])obj).Length) {
                    actualObject = ((T[])obj)[index];
                }
            }
            else {
                actualObject = obj as T;
            }
            return actualObject;
        }
    }
    /// <summary>
    /// Extension class for SerializedProperties
    /// See also: http://answers.unity3d.com/questions/627090/convert-serializedproperty-to-custom-class.html
    /// </summary>
    public static class SerializedPropertyExtensions 
     {
        public static object GetTargetObjectOfProperty(this SerializedProperty prop) {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements) {
                if (element.Contains("[")) {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else {
                    obj = GetValue_Imp(obj, element);
                }
            }
            return obj;
        }

        private static object GetValue_Imp(object source, string name, int index) {
            var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
            if (enumerable == null) return null;
            var enm = enumerable.GetEnumerator();
            //while (index-- >= 0)
            //    enm.MoveNext();
            //return enm.Current;

            for (int i = 0; i <= index; i++) {
                if (!enm.MoveNext()) return null;
            }
            return enm.Current;
        }

        private static object GetValue_Imp(object source, string name) {
            if (source == null)
                return null;
            var type = source.GetType();

            while (type != null) {
                var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f != null)
                    return f.GetValue(source);

                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null)
                    return p.GetValue(source, null);

                type = type.BaseType;
            }
            return null;
        }

        /// <summary>
        /// Get the object the serialized property holds by using reflection
        /// </summary>
        /// <typeparam name="T">The object type that the property contains</typeparam>
        /// <param name="property"></param>
        /// <returns>Returns the object type T if it is the type the property actually contains</returns>
        public static T GetValue<T>(this SerializedProperty property)
         {
             return GetNestedObject<T>(property.propertyPath, GetSerializedPropertyRootComponent(property));
         }
 
         /// <summary>
         /// Set the value of a field of the property with the type T
         /// </summary>
         /// <typeparam name="T">The type of the field that is set</typeparam>
         /// <param name="property">The serialized property that should be set</param>
         /// <param name="value">The new value for the specified property</param>
         /// <returns>Returns if the operation was successful or failed</returns>
         public static bool SetValue<T>(this SerializedProperty property, T value)
         {
             
             object obj = GetSerializedPropertyRootComponent(property);
             //Iterate to parent object of the value, necessary if it is a nested object
             string[] fieldStructure = property.propertyPath.Split('.');
             for (int i = 0; i < fieldStructure.Length - 1; i++)
             {
                 obj = GetFieldOrPropertyValue<object>(fieldStructure[i], obj);
             }
             string fieldName = fieldStructure.Last();
 
             return SetFieldOrPropertyValue(fieldName, obj, value);
             
         }
 
         /// <summary>
         /// Get the component of a serialized property
         /// </summary>
         /// <param name="property">The property that is part of the component</param>
         /// <returns>The root component of the property</returns>
         public static Component GetSerializedPropertyRootComponent(SerializedProperty property)
         {
             return (Component)property.serializedObject.targetObject;
         }
 
         /// <summary>
         /// Iterates through objects to handle objects that are nested in the root object
         /// </summary>
         /// <typeparam name="T">The type of the nested object</typeparam>
         /// <param name="path">Path to the object through other properties e.g. PlayerInformation.Health</param>
         /// <param name="obj">The root object from which this path leads to the property</param>
         /// <param name="includeAllBases">Include base classes and interfaces as well</param>
         /// <returns>Returns the nested object casted to the type T</returns>
         public static T GetNestedObject<T>(string path, object obj, bool includeAllBases = false)
         {
             foreach (string part in path.Split('.'))
             {
                 obj = GetFieldOrPropertyValue<object>(part, obj, includeAllBases);
             }
             return (T)obj;
         }
 
         public static T GetFieldOrPropertyValue<T>(string fieldName, object obj, bool includeAllBases = false, BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
         {
             FieldInfo field = obj.GetType().GetField(fieldName, bindings);
             if (field != null) return (T)field.GetValue(obj);
 
             PropertyInfo property = obj.GetType().GetProperty(fieldName, bindings);
             if (property != null) return (T)property.GetValue(obj, null);
 
             if (includeAllBases)
             {
 
                 foreach (Type type in GetBaseClassesAndInterfaces(obj.GetType()))
                 {
                     field = type.GetField(fieldName, bindings);
                     if (field != null) return (T)field.GetValue(obj);
 
                     property = type.GetProperty(fieldName, bindings);
                     if (property != null) return (T)property.GetValue(obj, null);
                 }
             }
 
             return default(T);
         }
 
         public static bool SetFieldOrPropertyValue(string fieldName, object obj, object value, bool includeAllBases = false, BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
         {
             FieldInfo field = obj.GetType().GetField(fieldName, bindings);
             if (field != null)
             {
                 field.SetValue(obj, value);
                 return true;
             }
 
             PropertyInfo property = obj.GetType().GetProperty(fieldName, bindings);
             if (property != null)
             {
                 property.SetValue(obj, value, null);
                 return true;
             }
 
             if (includeAllBases)
             {
                 foreach (Type type in GetBaseClassesAndInterfaces(obj.GetType()))
                 {
                     field = type.GetField(fieldName, bindings);
                     if (field != null)
                     {
                         field.SetValue(obj, value);
                         return true;
                     }
 
                     property = type.GetProperty(fieldName, bindings);
                     if (property != null)
                     {
                         property.SetValue(obj, value, null);
                         return true;
                     }
                 }
             }
             return false;
         }
 
         public static IEnumerable<Type> GetBaseClassesAndInterfaces(this Type type, bool includeSelf = false)
         {
             List<Type> allTypes = new List<Type>();
 
             if (includeSelf) allTypes.Add(type);
 
             if (type.BaseType == typeof(object))
             {
                 allTypes.AddRange(type.GetInterfaces());
             }
             else {
                 allTypes.AddRange(
                         Enumerable
                         .Repeat(type.BaseType, 1)
                         .Concat(type.GetInterfaces())
                         .Concat(type.BaseType.GetBaseClassesAndInterfaces())
                         .Distinct());
             }
 
             return allTypes;
         }
     }
}