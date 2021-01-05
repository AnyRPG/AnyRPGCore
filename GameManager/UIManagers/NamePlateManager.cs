using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class NamePlateManager : MonoBehaviour {

        #region Singleton
        private static NamePlateManager instance;

        public static NamePlateManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<NamePlateManager>();
                }

                return instance;
            }
        }
        #endregion

        [SerializeField]
        private NamePlateController namePlatePrefab = null;

        [SerializeField]
        private Transform namePlateContainer = null;

        /// <summary>
        /// The currently focused nameplate so we can highlight the outline
        /// </summary>
        private NamePlateUnit focus;

        private Dictionary<NamePlateUnit, NamePlateController> namePlates = new Dictionary<NamePlateUnit, NamePlateController>();

        private void Awake() {
            //Debug.Log("NamePlateManager.Awake(): " + NamePlateManager.MyInstance.gameObject.name);
            string wakeupString = NamePlateManager.MyInstance.gameObject.name;
            SystemEventManager.StartListening("AfterCameraUpdate", HandleAfterCameraUpdate);
        }

        private void Start() {
            //Debug.Log(gameObject.name + ".NamePlateManager.Start()");
        }

        public void HandleAfterCameraUpdate(string eventName, EventParamProperties eventParamProperties) {
            UpdateNamePlates();
        }

        public void LateUpdate() {
            if (SystemConfigurationManager.MyInstance.MyUseThirdPartyCameraControl == true && CameraManager.MyInstance.ThirdPartyCamera.activeInHierarchy == true) {
                UpdateNamePlates();
            }
        }

        private void UpdateNamePlates() {
            foreach (NamePlateController namePlateController in namePlates.Values) {
                namePlateController.UpdatePosition();
            }
        }

        public void SetFocus(NamePlateUnit newInteractable) {
            ClearFocus();
            //Debug.Log("NamePlateManager.SetFocus(" + characterUnit.MyCharacter.MyCharacterName + ")");
            if (namePlates.ContainsKey(newInteractable)) {
                focus = newInteractable;
                // enemy could be dead so we need to check if they exist in the nameplates dictionary
                namePlates[newInteractable].Highlight();
            }
        }

        public void ClearFocus() {
            //Debug.Log("NamePlateManager.ClearFocus()");
            if (focus != null) {
                if (namePlates.ContainsKey(focus)) {
                    // enemy could be dead so we need to check if they exist in the nameplates dictionary
                    namePlates[focus].UnHighlight();
                }
            }
            focus = null;
        }

        public NamePlateController SpawnNamePlate(NamePlateUnit namePlateUnit, bool usePositionOffset) {
            //Debug.Log("NamePlateManager.SpawnNamePlate(" + namePlateUnit.DisplayName + ")");
            NamePlateController namePlate = Instantiate(namePlatePrefab, namePlateContainer);
            namePlates.Add(namePlateUnit, namePlate);
            namePlate.SetNamePlateUnit(namePlateUnit, usePositionOffset);
            return namePlate;
        }

        public NamePlateController AddNamePlate(NamePlateUnit interactable, bool usePositionOffset) {
            //Debug.Log("NamePlateManager.AddNamePlate(" + interactable.DisplayName + ")");
            if (namePlates.ContainsKey(interactable) == false) {
                NamePlateController namePlate = SpawnNamePlate(interactable, usePositionOffset);
                interactable.NamePlateController.NamePlateNeedsRemoval += RemoveNamePlate;
                return namePlate;
            }
            //Debug.Log("NamePlateManager.AddNamePlate(" + namePlateUnit.DisplayName + "): key already existed.  returning null!!!");
            return null;
        }

        public void RemoveNamePlate(NamePlateUnit namePlateUnit) {
            //Debug.Log("NamePlatemanager.RemoveNamePlate(" + namePlateUnit.DisplayName + ")");
            if (namePlateUnit?.NamePlateController != null) {
                namePlateUnit.NamePlateController.NamePlateNeedsRemoval -= RemoveNamePlate;
            }
            if (namePlates.ContainsKey(namePlateUnit)) {
                if (namePlates[namePlateUnit] != null && namePlates[namePlateUnit].gameObject != null) {
                    Destroy(namePlates[namePlateUnit].gameObject);
                }
                namePlates.Remove(namePlateUnit);
            }
        }

        public bool MouseOverNamePlate() {
            foreach (NamePlateController namePlateController in namePlates.Values) {
                if (namePlateController.NamePlateCanvasController.MouseOverNamePlate() == true) {
                    return true;
                }
            }
            return false;
        }

        public void CleanupEventSubscriptions() {
            SystemEventManager.StopListening("AfterCameraUpdate", HandleAfterCameraUpdate);
        }

        public void OnDestroy() {
            CleanupEventSubscriptions();
        }

    }

}