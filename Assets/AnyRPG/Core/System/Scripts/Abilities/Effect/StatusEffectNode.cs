using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class StatusEffectNode : ConfiguredClass {

        private StatusEffectProperties statusEffect = null;

        private Coroutine monitorCoroutine = null;

        private AbilityEffectContext abilityEffectContext = null;

        // the reference to the character stats this node sits on
        private CharacterStats characterStats = null;

        // track state
        private int currentStacks = 1;
        private float remainingDuration = 0f;

        // list of status effect nodes to send updates to so multiple effects panels and bars can access this
        private Dictionary<StatusEffectPanelController, StatusEffectNodeScript> statusTrackers = new Dictionary<StatusEffectPanelController, StatusEffectNodeScript>();

        // keep track of any spell effect prefabs associated with this status effect.
        private Dictionary<PrefabProfile, List<GameObject>> prefabObjects = new Dictionary<PrefabProfile, List<GameObject>>();

        // game manager references
        private ObjectPooler objectPooler = null;

        public StatusEffectProperties StatusEffect { get => statusEffect; set => statusEffect = value; }
        public Coroutine MyMonitorCoroutine { get => monitorCoroutine; set => monitorCoroutine = value; }
        public AbilityEffectContext AbilityEffectContext { get => abilityEffectContext; set => abilityEffectContext = value; }
        public Dictionary<PrefabProfile, List<GameObject>> PrefabObjects { get => prefabObjects; set => prefabObjects = value; }
        public int CurrentStacks { get => currentStacks; set => currentStacks = value; }
        public float RemainingDuration { get => remainingDuration; set => remainingDuration = value; }

        public StatusEffectNode(SystemGameManager systemGameManager) {
            Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            objectPooler = systemGameManager.ObjectPooler;
        }

        //public void Setup(CharacterStats characterStats, StatusEffect _statusEffect, Coroutine newCoroutine) {
        public void Setup(CharacterStats characterStats, StatusEffectProperties statusEffect, AbilityEffectContext abilityEffectContext) {
            this.characterStats = characterStats;
            this.statusEffect = statusEffect;
            this.abilityEffectContext = abilityEffectContext;
            //this.monitorCoroutine = newCoroutine;
        }

        public void ClearEffectPrefabs() {
            //Debug.Log($"StatusEffectNode.ClearEffectPrefabs(): {StatusEffect.DisplayName}");

            if (prefabObjects != null) {
                foreach (List<GameObject> gameObjectList in prefabObjects.Values) {
                    foreach (GameObject go in gameObjectList) {
                        //Debug.Log("StatusEffectNode.ClearEffectPrefabs() statusEffect: " + statusEffect.DisplayName + "; Destroy: " + go.name);
                        objectPooler.ReturnObjectToPool(go, StatusEffect.PrefabDestroyDelay);
                    }

                }
                prefabObjects.Clear();
            }
        }

        public void CancelStatusEffect() {
            //Debug.Log("StatusEffectNode.CancelStatusEffect(): " + StatusEffect.DisplayName);
            ClearEffectPrefabs();
            StatusEffect.CancelEffect(characterStats.BaseCharacter);
            characterStats.HandleStatusEffectRemoval(statusEffect);
            ClearNodeScripts();
        }

        public void ClearNodeScripts() {
            foreach (StatusEffectPanelController statusEffectPanelController in statusTrackers.Keys) {
                statusEffectPanelController.ClearStatusEffectNode(this);
            }
            statusTrackers.Clear();
        }

        public void AddStatusTracker(StatusEffectPanelController statusEffectPanelController, StatusEffectNodeScript statusEffectNodeScript) {
            //Debug.Log("StatusEffect.SetStatusNode()");
            if (statusTrackers.ContainsKey(statusEffectPanelController)) {
                statusTrackers[statusEffectPanelController] = statusEffectNodeScript;
            } else {
                statusTrackers.Add(statusEffectPanelController, statusEffectNodeScript);
            }
            UpdateStatusNode();
        }

        public void RemoveStatusTracker(StatusEffectPanelController statusEffectPanelController) {
            //Debug.Log("StatusEffect.SetStatusNode()");
            statusTrackers.Remove(statusEffectPanelController);
            UpdateStatusNode();
        }


        public void UpdateStatusNode() {
            //Debug.Log(GetInstanceID() + ".StatusEffect.UpdateStatusNode(): COUNT statusEffectNodeScript: " + statusEffectNodeScripts.Count);
            foreach (StatusEffectNodeScript statusEffectNodeScript in statusTrackers.Values) {
                //Debug.Log("StatusEffect.UpdateStatusNode(): got statuseffectnodescript");
                if (statusEffectNodeScript != null) {
                    string statusText = string.Empty;
                    string stackText = string.Empty;
                    if (currentStacks > 1) {
                        stackText = currentStacks.ToString();
                    }
                    if (statusEffect.LimitedDuration == true && statusEffect.ClassTrait == false) {
                        float printedDuration = (int)remainingDuration;
                        if (printedDuration < 60 && printedDuration >= 0) {
                            // less than 1 minute
                            statusText = ((int)printedDuration).ToString() + "s";
                        } else if (printedDuration < 3600) {
                            //less than 1 hour
                            statusText = ((int)(printedDuration / 60)).ToString() + "m";
                        } else if (printedDuration > 3600f) {
                            //greater than 1 hour
                            statusText = ((int)(printedDuration / 3600)).ToString() + "h";
                        }
                    }

                    // set updated values
                    if (statusEffectNodeScript.UseTimerText == true && statusText != string.Empty) {
                        if (statusEffectNodeScript.Timer != null) {
                            if (statusEffectNodeScript.Timer.isActiveAndEnabled == false) {
                                statusEffectNodeScript.Timer.gameObject.SetActive(true);
                            }
                            statusEffectNodeScript.Timer.text = statusText;
                        }
                    } else {
                        if (statusEffectNodeScript.Timer != null) {
                            statusEffectNodeScript.Timer.gameObject.SetActive(false);
                        }
                    }
                    if (statusEffectNodeScript.UseStackText == true) {
                        if (statusEffectNodeScript.StackCount.isActiveAndEnabled == false) {
                            statusEffectNodeScript.StackCount.gameObject.SetActive(true);
                        }
                        statusEffectNodeScript.StackCount.gameObject.SetActive(true);
                        statusEffectNodeScript.StackCount.text = stackText;
                    } else {
                        if (statusEffectNodeScript.StackCount != null) {
                            statusEffectNodeScript.StackCount.gameObject.SetActive(false);
                        }
                    }
                    if (statusEffect.Duration == 0f) {
                        statusEffectNodeScript.UpdateFillIcon(0f);
                    } else {
                        float usedFillAmount = remainingDuration / statusEffect.Duration;
                        statusEffectNodeScript.UpdateFillIcon(usedFillAmount);
                    }
                }
            }
        }

        public bool AddStack() {
            bool returnValue = false;
            if (currentStacks < statusEffect.MaxStacks) {
                currentStacks++;
                // refresh the duration
                returnValue = true;
            }
            if (statusEffect.RefreshableDuration) {
                SetRemainingDuration(statusEffect.Duration);
            }
            return returnValue;
        }

        public void SetRemainingDuration(float remainingDuration) {
            this.remainingDuration = remainingDuration;
        }

        public float GetRemainingDuration() {
            return remainingDuration;
        }


    }
}
