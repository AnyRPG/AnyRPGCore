using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace AnyRPG {

    [System.Serializable]
    public class BaseNamePlateController : INamePlateController {

        public virtual event System.Action OnInitializeNamePlate = delegate { };
        public virtual event Action<INamePlateUnit> NamePlateNeedsRemoval = delegate { };
        public virtual event Action<int, int> ResourceBarNeedsUpdate = delegate { };
        public virtual event Action OnNameChange = delegate { };

        [Header("NAMEPLATE SETTINGS")]

        [Tooltip("This is what will be printed on the nameplate above the object.  It will also override whatever value is set for the Interactable mouseover display name.")]
        [SerializeField]
        protected string displayName = string.Empty;

        [Tooltip("If true, the nameplate is not shown above this unit.")]
        [SerializeField]
        protected bool suppressNamePlate = false;

        [Tooltip("If true, the faction will not be shown on the nameplate")]
        [SerializeField]
        protected bool suppressFaction = true;

        [Header("UNIT FRAME SETTINGS")]

        [Tooltip("An object or bone in the heirarchy to use as the camera target.")]
        [SerializeField]
        protected string unitFrameTarget = string.Empty;

        [Tooltip("The position the camera is looking at, relative to the target")]
        [SerializeField]
        protected Vector3 unitFrameCameraLookOffset = Vector3.zero;

        [Tooltip("The position of the camera relative to the target")]
        [SerializeField]
        protected Vector3 unitFrameCameraPositionOffset = Vector3.zero;

        protected NamePlateController namePlate;

        protected INamePlateUnit namePlateUnit;

        protected string playerPreviewTarget = string.Empty;

        protected Vector3 unitPreviewCameraLookOffset = new Vector3(0f, 1f, 0f);

        protected Vector3 unitPreviewCameraPositionOffset = new Vector3(0f, 1f, 1f);

        public virtual NamePlateController NamePlate { get => namePlate; set => namePlate = value; }

        public virtual string UnitFrameTarget { get => unitFrameTarget; }
        public virtual string PlayerPreviewTarget { get => playerPreviewTarget; }
        public virtual Vector3 UnitFrameCameraLookOffset { get => unitFrameCameraLookOffset; set => unitFrameCameraLookOffset = value; }
        public virtual Vector3 UnitFrameCameraPositionOffset { get => unitFrameCameraPositionOffset; set => unitFrameCameraPositionOffset = value; }
        public virtual Vector3 UnitPreviewCameraLookOffset { get => unitPreviewCameraLookOffset; set => unitPreviewCameraLookOffset = value; }
        public virtual Vector3 UnitPreviewCameraPositionOffset { get => unitPreviewCameraPositionOffset; set => unitPreviewCameraPositionOffset = value; }
        public virtual bool SuppressFaction { get => suppressFaction; set => suppressFaction = value; }
        public virtual List<PowerResource> PowerResourceList {
            get {
                return new List<PowerResource>();
            }
        }

        public virtual Interactable Interactable {
            get {
                return namePlateUnit.Interactable;
            }
        }
        public virtual string UnitDisplayName {
            get {
                return displayName;
            }
        }
        public virtual Faction Faction {
            get {
                return null;
            }
        }
        public virtual string Title {
            get {
                return string.Empty;
            }
        }

        public virtual Transform NamePlateTransform {
            get {
                if (namePlateUnit.UnitComponentController.NamePlateTransform != null) {
                    return namePlateUnit.UnitComponentController.NamePlateTransform;
                }
                return namePlateUnit.transform;
            }
        }
        public virtual int Level {
            get {
                return 1;
            }
        }

        public INamePlateUnit NamePlateUnit { get => namePlateUnit; set => namePlateUnit = value; }

        public virtual void Setup(INamePlateUnit namePlateUnit) {
            this.namePlateUnit = namePlateUnit;
            InitializeNamePlate();
        }

        public virtual void InitializeNamePlate() {
            //Debug.Log(gameObject.name + ".CharacterUnit.InitializeNamePlate()");
            if (suppressNamePlate == true) {
                return;
            }
            if (namePlateUnit != null) {
                //NamePlateController _namePlate = NamePlateManager.MyInstance.AddNamePlate(namePlateUnit, (unitController.UnitComponentController.NamePlateTransform == null ? true : false));
                NamePlateController _namePlate = NamePlateManager.MyInstance.AddNamePlate(namePlateUnit, false);
                if (_namePlate != null) {
                    namePlate = _namePlate;
                }
                OnInitializeNamePlate();
            }
        }

        public virtual int CurrentHealth() {
            return 1;
        }

        public virtual int MaxHealth() {
            //Debug.Log(gameObject.name + ".CharacterUnit.MaxHealth()");
            return 1;
        }

        public virtual bool HasPrimaryResource() {
            //Debug.Log(gameObject.name + ".CharacterUnit.HasHealth(): return true");
            return false;
        }

        public virtual bool HasSecondaryResource() {
            //Debug.Log(gameObject.name + ".CharacterUnit.HasHealth(): return true");
            return false;
        }

        public virtual bool HasHealth() {
            //Debug.Log(gameObject.name + ".CharacterUnit.HasHealth(): return true");
            return false;
        }

        public virtual void HandleNameChange() {
            OnNameChange();
        }

        public virtual void HandleNamePlateNeedsRemoval(CharacterStats _characterStats) {
            //Debug.Log(gameObject.name + ".CharacterUnit.HandleNamePlateNeedsRemoval()");
            if (namePlateUnit != null && _characterStats != null) {
                NamePlateNeedsRemoval(namePlateUnit);
            }
        }

        public virtual float GetPowerResourceMaxAmount(PowerResource powerResource) {
            return 0f;
        }

        public virtual float GetPowerResourceAmount(PowerResource powerResource) {
            return 0f;
        }




    }


}