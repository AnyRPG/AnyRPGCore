using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class DialogNode {

        [Tooltip("The number of seconds to wait after the dialog starts playing before showing this text.")]
        [SerializeField]
        private float startTime;

        [Tooltip("The length of time the bubble should be shown for")]
        [SerializeField]
        private float showTime = 10f;

        [Tooltip("The dialog text to display")]
        [SerializeField]
        [TextArea(10, 20)]
        private string description;

        [Tooltip("Audio file to play when this dialog option is displayed or played")]
        [SerializeField]
        private AudioClip audioClip;

        [Tooltip("If this dialog is manually driven, this text will show on the next button.")]
        [SerializeField]
        private string nextOption;

        // whether or not this node has been shown
        private bool shown;

        public string Description { get => description; set => description = value; }
        public string NextOption { get => nextOption; set => nextOption = value; }
        public float StartTime { get => startTime; set => startTime = value; }
        public bool Shown { get => shown; set => shown = value; }
        public float ShowTime { get => showTime; set => showTime = value; }
        public AudioClip AudioClip { get => audioClip; set => audioClip = value; }

        /// <summary>
        /// Set the shown value to false
        /// </summary>
        public void ResetStatus() {
            shown = false;
        }
    }

}