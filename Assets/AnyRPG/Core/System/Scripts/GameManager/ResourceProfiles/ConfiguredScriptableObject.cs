using AnyRPG;
using UnityEngine;

namespace AnyRPG {
    public abstract class ConfiguredScriptableObject : ScriptableObject {

        protected SystemGameManager systemGameManager = null;
        protected SystemConfigurationManager systemConfigurationManager = null;
        //protected SystemDataFactory systemDataFactory = null;

        public virtual void Configure(SystemGameManager systemGameManager) {
            this.systemGameManager = systemGameManager;
            SetGameManagerReferences();
        }

        public virtual void SetGameManagerReferences() {
            systemConfigurationManager = systemGameManager.SystemConfigurationManager;
            //systemDataFactory = systemGameManager.SystemDataFactory;
        }

    }

}