using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
//using UnityEditor.UIElements;
using UnityEngine;
//using UnityEngine.UIElements;


namespace AnyRPG {

    //[CustomPropertyDrawer(typeof(ItemLevelProperties))]
    public class ItemLevelPropertiesDrawer : PropertyDrawer {
        /*
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {

            VisualElement container = new VisualElement();

            VisualElement dynamicLevelField = new PropertyField(property.FindPropertyRelative("dynamicLevel"));
            SerializedProperty dynamicLevelProperty = property.FindPropertyRelative(nameof(ItemLevelProperties.dynamicLevel));
            VisualElement freezeDropLevelField = new PropertyField(property.FindPropertyRelative("freezeDropLevel"));
            VisualElement levelCapField = new PropertyField(property.FindPropertyRelative("levelCap"));
            VisualElement itemLevelField = new PropertyField(property.FindPropertyRelative("itemLevel"));
            VisualElement useLevelField = new PropertyField(property.FindPropertyRelative("useLevel"));

            container.Add(dynamicLevelField);
            if (dynamicLevelProperty.boolValue == true) {
                container.Add(freezeDropLevelField);
                container.Add(levelCapField);
            } else {
                container.Add(itemLevelField);
            }
            container.Add(useLevelField);

            return container;
        }
        */

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            base.OnGUI(position, property, label);

            /*
            EditorGUI.BeginProperty(position, label, property);

            Rect dynamicLevelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            Rect freezeDropLevelRect = new Rect(position.x, position.y + 20f, position.width, EditorGUIUtility.singleLineHeight);
            Rect levelCapRect = new Rect(position.x, position.y + 40f, position.width, EditorGUIUtility.singleLineHeight);
            Rect itemLevelRect = new Rect(position.x, position.y + 60f, position.width, EditorGUIUtility.singleLineHeight);
            Rect useLevelRect = new Rect(position.x, position.y + 80f, position.width, EditorGUIUtility.singleLineHeight);

            EditorGUI.PropertyField(dynamicLevelRect, property.FindPropertyRelative("dynamicLevel"));
            EditorGUI.PropertyField(freezeDropLevelRect, property.FindPropertyRelative("freezeDropLevel"));
            EditorGUI.PropertyField(levelCapRect, property.FindPropertyRelative("levelCap"));
            EditorGUI.PropertyField(itemLevelRect, property.FindPropertyRelative("itemLevel"));
            EditorGUI.PropertyField(useLevelRect, property.FindPropertyRelative("useLevel"));

            EditorGUI.EndProperty();
            */
        }
    }
}

