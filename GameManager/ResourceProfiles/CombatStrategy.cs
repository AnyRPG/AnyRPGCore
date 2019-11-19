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

            if (sourceCharacter != null && sourceCharacter.MyCharacterAbilityManager != null) {
                //Debug.Log(gameObject.name + ".AICombat.GetValidAttackAbility(): CHARACTER HAS ABILITY MANAGER");
                List<CombatStrategyNode> validPhaseNodes = GetValidPhaseNodes(sourceCharacter);

                foreach (CombatStrategyNode validPhaseNode in validPhaseNodes) {

                    validPhaseNode.StartPhase();

                    // ATTEMPT BUFF AND IMMEDIATELY RETURN ANY BUFF THAT NEEDS CASTING
                    foreach (string baseAbilityName in validPhaseNode.MyMaintainBuffs) {
                        BaseAbility baseAbility;
                        string usedBaseAbilityName = SystemResourceManager.prepareStringForMatch(baseAbilityName);
                        if (sourceCharacter.MyCharacterAbilityManager.HasAbility(usedBaseAbilityName)) {
                            baseAbility = sourceCharacter.MyCharacterAbilityManager.MyAbilityList[usedBaseAbilityName] as BaseAbility;
                            //Debug.Log(gameObject.name + ".AICombat.GetValidAttackAbility(): Checking ability: " + baseAbility.MyName);
                            //if (baseAbility.maxRange == 0 || Vector3.Distance(aiController.MyBaseCharacter.MyCharacterUnit.transform.position, aiController.MyTarget.transform.position) < baseAbility.maxRange) {
                            if (!sourceCharacter.MyCharacterStats.MyStatusEffects.ContainsKey(SystemResourceManager.prepareStringForMatch(baseAbility.abilityEffects[0].MyName)) && sourceCharacter.MyCharacterAbilityManager.CanCastAbility(baseAbility) && baseAbility.CanUseOn(sourceCharacter.MyCharacterUnit.gameObject, sourceCharacter as BaseCharacter)) {
                                //Debug.Log(gameObject.name + ".AICombat.GetValidAttackAbility(): ADDING AN ABILITY TO LIST");
                                return baseAbility;
                            }
                        }
                    }

                    // IF NO BUFF AVAILABLE, GET A LIST OF VALID ATTACKS
                    foreach (string baseAbilityName in validPhaseNode.MyAttackAbilities) {
                        BaseAbility baseAbility;
                        string usedBaseAbilityName = SystemResourceManager.prepareStringForMatch(baseAbilityName);
                        if (sourceCharacter.MyCharacterAbilityManager.HasAbility(usedBaseAbilityName)) {
                            baseAbility = sourceCharacter.MyCharacterAbilityManager.MyAbilityList[usedBaseAbilityName] as BaseAbility;
                            //Debug.Log(gameObject.name + ".AICombat.GetValidAttackAbility(): Checking ability: " + baseAbility.MyName);
                            //if (baseAbility.maxRange == 0 || Vector3.Distance(aiController.MyBaseCharacter.MyCharacterUnit.transform.position, aiController.MyTarget.transform.position) < baseAbility.maxRange) {
                            if (sourceCharacter.MyCharacterAbilityManager.CanCastAbility(baseAbility) && baseAbility.CanUseOn(sourceCharacter.MyCharacterController.MyTarget, sourceCharacter as BaseCharacter)) {
                                //Debug.Log(gameObject.name + ".AICombat.GetValidAttackAbility(): ADDING AN ABILITY TO LIST");
                                returnList.Add(baseAbility);
                            }
                        }
                    }

                }
            }
            if (returnList.Count > 0) {
                int randomIndex = Random.Range(0, returnList.Count);
                //Debug.Log(gameObject.name + ".AICombat.GetValidAttackAbility(): returnList.Count: " + returnList.Count + "; randomIndex: " + randomIndex);
                return returnList[randomIndex];
            }
            //Debug.Log(gameObject.name + ".AICombat.GetValidAttackAbility(): ABOUT TO RETURN NULL!");
            return null;

        }

        public List<CombatStrategyNode> GetValidPhaseNodes(BaseCharacter sourceCharacter) {
            List<CombatStrategyNode> returnList = new List<CombatStrategyNode>();
            foreach (CombatStrategyNode phaseNode in phaseNodes) {
                if (sourceCharacter != null && sourceCharacter.MyCharacterStats != null) {
                    if (Mathf.Ceil((sourceCharacter.MyCharacterStats.currentHealth / (float)sourceCharacter.MyCharacterStats.MyMaxHealth) * 100f) <= phaseNode.MyMaxHealthPercent && Mathf.Floor((sourceCharacter.MyCharacterStats.currentHealth / (float)sourceCharacter.MyCharacterStats.MyMaxHealth) * 100f) >= phaseNode.MyMinHealthPercent) {
                        //Debug.Log(sourceCharacter.MyName + ".GetValidPhaseNodes: currentHealth: " + sourceCharacter.MyCharacterStats.currentHealth + "; MaxHealth: " + sourceCharacter.MyCharacterStats.MyMaxHealth);
                        returnList.Add(phaseNode);
                    }
                }
            }
            return returnList;
        }
    }



}