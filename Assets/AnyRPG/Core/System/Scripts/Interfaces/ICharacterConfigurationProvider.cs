using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AnyRPG {
    public interface ICharacterConfigurationProvider {
        public CharacterConfigurationRequest GetCharacterConfigurationRequest();
    }
}

