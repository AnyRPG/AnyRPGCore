using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
// base class to hold amounts and spellpower calculations for heal and damage effects
public abstract class AmountEffect : InstantEffect {

    public int healthMinAmount = 0;
    public int healthBaseAmount = 0;
    public int healthMaxAmount = 0;
    public int manaMinAmount = 0;
    public int manaBaseAmount = 0;
    public int manaMaxAmount = 0;

    protected float CalculateAbilityAmount(int abilityBaseAmount, BaseCharacter source, CharacterUnit target) {
        float amountModifier = source.MyCharacterStats.MySpellPower;
        return (abilityBaseAmount == 0 ? abilityBaseAmount : (abilityBaseAmount + amountModifier));
    }
}
}