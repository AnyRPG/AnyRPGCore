using AnyRPG;
using UnityEditor;
using UnityEngine;

namespace AnyRPG {
    
    [InitializeOnLoad]
    public class AnyRPGStartup {

        static AnyRPGStartup() {
            // Double delay ensures we stay in the queue during the initial heavy import
            EditorApplication.delayCall += () => {
                EditorApplication.delayCall += Initialize;
            };
        }

        private static void Initialize() {
            if (!EditorPrefs.HasKey("AnyRPG_DisplayWelcome")) {
                EditorPrefs.SetBool("AnyRPG_DisplayWelcome", true);
            }

            // Don't even bother with the update loop if the user opted out 
            // OR if we've already shown it since the Editor opened.
            bool alreadyShown = SessionState.GetBool("AnyRPG_AlreadyShown", false);
            if (EditorPrefs.GetBool("AnyRPG_DisplayWelcome") && !alreadyShown) {
                EditorApplication.update -= TriggerWelcomeScreen;
                EditorApplication.update += TriggerWelcomeScreen;
            }
        }


        private static void TriggerWelcomeScreen() {
            if (EditorApplication.isUpdating || EditorApplication.isCompiling) return;

            // SessionState is the key: it clears when you close Unity, but survives recompiles
            bool alreadyShownThisSession = SessionState.GetBool("AnyRPG_AlreadyShown", false);
            bool showAtStartup = EditorPrefs.GetBool("AnyRPG_DisplayWelcome") && !alreadyShownThisSession;

            if (showAtStartup) {
                WelcomeWindow.Open();
                SessionState.SetBool("AnyRPG_AlreadyShown", true);
            }

            EditorApplication.update -= TriggerWelcomeScreen;
        }

    }

}