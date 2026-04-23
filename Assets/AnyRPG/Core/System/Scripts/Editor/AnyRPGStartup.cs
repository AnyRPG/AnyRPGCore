using AnyRPG;
using UnityEditor;
using UnityEngine;

namespace AnyRPG {
    /*
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
    */

    [InitializeOnLoad]
    public class AnyRPGStartup {

        static AnyRPGStartup() {
            // delayCall is the secret: it runs once the Editor is fully "idle" and ready.
            EditorApplication.delayCall += Initialize;
        }

        private static void Initialize() {
            // 1. Switch to EditorPrefs for more reliable Editor-only settings.
            // It persists even if the user clears their in-game PlayerPrefs.
            if (!EditorPrefs.HasKey("AnyRPG_DisplayWelcome")) {
                EditorPrefs.SetBool("AnyRPG_DisplayWelcome", true);
            }

            // 2. Subscribe to update ONLY if we need to show the window.
            if (EditorPrefs.GetBool("AnyRPG_DisplayWelcome") && EditorApplication.timeSinceStartup < 30f) {
                EditorApplication.update += TriggerWelcomeScreen;
            }
        }

        private static void TriggerWelcomeScreen() {
            // Double check the editor isn't currently busy/importing
            if (EditorApplication.isUpdating || EditorApplication.isCompiling) return;

            WelcomeWindow.Open();

            // Always unsubscribe immediately so it only runs once
            EditorApplication.update -= TriggerWelcomeScreen;
        }
    }


}