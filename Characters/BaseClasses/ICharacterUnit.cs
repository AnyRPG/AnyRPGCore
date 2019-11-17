using AnyRPG;
using UnityEngine;
using UnityEngine.AI;

namespace AnyRPG {
    public interface ICharacterUnit {
        BaseCharacter MyCharacter { get; set; }
        Interactable MyInteractable { get; set; }
        NamePlateController MyNamePlate { get; set; }

        void InitializeNamePlate();
        string MyDisplayName { get; }
    }
}