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
        private INamePlateUnit focus;

        private Dictionary<INamePlateUnit, NamePlateController> namePlates = new Dictionary<INamePlateUnit, NamePlateController>();

        private void Awake() {
            //Debug.Log("NamePlateManager.Awake(): " + NamePlateManager.MyInstance.gameObject.name);
            string wakeupString = NamePlateManager.MyInstance.gameObject.name;
        }

        private void Start() {
            //Debug.Log(gameObject.name + ".NamePlateManager.Start()");
        }

        public void SetFocus(INamePlateUnit namePlateUnit) {
            ClearFocus();
            //Debug.Log("NamePlateManager.SetFocus(" + characterUnit.MyCharacter.MyCharacterName + ")");
            if (namePlates.ContainsKey(namePlateUnit)) {
                focus = namePlateUnit;
                // enemy could be dead so we need to check if they exist in the nameplates dictionary
                namePlates[namePlateUnit].Highlight();
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

        public NamePlateController SpawnNamePlate(INamePlateUnit namePlateUnit, bool usePositionOffset) {
            //Debug.Log("NamePlateManager.SpawnNamePlate(" + namePlateUnit.UnitDisplayName + ")");
            NamePlateController namePlate = Instantiate(namePlatePrefab, namePlateContainer);
            namePlates.Add(namePlateUnit, namePlate);
            namePlate.SetNamePlateUnit(namePlateUnit, usePositionOffset);
            return namePlate;
        }

        public NamePlateController AddNamePlate(INamePlateUnit namePlateUnit, bool usePositionOffset) {
            //Debug.Log("NamePlateManager.AddNamePlate(" + namePlateUnit.UnitDisplayName + ")");
            if (namePlates.ContainsKey(namePlateUnit) == false) {
                NamePlateController namePlate = SpawnNamePlate(namePlateUnit, usePositionOffset);
                namePlateUnit.NamePlateNeedsRemoval += RemoveNamePlate;
                return namePlate;
            }
            //Debug.Log("NamePlateManager.AddNamePlate(" + namePlateUnit.MyDisplayName + "): key already existed.  returning null!!!");
            return null;
        }

        public void RemoveNamePlate(INamePlateUnit namePlateUnit) {
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