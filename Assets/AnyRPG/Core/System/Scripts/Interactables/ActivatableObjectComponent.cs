using UnityEngine;

namespace AnyRPG {
    public class ActivatableObjectComponent : InteractableOptionComponent {

        public ActivatableObjectProps Props { get => interactableOptionProps as ActivatableObjectProps; }

        //protected float currentTimer = 0f;

        //protected Coroutine spawnCoroutine = null;

        public override bool BlockTooltip {
            get {
                if (Props.SpawnObject == null) {
                    return false;
                }
                return (Props.SpawnObject.activeSelf == false);
            }
        }

        /*
        public override bool PrerequisitesMet(UnitController sourceUnitController) {
            bool returnResult = base.PrerequisitesMet(sourceUnitController);
            if (returnResult == false) {
                return returnResult;
            }
            if (spawnCoroutine != null) {
                return false;
            }
            if (Props.SpawnTimer == -1 && pickupCount > 0) {
                return false;
            }
            return returnResult;
        }
        */

        public ActivatableObjectComponent(Interactable interactable, ActivatableObjectProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
        }

        public override bool ProcessInteract(UnitController sourceUnitController, int componentIndex, int choiceIndex = 0) {
            //Debug.Log($"{gameObject.name}.LootableNode.Interact(" + source.name + ")");
            base.ProcessInteract(sourceUnitController, componentIndex, choiceIndex);

            if (Props.SpawnObject == null) {
                Debug.LogWarning($"{interactable.gameObject.name}.ActivatableObjectComponent.ProcessInteract(): no spawn object set, cannot activate");
                return false;
            }

            if (Props.SpawnObject.activeSelf == true) {
                Despawn();
            } else {
                Spawn();
            }

            return true;
        }

        /*
        protected IEnumerator StartSpawnCountdown() {
            //Debug.Log(interactable.gameObject.name + ".LootableNode.StartSpawnCountdown()");

            // DISABLE MINIMAP ICON WHILE ITEM IS NOT SPAWNED
            // this next line is already done outside the loop so should not be needed here ?
            //HandlePrerequisiteUpdates();

            currentTimer = Props.SpawnTimer;
            while (currentTimer > 0) {
                //Debug.Log("Spawn Timer: " + currentTimer);
                yield return new WaitForSeconds(1);
                currentTimer -= 1;
            }
            spawnCoroutine = null;

            //Debug.Log($"{gameObject.name}.LootableNode.StartSpawnCountdown(): countdown complete");
            Spawn();

            // ENABLE MINIMAP ICON AFTER SPAWN
            HandleOptionStateChange();
        }
        */

        public void Spawn() {
            if (Props.SpawnObject != null && Props.SpawnObject.activeSelf == false) {
                Props.SpawnObject.SetActive(true);
                interactable.InteractableEventController.NotifyOnLootableNodeSpawnObjectSetActive(true);
            }
        }

        public void Despawn() {
            //Debug.Log($"{interactable.gameObject.name}.LootableNode.Despawn()");

            if (Props.SpawnObject != null && Props.SpawnObject.activeSelf == true) {
                Props.SpawnObject.SetActive(false);
                interactable.InteractableEventController.NotifyOnLootableNodeSpawnObjectSetActive(false);
            }
        }

        public override void SetSaveData(InteractableSaveData interactableSaveData) {
            //Debug.Log($"{interactable.gameObject.name}.LootableNodeComponent.SetSaveData() lootDropped: {lootDropped} pickupCount: {pickupCount}");

            base.SetSaveData(interactableSaveData);
            ActivatableObjectSaveData activatableObjectSaveData = new ActivatableObjectSaveData() {
                SpawnObjectActive = (Props.SpawnObject != null ? Props.SpawnObject.activeSelf : false)
            };
            if (interactableSaveData.ActivatableObjectSaveData.Count == 0) {
                interactableSaveData.ActivatableObjectSaveData.Add(activatableObjectSaveData);
            } else {
                interactableSaveData.ActivatableObjectSaveData[0] = activatableObjectSaveData;
            }
        }

        public override void LoadFromSaveData(InteractableSaveData interactableSaveData) {
            //Debug.Log($"{interactable.gameObject.name}.LootableNodeComponent.LoadFromSaveData() lootDropped: {interactableSaveData.LootableNodeSaveData.LootDropped} SpawnObjectActive: {interactableSaveData.LootableNodeSaveData.SpawnObjectActive}");

            base.LoadFromSaveData(interactableSaveData);
            if (interactableSaveData.ActivatableObjectSaveData.Count == 0) {
                return;
            }
            if (Props.SpawnObject != null) {
                Props.SpawnObject.SetActive(interactableSaveData.ActivatableObjectSaveData[0].SpawnObjectActive);
            }
        }
    }

}