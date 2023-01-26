using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AnyRPG.EditorTools {

    [CustomEditor(typeof(SkyboxBlender))]
    public class SkyboxBlenderEditor : Editor {
        SerializedProperty skyBox1;
        SerializedProperty skyBox2;

        SerializedProperty blendedSkybox;
        SerializedProperty exposure;
        SerializedProperty rotation;
        SerializedProperty tint;
        SerializedProperty invertColors;
        SerializedProperty blendMode;
        SerializedProperty blend;

        SerializedProperty bindOnStart;
        SerializedProperty updateLightingOnStart;
        SerializedProperty updateLightingEveryFrame;
        SerializedProperty updateReflectionsOnStart;
        SerializedProperty updateReflectionsEveryFrame;

        SerializedProperty reflectionResolution;

        private void OnEnable() {
            skyBox1 = serializedObject.FindProperty("skyBox1");
            skyBox2 = serializedObject.FindProperty("skyBox2");

            blendedSkybox = serializedObject.FindProperty("blendedSkybox");
            exposure = serializedObject.FindProperty("exposure");
            rotation = serializedObject.FindProperty("rotation");
            tint = serializedObject.FindProperty("tint");
            invertColors = serializedObject.FindProperty("invertColors");
            blendMode = serializedObject.FindProperty("blendMode");
            blend = serializedObject.FindProperty("blend");

            bindOnStart = serializedObject.FindProperty("bindOnStart");
            updateLightingOnStart = serializedObject.FindProperty("updateLightingOnStart");
            updateLightingEveryFrame = serializedObject.FindProperty("updateLightingEveryFrame");
            updateReflectionsOnStart = serializedObject.FindProperty("updateReflectionsOnStart");
            updateReflectionsEveryFrame = serializedObject.FindProperty("updateReflectionsEveryFrame");

            reflectionResolution = serializedObject.FindProperty("reflectionResolution"); ;
        }

        public override void OnInspectorGUI() {
            SkyboxBlender skyboxBlender = (SkyboxBlender)target;

            serializedObject.Update();

            //Input skyboxes
            EditorGUILayout.BeginVertical("HelpBox");
            EditorGUILayout.LabelField("Input Skyboxes", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(skyBox1);
            EditorGUILayout.PropertyField(skyBox2);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(bindOnStart);
            if (GUILayout.Button("Bind Textures")) { skyboxBlender.BindTextures(); }

            EditorGUILayout.Space();
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            //Blended skyboxes
            EditorGUILayout.BeginVertical("HelpBox");
            EditorGUILayout.LabelField("Blended Skybox", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(blendedSkybox);
            EditorGUILayout.PropertyField(exposure);
            EditorGUILayout.PropertyField(rotation);
            EditorGUILayout.PropertyField(tint);
            EditorGUILayout.PropertyField(invertColors);
            EditorGUILayout.PropertyField(blendMode);
            EditorGUILayout.PropertyField(blend);

            EditorGUILayout.Space();
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            //Environment lighting
            EditorGUILayout.BeginVertical("HelpBox");
            EditorGUILayout.LabelField("Environment Lighting", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(updateLightingOnStart);
            EditorGUILayout.PropertyField(updateLightingEveryFrame);
            if (GUILayout.Button("Update Lighting")) { skyboxBlender.UpdateLighting(); }

            EditorGUILayout.Space();
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            //Environment reflections
            EditorGUILayout.BeginVertical("HelpBox");
            EditorGUILayout.LabelField("Environment Reflections", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(reflectionResolution);
            if (GUILayout.Button("Update Probe")) { skyboxBlender.UpdateReflectionProbe(); }

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(updateReflectionsOnStart);
            EditorGUILayout.PropertyField(updateReflectionsEveryFrame);
            if (GUILayout.Button("Update Reflections")) { skyboxBlender.UpdateReflections(); }

            EditorGUILayout.Space();
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();


            serializedObject.ApplyModifiedProperties();
        }
    }

}