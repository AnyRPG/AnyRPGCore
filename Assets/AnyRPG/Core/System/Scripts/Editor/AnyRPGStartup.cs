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
    /*
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
*/

    [InitializeOnLoad]
    public class AnyRPGStartup {

        static AnyRPGStartup() {
            // Double delay ensures we stay in the queue during the initial heavy import
            EditorApplication.delayCall += () => {
                EditorApplication.delayCall += Initialize;
            };
        }

        private static void Initialize() {
            // Preserving your EditorPrefs keys and logic
            if (!EditorPrefs.HasKey("AnyRPG_DisplayWelcome")) {
                EditorPrefs.SetBool("AnyRPG_DisplayWelcome", true);
            }

            // Removed the < 30f check so it doesn't time out during long imports.
            // We only subscribe if the user actually wants to see the window.
            if (EditorPrefs.GetBool("AnyRPG_DisplayWelcome")) {
                EditorApplication.update -= TriggerWelcomeScreen;
                EditorApplication.update += TriggerWelcomeScreen;
            }
        }

        private static void TriggerWelcomeScreen() {
            // This is the key for large packages: 
            // It stays here until the 3-minute import/compilation is 100% finished.
            if (EditorApplication.isUpdating || EditorApplication.isCompiling) {
                return;
            }

            WelcomeWindow.Open();

            // Unsubscribe immediately so it doesn't loop
            EditorApplication.update -= TriggerWelcomeScreen;
        }
    }




}