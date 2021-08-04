using AnyRPG;
using UnityEngine;

namespace AnyRPG {
    public class ConfiguredMonoBehaviour : MonoBehaviour {

        protected SystemGameManager systemGameManager = null;

        public virtual void Init(SystemGameManager systemGameManager) {
            this.systemGameManager = systemGameManager;
        }

    }

}