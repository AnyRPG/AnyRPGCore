using AnyRPG;
using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;
using TMPro;
using UMA.CharacterSystem;
using UMA;
using UMA.CharacterSystem.Examples;
using UnityEngine.UI;

namespace AnyRPG {
    public class ColorSelectionController : ConfiguredMonoBehaviour {

        [FormerlySerializedAs("ColorButtonPrefab")]
        [SerializeField]
        private GameObject colorButtonPrefab;

        [Tooltip("buttons will be spawned as children of this GameObject")]
        [FormerlySerializedAs("ColorPanel")]
        [SerializeField]
        public GameObject buttonParent;

        [Tooltip("buttons will be added to this navigationController")]
        [SerializeField]
        private UINavigationController navigationController = null;

        private DynamicCharacterAvatar dynamicCharacterAvatar;
        private SharedColorTable colorTable;
        private GameObject label;
        private string colorName;

        // game manager references

        private ObjectPooler objectPooler = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            objectPooler = systemGameManager.ObjectPooler;
        }

        public void Setup(DynamicCharacterAvatar avatar, string colorName, GameObject buttonParent, GameObject label, SharedColorTable colorTable) {
            //Debug.Log("AvailableColorsHandler.Setup(): colorPanel.name: " + colorPanel.name);
            this.colorName = colorName;
            dynamicCharacterAvatar = avatar;
            this.buttonParent = buttonParent;
            this.label = label;
            this.colorTable = colorTable;

            //Cleanup();

            int index = label.transform.GetSiblingIndex() + 1;
            GameObject go = AddRemoverButton();
            go.transform.SetSiblingIndex(index);
            index++;
            foreach (OverlayColorData ocd in this.colorTable.colors) {
                go = AddButton(ocd);
                go.transform.SetSiblingIndex(index);
                index++;
            }
        }

        /*  public OverlayColorData GetColor(Color c, Color additive)
            {
                OverlayColorData ocd = new OverlayColorData(3);
                ocd.channelMask[0] = c;
                ocd.channelAdditiveMask[0] = additive;
                return ocd;
            }*/


        private GameObject AddRemoverButton() {
            GameObject go = objectPooler.GetPooledObject(colorButtonPrefab, buttonParent.transform);
            ColorPickerButton colorPickerButton = go.GetComponent<ColorPickerButton>();
            colorPickerButton.Configure(systemGameManager);
            colorPickerButton.SetupRemover(dynamicCharacterAvatar, colorName, Color.white);
            //go.transform.SetParent(buttonParent.transform);
            //go.transform.SetAsLastSibling();
            navigationController.AddActiveButton(colorPickerButton);
            
            return go;
        }

        private GameObject AddButton(OverlayColorData ocd) {
            //Debug.Log("AvailableColorsHandler.AddButton(): " + ColorName);

            GameObject go = objectPooler.GetPooledObject(colorButtonPrefab, buttonParent.transform);
            ColorPickerButton colorPickerButton = go.GetComponent<ColorPickerButton>();
            colorPickerButton.Configure(systemGameManager);
            colorPickerButton.Setup(dynamicCharacterAvatar, colorName, ocd, ocd.color);
            //go.transform.SetParent(buttonParent.transform);
            //go.transform.SetAsLastSibling();
            navigationController.AddActiveButton(colorPickerButton);
            
            return go;
        }

        /*
        private void Cleanup() {
            foreach (Transform t in buttonParent.transform) {
                UMAUtils.DestroySceneObject(t.gameObject);
            }
            navigationController.ClearActiveButtons();
        }
        */
    }


}