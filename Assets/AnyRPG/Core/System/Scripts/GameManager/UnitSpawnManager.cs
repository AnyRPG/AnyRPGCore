using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class UnitSpawnManager : ConfiguredMonoBehaviour {

        public event System.Action OnConfirmAction = delegate { };
        public event System.Action OnEndInteraction = delegate { };

        private UnitSpawnControllerProps unitSpawnControllerProps = null;

        public UnitSpawnControllerProps UnitSpawnControllerProps { get => unitSpawnControllerProps; set => unitSpawnControllerProps = value; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
        }

        public void SpawnUnit(int unitLevel, int extraLevels, bool useDynamicLevel, UnitProfile unitProfile, UnitToughness unitToughness) {
            foreach (UnitSpawnNode unitSpawnNode in unitSpawnControllerProps.UnitSpawnNodeList) {
                if (unitSpawnNode != null) {
                    unitSpawnNode.ManualSpawn(unitLevel, extraLevels, useDynamicLevel, unitProfile, unitToughness);
                }
            }
            OnConfirmAction();
        }

        public void EndInteraction() {
            OnEndInteraction();
        }

        public void SetProps(UnitSpawnControllerProps unitSpawnControllerProps) {
            //Debug.Log("UnitSpawnManager.SetProps()");
            this.unitSpawnControllerProps = unitSpawnControllerProps;
        }


    }

}