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

        [Header("Audio")]

        [SerializeField]
        private string artistName;

        [Tooltip("List of sound files")]
        [SerializeField]
        private List<AudioClip> audioClips = new List<AudioClip>();

        public string ArtistName { get => artistName; set => artistName = value; }
        public AudioClip AudioClip {
            get {
                if (audioClips.Count > 0) {
                    return audioClips[0];
                }
                return null;
            }
        }
        public AudioClip RandomAudioClip {
            get {
                if (audioClips.Count > 0) {
                    return audioClips[Random.Range(0, audioClips.Count)];
                }
                return null;
            }
        }

        public List<AudioClip> AudioClips { get => audioClips; set => audioClips = value; }

        public void PreloadAudioClips() {
            foreach (AudioClip audioClip in audioClips) {
                audioClip.LoadAudioData();
            }
        }
    }

}