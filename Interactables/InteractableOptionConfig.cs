using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [System.Serializable]
    public abstract class InteractableOptionConfig : DescribableResource {

        public virtual InteractableOptionProps InteractableOptionProps { get => null; }

    }

}