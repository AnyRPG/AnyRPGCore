using AnyRPG;
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UMA.CharacterSystem;
using UMA;
using UMA.CharacterSystem.Examples;
using UnityEngine.UI;

namespace AnyRPG {
    public class ColorSelectionController : ConfiguredMonoBehaviour {
        public DynamicCharacterAvatar Avatar;

        // List<OverlayColorData> Colors = new List<OverlayColorData>();
        public SharedColorTable Colors;
        public GameObject ColorPanel;
        public GameObject ColorButtonPrefab;
        public string ColorName;
        public GameObject LabelPrefab;

        [SerializeField]
        private UINavigationController colorButtonsController = null;

        // game manager references

        private ObjectPooler objectPooler = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            objectPooler = systemGameManager.ObjectPooler;
        }

        public void Setup(DynamicCharacterAvatar avatar, string colorName, GameObject colorPanel, SharedColorTable colorTable) {
            //Debug.Log("AvailableColorsHandler.Setup(): colorPanel.name: " + colorPanel.name);
            ColorName = colorName;
            Avatar = avatar;
            ColorPanel = colorPanel;
            Colors = colorTable;

            Cleanup();

            AddRemoverButton();
            foreach (OverlayColorData ocd in Colors.colors) {
                AddButton(ocd);
            }
        }

        /*  public OverlayColorData GetColor(Color c, Color additive)
            {
                OverlayColorData ocd = new OverlayColorData(3);
                ocd.channelMask[0] = c;
                ocd.channelAdditiveMask[0] = additive;
                return ocd;
            }*/


        private void AddRemoverButton() {
            GameObject go = objectPooler.GetPooledObject(ColorButtonPrefab);
            ColorPickerButton colorPickerButton = go.GetComponent<ColorPickerButton>();
            colorPickerButton.Configure(systemGameManager);
            colorPickerButton.SetupRemover(Avatar, ColorName, Color.white);
            go.transform.SetParent(ColorPanel.transform);
            colorButtonsController.AddActiveButton(colorPickerButton);
        }

        private void AddButton(OverlayColorData ocd) {
            //Debug.Log("AvailableColorsHandler.AddButton(): " + ColorName);

            GameObject go = objectPooler.GetPooledObject(ColorButtonPrefab);
            ColorPickerButton colorPickerButton = go.GetComponent<ColorPickerButton>();
            colorPickerButton.Configure(systemGameManager);
            colorPickerButton.Setup(Avatar, ColorName, ocd, ocd.color);
            go.transform.SetParent(ColorPanel.transform);
            colorButtonsController.AddActiveButton(colorPickerButton);
        }

        private void Cleanup() {
            foreach (Transform t in ColorPanel.transform) {
                UMAUtils.DestroySceneObject(t.gameObject);
            }
            colorButtonsController.ClearActiveButtons();
        }
    }


}