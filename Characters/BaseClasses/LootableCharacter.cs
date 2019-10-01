using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Currently enemy is a subclass of interactable to inherit right click and move toward functionality.
/// This may be better implemented as an interface in the future.
/// </summary>
public class LootableCharacter : InteractableOption {

    public override event Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

    [SerializeField]
    private LootTable lootTable;

    private CharacterUnit characterUnit;

    public LootTable MyLootTable { get => lootTable; }
    public CharacterUnit MyCharacterUnit { get => characterUnit; set => characterUnit = value; }
    public override Sprite MyIcon { get => (SystemConfigurationManager.MyInstance.MyLootableCharacterInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MyLootableCharacterInteractionPanelImage : base.MyIcon); }
    public override Sprite MyNamePlateImage { get => (SystemConfigurationManager.MyInstance.MyLootableCharacterNamePlateImage != null ? SystemConfigurationManager.MyInstance.MyLootableCharacterNamePlateImage : base.MyNamePlateImage); }

    protected override void Awake() {
        base.Awake();
        characterUnit = GetComponent<CharacterUnit>();
    }

    protected override void Start() {
        base.Start();
        CreateEventReferences();
    }

    private void CreateEventReferences() {
        //Debug.Log("PlayerManager.CreateEventReferences()");
        if (eventReferencesInitialized || !startHasRun) {
            return;
        }
        characterUnit.MyCharacter.MyCharacterStats.BeforeDie += HandleDeath;
        characterUnit.MyCharacter.MyCharacterStats.OnReviveComplete += HandleRevive;
        eventReferencesInitialized = true;
    }

    public override void CleanupEventReferences() {
        //Debug.Log(gameObject.name + ".LootableCharacter.CleanupEventReferences()");
        base.CleanupEventReferences();
        if (characterUnit != null && characterUnit.MyCharacter != null && characterUnit.MyCharacter.MyCharacterStats != null) {
            characterUnit.MyCharacter.MyCharacterStats.BeforeDie -= HandleDeath;
            characterUnit.MyCharacter.MyCharacterStats.OnReviveComplete -= HandleRevive;
        }
        if (SystemEventManager.MyInstance != null) {
            SystemEventManager.MyInstance.OnTakeLoot -= TryToDespawn;
        }
    }

    public override void OnDisable() {
        //Debug.Log(gameObject.name + ".LootableCharacter.OnDisable()");
        base.OnDisable();
        CleanupEventReferences();
    }

    public void HandleDeath(CharacterStats characterStats) {
        //Debug.Log(gameObject.name + "LootableCharacter.HandleDeath()");
        if (PlayerManager.MyInstance == null) {
            // game is exiting
            return;
        }
        if (MyLootTable != null && characterStats.MyBaseCharacter.MyCharacterCombat.MyAggroTable.AggroTableContains(PlayerManager.MyInstance.MyCharacter.MyCharacterUnit)) {
            //Debug.Log(gameObject.name + "LootableCharacter.HandleDeath(): MyLootTable != null.  Getting loot");
            MyLootTable.GetLoot();
            if (MyLootTable.MyDroppedItems == null) {
                //Debug.Log(gameObject.name + "LootableCharacter.HandleDeath(): LootTable.droppedItems is null!!!!");
            } else if (MyLootTable.MyDroppedItems.Count > 0) {
                //Debug.Log(gameObject.name + "LootableCharacter.HandleDeath(): Loot count: " + MyLootTable.MyDroppedItems.Count + "; performing loot sparkle");

                PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.BeginAbility(SystemConfigurationManager.MyInstance.MyLootSparkleAbility as IAbility, gameObject);
            }
        } else {
            if (!characterStats.MyBaseCharacter.MyCharacterCombat.MyAggroTable.AggroTableContains(PlayerManager.MyInstance.MyCharacter.MyCharacterUnit)) {
                //Debug.Log(gameObject.name + ".LootableCharacter.HandleDeath(): Player not in agro table, no reason to drop loot.");
            }
            //Debug.Log(gameObject.name + ".LootableCharacter.HandleDeath(): MyLootTable == null. can't drop loot");
        }
        TryToDespawn();
    }

    public void TryToDespawn() {

        //Debug.Log(gameObject.name + ".LootableCharacter.TryToDespawn()");
        if (MyCharacterUnit.MyCharacter.MyCharacterStats.IsAlive == true) {
            //Debug.Log("LootableCharacter.TryToDespawn(): Character is alive.  Returning and doing nothing.");
            return;
        }
        if (MyLootTable == null) {
            //Debug.Log(gameObject.name + ".LootableCharacter.TryToDespawn(): loot table was null, despawning");
            Despawn();
            return;
        }
        if (MyLootTable.MyDroppedItems.Count == 0) {
            //Debug.Log(gameObject.name + ".LootableCharacter.TryToDespawn(): loot table had no dropped items, despawning");
            SystemEventManager.MyInstance.OnTakeLoot -= TryToDespawn;
            Despawn();
        }

    }

    public override bool CanInteract(CharacterUnit source) {
        //Debug.Log(gameObject.name + ".LootableCharacter.canInteract(" + (source == null ? "null" : source.MyName) + ")");
        if (base.CanInteract(source) == false) {
            //Debug.Log(gameObject.name + ".LootableCharacter.canInteract(): base.caninteract failed");
            return false;
        }
        
        if (MyCharacterUnit.MyCharacter.MyCharacterStats.IsAlive == false && lootTable != null) {
            //Debug.Log(gameObject.name + ".LootableCharacter.canInteract(): isalive: false lootTable: " + lootTable.MyDroppedItems.Count);
            return true;
        }
        return false;
    }

    public List<GameObject> GetLootableTargets() {
        Vector3 aoeSpawnCenter = MyCharacterUnit.transform.position;
        Collider[] colliders = new Collider[0];
        colliders = Physics.OverlapSphere(aoeSpawnCenter, 15f);
        //Debug.Log("AOEEffect.Cast(): Casting OverlapSphere with radius: " + aoeRadius);
        List<GameObject> validTargets = new List<GameObject>();
        foreach (Collider collider in colliders) {
            if (collider.gameObject.GetComponent<LootableCharacter>() != null) {
                validTargets.Add(collider.gameObject);
            }
        }
        return validTargets;
    }

    public override bool Interact(CharacterUnit source) {
        //Debug.Log(gameObject.name + ".LootableCharacter.Interact()");
        PopupWindowManager.MyInstance.interactionWindow.CloseWindow();
        if (!characterUnit.MyCharacter.MyCharacterStats.IsAlive) {
            //Debug.Log(gameObject.name + ".LootableCharacter.Interact(): Character is dead.  Showing Loot Window on interaction");
            List<LootDrop> drops = new List<LootDrop>();
            foreach (GameObject interactable in GetLootableTargets()) {
                LootableCharacter lootableCharacter = interactable.GetComponent<LootableCharacter>();
                if (lootableCharacter != null) {
                    CharacterStats characterStats = interactable.GetComponent<CharacterUnit>().MyCharacter.MyCharacterStats as CharacterStats;
                    if (characterStats != null && characterStats.IsAlive == false && lootableCharacter.MyLootTable != null) {
                        //Debug.Log("Adding drops to loot table from: " + lootableCharacter.gameObject.name);
                        drops.AddRange(lootableCharacter.MyLootTable.GetLoot());
                        lootableCharacter.MonitorLootTable();
                    }
                }
            }
            // that will ignore the current character because he will have been removed from the interactables list by death
            // this should take care of that situation
            //drops.AddRange(MyLootTable.GetLoot());
            // don't need anymore because of spherecast, not interactables

            if (drops.Count > 0) {
                //Debug.Log(gameObject.name + ".LootableCharacter.drops.Count: " + drops.Count);
                LootUI.MyInstance.CreatePages(drops);
                //Debug.Log(gameObject.name + ".LootableCharacter.Interact(): about to open window");
                PopupWindowManager.MyInstance.lootWindow.OpenWindow();
                return true;
            } else {
                //Debug.Log(gameObject.name + ".LootableCharacter.drops.Count: " + drops.Count);
                return false;
            }
        }
        return false;
    }

    public void MonitorLootTable() {
        SystemEventManager.MyInstance.OnTakeLoot += TryToDespawn;
    }

    public void ClearTakeLootHandler(ICloseableWindowContents windowContents) {
        ClearTakeLootHandler();
    }

    public void ClearTakeLootHandler() {
        //Debug.Log(gameObject.name + ".LootableCharacter.ClearTakeLoothandler(): MyLootTable.MyDroppedItems.Count" + MyLootTable.MyDroppedItems.Count);
        TryToDespawn();
    }


    public override void StopInteract() {
        //Debug.Log(gameObject.name + ".LootableCharacter.StopInteract()");
        base.StopInteract();
        PopupWindowManager.MyInstance.lootWindow.CloseWindow();
        TryToDespawn();
    }

    public override bool HasMiniMapText() {
        return false;
    }

    public override bool SetMiniMapText(Text text) {
        if (!base.SetMiniMapText(text)) {
            text.text = "";
            text.color = new Color32(0, 0, 0, 0);
            return false;
        }
        text.text = "o";
        text.color = Color.gray;
        text.fontSize = 50;
        return true;
    }

    public void Despawn() {
        //Debug.Log(gameObject.name + ".LootableCharacter.Despawn()");
        //gameObject.SetActive(false);
        MyCharacterUnit.Despawn();
    }

    public override int GetValidOptionCount() {
        return (MyCharacterUnit.MyCharacter.MyCharacterStats.IsAlive == false ? 1 : 0);
    }

    public override int GetCurrentOptionCount() {
        return ((GetValidOptionCount() == 1 && MyLootTable.MyDroppedItems.Count > 0) ? 1 : 0);
    }

    public void HandleRevive() {
        ClearLootTable();
    }

    public void ClearLootTable() {
        lootTable.HandleRevive();
    }

    public override void HandlePrerequisiteUpdates() {
        base.HandlePrerequisiteUpdates();
        MiniMapStatusUpdateHandler(this);
    }
}
