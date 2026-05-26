using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace AnyRPG {
    public class NewEquipmentSetWizard : NewEquipmentSetWizardBase {

        [Header("Equipment Pieces")]

        [SerializeField] private bool useHead = true;
        [SerializeField] private string headName = string.Empty;
        [SerializeField] private Sprite headIcon = null;
        
        [SerializeField] private bool useShoulders = true;
        [SerializeField] private string shouldersName = string.Empty;
        [SerializeField] private Sprite shouldersIcon = null;
        
        [SerializeField] private bool useChest = true;
        [SerializeField] private string chestName = string.Empty;
        [SerializeField] private Sprite chestIcon = null;
        
        [SerializeField] private bool useWrists = true;
        [SerializeField] private string wristsName = string.Empty;
        [SerializeField] private Sprite wristsIcon = null;
        
        [SerializeField] private bool useHands = true;
        [SerializeField] private string handsName = string.Empty;
        [SerializeField] private Sprite handsIcon = null;
        
        [SerializeField] private bool useWaist = true;
        [SerializeField] private string waistName = string.Empty;
        [SerializeField] private Sprite waistIcon = null;
        
        [SerializeField] private bool useLegs = true;
        [SerializeField] private string legsName = string.Empty;
        [SerializeField] private Sprite legsIcon = null;
        
        [SerializeField] private bool useFeet = true;
        [SerializeField] private string feetName = string.Empty;
        [SerializeField] private Sprite feetIcon = null;

        public override bool UseHead { get => useHead; set => useHead = value; }
        public override string HeadName { get => headName; set => headName = value; }
        public override Sprite HeadIcon { get => headIcon; set => headIcon = value; }

        public override bool UseShoulders { get => useShoulders; set => useShoulders = value; }
        public override string ShouldersName { get => shouldersName; set => shouldersName = value; }
        public override Sprite ShouldersIcon { get => shouldersIcon; set => shouldersIcon = value; }

        public override bool UseChest { get => useChest; set => useChest = value; }
        public override string ChestName { get => chestName; set => chestName = value; }
        public override Sprite ChestIcon { get => chestIcon; set => chestIcon = value; }

        public override bool UseWrists { get => useWrists; set => useWrists = value; }
        public override string WristsName { get => wristsName; set => wristsName = value; }
        public override Sprite WristsIcon { get => wristsIcon; set => wristsIcon = value; }

        public override bool UseHands { get => useHands; set => useHands = value; }
        public override string HandsName { get => handsName; set => handsName = value; }
        public override Sprite HandsIcon { get => handsIcon; set => handsIcon = value; }

        public override bool UseWaist { get => useWaist; set => useWaist = value; }
        public override string WaistName { get => waistName; set => waistName = value; }
        public override Sprite WaistIcon { get => waistIcon; set => waistIcon = value; }

        public override bool UseLegs { get => useLegs; set => useLegs = value; }
        public override string LegsName { get => legsName; set => legsName = value; }
        public override Sprite LegsIcon { get => legsIcon; set => legsIcon = value; }

        public override bool UseFeet { get => useFeet; set => useFeet = value; }
        public override string FeetName { get => feetName; set => feetName = value; }
        public override Sprite FeetIcon { get => feetIcon; set => feetIcon = value; }

        [MenuItem("Tools/AnyRPG/Wizard/New Equipment Set Wizard")]
        public static void CreateWizard() {
            ScriptableWizard.DisplayWizard<NewEquipmentSetWizard>("New Equipment Set Wizard", "Create");
        }
        
    }

}
