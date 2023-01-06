using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class UnitSpawnManager : InteractableOptionManager {

        private UnitSpawnControllerProps unitSpawnControllerProps = null;

        public UnitSpawnControllerProps UnitSpawnControllerProps { get => unitSpawnControllerProps; set => unitSpawnControllerProps = value; }


        public void SetProps(UnitSpawnControllerProps unitSpawnControllerProps, InteractableOptionComponent interactableOptionComponent) {
            //Debug.Log("UnitSpawnManager.SetProps()");

            this.unitSpawnControllerProps = unitSpawnControllerProps;
            BeginInteraction(interactableOptionComponent);
        }

        public void SpawnUnit(int unitLevel, int extraLevels, bool useDynamicLevel, UnitProfile unitProfile, UnitToughness unitToughness) {
            foreach (UnitSpawnNode unitSpawnNode in unitSpawnControllerProps.UnitSpawnNodeList) {
                if (unitSpawnNode != null) {
                    unitSpawnNode.ManualSpawn(unitLevel, extraLevels, useDynamicLevel, unitProfile, unitToughness);
                }
            }
            ConfirmAction();
        }

        public override void EndInteraction() {
            base.EndInteraction();

            unitSpawnControllerProps = null;
        }


    }

}