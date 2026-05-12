using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace AnyRPG {

    public class BaseNamePlateController : ConfiguredClass {

        public virtual event System.Action OnInitializeNamePlate = delegate { };
        public virtual event Action OnNameChange = delegate { };

        protected NamePlateController namePlate;

        protected Interactable interactable;

        // game manager references
        protected NamePlateManager namePlateManager = null;
        protected UIManager uIManager = null;
        protected PlayerManagerClient playerManagerClient = null;

        public virtual NamePlateController NamePlate { get => namePlate; }

        /*
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
        */

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
                if (interactable.NamePlateProps.DisplayName == string.Empty) {
                    return interactable.DisplayName;
                }
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

        public virtual Vector3 NameplatePosition {
            get {
                return interactable.GetNameplatePosition();
            }
        }
        public virtual int Level {
            get {
                return 1;
            }
        }

        public BaseNamePlateController(Interactable interactable, SystemGameManager systemGameManager) {
            this.interactable = interactable;
            Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            namePlateManager = systemGameManager.UIManager.NamePlateManager;
            uIManager = systemGameManager.UIManager;
            playerManagerClient = systemGameManager.PlayerManagerClient;
        }

        public void SetNameplatePosition() {
            //Debug.Log($"{namePlateUnit.gameObject.name}.BaseNamePlateController.SetNameplatePosition() override: {OverrideNamePlatePosition} position: {NamePlatePosition}");

            if (OverrideNamePlatePosition) {
                Interactable.InteractableEventController.NotifyOnSetNameplatePosition(NamePlatePosition);
            }
        }

        public virtual bool InitializeNamePlate() {
            //Debug.Log(namePlateUnit.gameObject.name + ".BasenamePlateController.InitializeNamePlate()");
            /*
            if (SuppressNamePlate == true) {
                //Debug.Log(namePlateUnit.gameObject.name + ".BasenamePlateController.InitializeNamePlate(): suppressing NamePlate");
                return false;
            }
            */
            if (networkManagerServer.ServerModeActive == true) {
                return false;
            }
            if (CanSpawnNamePlate()) {
                SetNameplatePosition();
                namePlate = AddNamePlate();
                SetupNamePlate();
                BroadcastInitializeNamePlate();
                return true;
            }
            return false;
        }

        public virtual NamePlateController AddNamePlate() {
            //Debug.Log(namePlateUnit.gameObject.name + ".BasenamePlateController.AddNamePlate()");
            return namePlateManager.AddNamePlate(interactable, false);
        }

        public virtual void RemoveNamePlate() {
            //Debug.Log($"{namePlateUnit.gameObject.name}.BasenamePlateController.RemoveNamePlate()");

            namePlateManager.RemoveNamePlate(interactable);
        }

        public virtual bool CanSpawnNamePlate() {
            //Debug.Log((namePlateUnit == null ? "null" : namePlateUnit.gameObject.name) + ".BasenamePlateController.CanSpawnNamePlate()");
            if (interactable == null) {
                return false;
            }
            return true;
        }

        public virtual void SetupNamePlate() {
            //Debug.Log(namePlateUnit.gameObject.name + ".BaseNamePlateController.SetupNamePlate()");
            /*
            if (namePlate == null) {
                Debug.LogWarning(namePlateUnit.gameObject.name + ".BaseNamePlateController.SetupNamePlate(): namePlate is null");
            }
            if (namePlate.HealthBar == null) {
                Debug.LogWarning(namePlateUnit.gameObject.name + ".BaseNamePlateController.SetupNamePlate(): namePlate.Healthbar is null");
            }
            */
            namePlate.HealthBar.SetActive(false);
        }

        public virtual void BroadcastInitializeNamePlate() {
            OnInitializeNamePlate();
            interactable.ProcessStatusIndicatorSourceInit();
        }

        public virtual int CurrentHealth() {
            return 1;
        }

        public virtual int MaxHealth() {
            //Debug.Log($"{gameObject.name}.CharacterUnit.MaxHealth()");
            return 1;
        }

        public virtual bool HasPrimaryResource() {
            //Debug.Log($"{gameObject.name}.CharacterUnit.HasHealth(): return true");
            return false;
        }

        public virtual bool HasSecondaryResource() {
            //Debug.Log($"{gameObject.name}.CharacterUnit.HasHealth(): return true");
            return false;
        }

        public virtual bool HasHealth() {
            //Debug.Log($"BaseNamePlateController.HasHealth(): return false");

            return false;
        }

        public virtual float GetPowerResourceMaxAmount(PowerResource powerResource) {
            return 0f;
        }

        public virtual float GetPowerResourceAmount(PowerResource powerResource) {
            return 0f;
        }

        public virtual string GetNameplateString() {

            return $"<color=white>{UnitDisplayName}</color>";
        }
    }


}