using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public interface ICapabilityConsumer {

        UnitProfile UnitProfile { get; }
        UnitType UnitType { get; }
        CharacterRace CharacterRace { get; }
        CharacterClass CharacterClass { get; }
        ClassSpecialization ClassSpecialization { get; }
        Faction Faction { get; }

        CapabilityConsumerProcessor CapabilityConsumerProcessor { get; }
    }

}