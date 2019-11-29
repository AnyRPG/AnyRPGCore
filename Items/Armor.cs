using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Armor", menuName = "AnyRPG/Inventory/Equipment/Armor", order = 2)]
    public class Armor : Equipment {

        // the armor class required to wear this item
        [SerializeField]
        private string armorClassName;

        public string MyArmorClassName { get => armorClassName; set => armorClassName = value; }

        public override float MyArmorModifier {
            get {
                float returnValue = base.MyArmorModifier;
                if (useArmorModifier && !useManualArmor) {
                    return (int)Mathf.Ceil(Mathf.Clamp(
                        (float)MyItemLevel * (LevelEquations.GetArmorForClass(MyArmorClassName) * GetItemQualityNumber()) * (1f / ((float)(SystemEquipmentSlotProfileManager.MyInstance.MyResourceList.Count - 2))),
                        0f,
                        Mathf.Infinity
                        ));
                }
                return returnValue;
            }
        }

        public override string GetSummary() {

            List<string> abilitiesList = new List<string>();

            string abilitiesString = string.Empty;
            if (abilitiesList.Count > 0) {
                abilitiesString = "\n" + string.Join("\n", abilitiesList);
            }
            List<string> allowedCharacterClasses = GetAllowedCharacterClasses();
            if (allowedCharacterClasses.Count > 0) {
                string colorString = "red";
                if (allowedCharacterClasses.Contains(PlayerManager.MyInstance.MyCharacter.MyCharacterClassName)) {
                    colorString = "white";
                }
                abilitiesString += string.Format("\n<color={0}>{1}</color>", colorString, armorClassName);
            }
            return base.GetSummary() + abilitiesString;
        }

        public List<string> GetAllowedCharacterClasses() {
            List<string> returnValue = new List<string>();
            foreach (CharacterClass characterClass in SystemCharacterClassManager.MyInstance.MyResourceList.Values) {
                if (characterClass.MyArmorClassList != null && characterClass.MyArmorClassList.Count > 0) {
                    //bool foundMatch = false;
                    if (characterClass.MyArmorClassList.Contains(armorClassName)) {
                        returnValue.Add(characterClass.MyName);
                    }
                }
            }
            return returnValue;
        }

        public override bool CanEquip(BaseCharacter baseCharacter) {
            bool returnValue = base.CanEquip(baseCharacter);
            if (returnValue == false) {
                return false;
            }
            List<string> allowedCharacterClasses = GetAllowedCharacterClasses();
            if (allowedCharacterClasses != null && allowedCharacterClasses.Count > 0 && !allowedCharacterClasses.Contains(baseCharacter.MyCharacterClassName)) {
                MessageFeedManager.MyInstance.WriteMessage("You do not have the right armor proficiency to equip " + MyName);
                return false;
            }
            return true;
        }

    }

}