using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        private bool lastResult = false;

        [SerializeField]
        private List<LevelPrerequisite> levelPrerequisites = new List<LevelPrerequisite>();

        [SerializeField]
        private List<CharacterClassPrerequisite> characterClassPrerequisites = new List<CharacterClassPrerequisite>();

        [SerializeField]
        private List<QuestPrerequisite> questPrerequisites = new List<QuestPrerequisite>();

        [SerializeField]
        private List<DialogPrerequisite> dialogPrerequisites = new List<DialogPrerequisite>();

        [SerializeField]
        private List<VisitZonePrerequisite> visitZonePrerequisites = new List<VisitZonePrerequisite>();

        [SerializeField]
        private List<TradeSkillPrerequisite> tradeSkillPrerequisites = new List<TradeSkillPrerequisite>();

        [SerializeField]
        private List<AbilityPrerequisite> abilityPrerequisites = new List<AbilityPrerequisite>();

        [SerializeField]
        private List<FactionPrerequisite> factionPrerequisites = new List<FactionPrerequisite>();

        //private IPrerequisiteOwner prerequisiteOwner = null;
        private List<IPrerequisiteOwner> prerequisiteOwners = new List<IPrerequisiteOwner>();

        List<List<IPrerequisite>> allPrerequisites = new List<List<IPrerequisite>>();

        public bool MyReverseMatch {
            get => reverseMatch;
        }

        public void HandlePrerequisiteUpdates() {
            //Debug.Log("PrerequisiteConditions.HandlePrerequisiteUpdates()");
            /*
            if ((prerequisiteOwner as MonoBehaviour) is MonoBehaviour) {
                Debug.Log("PrerequisiteConditions.HandlePrerequisiteUpdates(): calling prerequisiteOwner.HandlePrerequisiteUpdates(): owner: " + (prerequisiteOwner as MonoBehaviour).gameObject.name);
            }
            */
            bool oldResult = lastResult;
            if (IsMet()) {
                // do callback to the owning object
                //Debug.Log("PrerequisiteConditions.HandlePrerequisiteUpdates(): calling prerequisiteOwner.HandlePrerequisiteUpdates()");
                SendPrerequisiteUpdates();
            } else {
                if (oldResult != lastResult) {
                    //Debug.Log("PrerequisiteConditions.HandlePrerequisiteUpdates(): ismet: " + IsMet() + "; prerequisiteOwner: " + (prerequisiteOwner == null ? "null" : "set") + "; RESULT CHANGED!");
                    SendPrerequisiteUpdates();
                }
                //Debug.Log("PrerequisiteConditions.HandlePrerequisiteUpdates(): ismet: " + IsMet() + "; prerequisiteOwner: " + (prerequisiteOwner == null ? "null" : "set"));
            }
        }

        private void SendPrerequisiteUpdates() {
            foreach (IPrerequisiteOwner prerequisiteOwner in prerequisiteOwners) {
                if (prerequisiteOwner != null) {
                    prerequisiteOwner.HandlePrerequisiteUpdates();
                }
            }
        }

        private void CreateAllList() {
            if (allPrerequisites.Count == 0) {
                allPrerequisites.Add(levelPrerequisites.Cast<IPrerequisite>().ToList());
                allPrerequisites.Add(characterClassPrerequisites.Cast<IPrerequisite>().ToList());
                allPrerequisites.Add(tradeSkillPrerequisites.Cast<IPrerequisite>().ToList());
                allPrerequisites.Add(abilityPrerequisites.Cast<IPrerequisite>().ToList());
                allPrerequisites.Add(questPrerequisites.Cast<IPrerequisite>().ToList());
                allPrerequisites.Add(dialogPrerequisites.Cast<IPrerequisite>().ToList());
                allPrerequisites.Add(visitZonePrerequisites.Cast<IPrerequisite>().ToList());
                allPrerequisites.Add(factionPrerequisites.Cast<IPrerequisite>().ToList());
            }
        }

        public virtual bool IsMet() {
            //Debug.Log("PrerequisiteConditions.IsMet()");
            bool returnValue = false;
            int prerequisiteCount = 0;
            int tempCount = 0;
            int falseCount = 0;

            foreach (List<IPrerequisite> prerequisiteList in allPrerequisites) {
                tempCount = 0;
                foreach (IPrerequisite prerequisite in prerequisiteList) {
                    prerequisiteCount++;
                    bool checkResult = prerequisite.IsMet(SystemGameManager.Instance.PlayerManager.MyCharacter);
                    if (requireAny && checkResult == true) {
                        returnValue = true;
                        break;
                    }
                    if (!checkResult && requireAny == false) {
                        falseCount++;
                        break;
                    } else if (checkResult && requireAny == false) {
                        tempCount++;
                    }
                }
                if (tempCount > 0 && tempCount == prerequisiteList.Count && requireAny == false) {
                    returnValue = true;
                }
            }

            /*
            foreach (LevelPrerequisite levelPrerequisite in levelPrerequisites) {
                prerequisiteCount++;
                bool checkResult = levelPrerequisite.IsMet(SystemGameManager.Instance.PlayerManager.MyCharacter);
                if (requireAny && checkResult == true) {
                    returnValue = true;
                    break;
                }
                if (!checkResult && requireAny == false) {
                    falseCount++;
                    break;
                } else if (checkResult && requireAny == false) {
                    tempCount++;
                }
            }
            if (tempCount > 0 && tempCount == levelPrerequisites.Count && requireAny == false) {
                returnValue = true;
            }
            tempCount = 0;
            foreach (CharacterClassPrerequisite characterClassPrerequisite in characterClassPrerequisites) {
                //Debug.Log("PrerequisiteConditions.IsMet(): CHECKING CHARACTER CLASS PREREQUISITE");
                prerequisiteCount++;
                bool checkResult = characterClassPrerequisite.IsMet(SystemGameManager.Instance.PlayerManager.MyCharacter);
                if (requireAny && checkResult == true) {
                    returnValue = true;
                    break;
                }
                if (!checkResult && requireAny == false) {
                    falseCount++;
                    break;
                } else if (checkResult && requireAny == false) {
                    tempCount++;
                }
            }
            if (tempCount > 0 && tempCount == characterClassPrerequisites.Count && requireAny == false) {
                //Debug.Log("PrerequisiteConditions.IsMet(): checking character Class prerequisite: setting return value true");
                returnValue = true;
            }
            tempCount = 0;
            foreach (TradeSkillPrerequisite tradeSkillPrerequisite in tradeSkillPrerequisites) {
                //Debug.Log("PrerequisiteConditions.IsMet(): checking tradeskill prerequisite");
                prerequisiteCount++;
                bool checkResult = tradeSkillPrerequisite.IsMet(SystemGameManager.Instance.PlayerManager.MyCharacter);
                if (requireAny && checkResult == true) {
                    returnValue = true;
                    break;
                }
                if (!checkResult && requireAny == false) {
                    falseCount++;
                    break;
                } else if (checkResult && requireAny == false) {
                    tempCount++;
                }
            }
            if (tempCount > 0 && tempCount == tradeSkillPrerequisites.Count && requireAny == false) {
                returnValue = true;
            }
            tempCount = 0;
            foreach (AbilityPrerequisite abilityPrerequisite in abilityPrerequisites) {
                //Debug.Log("PrerequisiteConditions.IsMet(): checking ability prerequisite");
                prerequisiteCount++;
                bool checkResult = abilityPrerequisite.IsMet(SystemGameManager.Instance.PlayerManager.MyCharacter);
                if (requireAny && checkResult == true) {
                    returnValue = true;
                    break;
                }
                if (!checkResult && requireAny == false) {
                    falseCount++;
                    break;
                } else if (checkResult && requireAny == false) {
                    tempCount++;
                }
            }
            if (tempCount > 0 && tempCount == abilityPrerequisites.Count && requireAny == false) {
                //Debug.Log("PrerequisiteConditions.IsMet(): checking ability prerequisite: setting returnvalue true");
                returnValue = true;
            }
            tempCount = 0;
            foreach (QuestPrerequisite questPrerequisite in questPrerequisites) {
                //Debug.Log("PrerequisiteConditions.IsMet(): checking quest prerequisite");
                prerequisiteCount++;
                bool checkResult = questPrerequisite.IsMet(SystemGameManager.Instance.PlayerManager.MyCharacter);
                if (requireAny && checkResult == true) {
                    returnValue = true;
                    break;
                }
                if (!checkResult && requireAny == false) {
                    falseCount++;
                    break;
                } else if (checkResult && requireAny == false) {
                    tempCount++;
                }
            }
            if (tempCount > 0 && tempCount == questPrerequisites.Count && requireAny == false) {
                //Debug.Log("PrerequisiteConditions.IsMet(): checking quest prerequisite: setting return value true");
                returnValue = true;
            }
            tempCount = 0;
            foreach (DialogPrerequisite dialogPrerequisite in dialogPrerequisites) {
                //Debug.Log("PrerequisiteConditions.IsMet(): checking quest prerequisite");
                prerequisiteCount++;
                bool checkResult = dialogPrerequisite.IsMet(SystemGameManager.Instance.PlayerManager.MyCharacter);
                if (requireAny && checkResult == true) {
                    returnValue = true;
                    break;
                }
                if (!checkResult && requireAny == false) {
                    falseCount++;
                    break;
                } else if (checkResult && requireAny == false) {
                    tempCount++;
                }
            }
            if (tempCount > 0 && tempCount == dialogPrerequisites.Count && requireAny == false) {
                returnValue = true;
            }
            tempCount = 0;
            foreach (VisitZonePrerequisite visitZonePrerequisite in visitZonePrerequisites) {
                //Debug.Log("PrerequisiteConditions.IsMet(): checking quest prerequisite");
                prerequisiteCount++;
                bool checkResult = visitZonePrerequisite.IsMet(SystemGameManager.Instance.PlayerManager.MyCharacter);
                if (requireAny && checkResult == true) {
                    returnValue = true;
                    break;
                }
                if (!checkResult && requireAny == false) {
                    falseCount++;
                    break;
                } else if (checkResult && requireAny == false) {
                    tempCount++;
                }
            }
            if (tempCount > 0 && tempCount == visitZonePrerequisites.Count && requireAny == false) {
                returnValue = true;
            }
            tempCount = 0;
            foreach (FactionPrerequisite factionPrerequisite in factionPrerequisites) {
                //Debug.Log("PrerequisiteConditions.IsMet(): checking quest prerequisite");
                prerequisiteCount++;
                bool checkResult = factionPrerequisite.IsMet(SystemGameManager.Instance.PlayerManager.MyCharacter);
                if (requireAny && checkResult == true) {
                    returnValue = true;
                    break;
                }
                if (!checkResult && requireAny == false) {
                    falseCount++;
                    break;
                } else if (checkResult && requireAny == false) {
                    tempCount++;
                }
            }
            if (tempCount > 0 && tempCount == factionPrerequisites.Count && requireAny == false) {
                //Debug.Log("PrerequisiteConditions.IsMet(): checking faction : setting return value true");
                returnValue = true;
            }
            */
            if (falseCount > 0) {
                returnValue = false;
            }
            if (prerequisiteCount == 0) {
                lastResult = true;
                return true;
            }
            //Debug.Log("PrerequisiteConditions: reversematch: " + reverseMatch + "; returnvalue native: " + returnValue);
            bool returnResult = reverseMatch ? !returnValue : returnValue;
            lastResult = returnResult;
            return returnResult;
        }

        // force prerequisite status update outside normal event notification
        public void UpdatePrerequisites(bool notify = true) {
            foreach (List<IPrerequisite> prerequisiteList in allPrerequisites) {
                foreach (IPrerequisite prerequisite in prerequisiteList) {
                    prerequisite.UpdateStatus(notify);
                }
            }
            /*
            foreach (IPrerequisite prerequisite in levelPrerequisites) {
                prerequisite.UpdateStatus(notify);
            }
            foreach (IPrerequisite prerequisite in characterClassPrerequisites) {
                prerequisite.UpdateStatus(notify);
            }
            foreach (IPrerequisite prerequisite in questPrerequisites) {
                prerequisite.UpdateStatus(notify);
            }
            foreach (IPrerequisite prerequisite in dialogPrerequisites) {
                prerequisite.UpdateStatus(notify);
            }
            foreach (IPrerequisite prerequisite in visitZonePrerequisites) {
                prerequisite.UpdateStatus(notify);
            }
            foreach (IPrerequisite prerequisite in tradeSkillPrerequisites) {
                prerequisite.UpdateStatus(notify);
            }
            foreach (IPrerequisite prerequisite in abilityPrerequisites) {
                prerequisite.UpdateStatus(notify);
            }
            foreach (IPrerequisite prerequisite in factionPrerequisites) {
                prerequisite.UpdateStatus(notify);
            }
            */
        }

        public void SetupScriptableObjects(IPrerequisiteOwner prerequisiteOwner) {
            CreateAllList();

            if (prerequisiteOwners.Count == 0) {
                foreach (List<IPrerequisite> prerequisiteList in allPrerequisites) {
                    foreach (IPrerequisite prerequisite in prerequisiteList) {
                        prerequisite.SetupScriptableObjects();
                        prerequisite.OnStatusUpdated += HandlePrerequisiteUpdates;
                    }
                }
            }

            prerequisiteOwners.Add(prerequisiteOwner);
            //this.prerequisiteOwner = prerequisiteOwner;

            /*
            foreach (IPrerequisite prerequisite in levelPrerequisites) {
                prerequisite.SetupScriptableObjects();
                prerequisite.OnStatusUpdated += HandlePrerequisiteUpdates;
            }
            foreach (IPrerequisite prerequisite in characterClassPrerequisites) {
                prerequisite.SetupScriptableObjects();
                prerequisite.OnStatusUpdated += HandlePrerequisiteUpdates;
            }
            foreach (IPrerequisite prerequisite in questPrerequisites) {
                prerequisite.SetupScriptableObjects();
                prerequisite.OnStatusUpdated += HandlePrerequisiteUpdates;
            }
            foreach (IPrerequisite prerequisite in dialogPrerequisites) {
                prerequisite.SetupScriptableObjects();
                prerequisite.OnStatusUpdated += HandlePrerequisiteUpdates;
            }
            foreach (IPrerequisite prerequisite in visitZonePrerequisites) {
                prerequisite.SetupScriptableObjects();
                prerequisite.OnStatusUpdated += HandlePrerequisiteUpdates;
            }
            foreach (IPrerequisite prerequisite in tradeSkillPrerequisites) {
                prerequisite.SetupScriptableObjects();
                prerequisite.OnStatusUpdated += HandlePrerequisiteUpdates;
            }
            foreach (IPrerequisite prerequisite in abilityPrerequisites) {
                prerequisite.SetupScriptableObjects();
                prerequisite.OnStatusUpdated += HandlePrerequisiteUpdates;
            }
            foreach (IPrerequisite prerequisite in factionPrerequisites) {
                prerequisite.SetupScriptableObjects();
                prerequisite.OnStatusUpdated += HandlePrerequisiteUpdates;
            }
            */
            /*
            foreach (FactionDisposition prerequisite in factionDispositionPrerequisites) {
                prerequisite.SetupScriptableObjects();
            }
            */
        }

        public void CleanupScriptableObjects(IPrerequisiteOwner prerequisiteOwner) {

            prerequisiteOwners.Remove(prerequisiteOwner);

            if (prerequisiteOwners.Count == 0) {
                foreach (List<IPrerequisite> prerequisiteList in allPrerequisites) {
                    foreach (IPrerequisite prerequisite in prerequisiteList) {
                        prerequisite.CleanupScriptableObjects();
                        prerequisite.OnStatusUpdated -= HandlePrerequisiteUpdates;
                    }
                }
            }

            /*
            foreach (IPrerequisite prerequisite in levelPrerequisites) {
                prerequisite.CleanupScriptableObjects();
                prerequisite.OnStatusUpdated -= HandlePrerequisiteUpdates;
            }
            foreach (IPrerequisite prerequisite in characterClassPrerequisites) {
                prerequisite.CleanupScriptableObjects();
                prerequisite.OnStatusUpdated -= HandlePrerequisiteUpdates;
            }
            foreach (IPrerequisite prerequisite in questPrerequisites) {
                prerequisite.CleanupScriptableObjects();
                prerequisite.OnStatusUpdated -= HandlePrerequisiteUpdates;
            }
            foreach (IPrerequisite prerequisite in dialogPrerequisites) {
                prerequisite.CleanupScriptableObjects();
                prerequisite.OnStatusUpdated -= HandlePrerequisiteUpdates;
            }
            foreach (IPrerequisite prerequisite in tradeSkillPrerequisites) {
                prerequisite.CleanupScriptableObjects();
                prerequisite.OnStatusUpdated -= HandlePrerequisiteUpdates;
            }
            foreach (IPrerequisite prerequisite in abilityPrerequisites) {
                prerequisite.CleanupScriptableObjects();
                prerequisite.OnStatusUpdated -= HandlePrerequisiteUpdates;
            }
            foreach (IPrerequisite prerequisite in factionPrerequisites) {
                prerequisite.CleanupScriptableObjects();
                prerequisite.OnStatusUpdated -= HandlePrerequisiteUpdates;
            }
            */
        }

    }

}