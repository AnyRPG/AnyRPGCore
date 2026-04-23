using AnyRPG;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace AnyRPG {

    [InitializeOnLoad]
    public class AnyRPGStartup {

        static AnyRPGStartup() {
            EditorApplication.delayCall += () => {
                EditorApplication.delayCall += Initialize;
            };
        }

        private static void Initialize() {
            if (!EditorPrefs.HasKey("AnyRPG_DisplayWelcome")) {
                EditorPrefs.SetBool("AnyRPG_DisplayWelcome", true);
            }

            // Get the unique ID for this specific running instance of Unity
            int currentPID = Process.GetCurrentProcess().Id;
            int lastShownPID = EditorPrefs.GetInt("AnyRPG_LastShownPID", -1);

            // If the PIDs match, we've already shown it since Unity was opened.
            bool alreadyShownThisSession = (currentPID == lastShownPID);

            if (EditorPrefs.GetBool("AnyRPG_DisplayWelcome") && !alreadyShownThisSession) {
                EditorApplication.update -= TriggerWelcomeScreen;
                EditorApplication.update += TriggerWelcomeScreen;
            }
        }

        private static void TriggerWelcomeScreen() {
            if (EditorApplication.isUpdating || EditorApplication.isCompiling) return;

            // Final check: Mark this Process ID as "shown"
            int currentPID = Process.GetCurrentProcess().Id;
            EditorPrefs.SetInt("AnyRPG_LastShownPID", currentPID);

            WelcomeWindow.Open();

            EditorApplication.update -= TriggerWelcomeScreen;
        }
    }

}