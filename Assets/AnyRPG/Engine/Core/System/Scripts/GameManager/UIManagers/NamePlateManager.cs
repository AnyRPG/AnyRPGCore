using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class NamePlateManager : MonoBehaviour {

        [SerializeField]
        private GameObject namePlatePrefab = null;

        [SerializeField]
        private Transform namePlateContainer = null;

        /// <summary>
        /// The currently focused nameplate so we can highlight the outline
        /// </summary>
        private NamePlateUnit focus;

        private List<NamePlateController> mouseOverList = new List<NamePlateController>();

        private Dictionary<NamePlateUnit, NamePlateController> namePlates = new Dictionary<NamePlateUnit, NamePlateController>();

        public void Init() {
            //Debug.Log("NamePlateManager.Awake(): " + SystemGameManager.Instance.UIManager.NamePlateManager.gameObject.name);
            SystemEventManager.StartListening("AfterCameraUpdate", HandleAfterCameraUpdate);
            SystemEventManager.StartListening("OnLevelUnload", HandleLevelUnload);
        }

        public void AddMouseOver(NamePlateController namePlateController) {
            if (!mouseOverList.Contains(namePlateController)) {
                mouseOverList.Add(namePlateController);
            }
        }

        public void RemoveMouseOver(NamePlateController namePlateController) {
            mouseOverList.Remove(namePlateController);
        }

        public void HandleAfterCameraUpdate(string eventName, EventParamProperties eventParamProperties) {
            UpdateNamePlates();
        }

        public void HandleLevelUnload(string eventName, EventParamProperties eventParamProperties) {
            mouseOverList.Clear();
        }

        public void LateUpdate() {
            if (SystemConfigurationManager.Instance.UseThirdPartyCameraControl == true
                && SystemGameManager.Instance.CameraManager.ThirdPartyCamera.activeInHierarchy == true
                && PlayerManager.Instance.PlayerUnitSpawned == true) {
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
            NamePlateController namePlate = ObjectPooler.Instance.GetPooledObject(namePlatePrefab, namePlateContainer).GetComponent<NamePlateController>();
            namePlates.Add(namePlateUnit, namePlate);
            namePlate.SetNamePlateUnit(namePlateUnit, usePositionOffset);

            // testing - so nameplates spawned after setting target don't end up in front of the target
            namePlate.transform.SetAsFirstSibling();
            return namePlate;
        }

        public NamePlateController AddNamePlate(NamePlateUnit interactable, bool usePositionOffset) {
            //Debug.Log("NamePlateManager.AddNamePlate(" + interactable.DisplayName + ")");
            if (namePlates.ContainsKey(interactable) == false) {
                return SpawnNamePlate(interactable, usePositionOffset);
            }
            //Debug.Log("NamePlateManager.AddNamePlate(" + namePlateUnit.DisplayName + "): key already existed.  returning null!!!");
            return null;
        }

        public void RemoveNamePlate(NamePlateUnit namePlateUnit) {
            //Debug.Log("NamePlatemanager.RemoveNamePlate(" + namePlateUnit.DisplayName + ")");
            if (namePlates.ContainsKey(namePlateUnit)) {
                if (namePlates[namePlateUnit] != null && namePlates[namePlateUnit].gameObject != null) {
                    ObjectPooler.Instance.ReturnObjectToPool(namePlates[namePlateUnit].gameObject);
                }
                namePlates.Remove(namePlateUnit);
            }
        }

        public bool MouseOverNamePlate() {
            /*
            foreach (NamePlateController namePlateController in namePlates.Values) {
                if (namePlateController.NamePlateCanvasController.MouseOverNamePlate() == true) {
                    return true;
                }
            }
            return false;
            */
            return (mouseOverList.Count > 0);
        }

        public void CleanupEventSubscriptions() {
            SystemEventManager.StopListening("AfterCameraUpdate", HandleAfterCameraUpdate);
            SystemEventManager.StopListening("OnLevelUnload", HandleLevelUnload);
        }

        public void OnDestroy() {
            CleanupEventSubscriptions();
        }

    }

}