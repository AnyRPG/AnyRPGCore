using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class Skeleton : AICharacter {
        protected override void Start() {
            base.Start();
            if (characterName == null) {
                characterName = "Skeleton";
            }
        }


    }

}