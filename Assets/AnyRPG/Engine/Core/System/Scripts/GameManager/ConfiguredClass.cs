using AnyRPG;
using UnityEngine;

namespace AnyRPG {
    public class ConfiguredClass {

        protected SystemGameManager systemGameManager = null;
        protected SystemConfigurationManager systemConfigurationManager = null;

        public virtual void Configure(SystemGameManager systemGameManager) {
            //Debug.Log("ConfiguredClass.Configure(" + (systemGameManager == null ? "null" : systemGameManager.gameObject.name) + ")");
            this.systemGameManager = systemGameManager;
            SetGameManagerReferences();
        }

        public virtual void SetGameManagerReferences() {
            //Debug.Log("ConfiguredClass.SetGameManagerReferences()");
            systemConfigurationManager = systemGameManager.SystemConfigurationManager;
        }

    }

}