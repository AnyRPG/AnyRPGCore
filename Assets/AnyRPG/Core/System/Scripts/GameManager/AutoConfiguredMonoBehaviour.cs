using AnyRPG;
using UnityEngine;

namespace AnyRPG {
    /// <summary>
    /// This class will be auto-configured by the game manager when a level is loaded
    /// </summary>
    public class AutoConfiguredMonoBehaviour : ConfiguredMonoBehaviour {
        
        public void AutoConfigure(SystemGameManager systemGameManager) {
            Configure(systemGameManager);
            PostConfigure();
        }

        protected virtual void PostConfigure() {
            // nothing here for now
        }
    }

}