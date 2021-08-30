using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AnyRPG;

[CustomEditor(typeof(Interactable))]
public class InteractableEditor : Editor {
    List<int> validLayers = null;

    public void OnEnable() {
        validLayers = new List<int> {
            LayerMask.NameToLayer("Player"),
            LayerMask.NameToLayer("CharacterUnit"),
            LayerMask.NameToLayer("Interactable"),
            LayerMask.NameToLayer("Triggers")
            };
    }

    public override void OnInspectorGUI()
    {
        Interactable interactable = target as Interactable;
        if (!validLayers.Contains(interactable.gameObject.layer)) {
            string l = LayerMask.LayerToName(interactable.gameObject.layer);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.HelpBox("This game object is on the " + l + " layer and won't be reachable yet. Select Interactable instead.", MessageType.Warning);
            if (GUILayout.Button("Fix it")) {
                interactable.gameObject.layer = LayerMask.NameToLayer("Interactable");
            }
            EditorGUILayout.EndHorizontal();
        }
        DrawDefaultInspector();
    }
}
