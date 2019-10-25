using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
[System.Serializable]
public class Stat {

    public event System.Action OnModifierUpdate = delegate { };

    [SerializeField]
    private int baseValue = 0;
    private float baseMultiplyValue = 1f;

    private List<int> addModifiers = new List<int>();
    private List<float> multiplyModifiers = new List<float>();

    public int GetValue () {
        //Debug.Log("Stat.GetValue()");
        int finalAddValue = baseValue;
        addModifiers.ForEach(x => finalAddValue += x);
        //Debug.Log("Stat.GetValue() finalAddValue: " + finalAddValue);
        float finalMultiplyValue = baseMultiplyValue;
        multiplyModifiers.ForEach(x => finalMultiplyValue *= x);
        //Debug.Log("Stat.GetValue() finalMultiplyValue: " + finalMultiplyValue);
        return (int)(finalAddValue * finalMultiplyValue);
    }

    public float GetMultiplyValue() {
        //Debug.Log("Stat.GetMultiplyValue()");
        float finalMultiplyValue = baseMultiplyValue;
        multiplyModifiers.ForEach(x => finalMultiplyValue *= x);
        //Debug.Log("Stat.GetValue() finalMultiplyValue: " + finalMultiplyValue);
        return finalMultiplyValue;
    }

    public void AddModifier (int modifier) {
        //Debug.Log("Stat.AddModifier(" + modifier + ")");
        if (modifier != 0) {
            addModifiers.Add(modifier);
            OnModifierUpdate();
        }
    }

    public void AddMultiplyModifier(float modifier) {
        //Debug.Log("Stat.AddMultiplyModifier(" + modifier + ")");
        //if (modifier != 0) {
            multiplyModifiers.Add(modifier);
            OnModifierUpdate();
        //}
    }

    public void RemoveModifier (int modifier) {
        //Debug.Log("Stat.RemoveModifier(" + modifier + ")");
        if (modifier != 0) {
            addModifiers.Remove(modifier);
            OnModifierUpdate();
        }
    }

    public void RemoveMultiplyModifier(float modifier) {
        //Debug.Log("Stat.RemoveMultiplyModifier(" + modifier + ")");
        if (modifier != 0) {
            multiplyModifiers.Remove(modifier);
            OnModifierUpdate();
        }
    }
}

}