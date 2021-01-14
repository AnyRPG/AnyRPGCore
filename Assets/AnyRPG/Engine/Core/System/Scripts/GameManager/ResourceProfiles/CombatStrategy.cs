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

        [Header("Combat Strategy")]

        [SerializeField]
        private List<CombatStrategyNode> phaseNodes = new List<CombatStrategyNode>();

        public List<CombatStrategyNode> MyPhaseNodes { get => phaseNodes; set => phaseNodes = value; }

        public BaseAbility GetValidAbility(BaseCharacter sourceCharacter) {
            List<BaseAbility> returnList = new List<BaseAbility>();

            if (sourceCharacter != null && sourceCharacter.AbilityManager != null) {
                //Debug.Log(gameObject.name + ".AICombat.GetValidAttackAbility(): CHARACTER HAS ABILITY MANAGER");
                List<CombatStrategyNode> validPhaseNodes = GetValidPhaseNodes(sourceCharacter);

                foreach (CombatStrategyNode validPhaseNode in validPhaseNodes) {

                    AttempStartPhase(sourceCharacter, validPhaseNode);

                    // ATTEMPT BUFF AND IMMEDIATELY RETURN ANY BUFF THAT NEEDS CASTING
                    foreach (BaseAbility baseAbility in validPhaseNode.MyMaintainBuffList) {
                        if (sourceCharacter.AbilityManager.HasAbility(baseAbility)) {
                            //Debug.Log(MyName + ".AICombat.GetValidAttackAbility(): Checking ability: " + baseAbility.MyName);
                            //if (baseAbility.maxRange == 0 || Vector3.Distance(aiController.MyBaseCharacter.MyCharacterUnit.transform.position, aiController.MyTarget.transform.position) < baseAbility.maxRange) {
                            if (!sourceCharacter.CharacterStats.StatusEffects.ContainsKey(SystemResourceManager.prepareStringForMatch(baseAbility.GetAbilityEffects(sourceCharacter)[0].DisplayName))
                                && sourceCharacter.AbilityManager.CanCastAbility(baseAbility)
                                && baseAbility.CanUseOn(sourceCharacter.UnitController, sourceCharacter)) {
                                //Debug.Log(MyName + ".AICombat.GetValidAbility(): ADDING A BUFF ABILITY TO LIST");
                                return baseAbility;
                            }
                        }
                    }

                    // IF NO BUFF AVAILABLE, GET A LIST OF VALID ATTACKS
                    foreach (BaseAbility baseAbility in validPhaseNode.MyAttackAbilityList) {
                        //Debug.Log(sourceCharacter.UnitController.gameObject.name + ".CombatStrategy.GetValidAttackAbility(): Checking if ability known: " + baseAbility.DisplayName);
                        if (sourceCharacter.AbilityManager.HasAbility(baseAbility)) {
                            //Debug.Log(sourceCharacter.UnitController.gameObject.name + ".AICombat.GetValidAttackAbility(): Checking ability: " + baseAbility.DisplayName);
                            //if (baseAbility.maxRange == 0 || Vector3.Distance(aiController.MyBaseCharacter.MyCharacterUnit.transform.position, aiController.MyTarget.transform.position) < baseAbility.maxRange) {
                            if (sourceCharacter.AbilityManager.CanCastAbility(baseAbility)
                                && baseAbility.CanUseOn(sourceCharacter.UnitController.Target, sourceCharacter)
                                && sourceCharacter.AbilityManager.PerformLOSCheck(sourceCharacter.UnitController.Target, baseAbility)) {
                                //Debug.Log(sourceCharacter.UnitController.gameObject.name + ".AICombat.GetValidAttackAbility(): ADDING AN ABILITY TO LIST: " + baseAbility.DisplayName);
                                returnList.Add(baseAbility);
                            }
                        } else {
                            //Debug.Log("CombatStrategy.GetValidAttackAbility(): ABILITY NOT KNOWN: " + baseAbility.DisplayName);
                        }
                    }

                }
            }
            if (returnList.Count > 0) {
                int randomIndex = Random.Range(0, returnList.Count);
                //Debug.Log(sourceCharacter.AbilityManager.MyName + ".AICombat.GetValidAttackAbility(): returnList.Count: " + returnList.Count + "; randomIndex: " + randomIndex);
                return returnList[randomIndex];
            }
            //Debug.Log(sourceCharacter.UnitController.gameObject.name + ".CombatStrategy.GetValidAttackAbility(): ABOUT TO RETURN NULL!");
            return null;

        }

        public BaseAbility GetMeleeAbility(BaseCharacter sourceCharacter) {

            if (sourceCharacter != null && sourceCharacter.AbilityManager != null) {
                //Debug.Log(gameObject.name + ".AICombat.GetValidAttackAbility(): CHARACTER HAS ABILITY MANAGER");
                List<CombatStrategyNode> validPhaseNodes = GetValidPhaseNodes(sourceCharacter);

                foreach (CombatStrategyNode validPhaseNode in validPhaseNodes) {

                    AttempStartPhase(sourceCharacter, validPhaseNode);

                    // IF NO BUFF AVAILABLE, GET A LIST OF VALID ATTACKS
                    foreach (BaseAbility baseAbility in validPhaseNode.MyAttackAbilityList) {
                        //Debug.Log(sourceCharacter.AbilityManager.MyName + ".AICombat.GetValidAttackAbility(): Checking if ability known: " + usedBaseAbilityName);
                        if (sourceCharacter.AbilityManager.HasAbility(baseAbility)) {
                            //Debug.Log(sourceCharacter.AbilityManager.MyName + ".AICombat.GetValidAttackAbility(): Checking ability: " + baseAbility.MyName);
                            //if (baseAbility.maxRange == 0 || Vector3.Distance(aiController.MyBaseCharacter.MyCharacterUnit.transform.position, aiController.MyTarget.transform.position) < baseAbility.maxRange) {
                            if (baseAbility.GetTargetOptions(sourceCharacter).CanCastOnEnemy && baseAbility.GetTargetOptions(sourceCharacter).UseMeleeRange == true) {
                                //Debug.Log(sourceCharacter.AbilityManager.MyName + ".AICombat.GetValidAttackAbility(): ADDING AN ABILITY TO LIST: " + baseAbility.MyName);
                                return baseAbility;
                            }
                        } else {
                            Debug.Log("CombatStrategy.GetValidAttackAbility(): ABILITY NOT KNOWN: " + baseAbility.DisplayName);
                        }
                    }

                }
            }
            //Debug.Log(sourceCharacter.AbilityManager.MyName + ".AICombat.GetValidAttackAbility(): ABOUT TO RETURN NULL!");
            return null;

        }

        public void AttempStartPhase(BaseCharacter sourceCharacter, CombatStrategyNode validPhaseNode) {
            if ((sourceCharacter.UnitController as UnitController).StartCombatPhase(validPhaseNode)) {
                validPhaseNode.StartPhase();
            }
        }

        public List<BaseAbility> GetAttackRangeAbilityList(BaseCharacter sourceCharacter) {
            //Debug.Log(sourceCharacter.gameObject.name + ".CombatStrategy.GetAttackRangeAbilityList()");

            List<BaseAbility> returnList = new List<BaseAbility>();

            if (sourceCharacter != null && sourceCharacter.AbilityManager != null) {
                //Debug.Log(gameObject.name + ".AICombat.GetValidAttackAbility(): CHARACTER HAS ABILITY MANAGER");
                List<CombatStrategyNode> validPhaseNodes = GetValidPhaseNodes(sourceCharacter);

                foreach (CombatStrategyNode validPhaseNode in validPhaseNodes) {

                    AttempStartPhase(sourceCharacter, validPhaseNode);

                    foreach (BaseAbility baseAbility in validPhaseNode.MyAttackAbilityList) {
                        if (sourceCharacter.AbilityManager.HasAbility(baseAbility)) {
                            returnList.Add(baseAbility);
                        } else {
                            Debug.Log("CombatStrategy.GetValidAttackAbility(): ABILITY NOT KNOWN: " + baseAbility.DisplayName);
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
                        //Debug.Log(sourceCharacter.AbilityManager.MyName + ".GetValidPhaseNodes: currentHealth: " + sourceCharacter.AbilityManager.MyCharacterStats.currentHealth + "; MaxHealth: " + sourceCharacter.AbilityManager.MyCharacterStats.MyMaxHealth);
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