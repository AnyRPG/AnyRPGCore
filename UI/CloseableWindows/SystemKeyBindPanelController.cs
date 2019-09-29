using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemKeyBindPanelController : WindowContentController {

    [SerializeField]
    private GameObject movementKeyParent;

    [SerializeField]
    private GameObject actionBarsKeyParent;

    [SerializeField]
    private GameObject systemKeyParent;

    [SerializeField]
    private GameObject keyBindButtonPrefab;

    [Header("Panels")]
    [Tooltip("The UI Sub-Panel under KEY BINDINGS for MOVEMENT")]
    public GameObject PanelMovement;
    [Tooltip("The UI Sub-Panel under KEY BINDINGS for COMBAT")]
    public GameObject PanelCombat;
    [Tooltip("The UI Sub-Panel under KEY BINDINGS for GENERAL")]
    public GameObject PanelGeneral;

    [Header("Buttons")]
    public HighlightButton movementButton;
    public HighlightButton actionBarsButton;
    public HighlightButton systemButton;

    public override event Action<ICloseableWindowContents> OnOpenWindowHandler;

    private void Start() {
        //Debug.Log("KeyBindMenuController.Start()");
        InitializeKeys();
    }

    public void OnEnable() {
        ToggleMovementPanel();
    }

    private void InitializeKeys() {
        //Debug.Log("KeyBindMenuController.InitializeKeys()");
        foreach (KeyBindNode keyBindNode in KeyBindManager.MyInstance.MyKeyBinds.Values) {
            Transform nodeParent = null;
            if (keyBindNode.MyKeyBindType == KeyBindType.Action) {
                nodeParent = actionBarsKeyParent.transform;
            } else if (keyBindNode.MyKeyBindType == KeyBindType.Normal) {
                nodeParent = movementKeyParent.transform;
            } else if (keyBindNode.MyKeyBindType == KeyBindType.Constant || keyBindNode.MyKeyBindType == KeyBindType.System) {
                nodeParent = systemKeyParent.transform;
            }
            KeyBindSlotScript keyBindSlotScript = Instantiate(keyBindButtonPrefab, nodeParent).GetComponent<KeyBindSlotScript>();
            keyBindSlotScript.Initialize(keyBindNode);
            keyBindNode.SetSlotScript(keyBindSlotScript);
        }
    }

    public void ResetPanels() {
        // turn off all panels
        PanelMovement.gameObject.SetActive(false);
        PanelCombat.gameObject.SetActive(false);
        PanelGeneral.gameObject.SetActive(false);

    }

    public void ResetButtons() {
        movementButton.DeSelect();
        actionBarsButton.DeSelect();
        systemButton.DeSelect();
    }

    public void ToggleMovementPanel() {
        ResetPanels();
        PanelMovement.gameObject.SetActive(true);

        ResetButtons();
        movementButton.Select();
    }

    public void ToggleActionBarsPanel() {
        ResetPanels();
        PanelCombat.gameObject.SetActive(true);

        ResetButtons();
        actionBarsButton.Select();
    }

    public void ToggleSystemPanel() {
        ResetPanels();
        PanelGeneral.gameObject.SetActive(true);

        ResetButtons();
        systemButton.Select();
    }


}
