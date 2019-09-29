using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIAbilityManager : CharacterAbilityManager
{
    protected override void Awake() {
        base.Awake();
        baseCharacter = GetComponent<AICharacter>() as ICharacter;
    }

}
