using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace AnyRPG {

    public class BaseNamePlateController {

        public virtual event System.Action OnInitializeNamePlate = delegate { };
        public virtual event Action<Interactable> NamePlateNeedsRemoval = delegate { };
        public virtual event Action<int, int> ResourceBarNeedsUpdate = delegate { };
        public virtual event Action OnNameChange = delegate { };

        protected NamePlateController namePlate;

        protected Interactable interactable;

        public virtual NamePlateController NamePlate { get => namePlate; set => namePlate = value; }

        public virtual string UnitFrameTarget {
            get {
                return interactable.NamePlateProps.UnitFrameTarget;
            }
        }
        public virtual string UnitPreviewTarget {
            get {
                return interactable.NamePlateProps.UnitPreviewTarget;
            }
        }
        public virtual Vector3 UnitFrameCameraLookOffset {
            get {
                return interactable.NamePlateProps.UnitFrameCameraLookOffset;
            }
        }
        public virtual Vector3 UnitFrameCameraPositionOffset {
            get {
                return interactable.NamePlateProps.UnitFrameCameraPositionOffset;
            }
        }
        public virtual Vector3 UnitPreviewCameraLookOffset {
            get {
                return interactable.NamePlateProps.UnitPreviewCameraLookOffset;
            }
        }
        public virtual Vector3 UnitPreviewCameraPositionOffset {
            get {
                return interactable.NamePlateProps.UnitPreviewCameraPositionOffset;
            }
        }
        public virtual bool SuppressFaction {
            get {
                return interactable.NamePlateProps.SuppressFaction;
            }
        }

        public virtual bool SuppressNamePlate {
            get {
                return interactable.NamePlateProps.SuppressNamePlate;
            }
        }

        public virtual bool OverrideNamePlatePosition {
            get {
                return interactable.NamePlateProps.OverrideNameplatePosition;
            }
        }

        public Vector3 NamePlatePosition {
            get {
                return interactable.NamePlateProps.NameplatePosition;
            }
        }

        public virtual List<PowerResource> PowerResourceList {
            get {
                return new List<PowerResource>();
            }
        }

        public virtual Interactable Interactable {
            get {
                return interactable;
            }
        }
        public virtual string UnitDisplayName {
            get {
                return interactable.NamePlateProps.DisplayName;
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
                if (interactable.UnitComponentController.NamePlateTransform != null) {
                    return interactable.UnitComponentController.NamePlateTransform;
                }
                return interactable.transform;
            }
        }
        public virtual int Level {
            get {
                return 1;
            }
        }

        public Interactable NamePlateUnit { get => interactable; set => interactable = value; }

        public BaseNamePlateController(Interactable interactable) {
            this.interactable = interactable;
        }

        public virtual void Init() {
            Debug.Log(interactable.gameObject.name + "BasenamePlateController.Init()");
            InitializeNamePlate();
        }

        public virtual void Cleanup() {
            if (interactable != null) {
                NamePlateNeedsRemoval(interactable);
            }
        }

        public virtual void InitializeNamePlate() {
            Debug.Log(interactable.gameObject.name + "BasenamePlateController.InitializeNamePlate()");
            if (interactable.NamePlateProps.SuppressNamePlate == true) {
                return;
            }
            if (interactable != null) {
                //NamePlateController _namePlate = NamePlateManager.MyInstance.AddNamePlate(namePlateUnit, (unitController.UnitComponentController.NamePlateTransform == null ? true : false));
                if (OverrideNamePlatePosition) {
                    interactable.UnitComponentController.NamePlateTransform.localPosition = NamePlatePosition;
                }
                NamePlateController _namePlate = NamePlateManager.MyInstance.AddNamePlate(interactable, false);
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
            //if (namePlateUnit != null && _characterStats != null) {
            if (interactable != null) {
                NamePlateNeedsRemoval(interactable);
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