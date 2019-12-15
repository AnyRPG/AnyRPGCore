using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class DialogNode {

        [SerializeField]
        [TextArea(10, 20)]
        private string description;

        [SerializeField]
        private string nextOption;

        public string MyDescription { get => description; set => description = value; }
        public string MyNextOption { get => nextOption; set => nextOption = value; }
    }

}