using AnyRPG;
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UMA.CharacterSystem;
using UMA;
using UMA.CharacterSystem.Examples;
using UnityEngine.UI;

namespace AnyRPG {
    public class ColorSelectionController : MonoBehaviour {
        public DynamicCharacterAvatar Avatar;

        // List<OverlayColorData> Colors = new List<OverlayColorData>();
        public SharedColorTable Colors;
        public GameObject ColorPanel;
        public GameObject ColorButtonPrefab;
        public string ColorName;
        public GameObject LabelPrefab;

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
            GameObject go = GameObject.Instantiate(ColorButtonPrefab);
            ColorHandler ch = go.GetComponent<ColorHandler>();
            ch.SetupRemover(Avatar, ColorName);
            Image i = go.GetComponent<Image>();
            i.color = Color.white;
            TextMeshProUGUI t = go.GetComponentInChildren<TextMeshProUGUI>();
            t.text = "<default>";
            go.transform.SetParent(ColorPanel.transform);
        }

        private void AddButton(OverlayColorData ocd) {
            //Debug.Log("AvailableColorsHandler.AddButton(): " + ColorName);

            GameObject go = GameObject.Instantiate(ColorButtonPrefab);
            ColorHandler ch = go.GetComponent<ColorHandler>();
            ch.Setup(Avatar, ColorName, ocd);
            Image i = go.GetComponent<Image>();
            i.color = ocd.color;
            go.transform.SetParent(ColorPanel.transform);
        }

        private void Cleanup() {
            foreach (Transform t in ColorPanel.transform) {
                UMAUtils.DestroySceneObject(t.gameObject);
            }
        }
    }


}