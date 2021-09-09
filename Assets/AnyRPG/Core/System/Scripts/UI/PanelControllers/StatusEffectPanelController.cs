using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class StatusEffectPanelController : DraggableWindow {
        [SerializeField]
        private GameObject statusNodePrefab = null;

        // how many effects can be shown before not showing anymore.  needed for small controllers like the ones on nameplates
        [SerializeField]
        private int effectLimit = 0;

        private UnitController targetUnitController = null;

        private Dictionary<StatusEffectNode, StatusEffectNodeScript> statusEffectNodes = new Dictionary<StatusEffectNode, StatusEffectNodeScript>();

        public int EffectLimit { get => effectLimit; set => effectLimit = value; }

        // game manager references
        ObjectPooler objectPooler = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            objectPooler = systemGameManager.ObjectPooler;
        }

        public void SetTarget(UnitController unitController) {
            //Debug.Log(gameObject.name + "StatusEffectPanelController.SetTarget(" + unitController.DisplayName + ")");
            this.targetUnitController = unitController;
            if (targetUnitController.CharacterUnit.BaseCharacter != null && targetUnitController.CharacterUnit.BaseCharacter.CharacterStats != null) {
                //Debug.Log("StatusEffectPanelController.SetTarget(" + characterUnit.MyDisplayName + "): checking status effects");
                foreach (StatusEffectNode statusEffectNode in targetUnitController.CharacterUnit.BaseCharacter.CharacterStats.StatusEffects.Values) {
                    AddStatusNode(statusEffectNode);
                }
                CreateEventSubscriptions();
            }
        }

        public void ClearTarget() {
            //Debug.Log(gameObject.name + ".StatusEffectPanelController.ClearTarget()");

            // do this first or there will be no character to unsubscribe from
            CleanupEventSubscriptions();

            targetUnitController = null;
            List<StatusEffectNode> removeList = new List<StatusEffectNode>();
            removeList.AddRange(statusEffectNodes.Keys);
            foreach (StatusEffectNode statusEffectNode in removeList) {
                if (statusEffectNodes[statusEffectNode] != null) {
                    statusEffectNode.RemoveStatusTracker(this);
                    objectPooler.ReturnObjectToPool(statusEffectNodes[statusEffectNode].gameObject);
                }
            }
            statusEffectNodes.Clear();
        }

        public void HandleStatusEffectAdd(StatusEffectNode statusEffectNode) {
            //Debug.Log(gameObject.name + ".StatusEffectPanelController.HandleStatusEffectAdd(): character: " + (targetUnitController == null ? "null" : targetUnitController.DisplayName));
            
            AddStatusNode(statusEffectNode);
        }

        public void CreateEventSubscriptions() {
            //Debug.Log("StatusEffectPanelController.CreateEventSubscriptions(): character: " + (targetCharacterUnit == null ? "null" : targetCharacterUnit.MyDisplayName));
            if (targetUnitController != null) {
                //Debug.Log("StatusEffectPanelController.CreateEventSubscriptions(): characterStats is not null.");
                targetUnitController.OnStatusEffectAdd += HandleStatusEffectAdd;
            }
        }

        public void CleanupEventSubscriptions() {
            //Debug.Log(gameObject.name + ".StatusEffectPanelController.CleanupEventSubscriptions()");
            if (targetUnitController != null) {
                targetUnitController.OnStatusEffectAdd -= HandleStatusEffectAdd;
            }
        }

        public void ClearStatusEffectNode(StatusEffectNode statusEffectNode) {
            //Debug.Log(gameObject.name + ".StatusEffectPanelController.ClearStatusEffectNode()");
            if (statusEffectNodes.ContainsKey(statusEffectNode)) {
                if (statusEffectNodes[statusEffectNode] != null) {
                    objectPooler.ReturnObjectToPool(statusEffectNodes[statusEffectNode].gameObject);
                }
                statusEffectNodes.Remove(statusEffectNode);
                if (effectLimit > 0 && GetStatusEffectNodeScriptCount() < effectLimit) {
                    //Debug.Log("StatusEffectPanelController.SpawnStatusNode() Too many nodes(" + statusEffectNodeScripts.Count + "), can't spawn");
                    foreach (StatusEffectNode _statusEffectNode in statusEffectNodes.Keys) {
                        if (statusEffectNodes[_statusEffectNode] == null) {
                            SpawnStatusNode(_statusEffectNode);
                            break;
                        }
                    }
                }
            } else {
                Debug.Log(gameObject.name + ".StatusEffectPanelController.ClearStatusEffectNodeScript() received a clear request from a node script that was not in the list.  How did this happne?");
            }
        }

        public void AddStatusNode(StatusEffectNode statusEffectNode) {
            // add node to ensure it's tracked, even if script can't be created due to size limit
            if (statusEffectNode.StatusEffect.ClassTrait == true) {
                return;
            }
            statusEffectNodes.Add(statusEffectNode, null);
            SpawnStatusNode(statusEffectNode);
        }

        public StatusEffectNodeScript SpawnStatusNode(StatusEffectNode statusEffectNode) {
            //Debug.Log(gameObject.name + ".StatusEffectPanelController.SpawnStatusNode()");

            // do not spawn visible icons for traits
            if (statusEffectNode.StatusEffect.ClassTrait == true) {
                return null;
            }

            // clear invalid entries before trying to check for total entries
            // this should not be necessary because nodes are pooled and properly remove themselves now
            /*
            List<StatusEffectNodeScript> removeList = new List<StatusEffectNodeScript>();
            foreach (StatusEffectNodeScript _statusEffectNodeScript in statusEffectNodes) {
                if (_statusEffectNodeScript == null) {
                    removeList.Add(_statusEffectNodeScript);
                    Debug.Log("StatusEffectPanelController.SpawnStatusNode() a status effect node script was null.  This should not happen because scripts should clear themselves on disable");
                } else if (_statusEffectNodeScript.gameObject.activeSelf == false) {
                    removeList.Add(_statusEffectNodeScript);
                    Debug.Log("StatusEffectPanelController.SpawnStatusNode() a status effect node script was inactive.  This should not happen because scripts should clear themselves on disable");
                }
            }
            foreach (StatusEffectNodeScript _statusEffectNodeScript in removeList) {
                statusEffectNodes.Remove(_statusEffectNodeScript);
            }
            */

            // prevent status effect bars on unit frames from printing too many effects
            
            if (effectLimit > 0 && GetStatusEffectNodeScriptCount() >= effectLimit) {
                //Debug.Log(gameObject.name + ".StatusEffectPanelController.SpawnStatusNode() Too many nodes(" + statusEffectNodes.Count + "), can't spawn");
                statusEffectNode.AddStatusTracker(this, null);
                return null;
            }

            // determine if a node with that status effect already exists
            GameObject statusNode = objectPooler.GetPooledObject(statusNodePrefab, transform);
            StatusEffectNodeScript statusEffectNodeScript = statusNode.GetComponent<StatusEffectNodeScript>();
            if (statusEffectNodeScript != null) {
                statusEffectNodes[statusEffectNode] = statusEffectNodeScript;
                statusEffectNodeScript.Initialize(statusEffectNode, targetUnitController.CharacterUnit, systemGameManager);
                statusEffectNode.AddStatusTracker(this, statusEffectNodeScript);
            } else {
                //Debug.Log("StatusEffectPanelController.SpawnStatusNode(): statusEffectNodeScript is null!");
            }

            return statusEffectNodeScript;
        }


        public int GetStatusEffectNodeScriptCount() {
            int statusEffectNodeScriptCount = 0;
            foreach (StatusEffectNodeScript _statusEffectNodeScript in statusEffectNodes.Values) {
                if (_statusEffectNodeScript != null) {
                    statusEffectNodeScriptCount++;
                }
            }
            return statusEffectNodeScriptCount;
        }

        public override void OnDisable() {
            //Debug.Log(gameObject.name + ".StatusEffectPanelController.OnDisable()");
            if (SystemGameManager.IsShuttingDown) {
                return;
            }
            base.OnDisable();
            CleanupEventSubscriptions();
        }

        public override void OnEnable() {
            //Debug.Log(gameObject.name + ".StatusEffectPanelController.OnEnable()");
            base.OnEnable();
            CreateEventSubscriptions();
        }
    }

}