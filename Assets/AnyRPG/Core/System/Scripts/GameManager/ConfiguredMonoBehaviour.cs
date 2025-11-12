using AnyRPG;
using UnityEngine;

namespace AnyRPG {
    public abstract class ConfiguredMonoBehaviour : MonoBehaviour {

        protected PhysicsScene physicsScene;

        public PhysicsScene PhysicsScene { get => physicsScene; }

        // game manager references
        protected SystemGameManager systemGameManager = null;
        protected SystemConfigurationManager systemConfigurationManager = null;
        protected SystemEventManager systemEventManager = null;

        protected int configureCount = 0;

        public virtual void Configure(SystemGameManager systemGameManager) {
            this.systemGameManager = systemGameManager;
            physicsScene = gameObject.scene.GetPhysicsScene();
            SetGameManagerReferences();
            configureCount++;
        }

        public virtual void SetGameManagerReferences() {
            systemConfigurationManager = systemGameManager.SystemConfigurationManager;
            systemEventManager = systemGameManager.SystemEventManager;
        }

    }

}