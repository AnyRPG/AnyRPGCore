using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Audio Profile", menuName = "AnyRPG/AudioProfile")]
    [System.Serializable]
    public class AudioProfile : DescribableResource {

        [SerializeField]
        private string artistName;

        [SerializeField]
        private AudioClip audioClip;

        [SerializeField]
        private List<AudioClip> audioClips = new List<AudioClip>();

        public string MyArtistName { get => artistName; set => artistName = value; }
        public AudioClip MyAudioClip {
            get {
                if (audioClip !=null) {
                    return audioClip;
                }
                if (audioClips.Count > 0) {
                    return audioClips[0];
                }
                return null;
            }
            set => audioClip = value;
        }

        public List<AudioClip> MyAudioClips { get => audioClips; }
    }

}