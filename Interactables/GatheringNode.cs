using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GatheringNode : InteractableOption
{
    public override event Action<IInteractable> MiniMapStatusUpdateHandler;

    /// <summary>
    /// The ability to cast in order to mine this node
    /// </summary>
    [SerializeField]
    private BaseAbility ability;

    private BaseAbility realAbility;

    [SerializeField]
    private GatherLootTable lootTable;

    [SerializeField]
    private float spawnTimer = 5f;

    private float currentTimer = 0f;

    public BaseAbility MyAbility { get => realAbility; }

    protected override void Awake() {
        base.Awake();
        realAbility = SystemAbilityManager.MyInstance.GetResource(ability.MyName);
    }

    public override bool Interact(CharacterUnit source) {
        if (lootTable == null) {
            Debug.Log(gameObject.name + ".GatheringNode.Interact(" + source.name + "): lootTable was null!");
            return true;
        }
        if (lootTable.MyDroppedItems.Count > 0) {
            Gather();
        } else {
            source.GetComponent<CharacterUnit>().MyCharacter.MyCharacterAbilityManager.BeginAbility(MyAbility);
        }
        PopupWindowManager.MyInstance.interactionWindow.CloseWindow();
        return true;
        //return PickUp();
    }

    private IEnumerator StartSpawnCountdown() {
        currentTimer = spawnTimer;
        while (currentTimer > 0) {
            //Debug.Log("Spawn Timer: " + currentTimer);
            currentTimer -= 1;
            yield return new WaitForSeconds(1);
        }
        interactable.Spawn();
    }

    public void Gather() {
        PickUp();
    }

    /// <summary>
    /// Pick an item up off the ground and put it in the inventory
    /// </summary>
    void PickUp () {
        Debug.Log("GatheringNode.Pickup()");
        LootUI.MyInstance.CreatePages(lootTable.GetLoot());
        CreateEventReferences();
        PopupWindowManager.MyInstance.lootWindow.OpenWindow();
    }

    public void ClearTakeLootHandler(ICloseableWindowContents windowContents) {
        CleanupEventReferences();
    }

    public void CreateEventReferences() {
        //Debug.Log("GatheringNode.CreateEventReferences()");
        if (eventReferencesInitialized || !startHasRun) {
            return;
        }
        SystemEventManager.MyInstance.OnTakeLoot += CheckDropListSize;
        LootUI.MyInstance.OnCloseWindowHandler += ClearTakeLootHandler;
        eventReferencesInitialized = true;
    }

    public override void CleanupEventReferences() {
        //Debug.Log("GatheringNode.CleanupEventReferences()");
        if (!eventReferencesInitialized) {
            return;
        }
        if (SystemEventManager.MyInstance != null) {
            SystemEventManager.MyInstance.OnTakeLoot -= CheckDropListSize;
            LootUI.MyInstance.OnCloseWindowHandler -= ClearTakeLootHandler;
        }
        eventReferencesInitialized = false;
    }

    public override void OnDisable() {
        base.OnDisable();
        CleanupEventReferences();
        StopAllCoroutines();
    }

    public void CheckDropListSize() {
        //Debug.Log("GatheringNode.CheckDropListSize()");
        if (lootTable.MyDroppedItems.Count == 0) {
            PlayerManager.MyInstance.MyCharacter.MyCharacterController.RemoveInteractable(gameObject.GetComponent<Interactable>());
            interactable.DestroySpawn();
            lootTable.Reset();
            StartCoroutine(StartSpawnCountdown());
        }
    }

    public override void StopInteract() {
        base.StopInteract();

        PopupWindowManager.MyInstance.lootWindow.CloseWindow();
    }

    public override bool HasMiniMapText() {
        return true;
    }

    public override bool SetMiniMapText(Text text) {
        if (!base.SetMiniMapText(text)) {
            text.text = "";
            text.color = new Color32(0, 0, 0, 0);
            return false;
        }
        text.text = "o";
        text.fontSize = 50;
        text.color = Color.blue;
        return true;
    }

    public override int GetCurrentOptionCount() {
        // overwrite me or everything is valid as long as prerequisites are met, which isn't the case for things like dialog, which have multiple options
        //Debug.Log(gameObject.name + ".CharacterCreatorInteractable.GetCurrentOptionCount()");
        return (PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.HasAbility(MyAbility.MyName) == true ? 1 : 0);
    }


}
