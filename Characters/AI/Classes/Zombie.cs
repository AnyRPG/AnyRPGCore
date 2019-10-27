using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class Zombie : AICharacter {

        protected override void Start() {
            base.Start();
            if (characterName == null) {
                characterName = "Zombie";
            }
        }


    }

}