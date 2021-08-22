using AnyRPG;
using UnityEngine;

namespace AnyRPG {
    public abstract class ConfiguredMonoBehaviour : MonoBehaviour {

        protected SystemGameManager systemGameManager = null;
        protected SystemConfigurationManager systemConfigurationManager = null;

        public virtual void Configure(SystemGameManager systemGameManager) {
            this.systemGameManager = systemGameManager;
            SetGameManagerReferences();
        }

        public virtual void SetGameManagerReferences() {
            systemConfigurationManager = systemGameManager.SystemConfigurationManager;
        }

    }

}