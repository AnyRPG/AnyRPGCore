using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PrerequisiteConditions {

    [SerializeField]
    private bool reverseMatch = false;

    [SerializeField]
    private List<LevelPrerequisite> levelPrerequisites = new List<LevelPrerequisite>();

    [SerializeField]
    private List<QuestPrerequisite> questPrerequisites = new List<QuestPrerequisite>();

    [SerializeField]
    private List<DialogPrerequisite> dialogPrerequisites = new List<DialogPrerequisite>();

    [SerializeField]
    private List<TradeSkillPrerequisite> tradeSkillPrerequisites = new List<TradeSkillPrerequisite>();

    [SerializeField]
    private List<AbilityPrerequisite> abilityPrerequisites = new List<AbilityPrerequisite>();

    [SerializeField]
    private List<FactionDisposition> factionDispositionPrerequisites = new List<FactionDisposition>();

    public bool MyReverseMatch {
        get => reverseMatch;
    }
    
    public virtual bool IsMet() {
        //Debug.Log("PrerequisiteConditions.IsMet()");
        bool returnValue = true;
        foreach (LevelPrerequisite levelPrerequisite in levelPrerequisites) {
            if (!levelPrerequisite.IsMet(PlayerManager.MyInstance.MyCharacter)) {
                returnValue = false;
            }
        }
        foreach (TradeSkillPrerequisite tradeSkillPrerequisite in tradeSkillPrerequisites) {
            //Debug.Log("PrerequisiteConditions.IsMet(): checking tradeskill prerequisite");
            if (!tradeSkillPrerequisite.IsMet(PlayerManager.MyInstance.MyCharacter)) {
                returnValue = false;
            }
        }
        foreach (AbilityPrerequisite abilityPrerequisite in abilityPrerequisites) {
            //Debug.Log("PrerequisiteConditions.IsMet(): checking ability prerequisite");
            if (!abilityPrerequisite.IsMet(PlayerManager.MyInstance.MyCharacter)) {
                returnValue = false;
            }
        }
        foreach (QuestPrerequisite questPrerequisite in questPrerequisites) {
            //Debug.Log("PrerequisiteConditions.IsMet(): checking quest prerequisite");
            if (!questPrerequisite.IsMet(PlayerManager.MyInstance.MyCharacter)) {
                returnValue = false;
            }
        }
        foreach (DialogPrerequisite dialogPrerequisite in dialogPrerequisites) {
            //Debug.Log("PrerequisiteConditions.IsMet(): checking quest prerequisite");
            if (!dialogPrerequisite.IsMet(PlayerManager.MyInstance.MyCharacter)) {
                returnValue = false;
            }
        }
        foreach (FactionDisposition factionDisposition in factionDispositionPrerequisites) {
            if (Faction.RelationWith(PlayerManager.MyInstance.MyCharacter, factionDisposition.faction.MyName) < factionDisposition.disposition) {
                returnValue = false;
            }
        }
        //Debug.Log("PrerequisiteConditions: reversematch: " + reverseMatch + "; returnvalue native: " + returnValue);
        return reverseMatch ? !returnValue : returnValue;
    }
}
