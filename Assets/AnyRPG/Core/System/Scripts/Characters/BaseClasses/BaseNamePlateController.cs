using System;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    public class BaseNameplateController : ConfiguredClass {

        public virtual event System.Action OnInitializeNameplate = delegate { };
        public virtual event Action OnNameChange = delegate { };

        protected NameplateController nameplate;

        protected Interactable interactable;

        // game manager references
        protected NameplateManager namePlateManager = null;
        protected UIManager uIManager = null;
        protected PlayerManagerClient playerManagerClient = null;

        public virtual NameplateController Nameplate { get => nameplate; }

        /*
        public virtual string UnitFrameTarget {
            get {
                return interactable.NameplateProps.UnitFrameTarget;
            }
        }
        public virtual string UnitPreviewTarget {
            get {
                return interactable.NameplateProps.UnitPreviewTarget;
            }
        }
        public virtual Vector3 UnitFrameCameraLookOffset {
            get {
                return interactable.NameplateProps.UnitFrameCameraLookOffset;
            }
        }
        public virtual Vector3 UnitFrameCameraPositionOffset {
            get {
                return interactable.NameplateProps.UnitFrameCameraPositionOffset;
            }
        }
        public virtual Vector3 UnitPreviewCameraLookOffset {
            get {
                return interactable.NameplateProps.UnitPreviewCameraLookOffset;
            }
        }
        public virtual Vector3 UnitPreviewCameraPositionOffset {
            get {
                return interactable.NameplateProps.UnitPreviewCameraPositionOffset;
            }
        }
        public virtual bool SuppressFaction {
            get {
                return interactable.NameplateProps.SuppressFaction;
            }
        }
        */

        public virtual bool OverrideNameplatePosition {
            get {
                return interactable.NameplateProps.OverrideNameplatePosition;
            }
        }

        public Vector3 InteractableNameplatePosition {
            get {
                return interactable.NameplateProps.NameplatePosition;
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
                if (interactable.NameplateProps.DisplayName == string.Empty) {
                    return interactable.DisplayName;
                }
                return interactable.NameplateProps.DisplayName;
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

        public BaseNameplateController(Interactable interactable, SystemGameManager systemGameManager) {
            this.interactable = interactable;
            Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            namePlateManager = systemGameManager.UIManager.NameplateManager;
            uIManager = systemGameManager.UIManager;
            playerManagerClient = systemGameManager.PlayerManagerClient;
        }

        public void SetNameplatePosition() {
            //Debug.Log($"{namePlateUnit.gameObject.name}.BaseNameplateController.SetNameplatePosition() override: {OverrideNameplatePosition} position: {NameplatePosition}");

            if (OverrideNameplatePosition) {
                Interactable.InteractableEventController.NotifyOnSetNameplatePosition(InteractableNameplatePosition);
            }
        }

        public virtual bool InitializeNameplate() {
            //Debug.Log("BasenamePlateController.InitializeNameplate()");

            if (networkManagerServer.ServerModeActive == true) {
                return false;
            }
            if (CanSpawnNameplate()) {
                SetNameplatePosition();
                nameplate = AddNameplate();
                SetupNameplate();
                BroadcastInitializeNameplate();
                return true;
            }
            return false;
        }

        public virtual NameplateController AddNameplate() {
            //Debug.Log(namePlateUnit.gameObject.name + ".BasenamePlateController.AddNameplate()");
            return namePlateManager.AddNameplate(interactable, false);
        }

        public virtual void RemoveNameplate() {
            //Debug.Log($"{namePlateUnit.gameObject.name}.BasenamePlateController.RemoveNameplate()");

            namePlateManager.RemoveNameplate(interactable);
        }

        public virtual bool CanSpawnNameplate() {
            //Debug.Log((namePlateUnit == null ? "null" : namePlateUnit.gameObject.name) + ".BasenamePlateController.CanSpawnNameplate()");
            if (interactable == null) {
                return false;
            }
            return true;
        }

        public virtual void SetupNameplate() {
            //Debug.Log(namePlateUnit.gameObject.name + ".BaseNameplateController.SetupNameplate()");
            /*
            if (namePlate == null) {
                Debug.LogWarning(namePlateUnit.gameObject.name + ".BaseNameplateController.SetupNameplate(): namePlate is null");
            }
            if (namePlate.HealthBar == null) {
                Debug.LogWarning(namePlateUnit.gameObject.name + ".BaseNameplateController.SetupNameplate(): namePlate.Healthbar is null");
            }
            */
            nameplate.HealthBar.SetActive(false);
        }

        public virtual void BroadcastInitializeNameplate() {
            OnInitializeNameplate();
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
            //Debug.Log($"BaseNameplateController.HasHealth(): return false");

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