using AnyRPG;
using UnityEditor;
using UnityEngine;

namespace AnyRPG {

    [InitializeOnLoad]
    public class AnyRPGStartup {

        static AnyRPGStartup() {
            EditorApplication.update -= TriggerWelcomeScreen;
            EditorApplication.update += TriggerWelcomeScreen;
        }

        private static void TriggerWelcomeScreen() {
            var showAtStartup = PlayerPrefs.GetInt("DisplayWelcomeScreen") == 1 && EditorApplication.timeSinceStartup < 30f;

            if (showAtStartup) {
                WelcomeWindow.Open();
            }
            EditorApplication.update -= TriggerWelcomeScreen;
        }

        private static void PlayModeChanged() {
            EditorApplication.update -= TriggerWelcomeScreen;
        }
    }
}