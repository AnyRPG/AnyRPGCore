using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : BaseCharacter {

    private PlayerFactionManager playerFactionManager;
    private PlayerCurrencyManager playerCurrencyManager;

    new public PlayerController MyCharacterController { get => characterController as PlayerController; }
    new public PlayerStats MyCharacterStats { get => characterStats as PlayerStats; }
    new public PlayerAbilityManager MyCharacterAbilityManager { get => characterAbilityManager as PlayerAbilityManager; }
    new public PlayerCombat MyCharacterCombat { get => characterCombat as PlayerCombat; }
    public PlayerFactionManager MyPlayerFactionManager { get => playerFactionManager; set => playerFactionManager = value; }
    public PlayerCurrencyManager MyPlayerCurrencyManager { get => playerCurrencyManager; set => playerCurrencyManager = value; }

    protected override void Awake() {
        //Debug.Log(gameObject.name + ".PlayerCharcter.Awake()");
        base.Awake();
        
        characterController = GetComponent<PlayerController>();
        if (characterController == null) {
            Debug.Log(gameObject.name + ".PlayerCharcter.Awake(): characterController is null!");
        }
        characterStats = GetComponent<PlayerStats>();
        if (characterStats == null) {
            Debug.Log(gameObject.name + ".PlayerCharcter.Awake(): characterStats is null!");
        }
        characterAbilityManager = GetComponent<PlayerAbilityManager>();
        if (characterAbilityManager == null) {
            Debug.Log(gameObject.name + ".PlayerCharcter.Awake(): characterAbilityManager is null!");
        }
        characterCombat = GetComponent<PlayerCombat>();
        if (characterCombat == null) {
            Debug.Log(gameObject.name + ".PlayerCharcter.Awake(): characterCombat is null!");
        }
        playerFactionManager = GetComponent<PlayerFactionManager>();
        if (playerFactionManager == null) {
            Debug.Log(gameObject.name + ".PlayerCharcter.Awake(): playerFactionManager is null!");
        }
        playerCurrencyManager = GetComponent<PlayerCurrencyManager>();
        if (playerCurrencyManager == null) {
            Debug.Log(gameObject.name + ".PlayerCharcter.Awake(): playerCurrencyManager is null!");
        }

    }

    public override void SetCharacterFaction(string newFaction) {
        //Debug.Log(gameObject.name + ".PlayerCharacter.SetCharacterFaction(" + newFaction + ")");
        base.SetCharacterFaction(newFaction);

        if (newFaction != null && newFaction != string.Empty) {
            factionName = newFaction;
            playerFactionManager.SetReputation(newFaction);
        }
    }

    public void JoinFaction(string newFaction) {
        //Debug.Log(gameObject.name + ".PlayerCharacter.Joinfaction(" + newFaction + ")");
        if (newFaction != null && newFaction != string.Empty && SystemResourceManager.MatchResource(newFaction, PlayerManager.MyInstance.MyCharacter.MyFactionName) == false) {
            SetCharacterFaction(newFaction);
            LearnFactionAbilities(newFaction);
        }
    }

    public void LearnFactionAbilities(string newFaction) {
        //Debug.Log(gameObject.name + ".PlayerCharacter.LearnFactionAbilities(" + newFaction + ")");
        foreach (string abilityName in SystemFactionManager.MyInstance.GetResource(newFaction).MyLearnedAbilityList) {
            //Debug.Log(gameObject.name + ".PlayerCharacter.LearnFactionAbilities(" + newFaction + "); ability name: " + abilityName);
            if (SystemAbilityManager.MyInstance.GetResource(abilityName).MyRequiredLevel <= PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyLevel && PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.HasAbility(abilityName) == false) {
                //Debug.Log(gameObject.name + ".PlayerCharacter.LearnFactionAbilities(" + newFaction + "); ability name: " + abilityName + " is not learned yet, LEARNING!");
                PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.LearnAbility(abilityName);
            } else {
                //Debug.Log(gameObject.name + ".PlayerCharacter.LearnFactionAbilities(" + newFaction + "); ability name: " + abilityName + "; level: " + SystemAbilityManager.MyInstance.GetResource(abilityName).MyRequiredLevel + "; playerlevel: " + PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyLevel + "; hasability: " + (PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.HasAbility(abilityName)));
            }
        }
    }

}
