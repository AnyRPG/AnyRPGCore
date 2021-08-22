using AnyRPG;
using UnityEngine;

namespace AnyRPG {
    public class ConfiguredClass {

        protected SystemGameManager systemGameManager = null;

        public virtual void Configure(SystemGameManager systemGameManager) {
            this.systemGameManager = systemGameManager;
            SetGameManagerReferences();
        }

        public virtual void SetGameManagerReferences() {
            // meant to be overwritten
        }

    }

}