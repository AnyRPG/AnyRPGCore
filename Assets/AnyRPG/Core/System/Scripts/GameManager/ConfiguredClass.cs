using UnityEngine;

namespace AnyRPG {
    public class ConfiguredClass {

        protected SystemGameManager systemGameManager = null;
        protected SystemConfigurationManager systemConfigurationManager = null;
        protected SystemDataFactory systemDataFactory = null;
        protected NetworkManagerClient networkManagerClient = null;
        protected NetworkManagerServer networkManagerServer = null;
        protected UserAccountService userAccountService = null;
        protected PlayerCharacterService playerCharacterService = null;
        protected SystemItemManager systemItemManager = null;

        public virtual void Configure(SystemGameManager systemGameManager) {
            //Debug.Log("ConfiguredClass.Configure(" + (systemGameManager == null ? "null" : systemGameManager.gameObject.name) + ")");
            this.systemGameManager = systemGameManager;
            SetGameManagerReferences();
        }

        public virtual void SetGameManagerReferences() {
            //Debug.Log("ConfiguredClass.SetGameManagerReferences() systemGameManager = " + (systemGameManager == null ? "null" : systemGameManager.gameObject.name));
            systemConfigurationManager = systemGameManager.SystemConfigurationManager;
            systemDataFactory = systemGameManager.SystemDataFactory;
            networkManagerClient = systemGameManager.NetworkManagerClient;
            networkManagerServer = systemGameManager.NetworkManagerServer;
            userAccountService = systemGameManager.UserAccountService;
            playerCharacterService = systemGameManager.PlayerCharacterService;
            systemItemManager = systemGameManager.SystemItemManager;
        }

    }

}