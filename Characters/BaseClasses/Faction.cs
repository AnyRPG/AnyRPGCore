using AnyRPG;
﻿using UnityEngine;
using System.Collections.Generic;

namespace AnyRPG {
[CreateAssetMenu(fileName = "New Faction", menuName = "Factions/Faction")]
public class Faction : DescribableResource {

    public float defaultDisposition = 0f;

    //public Dictionary<Faction, float> dispositionDictionary = new Dictionary<Faction, float>();
    public List<FactionDisposition> dispositionDictionary;

    // abilities learned when joining this faction
    [SerializeField]
    private List<string> learnedAbilityList = new List<string>();

    public List<string> MyLearnedAbilityList { get => learnedAbilityList; set => learnedAbilityList = value; }

    public static Color GetFactionColor(INamePlateUnit namePlateUnit) {
        //Debug.Log("Faction.GetFactionColor(" + namePlateUnit.MyDisplayName + ")");
        if ((namePlateUnit as MonoBehaviour).gameObject == PlayerManager.MyInstance.MyPlayerUnitObject) {
            // when retrieving the color that should be displayed on the player character, always green even if it has no faction
            return Color.green;
        }
        // next check custom gained faction for either character
        if ((namePlateUnit is CharacterUnit) && PlayerManager.MyInstance.MyPlayerUnitSpawned) {
            //Debug.Log("Faction.GetFactionColor(" + namePlateUnit.MyDisplayName + ") : nameplate unit is a character unit");
            return GetFactionColor(PlayerManager.MyInstance.MyCharacter, (namePlateUnit as CharacterUnit).MyCharacter);
        } else {
            //Debug.Log("Faction.GetFactionColor(" + namePlateUnit.MyDisplayName + ") : nameplate unit is NOT a character unit");
        }

        // finally, fallback on dispisition dictionaries and defaults
        string sourceFactionName = namePlateUnit.MyFactionName;
        return GetFactionColor(sourceFactionName);
    }

    public static Color GetFactionColor(BaseCharacter characterToCheck, BaseCharacter myCharacter) {
        //Debug.Log("Faction.GetFactionColor(): " + myCharacter.MyCharacterName + " checking color for: "  + characterToCheck.MyCharacterName);
        float relationValue = Faction.RelationWith(characterToCheck, myCharacter);
        return GetColorFromRelationValue(relationValue);
    }

    /// <summary>
    /// gets the faction color between the player character and otherFaction
    /// </summary>
    /// <param name="otherFaction"></param>
    /// <returns></returns>
    public static Color GetFactionColor(string sourceFactionName) {
        if (PlayerManager.MyInstance.MyCharacter == null) {
            return new Color32(0, 0, 0, 0);
        }

        if (sourceFactionName == string.Empty) {
            return Color.yellow;
        }

        float relationValue = Faction.RelationWith(PlayerManager.MyInstance.MyCharacter, sourceFactionName);
        return GetColorFromRelationValue(relationValue);
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
    public string GetColoredDescription(string sourceFactionName) {
        Color factionColor = GetFactionColor(sourceFactionName);
        string colorString = ColorUtility.ToHtmlStringRGB(factionColor);
        return string.Format("<color=#{0}>{1}</color>\n{2}", colorString, MyName, GetExtendedSummary(sourceFactionName));
    }

    // return the summary of relationship between the player and the source faction
    public string GetExtendedSummary(string sourceFactionName) {
        float relationValue = RelationWith(PlayerManager.MyInstance.MyCharacter, sourceFactionName);
        return string.Format("{0}\nCurrent Reputation: {1}", base.GetSummary(), relationValue);
    }

    // return the relationship between the target characters faction and the source characters faction
    public static float RelationWith(BaseCharacter characterToCheck, BaseCharacter myCharacter) {
        //Debug.Log("Faction.RelationWith(" + (characterToCheck == null ? "null" : characterToCheck.gameObject.name) + ", " + (myCharacter != null ? myCharacter.MyCharacterName : "null" ) + ")");
        Faction otherFaction;
        Faction thisFaction;
        if (characterToCheck.MyFactionName == null || characterToCheck.MyFactionName == string.Empty) {
            otherFaction = null;
        } else {
            otherFaction = SystemFactionManager.MyInstance.GetResource(characterToCheck.MyFactionName);
        }
        if (myCharacter.MyFactionName == null || myCharacter.MyFactionName == string.Empty) {
            thisFaction = null;
        } else {
            thisFaction = SystemFactionManager.MyInstance.GetResource(myCharacter.MyFactionName);
        }
        if (otherFaction != null && thisFaction != null) {
            // first, checking if mycharacter has a reputation modifier for the other faction
            //Debug.Log("Faction.RelationWith(): " + myCharacter.MyName + " is checking if it's own faction manager has a reputation modifier for the faction of target: " + characterToCheck.MyName);
            if (myCharacter.MyCharacterFactionManager != null && myCharacter.MyCharacterFactionManager.HasReputationModifier(otherFaction)) {
                //Debug.Log(".Faction.RelationWith(" + (targetCharacter == null ? "null" : targetCharacter.gameObject.name) + ", " + (sourceCharacter != null ? sourceCharacter.MyCharacterName : "null") + "): SOURCE HAS MODIFIER!");
                return myCharacter.MyCharacterFactionManager.GetReputationValue(otherFaction);
            }
            //Debug.Log("Faction.RelationWith(): " + myCharacter.MyName + " did not have a local reputation modifer.  now checking modifer for it's own faction modifer exists in the target: " + characterToCheck.MyName);
            if (characterToCheck.MyCharacterFactionManager != null && characterToCheck.MyCharacterFactionManager.HasReputationModifier(thisFaction)) {
                //Debug.Log(".Faction.RelationWith(" + (targetCharacter == null ? "null" : targetCharacter.gameObject.name) + ", " + (sourceCharacter != null ? sourceCharacter.MyCharacterName : "null") + "): TARGET HAS MODIFIER!");
                return characterToCheck.MyCharacterFactionManager.GetReputationValue(thisFaction);
            }
        } else {
            //Debug.Log(".Faction.RelationWith(" + (targetCharacter == null ? "null" : targetCharacter.gameObject.name) + ", " + (sourceCharacter != null ? sourceCharacter.MyCharacterName : "null") + "): ONE CHARACTER WAS NULL!");
        }

        // neither had a special gained reputation with the other, go on to default dispositions
        //Debug.Log("Faction.RelationWith(): " + myCharacter.MyName + " did not have a local reputation modifer and there was no modifer in the target: " + characterToCheck.MyName + " now checking default dispositions for source and target");
        string sourceFactionName = myCharacter.MyFactionName;
        return RelationWith(characterToCheck, sourceFactionName);
    }

    // return the relationship between the target characters faction and the source faction
    public static float RelationWith(BaseCharacter targetCharacter, string sourceFactionName) {
        //Debug.Log("Faction.RelationWith(" + (targetCharacter == null ? "null" : targetCharacter.gameObject.name) + ", " + sourceFactionName + ")");
        Faction thisFaction = null;
        if (sourceFactionName != null && sourceFactionName != string.Empty) {
            thisFaction = SystemFactionManager.MyInstance.GetResource(sourceFactionName);
        }
        
        if (targetCharacter != null) {
            Faction otherFaction = null;
            if (targetCharacter.MyFactionName != null && targetCharacter.MyFactionName != string.Empty) {
                otherFaction = SystemFactionManager.MyInstance.GetResource(targetCharacter.MyFactionName);
            }
            // this is duplicated but needs to be here because you can check colors for ui panels and stuff
            if (targetCharacter.MyCharacterFactionManager != null && targetCharacter.MyCharacterFactionManager.HasReputationModifier(thisFaction)) {
                //Debug.Log("Faction.RelationWith(" + (targetCharacter == null ? "null" : targetCharacter.gameObject.name) + ", " + sourceFactionName + "): target had reputation modifer, returning it");
                return targetCharacter.MyCharacterFactionManager.GetReputationValue(thisFaction);
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
            if (SystemResourceManager.MatchResource(sourceFactionName, otherFaction.MyName)) {
                // the 2 factions are the same and so are automatically friendly
                //Debug.Log("Faction.relationWith(): otherFaction is my faction.  returning 1");
                return 1;
            }

            //Debug.Log("Faction.RelationWith(" + (targetCharacter == null ? "null" : targetCharacter.gameObject.name) + ", " + sourceFactionName + "): factions are not null or identical, searching my dictionary");
            foreach (FactionDisposition _factionDisposition in thisFaction.dispositionDictionary) {
                if (SystemResourceManager.MatchResource(_factionDisposition.factionName, otherFaction.MyName)) {
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

}

}