using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private NamePlateController namePlatePrefab;

    [SerializeField]
    private Transform namePlateCanvas;

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

    public NamePlateController SpawnNamePlate(INamePlateUnit namePlateUnit) {
        //Debug.Log("NamePlateManager.SpawnNamePlate(" + namePlateUnit.MyDisplayName + ")");
        NamePlateController namePlate = Instantiate(namePlatePrefab, namePlateCanvas);
        namePlates.Add(namePlateUnit, namePlate);
        namePlate.SetNamePlateUnit(namePlateUnit);
        return namePlate;
    }

    public NamePlateController AddNamePlate(INamePlateUnit namePlateUnit) {
        //Debug.Log("NamePlateManager.AddNamePlate(" + namePlateUnit.MyDisplayName + ")");
        if (namePlates.ContainsKey(namePlateUnit) == false) {
            NamePlateController namePlate = SpawnNamePlate(namePlateUnit);
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
}
