using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public interface ICapabilityConsumer {

        Faction Faction { get; }
        UnitType UnitType { get; }
        CharacterRace CharacterRace { get; }
        CharacterClass CharacterClass { get; }
        ClassSpecialization ClassSpecialization { get; }

    }

}