using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class ActionBarManager : MonoBehaviour {

        [SerializeField]
        private List<ActionBarController> actionBarControllers = new List<ActionBarController>();

        private bool abilityBarsPopulated = false;

        private ActionButton fromButton = null;

        protected bool eventSubscriptionsInitialized = false;

        public ActionButton MyFromButton { get => fromButton; set => fromButton = value; }
        public List<ActionBarController> MyActionBarControllers { get => actionBarControllers; set => actionBarControllers = value; }

        private void Awake() {
            //Debug.Log("ActionBarManager.Awake()");
        }

        private void Start() {
            //Debug.Log("ActionBarManager.Start()");
            AssociateActionBarKeyBinds();
            CreateEventSubscriptions();
        }

        private void CreateEventSubscriptions() {
            //Debug.Log("PlayerManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.MyInstance.OnPlayerConnectionDespawn += ClearActionBars;
                SystemEventManager.MyInstance.OnEquipmentChanged += HandleEquipmentChange;
            }
            eventSubscriptionsInitialized = true;
        }

        private void CleanupEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.MyInstance.OnPlayerConnectionDespawn -= ClearActionBars;
                SystemEventManager.MyInstance.OnEquipmentChanged -= HandleEquipmentChange;
            }
            eventSubscriptionsInitialized = false;
        }

        public void OnDisable() {
            //Debug.Log("PlayerManager.OnDisable()");
            CleanupEventSubscriptions();
        }

        public List<ActionButton> GetActionButtons() {
            //Debug.Log("ActionBarManager.GetActionButtons()");
            List<ActionButton> actionButtons = new List<ActionButton>();
            foreach (ActionBarController actionBarController in actionBarControllers) {
                actionButtons.AddRange(actionBarController.MyActionButtons);
            }
            return actionButtons;
        }

        private void AssociateActionBarKeyBinds() {
            //Debug.Log("ActionBarManager.AssociateActionBarKeyBinds()");
            int count = 1;
            foreach (ActionBarController actionBarController in actionBarControllers) {
                foreach (ActionButton actionButton in actionBarController.MyActionButtons) {
                    if (KeyBindManager.MyInstance.MyKeyBinds.Count >= count) {
                        //Debug.Log("ActionBarManager.AssociateActionBarKeyBinds(): associate count: " + count);
                        if (KeyBindManager.MyInstance.MyKeyBinds.ContainsKey("ACT" + count.ToString())) {
                            KeyBindManager.MyInstance.MyKeyBinds["ACT" + count.ToString()].MyActionButton = actionButton;
                            count++;
                        } else {
                            //Debug.Log("ActionBarManager.AssociateActionBarKeyBinds(): ran out of keybinds to associate with available action buttons!");
                            return;
                        }
                    } else {
                        //Debug.Log("ActionBarManager.AssociateActionBarKeyBinds(): ran out of keybinds to associate with available action buttons!");
                        return;
                    }
                }
            }
        }

        public bool AddNewAbility(BaseAbility newAbility) {
            //Debug.Log("ActionBarManager.AddNewAbility()");
            foreach (ActionBarController actionBarController in actionBarControllers) {
                //Debug.Log("ActionBarManager.AddNewAbility(): looping through a controller");
                if (actionBarController.AddNewAbility(newAbility)) {
                    //Debug.Log("ActionBarManager.AddNewAbility(): we were able to add " + newAbility.name);
                    return true;
                }
            }
            return false;
        }

        public void ClearActionBars() {
            //Debug.Log("ActionBarManager.AddNewAbility()");
            foreach (ActionBarController actionBarController in actionBarControllers) {
                //Debug.Log("ActionBarManager.AddNewAbility(): looping through a controller");
                actionBarController.ClearActionBar();
            }
        }


        /*
        public bool AbilityIsOnBars() {

        }
        */


        public void PopulateAbilityBars() {
            //Debug.Log("ActionBarmanager.PopulateAbilityBars()");
            if (abilityBarsPopulated) {
                //Debug.Log("ActionBarmanager.PopulateAbilityBars(): bars are already populated.  Doing nothing!");
                return;
            }
            // TODO: set maximum size of loop to less of abilitylist count or button count
            int abilityListCount = PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.MyAbilityList.Count;
            //Debug.Log("Updating ability bar with " + abilityListCount.ToString() + " abilities");
            foreach (BaseAbility newAbility in PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.MyAbilityList.Values) {
                AddNewAbility(newAbility);
            }
            abilityBarsPopulated = true;
        }

        public void UpdateVisuals(bool removeStaleActions = false) {
            foreach (ActionBarController actionBarController in actionBarControllers) {
                //Debug.Log("ActionBarManager.AddNewAbility(): looping through a controller");
                actionBarController.UpdateVisuals(removeStaleActions);
            }
        }

        public void HandleEquipmentChange(Equipment newEquipment, Equipment oldEquipment) {
            UpdateVisuals();
        }

    }

}