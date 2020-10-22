using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace AnyRPG {

    [System.Serializable]
    public class UnitNamePlateController : INamePlateController {

        public event System.Action OnInitializeNamePlate = delegate { };
        public event Action<INamePlateUnit> NamePlateNeedsRemoval = delegate { };
        public event Action<int, int> ResourceBarNeedsUpdate = delegate { };
        public event Action OnNameChange = delegate { };

        [Header("NAMEPLATE SETTINGS")]

        [Tooltip("This is what will be printed on the nameplate above the object.  It will also override whatever value is set for the Interactable mouseover display name.")]
        [SerializeField]
        private string displayName = string.Empty;

        [Tooltip("If true, the nameplate is not shown above this unit.")]
        [SerializeField]
        private bool suppressNamePlate = false;

        [Tooltip("If true, the faction will not be shown on the nameplate")]
        [SerializeField]
        private bool suppressFaction = true;

        [Header("UNIT FRAME SETTINGS")]

        [Tooltip("An object or bone in the heirarchy to use as the camera target.")]
        [SerializeField]
        private string unitFrameTarget = string.Empty;

        [Tooltip("The position the camera is looking at, relative to the target")]
        [SerializeField]
        private Vector3 unitFrameCameraLookOffset = Vector3.zero;

        [Tooltip("The position of the camera relative to the target")]
        [SerializeField]
        private Vector3 unitFrameCameraPositionOffset = Vector3.zero;

        private UnitController unitController = null;

        private NamePlateController namePlate;

        private Interactable interactable;

        private string playerPreviewTarget = string.Empty;

        private Vector3 unitPreviewCameraLookOffset = new Vector3(0f, 1f, 0f);

        private Vector3 unitPreviewCameraPositionOffset = new Vector3(0f, 1f, 1f);

        public NamePlateController NamePlate { get => namePlate; set => namePlate = value; }

        public string UnitFrameTarget { get => unitFrameTarget; }
        public string PlayerPreviewTarget { get => playerPreviewTarget; }
        public Vector3 UnitFrameCameraLookOffset { get => unitFrameCameraLookOffset; set => unitFrameCameraLookOffset = value; }
        public Vector3 UnitFrameCameraPositionOffset { get => unitFrameCameraPositionOffset; set => unitFrameCameraPositionOffset = value; }
        public Vector3 UnitPreviewCameraLookOffset { get => unitPreviewCameraLookOffset; set => unitPreviewCameraLookOffset = value; }
        public Vector3 UnitPreviewCameraPositionOffset { get => unitPreviewCameraPositionOffset; set => unitPreviewCameraPositionOffset = value; }
        public bool SuppressFaction { get => suppressFaction; set => suppressFaction = value; }
        public string UnitDisplayName {
            get {
                return (unitController.BaseCharacter != null ? unitController.BaseCharacter.CharacterName : displayName);
            }
        }
        public Faction Faction {
            get {
                if (unitController.BaseCharacter != null) {
                    return unitController.BaseCharacter.Faction;
                }
                return null;
            }
        }
        public string Title {
            get {
                return (unitController.BaseCharacter != null ? unitController.BaseCharacter.Title : string.Empty); }
        }

        public Transform NamePlateTransform {
            get {
                if (unitController.Mounted) {
                    if (unitController.NamePlateTarget != null) {
                        return unitController.NamePlateTarget.NamePlateTransform;
                    }
                    return unitController.transform;
                }
                if (unitController.UnitComponentController.NamePlateTransform != null) {
                    return unitController.UnitComponentController.NamePlateTransform;
                }
                return unitController.transform;
            }
        }
        public int Level {
            get {
                if (unitController.BaseCharacter != null && unitController.BaseCharacter.CharacterStats != null) {
                    return unitController.BaseCharacter.CharacterStats.Level;
                }
                return 1;
            }
        }

        public Interactable Interactable { get => interactable; set => interactable = value; }
        public UnitController UnitController { get => unitController; set => unitController = value; }

        public void Setup(UnitController unitController) {
            this.unitController = unitController;
            InitializeNamePlate();
        }

        public void InitializeNamePlate() {
            //Debug.Log(gameObject.name + ".CharacterUnit.InitializeNamePlate()");
            if (suppressNamePlate == true) {
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
                }
                OnInitializeNamePlate();
            } else {
                //Debug.Log(gameObject.name + ".CharacterUnit.InitializeNamePlate(): Character is null or start has not been run yet. exiting.");
                return;
            }
        }

        public int CurrentHealth() {
            if (unitController.BaseCharacter != null && unitController.BaseCharacter.CharacterStats != null) {
                return unitController.BaseCharacter.CharacterStats.CurrentPrimaryResource;
            }
            return 1;
        }

        public int MaxHealth() {
            //Debug.Log(gameObject.name + ".CharacterUnit.MaxHealth()");
            if (unitController.BaseCharacter != null && unitController.BaseCharacter.CharacterStats != null) {
                return unitController.BaseCharacter.CharacterStats.MaxPrimaryResource;
            }
            return 1;
        }

        public bool HasPrimaryResource() {
            //Debug.Log(gameObject.name + ".CharacterUnit.HasHealth(): return true");
            if (unitController.BaseCharacter != null && unitController.BaseCharacter.CharacterStats != null) {
                return unitController.BaseCharacter.CharacterStats.HasPrimaryResource;
            }
            return false;
        }

        public bool HasSecondaryResource() {
            //Debug.Log(gameObject.name + ".CharacterUnit.HasHealth(): return true");
            if (unitController.BaseCharacter != null && unitController.BaseCharacter.CharacterStats != null) {
                return unitController.BaseCharacter.CharacterStats.HasSecondaryResource;
            }
            return false;
        }

        public bool HasHealth() {
            //Debug.Log(gameObject.name + ".CharacterUnit.HasHealth(): return true");
            if (unitController.BaseCharacter != null && unitController.BaseCharacter.CharacterStats != null) {
                return unitController.BaseCharacter.CharacterStats.HasHealthResource;
            }
            return false;
        }

        public void HandleNameChange() {
            OnNameChange();
        }

        public void HandleNamePlateNeedsRemoval(CharacterStats _characterStats) {
            //Debug.Log(gameObject.name + ".CharacterUnit.HandleNamePlateNeedsRemoval()");
            if (unitController != null && _characterStats != null) {
                //Debug.Log(gameObject.name + ".CharacterUnit.HandleNamePlateNeedsRemoval(" + _characterStats + ")");
                NamePlateNeedsRemoval(unitController);
            }
            //baseCharacter.MyCharacterStats.OnHealthChanged -= HealthBarNeedsUpdate;
        }


    }


}