using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class DialogNode {

        // time in seconds to show this text
        [SerializeField]
        private float startTime;

        // the length of time the bubble should be shown for
        [SerializeField]
        private float showTime = 10f;

        [SerializeField]
        [TextArea(10, 20)]
        private string description;

        [SerializeField]
        private string nextOption;

        // whether or not this node has been shown
        private bool shown;

        public string MyDescription { get => description; set => description = value; }
        public string MyNextOption { get => nextOption; set => nextOption = value; }
        public float MyStartTime { get => startTime; set => startTime = value; }
        public bool MyShown { get => shown; set => shown = value; }
        public float MyShowTime { get => showTime; set => showTime = value; }
    }

}