using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Armor", menuName = "AnyRPG/Inventory/Equipment/Armor", order = 2)]
    public class Armor : Equipment {

        [Header("Armor")]

        [Tooltip("the armor class required to wear this item")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(ArmorClass))]
        private string armorClassName = string.Empty;

        private ArmorClass armorClass = null;

        public ArmorClass ArmorClass { get => armorClass; set => armorClass = value; }

        public override float GetArmorModifier(int characterLevel, ItemQuality usedItemQuality) {
            float returnValue = base.GetArmorModifier(characterLevel, usedItemQuality);
            if (useArmorModifier && !useManualArmor) {
                return (int)Mathf.Ceil(Mathf.Clamp(
                    (float)GetItemLevel(characterLevel) * (LevelEquations.GetArmorForClass(ArmorClass) * GetItemQualityNumber(usedItemQuality)) * (1f / ((float)(SystemDataFactory.Instance.GetResourceCount<EquipmentSlotProfile>() - 2))),
                    0f,
                    Mathf.Infinity
                    ));
            }
            return returnValue;
        }

        public override string GetSummary(ItemQuality usedItemQuality) {
            //Debug.Log(MyName + ".Armor.GetSummary()");

            List<string> abilitiesList = new List<string>();

            string abilitiesString = string.Empty;
            if (abilitiesList.Count > 0) {
                abilitiesString = "\n" + string.Join("\n", abilitiesList);
            }
            // TODO: this code does not yet account for all the new capabilityProviders and will show red if something like faction provides the capability
            /*
            List<CharacterClass> allowedCharacterClasses = GetAllowedCharacterClasses();
            if (allowedCharacterClasses.Count > 0) {
                string colorString = "red";
                if (allowedCharacterClasses.Contains(SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterClass)) {
                    colorString = "white";
                }
                abilitiesString += string.Format("\n<color={0}>{1}</color>", colorString, armorClassName);
            }
            */
            // testing replacement for above code
            if (armorClassName != null && armorClassName != string.Empty) {
                string colorString = "white";
                if (!CanEquip(SystemGameManager.Instance.PlayerManager.ActiveCharacter)) {
                    colorString = "red";
                }
                abilitiesString += string.Format("\n<color={0}>{1}</color>", colorString, armorClassName);
            }

            return base.GetSummary(usedItemQuality) + abilitiesString;
        }

        public override bool CapabilityConsumerSupported(ICapabilityConsumer capabilityConsumer) {
            return capabilityConsumer.CapabilityConsumerProcessor.IsArmorSupported(this);
        }

        /*
        public override bool CanEquip(BaseCharacter baseCharacter) {
            bool returnValue = base.CanEquip(baseCharacter);
            if (returnValue == false) {
                return false;
            }
            List<CharacterClass> allowedCharacterClasses = GetAllowedCharacterClasses();
            if (allowedCharacterClasses != null && allowedCharacterClasses.Count > 0 && !allowedCharacterClasses.Contains(baseCharacter.CharacterClass)) {
                SystemGameManager.Instance.UIManager.MessageFeedManager.WriteMessage("You do not have the right armor proficiency to equip " + DisplayName);
                return false;
            }
            return true;
        }
        */

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();

            armorClass = null;
            if (armorClassName != null && armorClassName != string.Empty) {
                ArmorClass tmpArmorClass = SystemDataFactory.Instance.GetResource<ArmorClass>(armorClassName);
                if (tmpArmorClass != null) {
                    armorClass = tmpArmorClass;
                } else {
                    Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find armor class : " + armorClassName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }

        }

    }

}