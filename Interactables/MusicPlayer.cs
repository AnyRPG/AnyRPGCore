using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class MusicPlayer : InteractableOption {

        [SerializeField]
        private MusicPlayerProps musicPlayerProps = new MusicPlayerProps();

        [SerializeField]
        private List<string> musicProfileNames = new List<string>();

        public override InteractableOptionProps InteractableOptionProps { get => musicPlayerProps; }
    }

}