using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace AnyRPG {

    [System.Serializable]
    public class UnitNamePlateController : BaseNamePlateController {

        public override event System.Action OnInitializeNamePlate = delegate { };
        public override event Action OnNameChange = delegate { };

        private UnitController unitController = null;

        public override string UnitDisplayName {
            get {
                return (unitController.CharacterUnit.BaseCharacter != null ? unitController.CharacterUnit.BaseCharacter.CharacterName : base.UnitDisplayName);
            }
        }
        public override Faction Faction {
            get {
                if (unitController.CharacterUnit.BaseCharacter != null) {
                    return unitController.CharacterUnit.BaseCharacter.Faction;
                }
                return base.Faction;
            }
        }
        public override string Title {
            get {
                return (unitController.CharacterUnit.BaseCharacter != null ? unitController.CharacterUnit.BaseCharacter.Title : base.Title); }
        }

        public override Transform NamePlateTransform {
            get {
                if (unitController.Mounted == true
                    && unitController.UnitMountManager.MountUnitController != null
                    && unitController.UnitMountManager.MountUnitController.NamePlateController != null
                    && unitController.UnitMountManager.MountUnitController.NamePlateController.NamePlateTransform != null) {
                    return unitController.UnitMountManager.MountUnitController.NamePlateController.NamePlateTransform;
                }
                return base.NamePlateTransform;
            }
        }
        public override int Level {
            get {
                if (unitController.CharacterUnit.BaseCharacter != null && unitController.CharacterUnit.BaseCharacter.CharacterStats != null) {
                    return unitController.CharacterUnit.BaseCharacter.CharacterStats.Level;
                }
                return base.Level;
            }
        }
        public override List<PowerResource> PowerResourceList {
            get {
                if (unitController != null && unitController.CharacterUnit.BaseCharacter != null) {
                    return unitController.CharacterUnit.BaseCharacter.CharacterStats.PowerResourceList;
                }
                return base.PowerResourceList;
            }
        }


        public UnitController UnitController { get => unitController; set => unitController = value; }

        public UnitNamePlateController(NamePlateUnit namePlateUnit) : base(namePlateUnit) {
            if ((namePlateUnit as UnitController) is UnitController) {
                unitController = (namePlateUnit as UnitController);
            }
        }

        /*
        public override void InitializeNamePlate() {
            //Debug.Log(unitController.gameObject.name + ".UnitNamePlateController.InitializeNamePlate()");
            if (SuppressNamePlate == true) {
                //Debug.Log(unitController.gameObject.name + ".UnitNamePlateController.InitializeNamePlate(): namePlate suppressed.  Returning.");
                return;
            }
            if (unitController != null) {
                if (unitController.CharacterUnit.BaseCharacter.CharacterStats != null
                    && unitController.CharacterUnit.BaseCharacter.CharacterStats.IsAlive == false
                    && unitController.CharacterUnit.BaseCharacter.SpawnDead == false) {
                    // if this is not a character that spawns dead, and is currently dead, then there is no reason to display a nameplate as dead characters usually cannot have nameplates
                    return;
                }
                SetNamePlatePosition();
                
                BroadcastInitializeNamePlate();
            } else {
                //Debug.Log(gameObject.name + ".CharacterUnit.InitializeNamePlate(): Character is null or start has not been run yet. exiting.");
                return;
            }
        }
        */

        public override NamePlateController AddNamePlate() {
            return NamePlateManager.MyInstance.AddNamePlate(unitController, (unitController.UnitComponentController.NamePlateTransform == null ? true : false));
        }

        public override void RemoveNamePlate() {
            unitController.OnResourceAmountChanged -= HandleResourceAmountChanged;
            unitController.OnTitleChange -= HandleTitleChange;
            unitController.OnNameChange -= HandleNameChange;
            unitController.OnReputationChange -= HandleReputationChange;

            base.RemoveNamePlate();
        }

        public override void SetupNamePlate() {
            // intentionally not calling base because it disables the healthbar
            if (HasHealth() == true) {
                namePlate.ProcessHealthChanged(MaxHealth(), CurrentHealth());
            }
            unitController.OnResourceAmountChanged += HandleResourceAmountChanged;
            unitController.OnTitleChange += HandleTitleChange;
            unitController.OnNameChange += HandleNameChange;
            unitController.OnReputationChange += HandleReputationChange;
        }

        public void HandleReputationChange() {
            namePlate.HandleReputationChange();
        }

        public void HandleTitleChange(string newTitle) {
            namePlate.HandleTitleChange(newTitle);
        }

        public void HandleNameChange(string newName) {
            namePlate.HandleNameChange(newName);
        }

        public void HandleResourceAmountChanged(PowerResource powerResource, int maxAmount, int currentAmount) {
            namePlate.HandleResourceAmountChanged(powerResource, maxAmount, currentAmount);
        }

        public override bool CanSpawnNamePlate() {
            if (unitController == null) {
                return false;
            }
            if (unitController.CharacterUnit.BaseCharacter.CharacterStats != null
                                && unitController.CharacterUnit.BaseCharacter.CharacterStats.IsAlive == false
                                && unitController.CharacterUnit.BaseCharacter.SpawnDead == false) {
                // if this is not a character that spawns dead, and is currently dead, then there is no reason to display a nameplate as dead characters usually cannot have nameplates
                return false;
            }
            return base.CanSpawnNamePlate();
        }

        public override void BroadcastInitializeNamePlate() {
            OnInitializeNamePlate();

        }

        public override int CurrentHealth() {
            if (unitController.CharacterUnit.BaseCharacter != null && unitController.CharacterUnit.BaseCharacter.CharacterStats != null) {
                return unitController.CharacterUnit.BaseCharacter.CharacterStats.CurrentPrimaryResource;
            }
            return base.CurrentHealth();
        }

        public override int MaxHealth() {
            //Debug.Log(gameObject.name + ".CharacterUnit.MaxHealth()");
            if (unitController.CharacterUnit.BaseCharacter != null && unitController.CharacterUnit.BaseCharacter.CharacterStats != null) {
                return unitController.CharacterUnit.BaseCharacter.CharacterStats.MaxPrimaryResource;
            }
            return base.MaxHealth();
        }

        public override bool HasPrimaryResource() {
            //Debug.Log(gameObject.name + ".CharacterUnit.HasHealth(): return true");
            if (unitController.CharacterUnit.BaseCharacter != null && unitController.CharacterUnit.BaseCharacter.CharacterStats != null) {
                return unitController.CharacterUnit.BaseCharacter.CharacterStats.HasPrimaryResource;
            }
            return base.HasPrimaryResource();
        }

        public override bool HasSecondaryResource() {
            //Debug.Log(gameObject.name + ".CharacterUnit.HasHealth(): return true");
            if (unitController.CharacterUnit.BaseCharacter != null && unitController.CharacterUnit.BaseCharacter.CharacterStats != null) {
                return unitController.CharacterUnit.BaseCharacter.CharacterStats.HasSecondaryResource;
            }
            return base.HasSecondaryResource();
        }

        public override bool HasHealth() {
            //Debug.Log(gameObject.name + ".CharacterUnit.HasHealth(): return true");
            if (unitController == null) {
                Debug.Log("UnitNamePlateController.HasHealth(): unitcontroller is null");
            }
            if (unitController.CharacterUnit == null) {
                Debug.Log(unitController.gameObject.name + ".UnitNamePlateController.HasHealth(): unitcontroller.CharacterUnit is null");
            }
            if (unitController.CharacterUnit.BaseCharacter != null && unitController.CharacterUnit.BaseCharacter.CharacterStats != null) {
                return unitController.CharacterUnit.BaseCharacter.CharacterStats.HasHealthResource;
            }
            return base.HasHealth();
        }

        public override float GetPowerResourceMaxAmount(PowerResource powerResource) {
            if (unitController != null && unitController.CharacterUnit.BaseCharacter != null) {
                return unitController.CharacterUnit.BaseCharacter.CharacterStats.GetPowerResourceMaxAmount(powerResource);
            }
            return base.GetPowerResourceMaxAmount(powerResource);
        }

        public override float GetPowerResourceAmount(PowerResource powerResource) {
            if (unitController != null && unitController.CharacterUnit.BaseCharacter != null) {
                return unitController.CharacterUnit.BaseCharacter.CharacterStats.GetPowerResourceAmount(powerResource);
            }
            return base.GetPowerResourceAmount(powerResource);
        }



    }


}