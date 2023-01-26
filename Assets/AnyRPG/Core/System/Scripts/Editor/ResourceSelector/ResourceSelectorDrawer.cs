using UnityEngine;
using UnityEditor;
using AnyRPG;

namespace AnyRPG.EditorTools {

    [CustomPropertyDrawer(typeof(ResourceSelectorAttribute))]
    public class ResourceSelectorDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            ResourceSelectorAttribute resourceAttribute = attribute as ResourceSelectorAttribute;
            if (property.propertyType == SerializedPropertyType.String) {
                EditorGUI.BeginProperty(position, label, property);
                position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
                Rect textPos = new Rect(position.x, position.y, position.width - 20, position.height);
                EditorGUI.PropertyField(textPos, property, GUIContent.none);
                if (GUI.Button(new Rect(position.x + position.width - 20, position.y, 20, position.height), ">")) {
                    ResourceSelector.DisplaySelectionDialog(resourceAttribute.resourceType, property);
                    GUIUtility.ExitGUI();
                }
                EditorGUI.EndProperty();
            } else {
                base.OnGUI(position, property, label);
            }
        }
    }

}

