using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnyRPG {
    public class DroppedItemComponent : InteractableOptionComponent {


        private List<InstantiatedItem> instantiatedItems = new List<InstantiatedItem>();
        private GameObject spawnObject = null;

        // references
        private Rigidbody rigidbody = null;
        private BoxCollider boxCollider = null;

        // game manager references
        private ObjectPooler objectPooler = null;
        private LevelManagerClient levelManagerClient = null;
        private LevelManagerServer levelManagerServer = null;
        private SaveManager saveManager = null;
        private ServerDataService serverDataService = null;

        public DroppedItemProps Props { get => interactableOptionProps as DroppedItemProps; }

        public override bool BlockTooltip {
            get {
                if (Props.SpawnObject == null) {
                    return false;
                }
                return (Props.SpawnObject.activeSelf == false);
            }
        }

        public BoxCollider BoxCollider { get => boxCollider; set => boxCollider = value; }
        public Rigidbody Rigidbody { get => rigidbody; set => rigidbody = value; }

        public DroppedItemComponent(Interactable interactable, DroppedItemProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
        }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            rigidbody = interactable.GetComponent<Rigidbody>();
            boxCollider = interactable.GetComponent<BoxCollider>();
            if (rigidbody != null && systemGameManager.GameMode == GameMode.Network && networkManagerServer.ServerModeActive == false) {
                // disable gravity, this will be controlled by network transform
                rigidbody.useGravity = false;
                rigidbody.isKinematic = true;
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            objectPooler = systemGameManager.ObjectPooler;
            levelManagerClient = systemGameManager.LevelManagerClient;
            levelManagerServer = systemGameManager.LevelManagerServer;
            saveManager = systemGameManager.SaveManager;
            serverDataService = systemGameManager.ServerDataService;
        }

        /*
        public override bool PrerequisitesMet(UnitController sourceUnitController) {
            bool returnResult = base.PrerequisitesMet(sourceUnitController);
            if (returnResult == false) {
                return returnResult;
            }
            return returnResult;
        }
        */

        public override int GetValidOptionCount(UnitController sourceUnitController) {
            //Debug.Log(interactable.gameObject.name + ".ItemPickupComponent.GetValidOptionCount()");
            int returnValue = base.GetValidOptionCount(sourceUnitController);
            if (returnValue == 0) {
                return returnValue;
            }
            if (spawnObject == null) {
                return 0;
            }
            return returnValue;
        }

        public override bool ProcessInteract(UnitController sourceUnitController, int componentIndex, int choiceIndex = 0) {
            //Debug.Log($"{gameObject.name}.LootableNode.Interact(" + source.name + ")");
            base.ProcessInteract(sourceUnitController, componentIndex, choiceIndex);

            DropLoot(sourceUnitController);
            return true;
        }

        public void Spawn() {
            //Debug.Log($"{interactable.gameObject.name}.DroppedItemComponent.Spawn()");

            if (instantiatedItems.Count == 0) {
                Debug.LogWarning($"{interactable.gameObject.name}.DroppedItemComponent.Spawn() no items to spawn");
                return;
            }
            InstantiatedItem instantiatedItem = instantiatedItems[0];
            if (instantiatedItem.Item.ItemPickupPrefabProfile?.Prefab != null) {
                spawnObject = objectPooler.GetPooledObject(instantiatedItem.Item.ItemPickupPrefabProfile.Prefab,
                                                interactable.transform.TransformPoint(instantiatedItem.Item.ItemPickupPrefabProfile.PickupPosition),
                                                interactable.transform.rotation,
                                                interactable.transform);
            } else {
                Debug.LogWarning($"{interactable.gameObject.name}.DroppedItemComponent.Spawn() no prefab profile or prefab for item {instantiatedItem.Item.ResourceName}");
            }
            if (rigidbody != null) {
                // prevent zero mass objects
                rigidbody.mass = (instantiatedItem.Item.Weight > 0f ? instantiatedItem.Item.Weight : 0.1f);
                rigidbody.solverIterations = 10;
                rigidbody.solverVelocityIterations = 8;
                rigidbody.sleepThreshold = 0.1f;
            }
            if (spawnObject == null) {
                Debug.LogWarning($"{interactable.gameObject.name}.DroppedItemComponent.Spawn() no spawn object");
                return;
            }// else {
                //Debug.Log($"{interactable.gameObject.name}.DroppedItemComponent.Spawn() spawned object {spawnObject.name}");
            //}

            // since this object is a spawn, and was not active and initialization, populate the materials array for mousover outlining
            interactable.PopulateOriginalMaterials();

            // determine spawnObject mesh bounds and set the interactable's collider bounds to match
            // get the first active mesh filter in the object or its children and use that for the bounds
            MeshFilter meshFilter = spawnObject.GetComponentInChildren<MeshFilter>();
            if (meshFilter == null) return;
            Bounds localBounds = meshFilter.sharedMesh.bounds;

            //Debug.Log($"{interactable.gameObject.name}.DroppedItemComponent.Spawn() meshBoundsSize: {meshBounds.size}");
            if (boxCollider != null) {
                //boxCollider.center = interactable.transform.InverseTransformPoint(meshBounds.center);
                boxCollider.center = interactable.transform.InverseTransformPoint(
                        meshFilter.transform.TransformPoint(localBounds.center)
                    );

                // you must divide the world size by the world scale to get local size.
                Vector3 meshLocalScale = meshFilter.transform.localScale;
                boxCollider.size = new Vector3(
                    localBounds.size.x * meshLocalScale.x,
                    localBounds.size.y * meshLocalScale.y,
                    localBounds.size.z * meshLocalScale.z
                );
            }

            // spawn minimap icon
            HandleOptionStateChange();
        }

        public virtual void DropLoot(UnitController sourceUnitController) {
            //Debug.Log($"{interactable.gameObject.name}.DroppedItemComponent.DropLoot()");

            if (sourceUnitController.CharacterInventoryManager.EmptySlotCount() == 0) {
                return;
            }
            List<InstantiatedItem> itemsToAdd = new List<InstantiatedItem>(instantiatedItems);
            foreach (InstantiatedItem instantiatedItem in itemsToAdd) {
                if (sourceUnitController.CharacterInventoryManager.AddItem(instantiatedItem, false) == false) { 
                    break;
                } else {
                    instantiatedItems.Remove(instantiatedItem);
                }
            }
            if (instantiatedItems.Count == 0) {
                Despawn();
            }

        }

        private void DespawnSpawnObject() {
            //Debug.Log($"{interactable.gameObject.name}.DroppedItemComponent.DespawnSpawnObject()");

            if (spawnObject != null) {
                objectPooler.ReturnObjectToPool(spawnObject);
                spawnObject = null;
            }
        }

        public void Despawn() {
            //Debug.Log($"{interactable.gameObject.name}.DroppedItemComponent.Despawn()");

            if (systemGameManager.GameMode == GameMode.Local) {
                // remove the save data for this object so it doesn't respawn when returning to the scene
                SceneNode sceneNode = levelManagerClient.GetActiveSceneNode();
                string UUID = interactable.UUID?.ID;
                if (sceneNode != null && UUID != null) {
                    saveManager.RemoveEphemeralObject(UUID, sceneNode);
                }
            }
            interactable.ResetSettings();
            if (networkManagerServer.ServerModeActive == false) {
                objectPooler.ReturnObjectToPool(interactable.gameObject);
            } else {
                networkManagerServer.ReturnObjectToPool(interactable.gameObject);
            }
        }

        public override bool CanInteract(UnitController sourceUnitController, bool processRangeCheck, bool passedRangeCheck, bool processNonCombatCheck, bool viaSwitch = false) {
            //Debug.Log(interactable.gameObject.name + ".LootableNode.CanInteract()");

            bool returnValue = base.CanInteract(sourceUnitController, processRangeCheck, passedRangeCheck, processNonCombatCheck);
            if (returnValue == false) {
                return false;
            }
            if (spawnObject == null) {
                return false;
            }
            return (GetCurrentOptionCount(sourceUnitController) == 0 ? false : true);
        }

        public override void SetSaveData(InteractableSaveData interactableSaveData) {
            //Debug.Log($"{interactable.gameObject.name}.LootableNodeComponent.SetSaveData()");

            base.SetSaveData(interactableSaveData);
            DroppedItemSaveData droppedItemSaveData = new DroppedItemSaveData() {
                InstantiatedItemIds = instantiatedItems.Select(i => i.InstanceId).ToList()
            };
            if (interactableSaveData.DroppedItemSaveData.Count == 0) {
                interactableSaveData.DroppedItemSaveData.Add(droppedItemSaveData);
            } else {
                interactableSaveData.DroppedItemSaveData[0] = droppedItemSaveData;
            }
        }

        public override void LoadFromSaveData(InteractableSaveData interactableSaveData) {
            //Debug.Log($"{interactable.gameObject.name}.DroppedItemComponent.LoadFromSaveData()");

            base.LoadFromSaveData(interactableSaveData);
            if (interactableSaveData.DroppedItemSaveData.Count == 0) {
                //Debug.LogWarning($"{interactable.gameObject.name}.DroppedItemComponent.LoadFromSaveData() no dropped item save data found");
                return;
            }
            List<InstantiatedItem> instantiatedItemsToAdd = new List<InstantiatedItem>();
            foreach (long itemInstanceId in interactableSaveData.DroppedItemSaveData[0].InstantiatedItemIds) {
                InstantiatedItem instantiatedItem = systemItemManager.GetExistingInstantiatedItem(itemInstanceId);
                if (instantiatedItem != null) {
                    instantiatedItemsToAdd.Add(instantiatedItem);
                } else {
                    Debug.LogWarning($"Could not find instantiated item with id {itemInstanceId} for {interactable.gameObject.name}");
                }
            }
            SetDroppedItems(instantiatedItemsToAdd);
        }

        public void SetDroppedItems(List<InstantiatedItem> itemsToDrop) {
            //Debug.Log($"{interactable.gameObject.name}.DroppedItemComponent.SetDroppedItems() itemsToDrop.Count: {itemsToDrop.Count}");

            if (itemsToDrop.Count == 0) {
                Debug.LogWarning($"{interactable.gameObject.name}.DroppedItemComponent.LoadFromSaveData() no items to add, despawning!");
                Despawn();
                return;
            }

            instantiatedItems = itemsToDrop;
            interactable.DisplayName = itemsToDrop[0].DisplayName;
            if (itemsToDrop.Count > 1) {
                interactable.DisplayName += $" ({itemsToDrop.Count})";
            }
            Spawn();
            interactable.InteractableEventController.NotifyOnSetDroppedItems(itemsToDrop);
        }

        public override void Cleanup() {
            //Debug.Log($"{interactable.gameObject.name}.DroppedItemComponent.Cleanup()");

            base.Cleanup();
            DespawnSpawnObject();
            levelManagerServer.UnregisterDroppedItem(interactable);

            // in network mode, we need to delete any items that are still in the dropped item component when it is cleaned up
            // otherwise they will pollute the server with orphaned items that are no longer referenced by anything and will never be cleaned up
            if (instantiatedItems.Count > 0 && networkManagerServer.ServerModeActive == true) {
                //Debug.LogWarning($"{interactable.gameObject.name}.DroppedItemComponent.Cleanup() there are still {instantiatedItems.Count} items in the dropped item component, these will be lost!");
                foreach (InstantiatedItem instantiatedItem in instantiatedItems) {
                    serverDataService.DeleteItemInstance(instantiatedItem);
                }
                instantiatedItems.Clear();
            }
        }

        public override bool ResetOnStopNetwork() {
            return true;
        }
    }

}