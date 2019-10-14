using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : CharacterCombat {

    public override void Start() {
        //Debug.Log("PlayerCombat.Start()");
        base.Start();
        AttemptRegen();

        // allow the player controller to send us events whenever the player moves or presses an ability button or the escape key
    }

    protected override void CreateEventReferences() {
        //Debug.Log("PlayerCombat.CreateEventReferences()");
        if (eventReferencesInitialized || !startHasRun) {
            return;
        }
        base.CreateEventReferences();
        SystemEventManager.MyInstance.OnEquipmentChanged += OnEquipmentChanged;
        SystemEventManager.MyInstance.OnEquipmentRefresh += OnEquipmentChanged;
        if (baseCharacter != null && baseCharacter.MyCharacterStats != null) {
            baseCharacter.MyCharacterStats.OnHealthChanged += AttemptRegen;
            baseCharacter.MyCharacterStats.OnManaChanged += AttemptRegen;
        }
        eventReferencesInitialized = true;
    }

    protected override void CleanupEventReferences() {
        //Debug.Log("PlayerCombat.CleanupEventReferences()");
        if (!eventReferencesInitialized) {
            return;
        }
        base.CleanupEventReferences();
        if (SystemEventManager.MyInstance != null) {
            SystemEventManager.MyInstance.OnEquipmentChanged -= OnEquipmentChanged;
            SystemEventManager.MyInstance.OnEquipmentRefresh -= OnEquipmentChanged;
        }

        // that next code would have never been necessary because that handler was never set : TEST THAT ESCAPE CANCELS SPELLCASTING - THAT METHOD IS NEVER SET
        if (KeyBindManager.MyInstance != null && KeyBindManager.MyInstance.MyKeyBinds != null && KeyBindManager.MyInstance.MyKeyBinds.ContainsKey("CANCEL")) {
            KeyBindManager.MyInstance.MyKeyBinds["CANCEL"].OnKeyPressedHandler -= OnEscapeKeyPressedHandler;
        }
        
        if (baseCharacter != null && baseCharacter.MyCharacterStats != null) {
            baseCharacter.MyCharacterStats.OnHealthChanged -= AttemptRegen;
            baseCharacter.MyCharacterStats.OnManaChanged -= AttemptRegen;
        }
        eventReferencesInitialized = false;
    }

    public override void OnEnable() {
        //Debug.Log("PlayerCombat.OnEnable()");
        base.OnEnable();
        AttemptRegen();
    }

    public override void OnDisable() {
        //Debug.Log("PlayerManager.OnDisable()");
        base.OnDisable();
        AttemptStopRegen();
    }

    protected override void Update() {
        base.Update();

        // leave combat if the combat cooldown has expired
        if ((Time.time - lastCombatEvent > combatCooldown) && inCombat) {
            //Debug.Log(gameObject.name + " Leaving Combat");
            TryToDropCombat();
        }

        HandleAutoAttack();
    }

    public void HandleAutoAttack() {
        if (baseCharacter.MyCharacterController.MyTarget == null && autoAttackActive == true) {
            //Debug.Log(gameObject.name + ": HandleAutoAttack(): target is null.  deactivate autoattack");
            DeActivateAutoAttack();
            return;
        }

        if (autoAttackActive == true && baseCharacter.MyCharacterController.MyTarget != null) {
            //Debug.Log("player controller is in combat and target is not null");
            //Interactable _interactable = controller.MyTarget.GetComponent<Interactable>();
            BaseCharacter targetCharacter = baseCharacter.MyCharacterController.MyTarget.GetComponent<CharacterUnit>().MyCharacter;
            if (targetCharacter != null && AutoAttackTargetIsValid(targetCharacter)) {
                //Debug.Log("the target is alive.  Attacking");
                Attack(baseCharacter.MyCharacterController.MyTarget.GetComponent<CharacterUnit>().MyCharacter);
                return;
            }
            // autoattack is active, but we were unable to attack the target because they were dead, or not a lootable character, or didn't have an interactable.
            // There is no reason for autoattack to remain active under these circumstances
            //Debug.Log(gameObject.name + ": target is not attackable.  deactivate autoattack");
            DeActivateAutoAttack();
        }
    }

    public override bool EnterCombat(BaseCharacter target) {
        //Debug.Log(gameObject.name + ".PlayerCombat.EnterCombat(" + (target != null && target.MyCharacterName != null ? target.MyCharacterName : "null") + ")");
        if (baseCharacter.MyCharacterStats.IsAlive == false) {
            Debug.Log("Player is dead but was asked to enter combat!!!");
            return false;
        }
        AttemptStopRegen();

        // If we do not have a focus, set the target as the focus
        if (baseCharacter.MyCharacterController != null) {
            if (baseCharacter.MyCharacterController.MyTarget == null) {
                baseCharacter.MyCharacterController.SetTarget(target.MyCharacterUnit.gameObject);
            }
        }

        if (base.EnterCombat(target)) {
            if (CombatLogUI.MyInstance != null) {
                CombatLogUI.MyInstance.WriteCombatMessage("Entered combat with " + (target.MyCharacterName != null ? target.MyCharacterName : target.name));
            }
            return true;
        }

        return false;
    }

    public override bool TakeDamage(int damage, Vector3 sourcePosition, BaseCharacter source, CombatType combatType, CombatMagnitude combatMagnitude, string abilityName) {
        //Debug.Log("PlayerCombat.TakeDamage(" + damage + ", " + source.name + ")");
        // enter combat first because if we die from this hit, we don't want to enter combat when dead
        EnterCombat(source);
        // added damageTaken bool to prevent blood effects from showing if you ran out of range of the attack while it was in progress
        bool damageTaken = base.TakeDamage(damage, sourcePosition, source, combatType, combatMagnitude, abilityName);
        if (onHitAbility == null && SystemConfigurationManager.MyInstance.MyTakeDamageAbility != null && damageTaken) {
            MyBaseCharacter.MyCharacterAbilityManager.BeginAbility(SystemConfigurationManager.MyInstance.MyTakeDamageAbility, MyBaseCharacter.MyCharacterUnit.gameObject);
        }
        return damageTaken;
    }

    /// <summary>
    /// Stop casting if the escape key is pressed
    /// </summary>
    public void OnEscapeKeyPressedHandler() {
        //Debug.Log("Received Escape Key Pressed Handler");
        baseCharacter.MyCharacterAbilityManager.StopCasting();

    }

    public void OnEquipmentChanged(Equipment newItem) {
        OnEquipmentChanged(newItem, null);
    }


    public void OnEquipmentChanged(Equipment newItem, Equipment oldItem) {
        onHitAbility = null;
        if (newItem != null) {
            //Debug.Log(gameObject.name + "Equipping " + newItem.name);
            if (newItem.equipSlot == EquipmentSlot.MainHand) {
                //Debug.Log(newItem.name + " is a weapon.");
                overrideHitSoundEffect = null;
                defaultHitSoundEffect = null;
                if (newItem is Weapon && (newItem as Weapon).OnHitAbility != null) {
                    //Debug.Log("New item is a weapon and has the on hit ability " + (newItem as Weapon).OnHitAbility.name);
                    onHitAbility = (newItem as Weapon).OnHitAbility;
                }
                if (newItem is Weapon && (newItem as Weapon).MyDefaultHitSoundEffect != null) {
                    //Debug.Log("New item is a weapon and has the on hit ability " + (newItem as Weapon).OnHitAbility.name);
                    overrideHitSoundEffect = (newItem as Weapon).MyDefaultHitSoundEffect;
                    defaultHitSoundEffect = (newItem as Weapon).MyDefaultHitSoundEffect;
                }
            }
        }
        AttemptRegen();
    }

    public override void OnKillConfirmed(BaseCharacter sourceCharacter, float creditPercent) {
        //Debug.Log("PlayerCombat.OnKillConfirmed()");
        base.OnKillConfirmed(sourceCharacter, creditPercent);
    }


    public override void TryToDropCombat() {
        //Debug.Log("PlayerCombat.TryToDropCombat()");
        base.TryToDropCombat();
    }

    protected override void DropCombat() {
        //Debug.Log("PlayerCombat.DropCombat()");
        if (!inCombat) {
            return;
        }
        base.DropCombat();
        AttemptRegen();
        CombatLogUI.MyInstance.WriteCombatMessage("Left combat");
    }

    public override void BroadcastCharacterDeath() {
        //Debug.Log("PlayerCombat.BroadcastCharacterDeath()");
        base.BroadcastCharacterDeath();
        if (!baseCharacter.MyCharacterStats.IsAlive) {
            //Debug.Log("PlayerCombat.BroadcastCharacterDeath(): not alive, attempt stop regen");
            AttemptStopRegen();
        }
    }

    public override bool AttackHit_AnimationEvent() {
        //Debug.Log(gameObject.name + ".PlayerCombat.AttackHit_AnimationEvent()");
        if (onHitAbility == null && SystemConfigurationManager.MyInstance.MyDoWhiteDamageAbility != null && MyBaseCharacter.MyCharacterController.MyTarget != null) {
            // TESTING, THIS WAS MESSING WITH ABILITIES THAT DONT' NEED A TARGET LIKE GROUND SLAM - OR NOT, ITS JUST FOR THE WHITE HIT...!!
            //Debug.Log(gameObject.name + ".PlayerCombat.AttackHit_AnimationEvent(): onHitAbility is not null");
            MyBaseCharacter.MyCharacterAbilityManager.BeginAbility(SystemConfigurationManager.MyInstance.MyDoWhiteDamageAbility, MyBaseCharacter.MyCharacterController.MyTarget);
        }
        bool attackSucceeded = base.AttackHit_AnimationEvent();
        if (attackSucceeded) {

            if (overrideHitSoundEffect == null && defaultHitSoundEffect == null) {
                AudioManager.MyInstance.PlayEffect(SystemConfigurationManager.MyInstance.MyDefaultHitSoundEffect);
            } else if (overrideHitSoundEffect == null && defaultHitSoundEffect != null) {
                // do nothing sound was suppressed for special attack sound
            } else if (overrideHitSoundEffect != null) {
                AudioManager.MyInstance.PlayEffect(overrideHitSoundEffect);
            }

            // reset back to default if possible now that an attack has hit
            if (overrideHitSoundEffect == null && defaultHitSoundEffect != null) {
                overrideHitSoundEffect = defaultHitSoundEffect;
            }
        }
        return attackSucceeded;
    }


}
