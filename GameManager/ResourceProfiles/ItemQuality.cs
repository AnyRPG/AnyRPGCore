using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Item Quality", menuName = "AnyRPG/ItemQuality")]
    [System.Serializable]
    public class ItemQuality : DescribableResource {

        [Header("Stats")]

        [Tooltip("Multiply the base stats for an item at any level by this amount")]
        [SerializeField]
        private float statMultiplier;



        [Header("Item Level")]

        [Tooltip("Any items of this quality, will automatically scale, regardless of whether they individually have scaling set")]
        [SerializeField]
        private bool dynamicItemLevel;

        [Header("Vendor")]

        [Tooltip("Multiply the base purchase price of an item by this amount")]
        [SerializeField]
        private float buyPriceMultiplier = 1f;

        [Tooltip("Multiply the base vendor sell price of an item by this amount")]
        [SerializeField]
        private float sellPriceMultiplier = 1f;

        [Tooltip("A popup window will appear and ask for confirmation when you try to sell")]
        [SerializeField]
        private bool requireSellConfirmation;

        [Header("Colors")]

        [Tooltip("The color that will be used for text and image backgrounds when an item of this quality is displayed")]
        [SerializeField]
        private Color qualityColor;

        [Tooltip("If true, the background image will have its color set to this color, intead of black")]
        [SerializeField]
        private bool tintBackgroundImage = false;


        public float MyStatMultiplier { get => statMultiplier; set => statMultiplier = value; }
        public Color MyQualityColor { get => qualityColor; set => qualityColor = value; }
        public bool MyDynamicItemLevel { get => dynamicItemLevel; set => dynamicItemLevel = value; }
        public bool MyRequireSellConfirmation { get => requireSellConfirmation; set => requireSellConfirmation = value; }
        public float BuyPriceMultiplier { get => buyPriceMultiplier; set => buyPriceMultiplier = value; }
        public float SellPriceMultiplier { get => sellPriceMultiplier; set => sellPriceMultiplier = value; }
        public bool TintBackgroundImage { get => tintBackgroundImage; set => tintBackgroundImage = value; }
    }

}