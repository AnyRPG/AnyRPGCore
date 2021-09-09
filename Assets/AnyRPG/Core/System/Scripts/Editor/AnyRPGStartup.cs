using AnyRPG;
using UnityEditor;
using UnityEngine;

namespace AnyRPG {

    [InitializeOnLoad]
    public class AnyRPGStartup {

        static AnyRPGStartup() {
            if (PlayerPrefs.HasKey("DisplayWelcomeWindow") == false) {
                PlayerPrefs.SetInt("DisplayWelcomeWindow", 1);
            }

            EditorApplication.update -= TriggerWelcomeScreen;
            EditorApplication.update += TriggerWelcomeScreen;
        }

        private static void TriggerWelcomeScreen() {
            bool showAtStartup = PlayerPrefs.GetInt("DisplayWelcomeWindow") == 1 && EditorApplication.timeSinceStartup < 30f;

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