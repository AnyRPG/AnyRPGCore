using AnyRPG;
using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Animation Profile", menuName = "AnyRPG/Animation Profile")]
    public class AnimationProfile : DescribableResource {

        [SerializeField]
        private AnimationProps animationProps = new AnimationProps();

        public AnimationProps AnimationProps { get => animationProps; set => animationProps = value; }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            //Debug.Log(DisplayName + ".AnimationProfile.SetupScriptableObjects()");
            base.SetupScriptableObjects(systemGameManager);
            animationProps.Configure();
        }
    }

}