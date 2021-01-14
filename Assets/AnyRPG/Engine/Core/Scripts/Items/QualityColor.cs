using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    /// <summary>
    /// Enum for declaring the quality of the item
    /// </summary>
    public enum Quality { Poor, Common, Uncommon, Rare, Epic, Legendary, Artifact, Heirloom }

    public static class QualityColor {
        private static Dictionary<Quality, string> colors = new Dictionary<Quality, string>()
        {
        {Quality.Poor,  "#"+ColorUtility.ToHtmlStringRGB(Color.gray)},
        {Quality.Common,  "#"+ColorUtility.ToHtmlStringRGB(Color.white)},
        {Quality.Uncommon,  "#"+ColorUtility.ToHtmlStringRGB(Color.green)},
        {Quality.Rare,  "#"+ColorUtility.ToHtmlStringRGB(Color.blue)},
        {Quality.Epic,  "#"+ColorUtility.ToHtmlStringRGB(Color.magenta)},
        {Quality.Legendary,  "#"+ColorUtility.ToHtmlStringRGB(Color.red)},
        {Quality.Artifact,  "#"+ColorUtility.ToHtmlStringRGB(Color.yellow)},
        {Quality.Heirloom,  "#"+ColorUtility.ToHtmlStringRGB(Color.cyan)},
    };

        public static Dictionary<Quality, string> MyColors {
            get {
                return colors;
            }
        }

        public static string GetQualityColorString(Item item) {
            if (item.ItemQuality != null) {
                return "#" + ColorUtility.ToHtmlStringRGB(item.ItemQuality.MyQualityColor);
            }
            return "#" + ColorUtility.ToHtmlStringRGB(Color.white);
        }
    }

}