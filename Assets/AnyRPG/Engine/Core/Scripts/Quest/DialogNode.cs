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

        [Tooltip("If this dialog is manually driven, this text will show on the next button.")]
        [SerializeField]
        private string nextOption;

        // whether or not this node has been shown
        private bool shown;

        public string MyDescription { get => description; set => description = value; }
        public string MyNextOption { get => nextOption; set => nextOption = value; }
        public float MyStartTime { get => startTime; set => startTime = value; }
        public bool Shown { get => shown; set => shown = value; }
        public float MyShowTime { get => showTime; set => showTime = value; }

        /// <summary>
        /// Set the shown value to false
        /// </summary>
        public void ResetStatus() {
            shown = false;
        }
    }

}