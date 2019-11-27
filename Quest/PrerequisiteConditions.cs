using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class PrerequisiteConditions {

        // default is require all (AND)
        // set requireAny to use OR logic instead
        [SerializeField]
        private bool requireAny = false;

        // NOT logic
        [SerializeField]
        private bool reverseMatch = false;

        [SerializeField]
        private List<LevelPrerequisite> levelPrerequisites = new List<LevelPrerequisite>();

        [SerializeField]
        private List<CharacterClassPrerequisite> characterClassPrerequisites = new List<CharacterClassPrerequisite>();

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
            bool returnValue = false;
            int prerequisiteCount = 0;
            int tempCount = 0;

            foreach (LevelPrerequisite levelPrerequisite in levelPrerequisites) {
                prerequisiteCount++;
                bool checkResult = levelPrerequisite.IsMet(PlayerManager.MyInstance.MyCharacter);
                if (requireAny && checkResult == true) {
                    returnValue = true;
                    break;
                }
                if (!checkResult && requireAny == false) {
                    returnValue = false;
                    break;
                } else if (checkResult && requireAny == false) {
                    tempCount++;
                }
            }
            if (tempCount > 0 && tempCount == levelPrerequisites.Count & requireAny == false) {
                returnValue = true;
            }
            tempCount = 0;
            foreach (CharacterClassPrerequisite characterClassPrerequisite in characterClassPrerequisites) {
                //Debug.Log("PrerequisiteConditions.IsMet(): CHECKING CHARACTER CLASS PREREQUISITE");
                prerequisiteCount++;
                bool checkResult = characterClassPrerequisite.IsMet(PlayerManager.MyInstance.MyCharacter);
                if (requireAny && checkResult == true) {
                    returnValue = true;
                    break;
                }
                if (!checkResult && requireAny == false) {
                    returnValue = false;
                    break;
                } else if (checkResult && requireAny == false) {
                    tempCount++;
                }
            }
            if (tempCount > 0 && tempCount == characterClassPrerequisites.Count & requireAny == false) {
                //Debug.Log("PrerequisiteConditions.IsMet(): checking character Class prerequisite: setting return value true");
                returnValue = true;
            }
            tempCount = 0;
            foreach (TradeSkillPrerequisite tradeSkillPrerequisite in tradeSkillPrerequisites) {
                //Debug.Log("PrerequisiteConditions.IsMet(): checking tradeskill prerequisite");
                prerequisiteCount++;
                bool checkResult = tradeSkillPrerequisite.IsMet(PlayerManager.MyInstance.MyCharacter);
                if (requireAny && checkResult == true) {
                    returnValue = true;
                    break;
                }
                if (!checkResult && requireAny == false) {
                    returnValue = false;
                    break;
                } else if (checkResult && requireAny == false) {
                    tempCount++;
                }
            }
            if (tempCount > 0 && tempCount == tradeSkillPrerequisites.Count & requireAny == false) {
                returnValue = true;
            }
            tempCount = 0;
            foreach (AbilityPrerequisite abilityPrerequisite in abilityPrerequisites) {
                //Debug.Log("PrerequisiteConditions.IsMet(): checking ability prerequisite");
                prerequisiteCount++;
                bool checkResult = abilityPrerequisite.IsMet(PlayerManager.MyInstance.MyCharacter);
                if (requireAny && checkResult == true) {
                    returnValue = true;
                    break;
                }
                if (!checkResult && requireAny == false) {
                    returnValue = false;
                    break;
                } else if (checkResult && requireAny == false) {
                    tempCount++;
                }
            }
            if (tempCount > 0 && tempCount == abilityPrerequisites.Count & requireAny == false) {
                returnValue = true;
            }
            tempCount = 0;
            foreach (QuestPrerequisite questPrerequisite in questPrerequisites) {
                //Debug.Log("PrerequisiteConditions.IsMet(): checking quest prerequisite");
                prerequisiteCount++;
                bool checkResult = questPrerequisite.IsMet(PlayerManager.MyInstance.MyCharacter);
                if (requireAny && checkResult == true) {
                    returnValue = true;
                    break;
                }
                if (!checkResult && requireAny == false) {
                    returnValue = false;
                    break;
                } else if (checkResult && requireAny == false) {
                    tempCount++;
                }
            }
            if (tempCount > 0 && tempCount == questPrerequisites.Count & requireAny == false) {
                //Debug.Log("PrerequisiteConditions.IsMet(): checking quest prerequisite: setting return value true");
                returnValue = true;
            }
            tempCount = 0;
            foreach (DialogPrerequisite dialogPrerequisite in dialogPrerequisites) {
                //Debug.Log("PrerequisiteConditions.IsMet(): checking quest prerequisite");
                prerequisiteCount++;
                bool checkResult = dialogPrerequisite.IsMet(PlayerManager.MyInstance.MyCharacter);
                if (requireAny && checkResult == true) {
                    returnValue = true;
                    break;
                }
                if (!checkResult && requireAny == false) {
                    returnValue = false;
                    break;
                } else if (checkResult && requireAny == false) {
                    tempCount++;
                }
            }
            if (tempCount > 0 && tempCount == dialogPrerequisites.Count & requireAny == false) {
                returnValue = true;
            }
            tempCount = 0;
            foreach (FactionDisposition factionDisposition in factionDispositionPrerequisites) {
                //Debug.Log("PrerequisiteConditions.IsMet(): checking quest prerequisite");
                prerequisiteCount++;
                bool checkResult = (Faction.RelationWith(PlayerManager.MyInstance.MyCharacter, factionDisposition.factionName) >= factionDisposition.disposition);
                if (requireAny && checkResult == true) {
                    returnValue = true;
                    break;
                }
                if (!checkResult && requireAny == false) {
                    returnValue = false;
                    break;
                } else if (checkResult && requireAny == false) {
                    tempCount++;
                }
            }
            if (tempCount > 0 && tempCount == factionDispositionPrerequisites.Count & requireAny == false) {
                //Debug.Log("PrerequisiteConditions.IsMet(): checking faction : setting return value true");
                returnValue = true;
            }
            if (prerequisiteCount == 0) {
                return true;
            }
            //Debug.Log("PrerequisiteConditions: reversematch: " + reverseMatch + "; returnvalue native: " + returnValue);
            return reverseMatch ? !returnValue : returnValue;
        }
    }

}