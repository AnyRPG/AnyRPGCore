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
        public override event Action<INamePlateUnit> NamePlateNeedsRemoval = delegate { };
        public override event Action<int, int> ResourceBarNeedsUpdate = delegate { };
        public override event Action OnNameChange = delegate { };

        private UnitController unitController = null;

        public override string UnitDisplayName {
            get {
                return (unitController.BaseCharacter != null ? unitController.BaseCharacter.CharacterName : base.UnitDisplayName);
            }
        }
        public override Faction Faction {
            get {
                if (unitController.BaseCharacter != null) {
                    return unitController.BaseCharacter.Faction;
                }
                return base.Faction;
            }
        }
        public override string Title {
            get {
                return (unitController.BaseCharacter != null ? unitController.BaseCharacter.Title : base.Title); }
        }

        public override Transform NamePlateTransform {
            get {
                if (unitController.Mounted) {
                    if (unitController.NamePlateTarget != null) {
                        return unitController.NamePlateTarget.NamePlateTransform;
                    }
                    return unitController.transform;
                }
                return base.NamePlateTransform;
            }
        }
        public override int Level {
            get {
                if (unitController.BaseCharacter != null && unitController.BaseCharacter.CharacterStats != null) {
                    return unitController.BaseCharacter.CharacterStats.Level;
                }
                return base.Level;
            }
        }
        public override List<PowerResource> PowerResourceList {
            get {
                if (unitController != null && unitController.BaseCharacter != null) {
                    return unitController.BaseCharacter.CharacterStats.PowerResourceList;
                }
                return base.PowerResourceList;
            }
        }


        public UnitController UnitController { get => unitController; set => unitController = value; }

        public UnitNamePlateController(INamePlateUnit namePlateUnit) : base(namePlateUnit) {

        }

        public override void Init() {
            Debug.Log("UnitNamePlateController.Init()");
            if ((namePlateUnit as UnitController) is UnitController) {
                unitController = (namePlateUnit as UnitController);
            }
            base.Init();
        }

        public override void InitializeNamePlate() {
            //Debug.Log(gameObject.name + ".CharacterUnit.InitializeNamePlate()");
            if (SuppressNamePlate == true) {
                return;
            }
            if (unitController != null) {
                if (unitController.BaseCharacter.CharacterStats != null && unitController.BaseCharacter.CharacterStats.IsAlive == false && unitController.BaseCharacter.MySpawnDead == false) {
                    // if this is not a character that spawns dead, and is currently dead, then there is no reason to display a nameplate as dead characters usually cannot have nameplates
                    return;
                }
                NamePlateController _namePlate = NamePlateManager.MyInstance.AddNamePlate(unitController, (unitController.UnitComponentController.NamePlateTransform == null ? true : false));
                if (_namePlate != null) {
                    namePlate = _namePlate;
                    if (OverrideNamePlatePosition) {
                        _namePlate.transform.localPosition = NamePlatePosition;
                    }
                }
                OnInitializeNamePlate();
            } else {
                //Debug.Log(gameObject.name + ".CharacterUnit.InitializeNamePlate(): Character is null or start has not been run yet. exiting.");
                return;
            }
        }

        public override int CurrentHealth() {
            if (unitController.BaseCharacter != null && unitController.BaseCharacter.CharacterStats != null) {
                return unitController.BaseCharacter.CharacterStats.CurrentPrimaryResource;
            }
            return base.CurrentHealth();
        }

        public override int MaxHealth() {
            //Debug.Log(gameObject.name + ".CharacterUnit.MaxHealth()");
            if (unitController.BaseCharacter != null && unitController.BaseCharacter.CharacterStats != null) {
                return unitController.BaseCharacter.CharacterStats.MaxPrimaryResource;
            }
            return base.MaxHealth();
        }

        public override bool HasPrimaryResource() {
            //Debug.Log(gameObject.name + ".CharacterUnit.HasHealth(): return true");
            if (unitController.BaseCharacter != null && unitController.BaseCharacter.CharacterStats != null) {
                return unitController.BaseCharacter.CharacterStats.HasPrimaryResource;
            }
            return base.HasPrimaryResource();
        }

        public override bool HasSecondaryResource() {
            //Debug.Log(gameObject.name + ".CharacterUnit.HasHealth(): return true");
            if (unitController.BaseCharacter != null && unitController.BaseCharacter.CharacterStats != null) {
                return unitController.BaseCharacter.CharacterStats.HasSecondaryResource;
            }
            return base.HasSecondaryResource();
        }

        public override bool HasHealth() {
            //Debug.Log(gameObject.name + ".CharacterUnit.HasHealth(): return true");
            if (unitController.BaseCharacter != null && unitController.BaseCharacter.CharacterStats != null) {
                return unitController.BaseCharacter.CharacterStats.HasHealthResource;
            }
            return base.HasHealth();
        }


        public override void HandleNamePlateNeedsRemoval(CharacterStats _characterStats) {
            //Debug.Log(gameObject.name + ".CharacterUnit.HandleNamePlateNeedsRemoval()");
            if (unitController != null && _characterStats != null) {
                //Debug.Log(gameObject.name + ".CharacterUnit.HandleNamePlateNeedsRemoval(" + _characterStats + ")");
                NamePlateNeedsRemoval(unitController);
            }
            //baseCharacter.MyCharacterStats.OnHealthChanged -= HealthBarNeedsUpdate;
        }

        public override void HandleNameChange() {
            OnNameChange();
        }

        public override float GetPowerResourceMaxAmount(PowerResource powerResource) {
            if (unitController != null && unitController.BaseCharacter != null) {
                return unitController.BaseCharacter.CharacterStats.GetPowerResourceMaxAmount(powerResource);
            }
            return base.GetPowerResourceMaxAmount(powerResource);
        }

        public override float GetPowerResourceAmount(PowerResource powerResource) {
            if (unitController != null && unitController.BaseCharacter != null) {
                return unitController.BaseCharacter.CharacterStats.GetPowerResourceAmount(powerResource);
            }
            return base.GetPowerResourceAmount(powerResource);
        }



    }


}