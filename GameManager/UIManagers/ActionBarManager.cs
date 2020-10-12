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

        private Coroutine targetRangeRoutine = null;

        // the action bar target for range checks
        private GameObject target = null;

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
                SystemEventManager.StartListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
                SystemEventManager.MyInstance.OnPlayerUnitDespawn += HandlePlayerUnitDespawn;
                SystemEventManager.MyInstance.OnPlayerConnectionDespawn += HandlePlayerConnectionDespawn;
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
                SystemEventManager.StopListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
                SystemEventManager.MyInstance.OnPlayerUnitDespawn -= HandlePlayerUnitDespawn;
                SystemEventManager.MyInstance.OnPlayerConnectionDespawn -= HandlePlayerConnectionDespawn;
                SystemEventManager.MyInstance.OnEquipmentChanged -= HandleEquipmentChange;
            }
            eventSubscriptionsInitialized = false;
        }

        public void HandlePlayerConnectionDespawn() {
            ClearActionBars(true);
        }

        public void HandlePlayerUnitSpawn(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log(gameObject.name + ".InanimateUnit.HandlePlayerUnitSpawn()");
            ProcessPlayerUnitSpawn();
        }


        public void OnDisable() {
            //Debug.Log("PlayerManager.OnDisable()");
            CleanupEventSubscriptions();
        }

        public void ProcessPlayerUnitSpawn() {
            //Debug.Log("ActionBarmanager.HandlePlayerUnitSpawn()");
            PlayerManager.MyInstance.MyCharacter.CharacterController.OnSetTarget += HandleSetTarget;
            PlayerManager.MyInstance.MyCharacter.CharacterController.OnClearTarget += HandleClearTarget;
        }

        public void HandlePlayerUnitDespawn() {
            //Debug.Log("ActionBarmanager.HandlePlayerUnitDespawn()");

            // this needs to be called manually here because if the character controller processes the player unit despawn after us, we will miss the event
            HandleClearTarget();

            PlayerManager.MyInstance.MyCharacter.CharacterController.OnSetTarget -= HandleSetTarget;
            PlayerManager.MyInstance.MyCharacter.CharacterController.OnClearTarget -= HandleClearTarget;
        }

        public void HandleSetTarget(GameObject target) {
            //Debug.Log("ActionBarmanager.HandleSetTarget()");
            this.target = target;
            if (targetRangeRoutine == null) {
                targetRangeRoutine = StartCoroutine(UpdateTargetRange());
            }
        }

        public void HandleClearTarget() {
            //Debug.Log("ActionBarmanager.HandleClearTarget()");
            if (targetRangeRoutine != null) {
                StopCoroutine(targetRangeRoutine);
                targetRangeRoutine = null;
            }
            ResetRangeColors();
            target = null;
        }

        public bool HasTarget() {
            return (target != null);
        }

        public void ResetRangeColors() {
            //Debug.Log("ActionBarmanager.ResetRangeColors()");
            foreach (ActionButton actionButton in GetActionButtons()) {
                if (actionButton.KeyBindText.color != Color.white) {
                    actionButton.KeyBindText.color = Color.white;
                }
            }
        }

        public IEnumerator UpdateTargetRange() {
            //Debug.Log("ActionBarmanager.UpdateTargetRange()");
            //float distanceToTarget = 0f;
            bool inRange = false;
            while (HasTarget()) {
                if (PlayerManager.MyInstance.MyCharacter == null || PlayerManager.MyInstance.MyCharacter.AnimatedUnit == null) {
                    break;
                }
                //Debug.Log("ActionBarmanager.UpdateTargetRange(): still have target at distance: " + distanceToTarget);
                foreach (ActionButton actionButton in GetActionButtons()) {
                    if ((actionButton.Useable as BaseAbility) is BaseAbility) {
                        BaseAbility baseAbility = actionButton.Useable as BaseAbility;
                        //Debug.Log("ActionBarmanager.UpdateTargetRange(): actionbutton: " + baseAbility.MyName);

                        GameObject finalTarget = baseAbility.ReturnTarget(PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager, target, false);
                        //distanceToTarget = Vector3.Distance(PlayerManager.MyInstance.MyCharacter.AnimatedUnit.transform.position, target.transform.position);
                        inRange = false;
                        if (finalTarget != null) {
                            inRange = PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.IsTargetInAbilityRange(baseAbility, finalTarget);
                        }
                        /*
                        if (baseAbility.UseMeleeRange == true) {
                            if (PlayerManager.MyInstance.MyCharacter.CharacterUnit.HitBoxSize < distanceToTarget) {
                                // red text
                                inRange = false;
                            } else {
                                // white text
                                inRange = true;
                            }
                            //Debug.Log("ActionBarmanager.UpdateTargetRange(): melee " + baseAbility.MyName + "; distance: " + distanceToTarget + "; maxrange: " + baseAbility.MyMaxRange + "; inrange: " + inRange);
                        } else {
                            if (baseAbility.MaxRange > 0 && distanceToTarget > baseAbility.MaxRange) {
                                // set text color to red
                                inRange = false;
                            } else {
                                // set the text color to white
                                inRange = true;
                            }
                            //Debug.Log("ActionBarmanager.UpdateTargetRange(): " + baseAbility.MyName + "; distance: " + distanceToTarget + "; maxrange: " + baseAbility.MyMaxRange + "; inrange: " + inRange);
                        }
                        */
                        if (inRange) {
                            if (actionButton.KeyBindText.color != Color.white) {
                                actionButton.KeyBindText.color = Color.white;
                                //Debug.Log("ActionBarmanager.UpdateTargetRange(): setting color to white for ability " + baseAbility.MyName);
                            }
                        } else {
                            if (actionButton.KeyBindText.color != Color.red) {
                                actionButton.KeyBindText.color = Color.red;
                                //Debug.Log("ActionBarmanager.UpdateTargetRange(): setting color to red for ability " + baseAbility.MyName);
                            }
                        }
                    }
                }
                yield return null;
            }
            targetRangeRoutine = null;
            //Debug.Log("ActionBarmanager.UpdateTargetRange(): exiting coroutine");
        }

        public List<ActionButton> GetActionButtons() {
            //Debug.Log("ActionBarManager.GetActionButtons()");
            List<ActionButton> actionButtons = new List<ActionButton>();
            int count = 0;
            foreach (ActionBarController actionBarController in actionBarControllers) {
                foreach (ActionButton actionButton in actionBarController.MyActionButtons) {
                    actionButtons.Add(actionButton);
                    //Debug.Log("ActionBarManager.GetActionButtons() count: " + count + "; actionbutton: " + actionButton.name + actionButton.GetInstanceID());
                    count++;
                }
                //actionButtons.AddRange(actionBarController.MyActionButtons);
            }
            return actionButtons;
        }

        private void AssociateActionBarKeyBinds() {
            //Debug.Log("ActionBarManager.AssociateActionBarKeyBinds()");
            int count = 1;
            foreach (ActionButton actionButton in GetActionButtons()) {
                if (KeyBindManager.MyInstance.MyKeyBinds.Count >= count) {
                    //Debug.Log("ActionBarManager.AssociateActionBarKeyBinds(): associate count: ACT" + count + " with actionButton " + actionButton.name + actionButton.GetInstanceID());
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

        public bool AddNewAbility(BaseAbility newAbility) {
            //Debug.Log("ActionBarManager.AddNewAbility()");
            foreach (ActionBarController actionBarController in actionBarControllers) {
                //Debug.Log("ActionBarManager.AddNewAbility(): looping through a controller");
                if (actionBarController.AddSavedAbility(newAbility)) {
                    return true;
                }
            }
            foreach (ActionBarController actionBarController in actionBarControllers) {
                if (actionBarController.AddNewAbility(newAbility)) {
                    //Debug.Log("ActionBarManager.AddNewAbility(): we were able to add " + newAbility.name);
                    return true;
                }
            }
            return false;
        }

        public void ClearActionBars(bool clearSavedUseables = false) {
            //Debug.Log("ActionBarManager.AddNewAbility()");
            foreach (ActionBarController actionBarController in actionBarControllers) {
                //Debug.Log("ActionBarManager.AddNewAbility(): looping through a controller");
                actionBarController.ClearActionBar(clearSavedUseables);
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
            int abilityListCount = PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.AbilityList.Count;
            //Debug.Log("Updating ability bar with " + abilityListCount.ToString() + " abilities");
            foreach (BaseAbility newAbility in PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.AbilityList.Values) {
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