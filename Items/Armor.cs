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
        private string armorClassName = string.Empty;

        private ArmorClass armorClass = null;

        public ArmorClass MyArmorClass { get => armorClass; set => armorClass = value; }

        public override float GetArmorModifier(int characterLevel) {
            float returnValue = base.GetArmorModifier(characterLevel);
            if (useArmorModifier && !useManualArmor) {
                return (int)Mathf.Ceil(Mathf.Clamp(
                    (float)GetItemLevel(characterLevel) * (LevelEquations.GetArmorForClass(MyArmorClass) * GetItemQualityNumber()) * (1f / ((float)(SystemEquipmentSlotProfileManager.MyInstance.MyResourceList.Count - 2))),
                    0f,
                    Mathf.Infinity
                    ));
            }
            return returnValue;
        }

        public override string GetSummary() {

            List<string> abilitiesList = new List<string>();

            string abilitiesString = string.Empty;
            if (abilitiesList.Count > 0) {
                abilitiesString = "\n" + string.Join("\n", abilitiesList);
            }
            List<CharacterClass> allowedCharacterClasses = GetAllowedCharacterClasses();
            if (allowedCharacterClasses.Count > 0) {
                string colorString = "red";
                if (allowedCharacterClasses.Contains(PlayerManager.MyInstance.MyCharacter.CharacterClass)) {
                    colorString = "white";
                }
                abilitiesString += string.Format("\n<color={0}>{1}</color>", colorString, armorClassName);
            }
            return base.GetSummary() + abilitiesString;
        }

        public List<CharacterClass> GetAllowedCharacterClasses() {
            List<CharacterClass> returnValue = new List<CharacterClass>();
            foreach (CharacterClass characterClass in SystemCharacterClassManager.MyInstance.MyResourceList.Values) {
                if (characterClass.MyArmorClassList != null && characterClass.MyArmorClassList.Count > 0) {
                    //bool foundMatch = false;
                    if (characterClass.MyArmorClassList.Contains(armorClassName)) {
                        returnValue.Add(characterClass);
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
            List<CharacterClass> allowedCharacterClasses = GetAllowedCharacterClasses();
            if (allowedCharacterClasses != null && allowedCharacterClasses.Count > 0 && !allowedCharacterClasses.Contains(baseCharacter.CharacterClass)) {
                MessageFeedManager.MyInstance.WriteMessage("You do not have the right armor proficiency to equip " + MyName);
                return false;
            }
            return true;
        }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();

            armorClass = null;
            if (armorClassName != null && armorClassName != string.Empty) {
                ArmorClass tmpArmorClass = SystemArmorClassManager.MyInstance.GetResource(armorClassName);
                if (tmpArmorClass != null) {
                    armorClass = tmpArmorClass;
                } else {
                    Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find armor class : " + armorClassName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                }
            }

        }

    }

}