using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public abstract class EquipmentModel : ConfiguredClass {

        public virtual void SetupScriptableObjects(IDescribable describable) {
            // nothing in the base class here
        }
    }
}

