using UnityEngine;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Voice Profile", menuName = "AnyRPG/VoiceProfile")]
    [System.Serializable]
    public class VoiceProfile : DescribableResource {

        [SerializeField]
        private VoiceProps voiceProps = new VoiceProps();

        public VoiceProps VoiceProps { get => voiceProps; set => voiceProps = value; }
    }

}