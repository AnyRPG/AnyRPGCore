using AnyRPG;
using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Animation Profile", menuName = "AnyRPG/Animation/Profile")]
    public class AnimationProfile : DescribableResource {

        [SerializeField]
        private AnimationProps animationProps = new AnimationProps();

        public AnimationProps AnimationProps { get => animationProps; set => animationProps = value; }
    }

}