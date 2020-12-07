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

        private List<StatusEffectNodeScript> statusEffectNodeScripts = new List<StatusEffectNodeScript>();

        public int MyEffectLimit { get => effectLimit; set => effectLimit = value; }

        public void SetTarget(UnitController unitController) {
            //Debug.Log("StatusEffectPanelController.SetTarget(" + characterUnit.MyDisplayName + ")");
            this.targetUnitController = unitController;
            if (targetUnitController.CharacterUnit.BaseCharacter != null && targetUnitController.CharacterUnit.BaseCharacter.CharacterStats != null) {
                //Debug.Log("StatusEffectPanelController.SetTarget(" + characterUnit.MyDisplayName + "): checking status effects");
                foreach (StatusEffectNode statusEffectNode in targetUnitController.CharacterUnit.BaseCharacter.CharacterStats.StatusEffects.Values) {
                    SpawnStatusNode(statusEffectNode, targetUnitController.CharacterUnit);
                }
                CreateEventSubscriptions();
            }
        }

        public void ClearTarget() {
            //Debug.Log("StatusEffectPanelController.ClearTarget()");

            // do this first or there will be no character to unsubscribe from
            CleanupEventSubscriptions();

            targetUnitController = null;
            foreach (StatusEffectNodeScript _statusEffectNodeScript in statusEffectNodeScripts) {
                if (_statusEffectNodeScript != null) {
                    Destroy(_statusEffectNodeScript.gameObject);
                }
            }
            statusEffectNodeScripts.Clear();
        }

        public void HandleStatusEffectAdd(StatusEffectNode statusEffectNode) {
            //Debug.Log("StatusEffectPanelController.HandleStatusEffectAdd(): character: " + (targetUnitController == null ? "null" : targetUnitController.DisplayName));
            SpawnStatusNode(statusEffectNode, targetUnitController.CharacterUnit);
        }

        public void CreateEventSubscriptions() {
            //Debug.Log("StatusEffectPanelController.CreateEventSubscriptions(): character: " + (targetCharacterUnit == null ? "null" : targetCharacterUnit.MyDisplayName));
            if (targetUnitController != null) {
                //Debug.Log("StatusEffectPanelController.CreateEventSubscriptions(): characterStats is not null.");
                targetUnitController.OnStatusEffectAdd += HandleStatusEffectAdd;
            }
        }

        public void CleanupEventSubscriptions() {
            //Debug.Log("StatusEffectPanelController.CleanupEventSubscriptions()");
            if (targetUnitController != null) {
                targetUnitController.OnStatusEffectAdd -= HandleStatusEffectAdd;
            }
        }

        public StatusEffectNodeScript SpawnStatusNode(StatusEffectNode statusEffectNode, CharacterUnit target) {
            //Debug.Log("StatusEffectPanelController.SpawnStatusNode()");

            // do not spawn visible icons for traits
            if (statusEffectNode.StatusEffect.ClassTrait == true) {
                return null;
            }

            // clear invalid entries before trying to check for total entries
            List<StatusEffectNodeScript> removeList = new List<StatusEffectNodeScript>();
            foreach (StatusEffectNodeScript _statusEffectNodeScript in statusEffectNodeScripts) {
                if (_statusEffectNodeScript == null) {
                    removeList.Add(_statusEffectNodeScript);
                }
            }
            foreach (StatusEffectNodeScript _statusEffectNodeScript in removeList) {
                statusEffectNodeScripts.Remove(_statusEffectNodeScript);
            }


            // prevent status effect bars on unit frames from printing too many effects
            if (effectLimit > 0 && statusEffectNodeScripts.Count >= effectLimit) {
                //Debug.Log("StatusEffectPanelController.SpawnStatusNode() Too many nodes(" + statusEffectNodeScripts.Count + "), can't spawn");
                return null;
            }

            // determine if a node with that status effect already exists
            GameObject statusNode = Instantiate(statusNodePrefab, transform);
            StatusEffectNodeScript statusEffectNodeScript = statusNode.GetComponent<StatusEffectNodeScript>();
            if (statusEffectNodeScript != null) {
                statusEffectNodeScript.Initialize(statusEffectNode, target);
            } else {
                //Debug.Log("StatusEffectPanelController.SpawnStatusNode(): statusEffectNodeScript is null!");
            }
            statusEffectNodeScripts.Add(statusEffectNodeScript);

            return statusEffectNodeScript;
        }

        public override void OnDisable() {
            base.OnDisable();
            CleanupEventSubscriptions();
        }
    }

}