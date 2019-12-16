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


        public string MyArtistName { get => artistName; set => artistName = value; }
        public AudioClip MyAudioClip { get => audioClip; set => audioClip = value; }
    }

}