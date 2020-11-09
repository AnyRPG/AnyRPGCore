using AnyRPG;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Faction", menuName = "AnyRPG/Factions/Faction")]
    public class Faction : DescribableResource, ICapabilityProvider {

        [Header("NewGame")]

        [Tooltip("If true, this faction is available for Players to choose on the new game menu")]
        [SerializeField]
        private bool newGameOption = false;

        [Tooltip("When a new game is started, the character will initially spawn in this scene")]
        [SerializeField]
        private string defaultStartingZone = string.Empty;

        [Tooltip("If true, hide any default unit profiles when this faction is used")]
        [SerializeField]
        private bool hideDefaultProfiles = false;

        [Tooltip("The options available when the character creator is used")]
        [SerializeField]
        private List<string> characterCreatorProfileNames = new List<string>();

        // reference to the default profile
        private List<UnitProfile> characterCreatorProfiles = new List<UnitProfile>();

        [Header("Start Equipment")]

        [Tooltip("The names of the equipment that will be worn by this class when a new game is started")]
        [SerializeField]
        private List<string> equipmentNames = new List<string>();

        private List<Equipment> equipmentList = new List<Equipment>();


        [Header("Faction Dispositions")]

        [Tooltip("The disposition that this faction has toward any faction not specificially in its disposition list")]
        public float defaultDisposition = 0f;

        [Tooltip("Specific dispositions toward other factions")]
        public List<FactionDisposition> dispositionList = new List<FactionDisposition>();

        [Header("Capabilities")]

        [Tooltip("Capabilities that apply to all characters of this faction")]
        [SerializeField]
        private CapabilityProps capabilities = new CapabilityProps();

        [Tooltip("Capabilities that only apply to specific character classes")]
        [SerializeField]
        private List<CharacterClassCapabilityNode> classCapabilityList = new List<CharacterClassCapabilityNode>();


        public bool NewGameOption { get => newGameOption; set => newGameOption = value; }
        public string DefaultStartingZone { get => defaultStartingZone; set => defaultStartingZone = value; }
        public List<UnitProfile> CharacterCreatorProfiles { get => characterCreatorProfiles; set => characterCreatorProfiles = value; }
        public bool HideDefaultProfiles { get => hideDefaultProfiles; set => hideDefaultProfiles = value; }
        public List<Equipment> EquipmentList { get => equipmentList; set => equipmentList = value; }

        public CapabilityProps GetFilteredCapabilities(ICapabilityConsumer capabilityConsumer) {
            CapabilityProps returnValue = capabilities;
            foreach (CharacterClassCapabilityNode characterClassCapabilityNode in classCapabilityList) {
                if (capabilityConsumer != null && capabilityConsumer.CharacterClass != null && characterClassCapabilityNode.CharacterClassList.Contains(capabilityConsumer.CharacterClass)) {
                    returnValue = returnValue.Join(characterClassCapabilityNode.Capabilities);
                }
            }
            return returnValue;
        }

        public static Color GetFactionColor(NamePlateUnit namePlateUnit) {
            //Debug.Log("Faction.GetFactionColor(" + namePlateUnit.MyDisplayName + ")");
            if ((namePlateUnit as MonoBehaviour).gameObject == PlayerManager.MyInstance.ActiveUnitController.gameObject) {
                // when retrieving the color that should be displayed on the player character, always green even if it has no faction
                return Color.green;
            }
            // next check custom gained faction for either character
            if (namePlateUnit.CharacterUnit != null && PlayerManager.MyInstance.ActiveUnitController != null) {
                //Debug.Log("Faction.GetFactionColor(" + namePlateUnit.MyDisplayName + ") : nameplate unit is a character unit AND PLAYER UNIT IS SPAWNED");
                return GetFactionColor(PlayerManager.MyInstance.MyCharacter, namePlateUnit.CharacterUnit.BaseCharacter);
            } else {
                //Debug.Log("Faction.GetFactionColor(" + namePlateUnit.MyDisplayName + ") : nameplate unit is NOT a character unit");
            }

            // finally, fallback on dispisition dictionaries and defaults
            return GetFactionColor(namePlateUnit.NamePlateController.Faction);
        }

        public static Color GetFactionColor(BaseCharacter characterToCheck, BaseCharacter myCharacter) {
            //Debug.Log("Faction.GetFactionColor(): " + myCharacter.MyCharacterName + " checking color for: "  + characterToCheck.MyCharacterName);
            float relationValue = Faction.RelationWith(characterToCheck, myCharacter);
            //Debug.Log("Faction.GetFactionColor(): " + myCharacter.MyCharacterName + " checking color for: "  + characterToCheck.MyCharacterName + "; relationValue: " + relationValue);
            return GetColorFromRelationValue(relationValue);
        }

        /// <summary>
        /// gets the faction color between the player character and otherFaction
        /// </summary>
        /// <param name="otherFaction"></param>
        /// <returns></returns>
        public static Color GetFactionColor(Faction sourceFaction) {
            if (PlayerManager.MyInstance.MyCharacter == null) {
                return new Color32(0, 0, 0, 0);
            }

            if (sourceFaction == null) {
                return Color.yellow;
            }

            float relationValue = Faction.RelationWith(PlayerManager.MyInstance.MyCharacter, sourceFaction);
            // override relationValue with default if player is not spawned
            return GetColorFromRelationValue(relationValue);
        }

        public Color GetFactionColor() {
            return GetColorFromRelationValue(defaultDisposition);
        }

        public static Color GetColorFromRelationValue(float relationValue) {
            //Debug.Log("GetColorFromRelationValue(" + relationValue + ")");
            if (PlayerManager.MyInstance.MyCharacter == null) {
                return new Color32(0, 0, 0, 0);
            }

            if (relationValue < 0 && relationValue > -1) {
                //Debug.Log("Faction.GetColorFromRelationValue(" + relationValue + "): returning orange");
                return new Color32(255, 125, 0, 255);
            } else if (relationValue <= -1) {
                return Color.red;
            } else if (relationValue > 0) {
                return Color.green;
            } else {
                return Color.yellow;
            }
        }

        // return the description of relationship between the player and the source faction
        public string GetColoredDescription(Faction sourceFaction) {
            Color factionColor = GetFactionColor(sourceFaction);
            string colorString = ColorUtility.ToHtmlStringRGB(factionColor);
            return string.Format("<color=#{0}>{1}</color>\n{2}", colorString, DisplayName, GetReputationSummary(sourceFaction));
        }

        // return the summary of relationship between the player and the source faction
        public string GetReputationSummary(Faction sourceFaction) {
            float relationValue = RelationWith(PlayerManager.MyInstance.MyCharacter, sourceFaction);
            return string.Format("Reputation: {0}", relationValue);
        }

        // return the relationship between the target characters faction and the source characters faction
        public static float RelationWith(BaseCharacter characterToCheck, BaseCharacter myCharacter) {
            //Debug.Log("Faction.RelationWith(" + (characterToCheck == null ? "null" : characterToCheck.gameObject.name) + ", " + (myCharacter != null ? myCharacter.MyCharacterName : "null" ) + ")");
            Faction otherFaction;
            Faction thisFaction;
            otherFaction = characterToCheck.Faction;
            thisFaction = myCharacter.Faction;
            if (otherFaction != null && thisFaction != null) {
                // first, checking if mycharacter has a reputation modifier for the other faction
                //Debug.Log("Faction.RelationWith(): " + myCharacter.MyName + " is checking if it's own faction manager has a reputation modifier for the faction of target: " + characterToCheck.MyName);
                if (myCharacter.CharacterFactionManager != null && myCharacter.CharacterFactionManager.HasReputationModifier(otherFaction)) {
                    //Debug.Log(".Faction.RelationWith(" + (targetCharacter == null ? "null" : targetCharacter.gameObject.name) + ", " + (sourceCharacter != null ? sourceCharacter.AbilityManager.MyCharacterName : "null") + "): SOURCE HAS MODIFIER!");
                    return myCharacter.CharacterFactionManager.GetReputationValue(otherFaction);
                }
                //Debug.Log("Faction.RelationWith(): " + myCharacter.MyName + " did not have a local reputation modifer.  now checking modifer for it's own faction modifer exists in the target: " + characterToCheck.MyName);
                if (characterToCheck.CharacterFactionManager != null && characterToCheck.CharacterFactionManager.HasReputationModifier(thisFaction)) {
                    //Debug.Log(".Faction.RelationWith(" + (targetCharacter == null ? "null" : targetCharacter.gameObject.name) + ", " + (sourceCharacter != null ? sourceCharacter.AbilityManager.MyCharacterName : "null") + "): TARGET HAS MODIFIER!");
                    return characterToCheck.CharacterFactionManager.GetReputationValue(thisFaction);
                }
            } else {
                //Debug.Log(".Faction.RelationWith(" + (targetCharacter == null ? "null" : targetCharacter.gameObject.name) + ", " + (sourceCharacter != null ? sourceCharacter.AbilityManager.MyCharacterName : "null") + "): ONE CHARACTER WAS NULL!");
            }

            // neither had a special gained reputation with the other, go on to default dispositions
            //Debug.Log("Faction.RelationWith(): " + myCharacter.MyName + " did not have a local reputation modifer and there was no modifer in the target: " + characterToCheck.MyName + " now checking default dispositions for source and target");
            return RelationWith(characterToCheck, myCharacter.Faction);
        }

        // return the relationship between the target characters faction and the source faction
        public static float RelationWith(BaseCharacter targetCharacter, Faction sourceFaction) {
            //Debug.Log("Faction.RelationWith(" + (targetCharacter == null ? "null" : targetCharacter.gameObject.name) + ", " + sourceFactionName + ")");
            Faction thisFaction = sourceFaction;

            if (targetCharacter != null) {
                Faction otherFaction = targetCharacter.Faction;
                // this is duplicated but needs to be here because you can check colors for ui panels and stuff
                if (targetCharacter.CharacterFactionManager != null && targetCharacter.CharacterFactionManager.HasReputationModifier(thisFaction)) {
                    //Debug.Log("Faction.RelationWith(" + (targetCharacter == null ? "null" : targetCharacter.gameObject.name) + ", " + sourceFactionName + "): target had reputation modifer, returning it");
                    return targetCharacter.CharacterFactionManager.GetReputationValue(thisFaction);
                }

                if (otherFaction == null && thisFaction != null) {
                    //Debug.Log("Faction.relationWith(): otherFaction is null");
                    //Debug.Log("Faction.RelationWith(" + (targetCharacter == null ? "null" : targetCharacter.gameObject.name) + ", " + sourceFactionName + "): the other faction was null, returning my default disposition: " + thisFaction.defaultDisposition);
                    return thisFaction.defaultDisposition;
                }
                if (thisFaction == null && otherFaction != null) {
                    //Debug.Log("Faction.RelationWith(" + (targetCharacter == null ? "null" : targetCharacter.gameObject.name) + ", " + sourceFactionName + "): my faction was null, returning other default disposition: " + otherFaction.defaultDisposition);
                    return otherFaction.defaultDisposition;
                }
                if (otherFaction == null && thisFaction == null) {
                    //Debug.Log("Faction.RelationWith(" + (targetCharacter == null ? "null" : targetCharacter.gameObject.name) + ", " + sourceFactionName + "): both factions are null, return 0");
                    return 0;
                }

                // we have passed the null checks, so we can actually compare values now
                if (sourceFaction == otherFaction) {
                    // the 2 factions are the same and so are automatically friendly
                    //Debug.Log("Faction.relationWith(): otherFaction is my faction.  returning 1");
                    return 1;
                }

                //Debug.Log("Faction.RelationWith(" + (targetCharacter == null ? "null" : targetCharacter.gameObject.name) + ", " + sourceFactionName + "): factions are not null or identical, searching my dictionary");
                foreach (FactionDisposition _factionDisposition in thisFaction.dispositionList) {
                    if (_factionDisposition.MyFaction == otherFaction) {
                        // There is a specific entry for the other faction in our disposition table, return it.
                        //Debug.Log("Faction.relationWith(): There is a specific entry for " + otherFaction.MyName + " in our disposition table, return it: " + _factionDisposition.factionName);
                        return _factionDisposition.disposition;
                    }
                }

                // if we made it this far, return the lower of the two default dispositions
                //Debug.Log("Faction.RelationWith(" + (targetCharacter == null ? "null" : targetCharacter.gameObject.name) + ", " + sourceFactionName + "): factions are not null or identical, no dictionary entry exists return lower of 2 defaults: " + (thisFaction.defaultDisposition <= otherFaction.defaultDisposition ? thisFaction.defaultDisposition : otherFaction.defaultDisposition));
                return thisFaction.defaultDisposition <= otherFaction.defaultDisposition ? thisFaction.defaultDisposition : otherFaction.defaultDisposition;
            } else {
                if (thisFaction != null) {
                    return thisFaction.defaultDisposition;
                } else {
                    return 0;
                }
            }

            // If neither faction has an entry for the other faction, return the lowest default disposition
            // This ensures that if a character who is generally neutral or friendly with everyone sees someone whose default is to agro, they will see them as a threat
            //Debug.Log("Faction.relationWith(): factions are unaware of each other. returning lowest default disposition from both");
        }

        public override void SetupScriptableObjects() {
            //Debug.Log("Faction.SetupScriptableObjects()");
            base.SetupScriptableObjects();

            if (equipmentNames != null) {
                foreach (string equipmentName in equipmentNames) {
                    Equipment tmpEquipment = null;
                    tmpEquipment = SystemItemManager.MyInstance.GetResource(equipmentName) as Equipment;
                    if (tmpEquipment != null) {
                        equipmentList.Add(tmpEquipment);
                    } else {
                        Debug.LogError("CharacterClass.SetupScriptableObjects(): Could not find equipment : " + equipmentName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            if (dispositionList != null) {
                foreach (FactionDisposition factionDisposition in dispositionList) {
                    if (factionDisposition != null) {
                        factionDisposition.SetupScriptableObjects();
                    }
                }
            }

            if (characterCreatorProfileNames != null) {
                //Debug.Log("Faction.SetupScriptableObjects(): characterCreatorProfileNames is not null");
                foreach (string characterCreatorProfileName in characterCreatorProfileNames) {
                    //Debug.Log("Faction.SetupScriptableObjects(): found a string");
                    if (characterCreatorProfileName != null && characterCreatorProfileName != string.Empty) {
                        //Debug.Log("Faction.SetupScriptableObjects(): found a string that is not empty");
                        UnitProfile tmpUnitProfile = SystemUnitProfileManager.MyInstance.GetResource(characterCreatorProfileName);
                        if (tmpUnitProfile != null) {
                            //Debug.Log("Faction.SetupScriptableObjects(): found a string that is not empty and added it to the list");
                            characterCreatorProfiles.Add(tmpUnitProfile);
                        } else {
                            Debug.LogError("Faction.SetupScriptableObjects(): could not find unit profile " + characterCreatorProfileName + " while initializing " + DisplayName + ".  Check Inspector");
                        }
                    } else {
                        Debug.LogError("Faction.SetupScriptableObjects(): a character creator profile string was empty while initializing " + DisplayName + ".  Check Inspector");
                    }

                }
            }

        }

    }

    [System.Serializable]
    public class CharacterClassCapabilityNode {

        [Tooltip("The character classes that will have these capabilities")]
        [SerializeField]
        private List<string> characterClasses = new List<string>();

        private List<CharacterClass> characterClassList = new List<CharacterClass>();

        [Tooltip("Traits are status effects which are automatically active at all times if the level requirement is met.")]
        [SerializeField]
        private CapabilityProps capabilities = new CapabilityProps();

        public List<CharacterClass> CharacterClassList { get => characterClassList; set => characterClassList = value; }
        public CapabilityProps Capabilities { get => capabilities; set => capabilities = value; }

        public void SetupScriptableObjects() {

            foreach (string characterClassName in characterClasses) {
                if (characterClassName != null && characterClassName != string.Empty) {
                    CharacterClass tmpCharacterClass = SystemCharacterClassManager.MyInstance.GetResource(characterClassName);
                    if (tmpCharacterClass != null) {
                        characterClassList.Add(tmpCharacterClass);
                    } else {
                        Debug.LogError("UnitProfile.SetupScriptableObjects(): Could not find faction : " + characterClassName + " while inititalizing characterClassAbilityNode.  CHECK INSPECTOR");
                    }
                } else {
                    Debug.LogError("UnitProfile.SetupScriptableObjects(): null or empty character class name while inititalizing characterClassAbilityNode.  CHECK INSPECTOR");
                }
            }

            capabilities.SetupScriptableObjects();
        }
    }


   

}