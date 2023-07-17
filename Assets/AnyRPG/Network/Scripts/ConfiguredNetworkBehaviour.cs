using AnyRPG;
using FishNet.Object;
using UnityEngine;

namespace AnyRPG {
    public abstract class ConfiguredNetworkBehaviour : NetworkBehaviour {

        protected SystemGameManager systemGameManager = null;
        protected SystemConfigurationManager systemConfigurationManager = null;

        protected int configureCount = 0;

        public virtual void Configure(SystemGameManager systemGameManager) {
            this.systemGameManager = systemGameManager;
            SetGameManagerReferences();
            configureCount++;
        }

        public virtual void SetGameManagerReferences() {
            systemConfigurationManager = systemGameManager.SystemConfigurationManager;
        }

    }

}