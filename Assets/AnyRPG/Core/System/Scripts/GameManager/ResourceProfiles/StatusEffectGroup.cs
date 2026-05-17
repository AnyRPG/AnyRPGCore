using UnityEngine;

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