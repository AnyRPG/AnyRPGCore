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
        private Interactable focus;

        private Dictionary<Interactable, NamePlateController> namePlates = new Dictionary<Interactable, NamePlateController>();

        private void Awake() {
            //Debug.Log("NamePlateManager.Awake(): " + NamePlateManager.MyInstance.gameObject.name);
            string wakeupString = NamePlateManager.MyInstance.gameObject.name;
        }

        private void Start() {
            //Debug.Log(gameObject.name + ".NamePlateManager.Start()");
        }

        public void SetFocus(Interactable newInteractable) {
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

        public NamePlateController SpawnNamePlate(Interactable namePlateUnit, bool usePositionOffset) {
            //Debug.Log("NamePlateManager.SpawnNamePlate(" + namePlateUnit.UnitDisplayName + ")");
            NamePlateController namePlate = Instantiate(namePlatePrefab, namePlateContainer);
            namePlates.Add(namePlateUnit, namePlate);
            namePlate.SetNamePlateUnit(namePlateUnit, usePositionOffset);
            return namePlate;
        }

        public NamePlateController AddNamePlate(Interactable interactable, bool usePositionOffset) {
            //Debug.Log("NamePlateManager.AddNamePlate(" + namePlateUnit.UnitDisplayName + ")");
            if (namePlates.ContainsKey(interactable) == false) {
                NamePlateController namePlate = SpawnNamePlate(interactable, usePositionOffset);
                interactable.NamePlateController.NamePlateNeedsRemoval += RemoveNamePlate;
                return namePlate;
            }
            //Debug.Log("NamePlateManager.AddNamePlate(" + namePlateUnit.MyDisplayName + "): key already existed.  returning null!!!");
            return null;
        }

        public void RemoveNamePlate(Interactable namePlateUnit) {
            //Debug.Log("NamePlatemanager.RemoveNamePlate(" + namePlateUnit.MyDisplayName + ")");
            if (namePlates.ContainsKey(namePlateUnit)) {
                if (namePlates[namePlateUnit] != null && namePlates[namePlateUnit].gameObject != null) {
                    Destroy(namePlates[namePlateUnit].gameObject);
                }
                namePlates.Remove(namePlateUnit);
            }
        }

        public bool MouseOverNamePlate() {
            foreach (NamePlateController namePlateController in namePlates.Values) {
                if (namePlateController.MyNamePlateCanvasController.MouseOverNamePlate() == true) {
                    return true;
                }
            }
            return false;
        }

    }

}