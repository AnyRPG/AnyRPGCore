using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Status Effect Group", menuName = "AnyRPG/StatusEffectGroup")]
    [System.Serializable]
    public class StatusEffectGroup : DescribableResource {

        [Header("Status Effect Group")]

        [Tooltip("Last = the last effect of this type will overwrite the previous effect.  First = no other effects of this type can overwrite the first one")]
        [SerializeField]
        private StatusEffectGroupOption exclusiveOption = StatusEffectGroupOption.Last;

        public StatusEffectGroupOption ExclusiveOption { get => exclusiveOption; }
    }

    public enum StatusEffectGroupOption { Last, First }

}