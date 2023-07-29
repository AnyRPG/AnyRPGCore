using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Armor", menuName = "AnyRPG/Inventory/Equipment/Armor", order = 2)]
    public class Armor : Equipment {

        [Header("Armor")]

        [Tooltip("If true, the character must have the armor class below in order to equip this item")]
        [SerializeField]
        private bool requireArmorClass = false;

        [Tooltip("the armor class this item gets its armor value from")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(ArmorClass))]
        private string armorClassName = string.Empty;

        private ArmorClass armorClass = null;

        public ArmorClass ArmorClass { get => armorClass; set => armorClass = value; }
        public bool RequireArmorClass { get => requireArmorClass; set => requireArmorClass = value; }
        public string ArmorClassName { get => armorClassName; set => armorClassName = value; }

        public override float GetArmorModifier(int characterLevel, ItemQuality usedItemQuality) {
            float returnValue = base.GetArmorModifier(characterLevel, usedItemQuality);
            if (useArmorModifier && !useManualArmor) {
                return (int)Mathf.Ceil(Mathf.Clamp(
                    (float)GetItemLevel(characterLevel) * (LevelEquations.GetArmorForClass(ArmorClass) * GetItemQualityNumber(usedItemQuality)) * (1f / ((float)(systemDataFactory.GetResourceCount<EquipmentSlotProfile>() - 2))),
                    0f,
                    Mathf.Infinity
                    ));
            }
            return returnValue;
        }

        public override string GetDescription(ItemQuality usedItemQuality) {
            //Debug.Log(DisplayName + ".Armor.GetSummary()");

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
                if (allowedCharacterClasses.Contains(playerManager.UnitController.BaseCharacter.CharacterClass)) {
                    colorString = "white";
                }
                abilitiesString += string.Format("\n<color={0}>{1}</color>", colorString, armorClassName);
            }
            */
            // testing replacement for above code
            if (armorClassName != null && armorClassName != string.Empty) {
                string colorString = "white";
                if (!CanEquip(playerManager.UnitController)) {
                    colorString = "red";
                }
                abilitiesString += string.Format("\n<color={0}>{1}</color>", colorString, armorClassName);
            }

            return base.GetDescription(usedItemQuality) + abilitiesString;
        }

        public override bool CapabilityConsumerSupported(ICapabilityConsumer capabilityConsumer) {
            if (armorClass == null || requireArmorClass == false) {
                return true;
            }
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
                messageFeedManager.WriteMessage("You do not have the right armor proficiency to equip " + DisplayName);
                return false;
            }
            return true;
        }
        */

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            armorClass = null;
            if (armorClassName != null && armorClassName != string.Empty) {
                ArmorClass tmpArmorClass = systemDataFactory.GetResource<ArmorClass>(armorClassName);
                if (tmpArmorClass != null) {
                    armorClass = tmpArmorClass;
                } else {
                    Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find armor class : " + armorClassName + " while inititalizing " + ResourceName + ".  CHECK INSPECTOR");
                }
            }

        }

    }

}