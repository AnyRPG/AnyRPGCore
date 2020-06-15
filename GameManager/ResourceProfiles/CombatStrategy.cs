using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Combat Strategy", menuName = "AnyRPG/Combat/CombatStrategy")]
    [System.Serializable]
    public class CombatStrategy : DescribableResource {

        [SerializeField]
        private List<CombatStrategyNode> phaseNodes = new List<CombatStrategyNode>();

        public List<CombatStrategyNode> MyPhaseNodes { get => phaseNodes; set => phaseNodes = value; }

        public BaseAbility GetValidAbility(BaseCharacter sourceCharacter) {
            List<BaseAbility> returnList = new List<BaseAbility>();

            if (sourceCharacter != null && sourceCharacter.CharacterAbilityManager != null) {
                //Debug.Log(gameObject.name + ".AICombat.GetValidAttackAbility(): CHARACTER HAS ABILITY MANAGER");
                List<CombatStrategyNode> validPhaseNodes = GetValidPhaseNodes(sourceCharacter);

                foreach (CombatStrategyNode validPhaseNode in validPhaseNodes) {

                    validPhaseNode.StartPhase();

                    // ATTEMPT BUFF AND IMMEDIATELY RETURN ANY BUFF THAT NEEDS CASTING
                    foreach (BaseAbility baseAbility in validPhaseNode.MyMaintainBuffList) {
                        if (sourceCharacter.CharacterAbilityManager.HasAbility(baseAbility)) {
                            //Debug.Log(MyName + ".AICombat.GetValidAttackAbility(): Checking ability: " + baseAbility.MyName);
                            //if (baseAbility.maxRange == 0 || Vector3.Distance(aiController.MyBaseCharacter.MyCharacterUnit.transform.position, aiController.MyTarget.transform.position) < baseAbility.maxRange) {
                            if (!sourceCharacter.CharacterStats.StatusEffects.ContainsKey(SystemResourceManager.prepareStringForMatch(baseAbility.MyAbilityEffects[0].MyDisplayName))
                                && sourceCharacter.CharacterAbilityManager.CanCastAbility(baseAbility)
                                && baseAbility.CanUseOn(sourceCharacter.CharacterUnit.gameObject, sourceCharacter.CharacterAbilityManager)) {
                                //Debug.Log(MyName + ".AICombat.GetValidAbility(): ADDING A BUFF ABILITY TO LIST");
                                return baseAbility;
                            }
                        }
                    }

                    // IF NO BUFF AVAILABLE, GET A LIST OF VALID ATTACKS
                    foreach (BaseAbility baseAbility in validPhaseNode.MyAttackAbilityList) {
                        //Debug.Log(sourceCharacter.MyName + ".AICombat.GetValidAttackAbility(): Checking if ability known: " + usedBaseAbilityName);
                        if (sourceCharacter.CharacterAbilityManager.HasAbility(baseAbility)) {
                            //Debug.Log(sourceCharacter.MyName + ".AICombat.GetValidAttackAbility(): Checking ability: " + baseAbility.MyName);
                            //if (baseAbility.maxRange == 0 || Vector3.Distance(aiController.MyBaseCharacter.MyCharacterUnit.transform.position, aiController.MyTarget.transform.position) < baseAbility.maxRange) {
                            if (sourceCharacter.CharacterAbilityManager.CanCastAbility(baseAbility)
                                && baseAbility.CanUseOn(sourceCharacter.CharacterController.MyTarget, sourceCharacter.CharacterAbilityManager)
                                && sourceCharacter.CharacterAbilityManager.PerformLOSCheck(sourceCharacter.CharacterController.MyTarget, baseAbility)) {
                                //Debug.Log(sourceCharacter.MyName + ".AICombat.GetValidAttackAbility(): ADDING AN ABILITY TO LIST: " + baseAbility.MyName);
                                returnList.Add(baseAbility);
                            }
                        } else {
                            Debug.Log(sourceCharacter.MyName + ".AICombat.GetValidAttackAbility(): ABILITY NOT KNOWN: " + baseAbility.MyDisplayName);
                        }
                    }

                }
            }
            if (returnList.Count > 0) {
                int randomIndex = Random.Range(0, returnList.Count);
                //Debug.Log(sourceCharacter.MyName + ".AICombat.GetValidAttackAbility(): returnList.Count: " + returnList.Count + "; randomIndex: " + randomIndex);
                return returnList[randomIndex];
            }
            //Debug.Log(sourceCharacter.MyName + ".AICombat.GetValidAttackAbility(): ABOUT TO RETURN NULL!");
            return null;

        }

        public BaseAbility GetMeleeAbility(BaseCharacter sourceCharacter) {

            if (sourceCharacter != null && sourceCharacter.CharacterAbilityManager != null) {
                //Debug.Log(gameObject.name + ".AICombat.GetValidAttackAbility(): CHARACTER HAS ABILITY MANAGER");
                List<CombatStrategyNode> validPhaseNodes = GetValidPhaseNodes(sourceCharacter);

                foreach (CombatStrategyNode validPhaseNode in validPhaseNodes) {

                    validPhaseNode.StartPhase();

                    // IF NO BUFF AVAILABLE, GET A LIST OF VALID ATTACKS
                    foreach (BaseAbility baseAbility in validPhaseNode.MyAttackAbilityList) {
                        //Debug.Log(sourceCharacter.MyName + ".AICombat.GetValidAttackAbility(): Checking if ability known: " + usedBaseAbilityName);
                        if (sourceCharacter.CharacterAbilityManager.HasAbility(baseAbility)) {
                            //Debug.Log(sourceCharacter.MyName + ".AICombat.GetValidAttackAbility(): Checking ability: " + baseAbility.MyName);
                            //if (baseAbility.maxRange == 0 || Vector3.Distance(aiController.MyBaseCharacter.MyCharacterUnit.transform.position, aiController.MyTarget.transform.position) < baseAbility.maxRange) {
                            if (baseAbility.CanCastOnEnemy && baseAbility.UseMeleeRange == true) {
                                //Debug.Log(sourceCharacter.MyName + ".AICombat.GetValidAttackAbility(): ADDING AN ABILITY TO LIST: " + baseAbility.MyName);
                                return baseAbility;
                            }
                        } else {
                            Debug.Log(sourceCharacter.MyName + ".AICombat.GetValidAttackAbility(): ABILITY NOT KNOWN: " + baseAbility.MyDisplayName);
                        }
                    }

                }
            }
            //Debug.Log(sourceCharacter.MyName + ".AICombat.GetValidAttackAbility(): ABOUT TO RETURN NULL!");
            return null;

        }

        public List<BaseAbility> GetAttackRangeAbilityList(BaseCharacter sourceCharacter) {

            List<BaseAbility> returnList = new List<BaseAbility>();

            if (sourceCharacter != null && sourceCharacter.CharacterAbilityManager != null) {
                //Debug.Log(gameObject.name + ".AICombat.GetValidAttackAbility(): CHARACTER HAS ABILITY MANAGER");
                List<CombatStrategyNode> validPhaseNodes = GetValidPhaseNodes(sourceCharacter);

                foreach (CombatStrategyNode validPhaseNode in validPhaseNodes) {

                    validPhaseNode.StartPhase();

                    foreach (BaseAbility baseAbility in validPhaseNode.MyAttackAbilityList) {
                        if (sourceCharacter.CharacterAbilityManager.HasAbility(baseAbility)) {
                            returnList.Add(baseAbility);
                        } else {
                            Debug.Log(sourceCharacter.MyName + ".AICombat.GetValidAttackAbility(): ABILITY NOT KNOWN: " + baseAbility.MyDisplayName);
                        }
                    }
                }
            }
            return returnList;

        }

        public List<CombatStrategyNode> GetValidPhaseNodes(BaseCharacter sourceCharacter) {
            List<CombatStrategyNode> returnList = new List<CombatStrategyNode>();
            foreach (CombatStrategyNode phaseNode in phaseNodes) {
                if (sourceCharacter != null && sourceCharacter.CharacterStats != null) {
                    if (Mathf.Ceil((sourceCharacter.CharacterStats.CurrentPrimaryResource / (float)sourceCharacter.CharacterStats.MaxPrimaryResource) * 100f) <= phaseNode.MyMaxHealthPercent && Mathf.Floor((sourceCharacter.CharacterStats.CurrentPrimaryResource / (float)sourceCharacter.CharacterStats.MaxPrimaryResource) * 100f) >= phaseNode.MyMinHealthPercent) {
                        //Debug.Log(sourceCharacter.MyName + ".GetValidPhaseNodes: currentHealth: " + sourceCharacter.MyCharacterStats.currentHealth + "; MaxHealth: " + sourceCharacter.MyCharacterStats.MyMaxHealth);
                        returnList.Add(phaseNode);
                    }
                }
            }
            return returnList;
        }

        public bool HasMusic() {
            foreach (CombatStrategyNode phaseNode in phaseNodes) {
                if (phaseNode.MyPhaseMusicProfileName != null && phaseNode.MyPhaseMusicProfileName != string.Empty) {
                    return true;
                }
            }
            return false;
        }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();

            if (phaseNodes != null) {
                foreach (CombatStrategyNode combatStrategyNode in phaseNodes) {
                    if (combatStrategyNode != null) {
                        combatStrategyNode.SetupScriptableObjects();
                    }
                    
                }
            }
        }
    }

}