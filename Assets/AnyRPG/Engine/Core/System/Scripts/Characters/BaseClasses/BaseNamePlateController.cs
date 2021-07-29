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
        public virtual event Action OnNameChange = delegate { };

        protected NamePlateController namePlate;

        protected NamePlateUnit namePlateUnit;

        public virtual NamePlateController NamePlate { get => namePlate; }

        public virtual string UnitFrameTarget {
            get {
                return namePlateUnit.NamePlateProps.UnitFrameTarget;
            }
        }
        public virtual string UnitPreviewTarget {
            get {
                return namePlateUnit.NamePlateProps.UnitPreviewTarget;
            }
        }
        public virtual Vector3 UnitFrameCameraLookOffset {
            get {
                return namePlateUnit.NamePlateProps.UnitFrameCameraLookOffset;
            }
        }
        public virtual Vector3 UnitFrameCameraPositionOffset {
            get {
                return namePlateUnit.NamePlateProps.UnitFrameCameraPositionOffset;
            }
        }
        public virtual Vector3 UnitPreviewCameraLookOffset {
            get {
                return namePlateUnit.NamePlateProps.UnitPreviewCameraLookOffset;
            }
        }
        public virtual Vector3 UnitPreviewCameraPositionOffset {
            get {
                return namePlateUnit.NamePlateProps.UnitPreviewCameraPositionOffset;
            }
        }
        public virtual bool SuppressFaction {
            get {
                return namePlateUnit.NamePlateProps.SuppressFaction;
            }
        }

        public virtual bool SuppressNamePlate {
            get {
                return namePlateUnit.NamePlateProps.SuppressNamePlate;
            }
        }

        public virtual bool OverrideNamePlatePosition {
            get {
                return namePlateUnit.NamePlateProps.OverrideNameplatePosition;
            }
        }

        public Vector3 NamePlatePosition {
            get {
                return namePlateUnit.NamePlateProps.NameplatePosition;
            }
        }

        public virtual List<PowerResource> PowerResourceList {
            get {
                return new List<PowerResource>();
            }
        }

        public virtual Interactable Interactable {
            get {
                return namePlateUnit;
            }
        }
        public virtual string UnitDisplayName {
            get {
                return namePlateUnit.NamePlateProps.DisplayName;
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

        public NamePlateUnit NamePlateUnit { get => namePlateUnit; set => namePlateUnit = value; }

        public BaseNamePlateController(NamePlateUnit namePlateUnit) {
            this.namePlateUnit = namePlateUnit;
        }

        public void SetNamePlatePosition() {
            if (OverrideNamePlatePosition) {
                //_namePlate.transform.localPosition = NamePlatePosition;
                namePlateUnit.UnitComponentController.NamePlateTransform.localPosition = NamePlatePosition;
            }
        }

        public virtual void InitializeNamePlate() {
            //Debug.Log(namePlateUnit.gameObject.name + "BasenamePlateController.InitializeNamePlate()");
            if (SuppressNamePlate == true) {
                return;
            }
            if (CanSpawnNamePlate()) {
                //NamePlateController _namePlate = SystemGameManager.Instance.UIManager.NamePlateManager.AddNamePlate(namePlateUnit, (unitController.UnitComponentController.NamePlateTransform == null ? true : false));
                SetNamePlatePosition();
                namePlate = AddNamePlate();
                SetupNamePlate();
                BroadcastInitializeNamePlate();
            }
        }

        public virtual NamePlateController AddNamePlate() {
            return SystemGameManager.Instance.UIManager.NamePlateManager.AddNamePlate(namePlateUnit, false);
        }

        public virtual void RemoveNamePlate() {
            if (SystemGameManager.Instance.UIManager.NamePlateManager != null) {
                SystemGameManager.Instance.UIManager.NamePlateManager.RemoveNamePlate(namePlateUnit);
            }
        }

        public virtual bool CanSpawnNamePlate() {
            if (namePlateUnit == null) {
                return false;
            }
            return true;
        }

        public virtual void SetupNamePlate() {
            namePlate.HealthBar.SetActive(false);
        }

        public virtual void BroadcastInitializeNamePlate() {
            OnInitializeNamePlate();
            NamePlateUnit.ProcessStatusIndicatorSourceInit();
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

        public virtual float GetPowerResourceMaxAmount(PowerResource powerResource) {
            return 0f;
        }

        public virtual float GetPowerResourceAmount(PowerResource powerResource) {
            return 0f;
        }




    }


}