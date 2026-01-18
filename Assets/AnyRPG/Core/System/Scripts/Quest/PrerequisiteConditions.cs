using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class PrerequisiteConditions : ConfiguredClass {

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

        // game manager references
        private PlayerManager playerManager = null;

        public bool ReverseMatch {
            get => reverseMatch;
        }

        public void HandlePrerequisiteUpdates(UnitController sourceUnitController) {
            //Debug.Log($"PrerequisiteConditions.HandlePrerequisiteUpdates({sourceUnitController.gameObject.name})");
            /*
            if ((prerequisiteOwner as MonoBehaviour) is MonoBehaviour) {
                //Debug.Log("PrerequisiteConditions.HandlePrerequisiteUpdates(): calling prerequisiteOwner.HandlePrerequisiteUpdates(): owner: " + (prerequisiteOwner as MonoBehaviour).gameObject.name);
            }
            */
            bool oldResult = lastResult;
            if (IsMet(sourceUnitController)) {
                // do callback to the owning object
                //Debug.Log("PrerequisiteConditions.HandlePrerequisiteUpdates(): calling prerequisiteOwner.HandlePrerequisiteUpdates()");
                SendPrerequisiteUpdates(sourceUnitController);
            } else {
                if (oldResult != lastResult) {
                    //Debug.Log("PrerequisiteConditions.HandlePrerequisiteUpdates(): ismet: " + IsMet() + "; prerequisiteOwner: " + (prerequisiteOwner == null ? "null" : "set") + "; RESULT CHANGED!");
                    SendPrerequisiteUpdates(sourceUnitController);
                }
                //Debug.Log("PrerequisiteConditions.HandlePrerequisiteUpdates(): ismet: " + IsMet() + "; prerequisiteOwner: " + (prerequisiteOwner == null ? "null" : "set"));
            }
        }

        private void SendPrerequisiteUpdates(UnitController sourceUnitController) {
            foreach (IPrerequisiteOwner prerequisiteOwner in prerequisiteOwners) {
                if (prerequisiteOwner != null) {
                    prerequisiteOwner.HandlePrerequisiteUpdates(sourceUnitController);
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

        public virtual bool IsMet(UnitController sourceUnitController) {
            //Debug.Log("PrerequisiteConditions.IsMet()");
            bool returnValue = false;
            int prerequisiteCount = 0;
            int tempCount = 0;
            int falseCount = 0;

            foreach (List<IPrerequisite> prerequisiteList in allPrerequisites) {
                tempCount = 0;
                foreach (IPrerequisite prerequisite in prerequisiteList) {
                    prerequisiteCount++;
                    bool checkResult = prerequisite.IsMet(sourceUnitController);
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
        public void UpdatePrerequisites(UnitController sourceUnitController, bool notify = true) {
            foreach (List<IPrerequisite> prerequisiteList in allPrerequisites) {
                foreach (IPrerequisite prerequisite in prerequisiteList) {
                    prerequisite.UpdateStatus(sourceUnitController, notify);
                }
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
        }

        public void SetupScriptableObjects(SystemGameManager systemGameManager, IPrerequisiteOwner prerequisiteOwner) {
            //Debug.Log("PrerequisiteConditions.SetupScriptableObjects(" + (systemGameManager == null ? "null" : systemGameManager.gameObject.name) + ")");
            Configure(systemGameManager);
            CreateAllList();

            if (prerequisiteOwners.Count == 0) {
                foreach (List<IPrerequisite> prerequisiteList in allPrerequisites) {
                    foreach (IPrerequisite prerequisite in prerequisiteList) {
                        prerequisite.SetupScriptableObjects(systemGameManager, prerequisiteOwner.DisplayName);
                        prerequisite.OnStatusUpdated += HandlePrerequisiteUpdates;
                    }
                }
            }

            prerequisiteOwners.Add(prerequisiteOwner);
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
        }

    }

}