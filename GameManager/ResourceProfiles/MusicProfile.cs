using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Music Profile", menuName = "AnyRPG/MusicProfile")]
    [System.Serializable]
    public class MusicProfile : DescribableResource {

        [SerializeField]
        private string artistName;

        [SerializeField]
        private AudioClip audioClip;


        public string MyArtistName { get => artistName; set => artistName = value; }
        public AudioClip MyAudioClip { get => audioClip; set => audioClip = value; }
    }

}