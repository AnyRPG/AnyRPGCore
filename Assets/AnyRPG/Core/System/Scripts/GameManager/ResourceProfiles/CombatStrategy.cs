using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Combat Strategy", menuName = "AnyRPG/CombatStrategy")]
    [System.Serializable]
    public class CombatStrategy : DescribableResource {

        [Header("Combat Strategy")]

        [SerializeField]
        private List<CombatStrategyNode> phaseNodes = new List<CombatStrategyNode>();

        public List<CombatStrategyNode> PhaseNodes { get => phaseNodes; set => phaseNodes = value; }

        public BaseAbilityProperties GetValidAbility(BaseCharacter sourceCharacter) {
            List<BaseAbilityProperties> returnList = new List<BaseAbilityProperties>();

            if (sourceCharacter != null && sourceCharacter.AbilityManager != null) {
                //Debug.Log(gameObject.name + ".AICombat.GetValidAttackAbility(): CHARACTER HAS ABILITY MANAGER");
                List<CombatStrategyNode> validPhaseNodes = GetValidPhaseNodes(sourceCharacter);

                foreach (CombatStrategyNode validPhaseNode in validPhaseNodes) {

                    AttempStartPhase(sourceCharacter, validPhaseNode);

                    // ATTEMPT BUFF AND IMMEDIATELY RETURN ANY BUFF THAT NEEDS CASTING
                    foreach (BaseAbilityProperties baseAbility in validPhaseNode.MaintainBuffList) {
                        if (sourceCharacter.AbilityManager.HasAbility(baseAbility)) {
                            if (!sourceCharacter.CharacterStats.StatusEffects.ContainsKey(SystemDataFactory.PrepareStringForMatch(baseAbility.GetAbilityEffects(sourceCharacter)[0].DisplayName))
                                && sourceCharacter.AbilityManager.CanCastAbility(baseAbility)
                                && baseAbility.CanUseOn(sourceCharacter.UnitController, sourceCharacter)) {
                                return baseAbility;
                            }
                        }
                    }

                    // IF NO BUFF AVAILABLE, GET A LIST OF VALID ATTACKS
                    foreach (BaseAbilityProperties baseAbility in validPhaseNode.AttackAbilityList) {
                        if (sourceCharacter.AbilityManager.HasAbility(baseAbility)) {
                            if (sourceCharacter.AbilityManager.CanCastAbility(baseAbility)
                                && baseAbility.CanUseOn(sourceCharacter.UnitController.Target, sourceCharacter)
                                && sourceCharacter.AbilityManager.PerformLOSCheck(sourceCharacter.UnitController.Target, baseAbility)) {
                                returnList.Add(baseAbility);
                            }
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

        public BaseAbilityProperties GetMeleeAbility(BaseCharacter sourceCharacter) {

            if (sourceCharacter != null && sourceCharacter.AbilityManager != null) {
                //Debug.Log(gameObject.name + ".AICombat.GetValidAttackAbility(): CHARACTER HAS ABILITY MANAGER");
                List<CombatStrategyNode> validPhaseNodes = GetValidPhaseNodes(sourceCharacter);

                foreach (CombatStrategyNode validPhaseNode in validPhaseNodes) {

                    AttempStartPhase(sourceCharacter, validPhaseNode);

                    // IF NO BUFF AVAILABLE, GET A LIST OF VALID ATTACKS
                    foreach (BaseAbilityProperties baseAbility in validPhaseNode.AttackAbilityList) {
                        if (sourceCharacter.AbilityManager.HasAbility(baseAbility)) {
                            if (baseAbility.GetTargetOptions(sourceCharacter).CanCastOnEnemy && baseAbility.GetTargetOptions(sourceCharacter).UseMeleeRange == true) {
                                return baseAbility;
                            }
                        } else {
                            Debug.Log("CombatStrategy.GetValidAttackAbility(): ABILITY NOT KNOWN: " + baseAbility.DisplayName);
                        }
                    }

                }
            }
            return null;

        }

        public void AttempStartPhase(BaseCharacter sourceCharacter, CombatStrategyNode validPhaseNode) {
            if ((sourceCharacter.UnitController as UnitController).StartCombatPhase(validPhaseNode)) {
                validPhaseNode.StartPhase();
            }
        }

        public List<BaseAbilityProperties> GetAttackRangeAbilityList(BaseCharacter sourceCharacter) {
            //Debug.Log(sourceCharacter.gameObject.name + ".CombatStrategy.GetAttackRangeAbilityList()");

            List<BaseAbilityProperties> returnList = new List<BaseAbilityProperties>();

            if (sourceCharacter != null && sourceCharacter.AbilityManager != null) {
                //Debug.Log(gameObject.name + ".AICombat.GetValidAttackAbility(): CHARACTER HAS ABILITY MANAGER");
                List<CombatStrategyNode> validPhaseNodes = GetValidPhaseNodes(sourceCharacter);

                foreach (CombatStrategyNode validPhaseNode in validPhaseNodes) {

                    AttempStartPhase(sourceCharacter, validPhaseNode);

                    foreach (BaseAbilityProperties baseAbility in validPhaseNode.AttackAbilityList) {
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
                    if (Mathf.Ceil((sourceCharacter.CharacterStats.CurrentPrimaryResource / (float)sourceCharacter.CharacterStats.MaxPrimaryResource) * 100f) <= phaseNode.MaxHealthPercent
                        && Mathf.Floor((sourceCharacter.CharacterStats.CurrentPrimaryResource / (float)sourceCharacter.CharacterStats.MaxPrimaryResource) * 100f) >= phaseNode.MinHealthPercent) {
                        returnList.Add(phaseNode);
                    }
                }
            }
            return returnList;
        }

        public bool HasMusic() {
            foreach (CombatStrategyNode phaseNode in phaseNodes) {
                if (phaseNode.PhaseMusicProfileName != null && phaseNode.PhaseMusicProfileName != string.Empty) {
                    return true;
                }
            }
            return false;
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            if (phaseNodes != null) {
                foreach (CombatStrategyNode combatStrategyNode in phaseNodes) {
                    if (combatStrategyNode != null) {
                        combatStrategyNode.SetupScriptableObjects(systemGameManager);
                    }
                    
                }
            }
        }
    }

}