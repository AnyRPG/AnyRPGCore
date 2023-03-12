using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public interface ICharacterEditor : ICapabilityConsumer {

        void SetUnitProfile(UnitProfile unitProfile);
    }

}