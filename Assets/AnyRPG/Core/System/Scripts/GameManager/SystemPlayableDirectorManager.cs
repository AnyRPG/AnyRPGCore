using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace AnyRPG {
    public class SystemPlayableDirectorManager : ConfiguredMonoBehaviour {

        // key is the timeline name as a string
        private Dictionary<string, PlayableDirector> playableDirectorDictionary = new Dictionary<string, PlayableDirector>();

        public Dictionary<string, PlayableDirector> PlayableDirectorDictionary { get => playableDirectorDictionary; set => playableDirectorDictionary = value; }

    }

}