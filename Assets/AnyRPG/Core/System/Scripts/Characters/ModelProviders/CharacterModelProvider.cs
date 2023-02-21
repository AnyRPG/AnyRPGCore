using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    public abstract class CharacterModelProvider : ConfiguredClass {

        public abstract ModelAppearanceController GetAppearanceController(UnitController unitController, UnitModelController unitModelController, SystemGameManager systemGameManager);

        [Tooltip("The character editor panel to use in the new game window")]
        [SerializeField]
        private GameObject appearancePanel = null;

        public GameObject AppearancePanel { get => appearancePanel; }
    }

}

