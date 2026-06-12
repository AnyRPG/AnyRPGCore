using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace AnyRPG {

    public class SceneData {
        public SceneInstanceType SceneInstanceType = SceneInstanceType.World;
        public Scene Scene;
        public SceneNode SceneNode = null;
        public bool HasNavMesh = false;
        public List<InteractableBase> Interactables = new List<InteractableBase>();
        public List<UnitController> UnitControllers = new List<UnitController>();
        public List<IPersistentObjectOwner> PersistentObjectOwners = new List<IPersistentObjectOwner>();
        public List<InteractableBase> DroppedItems = new List<InteractableBase>();

        // the time that this scene became empty of players
        // used for tracking instance unloading timeouts
        public DateTime EmptyTime = DateTime.MinValue;

        public int ClientCount = 0;
        
        public SceneData(SceneInstanceType sceneInstanceType, Scene scene, SceneNode sceneNode, bool hasNavMesh) {
            SceneInstanceType = sceneInstanceType;
            Scene = scene;
            EmptyTime = DateTime.Now;
            SceneNode = sceneNode;
            HasNavMesh = hasNavMesh;
        }

        public void RegisterInteractable(InteractableBase interactable) {
            if (!Interactables.Contains(interactable)) {
                Interactables.Add(interactable);
            }
        }

        public void UnregisterInteractable(InteractableBase interactable) {
            if (Interactables.Contains(interactable)) {
                Interactables.Remove(interactable);
            }
        }

        public void RegisterUnitController(UnitController unitController) {
            if (!UnitControllers.Contains(unitController)) {
                UnitControllers.Add(unitController);
            }
        }

        public void UnregisterUnitController(UnitController unitController) {
            if (UnitControllers.Contains(unitController)) {
                UnitControllers.Remove(unitController);
            }
        }

        public void RegisterPersistentObject(IPersistentObjectOwner persistentObjectOwner) {
            if (!PersistentObjectOwners.Contains(persistentObjectOwner)) {
                PersistentObjectOwners.Add(persistentObjectOwner);
            }
        }

        public void UnregisterPersistentObject(IPersistentObjectOwner persistentObjectOwner) {
            if (PersistentObjectOwners.Contains(persistentObjectOwner)) {
                PersistentObjectOwners.Remove(persistentObjectOwner);
            }
        }

        public void RegisterDroppedItem(InteractableBase interactable) {
            if (!DroppedItems.Contains(interactable)) {
                DroppedItems.Add(interactable);
            }
        }

        public void UnregisterDroppedItem(InteractableBase interactable) {
            if (DroppedItems.Contains(interactable)) {
                DroppedItems.Remove(interactable);
            }
        }
    }

}
