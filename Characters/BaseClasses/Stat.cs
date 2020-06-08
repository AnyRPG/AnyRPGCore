using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class Stat {

        public event System.Action<string> OnModifierUpdate = delegate { };

        private string statName = string.Empty;

        // this value is calculated every level and does not include modifiers
        private int baseValue = 0;

        // this value is calculated every time modifiers are added or removed
        private int currentValue = 0;

        // this value will be added to any calculated values
        private int defaultAddValue = 0;

        private float baseMultiplyValue = 1f;

        private List<float> addModifiers = new List<float>();
        private List<float> multiplyModifiers = new List<float>();

        public int BaseValue { get => baseValue; set => baseValue = value; }
        public int CurrentValue { get => currentValue; set => currentValue = value; }
        public int DefaultAddValue { get => defaultAddValue; set => defaultAddValue = value; }

        public Stat(string statName) {
            this.statName = statName;
        }

        public float GetValue() {
            //Debug.Log("Stat.GetValue()");
            float finalAddValue = defaultAddValue;
            addModifiers.ForEach(x => finalAddValue += x);
            //Debug.Log("Stat.GetValue() finalAddValue: " + finalAddValue);
            float finalMultiplyValue = baseMultiplyValue;
            multiplyModifiers.ForEach(x => finalMultiplyValue *= x);
            //Debug.Log("Stat.GetValue() finalMultiplyValue: " + finalMultiplyValue);
            return (finalAddValue * finalMultiplyValue);
        }

        public float GetAddValue() {
            float finalAddValue = 0;
            addModifiers.ForEach(x => finalAddValue += x);
            //Debug.Log("Stat.GetValue() finalAddValue: " + finalAddValue);
            return finalAddValue;
        }

        public float GetMultiplyValue() {
            //Debug.Log("Stat.GetMultiplyValue()");
            float finalMultiplyValue = baseMultiplyValue;
            multiplyModifiers.ForEach(x => finalMultiplyValue *= x);
            //Debug.Log("Stat.GetValue() finalMultiplyValue: " + finalMultiplyValue);
            return finalMultiplyValue;
        }

        public void AddModifier(float modifier) {
            //Debug.Log("Stat.AddModifier(" + modifier + ")");
            if (modifier != 0) {
                addModifiers.Add(modifier);
                OnModifierUpdate(statName);
            }
        }

        public void AddMultiplyModifier(float modifier) {
            //Debug.Log("Stat.AddMultiplyModifier(" + modifier + ")");
            //if (modifier != 0) {
            multiplyModifiers.Add(modifier);
            OnModifierUpdate(statName);
            //}
        }

        public void RemoveModifier(float modifier) {
            //Debug.Log("Stat.RemoveModifier(" + modifier + ")");
            if (modifier != 0) {
                addModifiers.Remove(modifier);
                OnModifierUpdate(statName);
            }
        }

        public void RemoveMultiplyModifier(float modifier) {
            //Debug.Log("Stat.RemoveMultiplyModifier(" + modifier + ")");
            if (modifier != 0) {
                multiplyModifiers.Remove(modifier);
                OnModifierUpdate(statName);
            }
        }

        public void ClearAddModifiers() {
            addModifiers.Clear();
        }

        public void ClearMultiplyModifiers() {
            multiplyModifiers.Clear();
        }
    }

}