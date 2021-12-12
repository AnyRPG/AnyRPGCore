using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class TooltipController : ConfiguredMonoBehaviour {

        /*
        [SerializeField]
        private GameObject toolTip = null;
        */

        [SerializeField]
        private TextMeshProUGUI toolTipText = null;

        [SerializeField]
        private CurrencyBarController toolTipCurrencyBarController = null;

        [SerializeField]
        private RectTransform tooltipRect = null;

        // keep track of tooltip parameters for updating on window move
        private RectTransform toolTipPanelTransform = null;
        private Transform toolTipButtonTransform = null;
        private IDescribable toolTipDescribable = null;
        private string toolTipSellString = string.Empty;
        private bool toolTipVisible = false;

        // game manager references
        protected UIManager uIManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            toolTipCurrencyBarController.Configure(systemGameManager);
            toolTipCurrencyBarController.DisableTooltip();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
        }

        public void ShowToolTip(Vector2 pivot, Vector3 position, IDescribable describable) {
            ShowToolTip(pivot, position, describable, string.Empty);
        }

        public void ShowToolTip(Vector3 position, IDescribable describable) {
            ShowToolTip(position, describable, string.Empty);
        }


        public void ShowToolTip(Vector3 position, IDescribable describable, string showSellPrice) {
            if (describable == null) {
                HideToolTip();
                return;
            }
            int pivotX;
            int pivotY;
            if (Input.mousePosition.x < (Screen.width / 2)) {
                pivotX = 0;
            } else {
                pivotX = 1;
            }
            if (Input.mousePosition.y < (Screen.height / 2)) {
                pivotY = 0;
            } else {
                pivotY = 1;
            }
            ShowToolTip(new Vector2(pivotX, pivotY), position, describable, showSellPrice);
        }

        /// <summary>
        /// Show the tooltip
        /// </summary>
        public void ShowToolTip(Vector2 pivot, Vector3 position, IDescribable describable, string showSellPrice) {
            //Debug.Log("UIManager.ShowToolTip(" + pivot + ", " + position + ", " + (describable == null ? "null" : describable.DisplayName) + ", " + showSellPrice + ")");
            if (describable == null) {
                HideToolTip();
                return;
            }
            tooltipRect.pivot = pivot;
            gameObject.SetActive(true);

            transform.position = position;
            ShowToolTipCommon(describable, showSellPrice);
            //toolTipText.text = description.GetDescription();

            LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRect);
            float topPoint = tooltipRect.rect.yMax + position.y;
            float bottomPoint = tooltipRect.rect.yMin + position.y;
            //Debug.Log("screen height : " + Screen.height + "; position: " + position + "; top: " + tooltipRect.rect.yMax + "; bottom: " + tooltipRect.rect.yMin);

            // move up if too low
            if (bottomPoint < 0f) {
                transform.position = new Vector3(transform.position.x, (transform.position.y - bottomPoint) + 20, transform.position.z);
                LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRect);
            }

            // move down if too high
            if (topPoint > Screen.height) {
                transform.position = new Vector3(transform.position.x, transform.position.y - ((topPoint - Screen.height) + 20), transform.position.z);
                LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRect);
            }


        }

        public void ShowToolTipCommon(IDescribable describable, string showSellPrice) {
            //Debug.Log("UIManager.ShowToolTipCommon(" + (describable == null ? "null" : describable.DisplayName) + ", " + showSellPrice + ")");
            if (describable == null) {
                HideToolTip();
                return;
            }

            // show new price
            toolTipText.text = describable.GetDescription();
            if (toolTipCurrencyBarController != null) {
                toolTipCurrencyBarController.ClearCurrencyAmounts();
                if (describable is Item && showSellPrice != string.Empty) {
                    KeyValuePair<Currency, int> sellAmount = (describable as Item).GetSellPrice();
                    if (sellAmount.Value == 0 || sellAmount.Key == null) {
                        // don't print a sell price on things that cannot be sold
                        return;
                    }
                    toolTipCurrencyBarController.UpdateCurrencyAmount(sellAmount.Key, sellAmount.Value, showSellPrice);
                }
            }
        }


        /// <summary>
        /// Hide the tooltip
        /// </summary>
        public void HideToolTip() {
            //Debug.Log("UIManager.HideToolTip()");
            gameObject.SetActive(false);
            toolTipVisible = false;
        }

        public void RefreshTooltip(IDescribable describable, string showSellPrice) {
            if (describable != null && toolTipText != null && toolTipText.text != null) {
                ShowToolTipCommon(describable, showSellPrice);
                //toolTipText.text = description.GetDescription();
            } else {
                HideToolTip();
            }
        }

        public void RefreshTooltip(IDescribable describable) {
            RefreshTooltip(describable, string.Empty);
        }

        public void ShowGamepadTooltip(RectTransform paneltransform, Transform buttonTransform, IDescribable describable, string sellPriceString) {
            //Debug.Log("UIManager.ShowGamepadTooltip()");
            //Rect panelRect = RectTransformToScreenSpace((BagPanel.ContentArea as RectTransform));
            toolTipPanelTransform = paneltransform;
            toolTipButtonTransform = buttonTransform;
            toolTipDescribable = describable;
            toolTipSellString = sellPriceString;

            Vector3[] WorldCorners = new Vector3[4];
            paneltransform.GetWorldCorners(WorldCorners);
            float xMin = WorldCorners[0].x;
            float xMax = WorldCorners[2].x;
            //Debug.Log("panel bounds: xmin: " + xMin + "; xmax: " + xMax);

            if (Mathf.Abs((Screen.width / 2f) - xMin) < Mathf.Abs((Screen.width / 2f) - xMax)) {
                // left side is closer to center of the screen
                ShowToolTip(new Vector2(1, 0.5f), new Vector3(xMin, buttonTransform.position.y, 0f), describable, sellPriceString);
            } else {
                // right side is closer to the center of the screen
                ShowToolTip(new Vector2(0, 0.5f), new Vector3(xMax, buttonTransform.position.y, 0f), describable, sellPriceString);
            }
            //uIManager.ShowToolTip(transform.position, inventorySlot.Item, "Sell Price: ");
            toolTipVisible = true;
        }

        public void RefreshGamepadToolTip() {
            if (toolTipVisible == false) {
                return;
            }
            ShowGamepadTooltip(toolTipPanelTransform, toolTipButtonTransform, toolTipDescribable, toolTipSellString);
        }

        



    }

}