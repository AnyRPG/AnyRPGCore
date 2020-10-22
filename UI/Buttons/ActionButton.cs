using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class ActionButton : MonoBehaviour, IPointerClickHandler, IClickable, IPointerEnterHandler, IPointerExitHandler {

        // A reference to the useable on the actionbutton
        private IUseable useable;

        // keep track of the last usable that was on this button in case an ability is re-learned
        private IUseable savedUseable;

        [SerializeField]
        private TextMeshProUGUI stackSizeText = null;

        [SerializeField]
        private TextMeshProUGUI keyBindText = null;

        [SerializeField]
        private Image icon = null;

        [SerializeField]
        private Image coolDownIcon = null;

        //private float remainingCooldown = 0f;

        private int count = 0;

        private bool initialized = false;

        private Coroutine autoAttackCoRoutine = null;

        private Coroutine abilityCoRoutine = null;

        /// <summary>
        /// A reference to the actual button that this button uses
        /// </summary>
        public Button MyButton { get; private set; }

        public Image MyIcon { get => icon; set => icon = value; }
        public int MyCount { get => count; }
        public TextMeshProUGUI StackSizeText { get => stackSizeText; }
        public TextMeshProUGUI KeyBindText { get => keyBindText; }
        public Coroutine AutoAttackCoRoutine { get => autoAttackCoRoutine; set => autoAttackCoRoutine = value; }
        public Coroutine AbilityCoRoutine { get => abilityCoRoutine; set => abilityCoRoutine = value; }
        public IUseable SavedUseable { get => savedUseable; set => savedUseable = value; }
        public IUseable Useable {
            get {
                return useable;
            }
            set {
                //Debug.Log(gameObject.name + GetInstanceID() + ".ActionButton.Useable = " + (useable == null ? "null" : useable.MyName) + "; new = " + value);
                useable = value;
                //UpdateVisual(true);
            }
        }

        [SerializeField]
        protected Image backGroundImage;

        public void SetBackGroundColor(Color color) {
            if (backGroundImage != null) {
                backGroundImage.color = color;
            }
        }

        private void Awake() {
            //Debug.Log("ActionButton.Awake()");
            if (initialized == false) {
                Useable = null;
            }
            MyButton = GetComponent<Button>();
            MyButton.onClick.AddListener(OnClickFromButton);
            if (backGroundImage == null) {
                backGroundImage = GetComponent<Image>();
            }
        }

        void Start() {
            //Debug.Log("ActionButton.Start()");
            SystemEventManager.MyInstance.OnItemCountChanged += UpdateItemCount;
        }

        public void OnClickFromButton() {
            //Debug.Log("ActionButton.OnClickFromButton(): useable: " + (Useable != null ? Useable.MyName : "null"));
            OnClick();
        }

        public void OnClick(bool fromKeyBind = false) {
            //Debug.Log(gameObject.name + GetInstanceID() +  ".ActionButton.OnClick(" + fromKeyBind + "): useable: " + (Useable != null ? Useable.MyName : "null"));
            // this may seem like duplicate with the next method, but right now it is used to simulate click events when keypresses happen

            if (!fromKeyBind) {
                // if we did come from a keybind, we don't want to ignore left shift
                if (Input.GetKey(KeyCode.LeftShift)) {
                    return;
                }
                if (HandScript.MyInstance.MyMoveable != null) {
                    // if we have something in the handscript we are trying to drop an item, not use one
                    return;
                }
            }

            if (Useable != null && (!(Useable is Item) || InventoryManager.MyInstance.GetUseableCount(Useable) > 0)) {
                //Debug.Log("ActionButton.OnClick(): Using MyUseable");
                //InventoryScript.MyInstance.GetUseable(MyUseable).Use();
                Useable.Use();
            } else {
                //Debug.Log("ActionButton.OnClick(): MyUseable is null!!!");
            }
        }

        public void OnPointerClick(PointerEventData eventData) {
            //Debug.Log(gameObject.name + GetInstanceID() + ".ActionButton.OnPointerClick(): useable: " + (Useable != null ? Useable.MyName : "null"));

            // left click
            if (eventData.button == PointerEventData.InputButton.Left) {

                if (Input.GetKey(KeyCode.LeftShift)) {
                    // attempt to pick up - the only valid option when shift is held down
                    if (Useable != null && UIManager.MyInstance.MyActionBarManager.MyFromButton == null && HandScript.MyInstance.MyMoveable == null) {
                        // left shift down, pick up a useable
                        //Debug.Log("ActionButton: OnPointerClick(): shift clicked and useable is not null. picking up");
                        HandScript.MyInstance.TakeMoveable(Useable as IMoveable);
                        UIManager.MyInstance.MyActionBarManager.MyFromButton = this;
                    }
                } else {
                    // attempt to put down
                    if (HandScript.MyInstance.MyMoveable != null && HandScript.MyInstance.MyMoveable is IUseable) {
                        if (UIManager.MyInstance.MyActionBarManager.MyFromButton != null) {
                            //Debug.Log("ActionButton: OnPointerClick(): FROMBUTTON IS NOT NULL, SWAPPING ACTIONBAR ITEMS");
                            // this came from another action button slot.  now decide to swap (if we are not empty), or remove from original (if we are empty)
                            if (Useable != null) {
                                UIManager.MyInstance.MyActionBarManager.MyFromButton.ClearUseable();
                                UIManager.MyInstance.MyActionBarManager.MyFromButton.SetUseable(Useable);
                            } else {
                                UIManager.MyInstance.MyActionBarManager.MyFromButton.ClearUseable();
                            }
                        }
                        // no matter whether we sent our useable over or not, we can now clear our useable and set whatever is in the handscript
                        ClearUseable();
                        SetUseable(HandScript.MyInstance.MyMoveable as IUseable);

                        HandScript.MyInstance.Drop();
                    }
                }
            }
        }

        public void HandleDropCombat() {
            UpdateVisual();
        }

        public void HandleEnterCombat(IAbilityCaster abilityCaster) {
            UpdateVisual();
        }

        /// <summary>
        /// Sets the useable on the actionbutton
        /// </summary>
        /// <param name="useable"></param>
        public void SetUseable(IUseable useable) {
            //Debug.Log(gameObject.name + GetInstanceID() + ".ActionButton.SetUsable(" + (useable == null ? "null" : useable.ToString()) + ")");
            // clear reference to any existing useable on this button.
            if (Useable != null && Useable is BaseAbility) {
                //Debug.Log("ActionButton.SetUsable(" + (useable == null ? "null" : useable.ToString()) + "): there was already something on this button");
                if (SystemConfigurationManager.MyInstance.MyAllowAutoAttack == true && Useable is AnimatedAbility && (Useable as AnimatedAbility).IsAutoAttack == true) {
                    // this statement exists to trigger flashing icon, but before the ability executes, and therefore the gcd is null
                    PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.OnAttemptPerformAbility -= OnAttemptUseableUse;
                } else {
                    PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.OnPerformAbility -= OnUseableUse;
                }
                UnsubscribeFromCombatEvents();
            }
            if (useable is Item) {
                //Debug.Log("the useable is an item");
                if (InventoryManager.MyInstance == null) {
                    //Debug.Log("ActionButton.SetUseable(): inventorymanager.myinstance = null!!!");
                }
                if (InventoryManager.MyInstance.FromSlot != null) {
                    // white, really?  this doesn't actually happen...
                    InventoryManager.MyInstance.FromSlot.MyIcon.color = Color.white;
                    InventoryManager.MyInstance.FromSlot = null;
                } else {
                    //Debug.Log("ActionButton.SetUseable(): This must have come from another actionbar, not the inventory");
                }
            }
            Useable = useable;
            if (useable is BaseAbility) {
                //Debug.Log("ActionButton.SetUsable(" + (useable == null ? "null" : useable.ToString()) + "): setting ability");
                //(MyUseable as BaseAbility).OnAbilityCast += OnUseableUse;
                //Debug.Log("id: " + SystemAbilityManager.MyInstance.GetResourceList().Find(x => x == (BaseAbility)useable).GetInstanceID());
                //Debug.Log("SystemAbilityManager: " + SystemAbilityManager.MyInstance.GetResource((BaseAbility)useable));
                if (SystemConfigurationManager.MyInstance.MyAllowAutoAttack == true && Useable is AnimatedAbility && (Useable as AnimatedAbility).IsAutoAttack == true) {
                    // this statement exists to trigger flashing icon, but before the ability executes, and therefore the gcd is null
                    PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.OnAttemptPerformAbility += OnAttemptUseableUse;
                } else {
                    PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.OnPerformAbility += OnUseableUse;
                }
                SubscribeToCombatEvents();
            }
            UpdateVisual();

            // there was the assumption that these were only being called when a player clicked to add an ability
            if (UIManager.MouseInRect(MyIcon.rectTransform)) {
                //if (RectTransformUtility.RectangleContainsScreenPoint(MyIcon.rectTransform, Input.mousePosition)) {
                //UIManager.MyInstance.RefreshTooltip(describable as IDescribable);
                UIManager.MyInstance.ShowToolTip(transform.position, useable as IDescribable);
            }

            //UIManager.MyInstance.RefreshTooltip(useable as IDescribable);

            initialized = true;
        }

        public void SubscribeToCombatEvents() {
            if (Useable != null && Useable is BaseAbility && (Useable as BaseAbility).RequireOutOfCombat == true) {
                PlayerManager.MyInstance.MyCharacter.CharacterCombat.OnEnterCombat += HandleEnterCombat;
                PlayerManager.MyInstance.MyCharacter.CharacterCombat.OnDropCombat += HandleDropCombat;
            }
        }

        public void OnAttemptUseableUse(IAbility ability) {
            //Debug.Log("ActionButton.OnUseableUse(" + ability.MyName + ")");
            if (Useable is IAbility) {
                // actionbuttons can be disabled, but the systemability manager will not.  That's why the ability is monitored here
                if (SystemConfigurationManager.MyInstance.MyAllowAutoAttack == true && (Useable is AnimatedAbility) && (Useable as AnimatedAbility).IsAutoAttack == true) {
                    //Debug.Log("ActionButton.OnUseableUse(" + ability.MyName + "): WAS ANIMATED AUTO ATTACK");
                    if (autoAttackCoRoutine == null) {
                        autoAttackCoRoutine = SystemAbilityManager.MyInstance.StartCoroutine(MonitorAutoAttack(Useable as IAbility));
                    }
                } else {
                    //Debug.Log("ActionButton.OnUseableUse(" + ability.MyName + "): WAS NOT ANIMATED AUTO ATTACK");
                    if (abilityCoRoutine == null) {
                        abilityCoRoutine = SystemAbilityManager.MyInstance.StartCoroutine(MonitorAbility(Useable as IAbility));
                    }
                }
            }
        }


        public void OnUseableUse(IAbility ability) {
            //Debug.Log("ActionButton.OnUseableUse(" + ability.MyName + ")");
            if (Useable is IAbility) {
                // actionbuttons can be disabled, but the systemability manager will not.  That's why the ability is monitored here
                if (SystemConfigurationManager.MyInstance.MyAllowAutoAttack == true && (Useable is AnimatedAbility) && (Useable as AnimatedAbility).IsAutoAttack == true) {
                    //Debug.Log("ActionButton.OnUseableUse(" + ability.MyName + "): WAS ANIMATED AUTO ATTACK");
                    if (autoAttackCoRoutine == null) {
                        autoAttackCoRoutine = SystemAbilityManager.MyInstance.StartCoroutine(MonitorAutoAttack(Useable as IAbility));
                    }
                } else {
                    //Debug.Log("ActionButton.OnUseableUse(" + ability.MyName + "): WAS NOT ANIMATED AUTO ATTACK");
                    if (abilityCoRoutine == null) {
                        abilityCoRoutine = SystemAbilityManager.MyInstance.StartCoroutine(MonitorAbility(Useable as IAbility));
                    }
                }
            }
        }

        public IEnumerator MonitorAutoAttack(IAbility ability) {
            //Debug.Log("ActionButton.MonitorautoAttack(" + ability.MyName + ")");
            //Debug.Log("Monitoring cooldown of AbilityInstanceID: " + SystemAbilityManager.MyInstance.GetResource((BaseAbility)ability).GetInstanceID());
            yield return null;

            while (Useable != null && PlayerManager.MyInstance.MyCharacter.CharacterCombat.GetInCombat() == true) {
                //Debug.Log("ActionButton.MonitorAbility(): cooldown : " + remainingCooldown + "useable cooldown: " + (MyUseable as IAbility).MyRemainingCoolDown);
                UpdateVisual();
                yield return new WaitForSeconds(0.5f);
            }
            //Debug.Log("ActionButton.MonitorAbility(" + ability.MyName + "): Done Monitoring");
            if (Useable != null) {
                // could switch buttons while an ability is on cooldown
                UpdateVisual();
            }
            autoAttackCoRoutine = null;
        }

        public IEnumerator MonitorAbility(IAbility ability) {
            //Debug.Log("ActionButton.MonitorAbility(" + ability.MyName + ")");
            //Debug.Log("Monitoring cooldown of AbilityInstanceID: " + SystemAbilityManager.MyInstance.GetResource((BaseAbility)ability).GetInstanceID());
            while (Useable != null && (PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.MyAbilityCoolDownDictionary.ContainsKey((Useable as IAbility).DisplayName) || PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.MyRemainingGlobalCoolDown > 0f || PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.MyAbilityCoolDownDictionary.ContainsKey(ability.DisplayName))) {
                /*
                if (PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.MyAbilityCoolDownDictionary.ContainsKey(ability.MyName)) {
                    remainingCooldown = PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.MyAbilityCoolDownDictionary[ability.MyName].MyRemainingCoolDown;
                } else {
                    remainingCooldown = PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.MyRemainingGlobalCoolDown;
                }
                */
                //Debug.Log("ActionButton.MonitorAbility(): cooldown : " + remainingCooldown + "useable cooldown: " + (MyUseable as IAbility).MyRemainingCoolDown);
                UpdateVisual();
                yield return null;
            }
            //Debug.Log("ActionButton.MonitorAbility(" + ability.MyName + "): Done Monitoring");
            if (Useable != null) {
                // could switch buttons while an ability is on cooldown
                UpdateVisual();
            }
            abilityCoRoutine = null;
        }

        private void DisableCoolDownIcon() {
            if (coolDownIcon.isActiveAndEnabled != false) {
                coolDownIcon.sprite = null;
                coolDownIcon.color = new Color32(0, 0, 0, 0);
                coolDownIcon.enabled = false;
            }
        }

        /// <summary>
        /// UPdates the visual representation of the actionbutton
        /// </summary>
        public void UpdateVisual(bool removeStaleActions = false) {
            //Debug.Log(gameObject.name + GetInstanceID() + ".ActionButton.UpdateVisual() useable: " + (useable == null ? "null" : useable.MyName));

            // attempt to remove unlearned spells from the bars
            if (removeStaleActions) {
                //Debug.Log("ActionButton.UpdateVisual(): removeStaleActions = true");
                if (Useable != null && (Useable is IAbility)) {
                    if (!PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.HasAbility(Useable as BaseAbility)) {
                        savedUseable = Useable;
                        Useable = null;
                    }
                }
            }
            if (Useable == null) {
                //Debug.Log("ActionButton.UpdateVisual(): useable is null. clearing stack count and setting icon to empty");
                UIManager.MyInstance.ClearStackCount(this);
                MyIcon.sprite = null;
                MyIcon.color = icon.color = new Color32(0, 0, 0, 0);
                DisableCoolDownIcon();
                return;
            }

            if (MyIcon.sprite != Useable.MyIcon) {
                MyIcon.sprite = Useable.MyIcon;
            }
            if (MyIcon.color != Color.white) {
                MyIcon.color = Color.white;
            }

            //Debug.Log("ActionButton.UpdateVisual(): about to get useable count");
            if (Useable is Item) {
                int count = InventoryManager.MyInstance.GetUseableCount(Useable);
                // we have to do this to ensure we have a reference to the top item on the stack, otherwise we will try to use an item that has been used already
                if ((count == 0 && removeStaleActions) || count > 0) {
                    Useable = InventoryManager.MyInstance.GetUseable(Useable as IUseable);
                }
                UIManager.MyInstance.UpdateStackSize(this, count, true);
                if (count == 0) {
                    EnableFullCoolDownIcon();
                } else {
                    DisableCoolDownIcon();
                }
            } else if (Useable is BaseAbility) {
                UIManager.MyInstance.ClearStackCount(this);

                //TESTING MOVING TO BEFORE BASEABILTY AND WEAPONAFFINITY CHECKS
                // auto-attack buttons are special and display the current weapon of the character
                if ((Useable is AnimatedAbility) && (Useable as AnimatedAbility).IsAutoAttack == true) {
                    //Debug.Log("ActionButton.UpdateVisual(): updating auto-attack ability");
                    foreach (Equipment equipment in PlayerManager.MyInstance.MyCharacter.CharacterEquipmentManager.CurrentEquipment.Values) {
                        if (equipment != null && equipment is Weapon && (equipment as Weapon).MyUseDamagePerSecond == true) {
                            if (MyIcon.sprite != equipment.MyIcon) {
                                MyIcon.sprite = equipment.MyIcon;
                                break;
                            }
                        }
                    }
                }
                if (SystemConfigurationManager.MyInstance.MyAllowAutoAttack == true && (Useable is AnimatedAbility) && (Useable as AnimatedAbility).IsAutoAttack == true) {

                    /*
                    if (PlayerManager.MyInstance.MyCharacter.MyCharacterEquipmentManager.MyCurrentEquipment.ContainsKey(EquipmentSlot.MainHand) && PlayerManager.MyInstance.MyCharacter.MyCharacterEquipmentManager.MyCurrentEquipment[EquipmentSlot.MainHand] != null) {
                        if (PlayerManager.MyInstance.MyCharacter.MyCharacterEquipmentManager.MyCurrentEquipment[EquipmentSlot.MainHand].MyIcon != null) {
                            MyIcon.sprite = PlayerManager.MyInstance.MyCharacter.MyCharacterEquipmentManager.MyCurrentEquipment[EquipmentSlot.MainHand].MyIcon;
                            //Debug.Log("ActionButton.UpdateVisual(): setting icon");
                        }
                    }
                    */
                    if (PlayerManager.MyInstance.MyCharacter.CharacterCombat.GetInCombat() == true) {
                        if (coolDownIcon.isActiveAndEnabled == false) {
                            coolDownIcon.enabled = true;
                        }
                        /*
                        if (coolDownIcon.sprite != MyIcon.sprite) {
                            Debug.Log("ActionButton.UpdateVisual(): Setting coolDownIcon to match MyIcon");
                            coolDownIcon.sprite = MyIcon.sprite;
                        }
                        */
                        if (coolDownIcon.color == new Color32(255, 0, 0, 155)) {
                            coolDownIcon.color = new Color32(255, 146, 146, 155);
                        } else {
                            coolDownIcon.color = new Color32(255, 0, 0, 155);
                        }

                        if (coolDownIcon.fillMethod != Image.FillMethod.Radial360) {
                            coolDownIcon.fillMethod = Image.FillMethod.Radial360;
                        }
                        if (coolDownIcon.fillAmount != 1f) {
                            coolDownIcon.fillAmount = 1f;
                        }
                    } else {
                        //Debug.Log("ActionButton.UpdateVisual(): Player is not in combat");
                        if (coolDownIcon.sprite != null) {
                            coolDownIcon.sprite = null;
                        }
                        if (coolDownIcon.enabled != false) {
                            coolDownIcon.enabled = false;
                        }
                    }
                    // don't need to continue on and do radial fill on auto-attack icons
                    return;
                }

                if ((Useable as BaseAbility) is BaseAbility && (Useable as BaseAbility).RequireOutOfCombat) {
                    if (PlayerManager.MyInstance != null && PlayerManager.MyInstance.MyCharacter != null && PlayerManager.MyInstance.MyCharacter.CharacterCombat.GetInCombat() == true) {
                        //Debug.Log("ActionButton.UpdateVisual(): can't cast due to being in combat");
                        EnableFullCoolDownIcon();
                        return;
                    }
                }

                if ((Useable as BaseAbility) is BaseAbility && (Useable as BaseAbility).WeaponAffinityNames.Count > 0) {
                    if (PlayerManager.MyInstance != null && PlayerManager.MyInstance.MyCharacter != null) {
                        if (!((Useable as BaseAbility).CanCast(PlayerManager.MyInstance.MyCharacter))) {
                            //Debug.Log("ActionButton.UpdateVisual(): can't cast due to missing weaponaffinity");
                            EnableFullCoolDownIcon();
                            return;
                        }
                    }
                }


                if (PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.MyAbilityCoolDownDictionary.ContainsKey((Useable as IAbility).DisplayName) || PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.MyRemainingGlobalCoolDown > 0f || PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.MyAbilityCoolDownDictionary.ContainsKey(Useable.DisplayName)) {
                    //Debug.Log("ActionButton.UpdateVisual(): Ability is on cooldown");
                    if (coolDownIcon.isActiveAndEnabled != true) {
                        coolDownIcon.enabled = true;
                    }
                    if (coolDownIcon.sprite != MyIcon.sprite) {
                        //Debug.Log("Setting coolDownIcon to match MyIcon");
                        coolDownIcon.sprite = MyIcon.sprite;
                        coolDownIcon.color = new Color32(0, 0, 0, 230);
                        coolDownIcon.fillMethod = Image.FillMethod.Radial360;
                        //coolDownIcon.fillOrigin = Image.Origin360.Top;
                        coolDownIcon.fillClockwise = false;
                    }
                    //Debug.Log("remainingCooldown: " + this.remainingCooldown + "; totalcooldown: " + (MyUseable as BaseAbility).abilityCoolDown);
                    float abilityCoolDown = 0f;
                    float initialCoolDown = 0f;
                    if (PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.MyAbilityCoolDownDictionary.ContainsKey((Useable as IAbility).DisplayName)) {
                        abilityCoolDown = PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.MyAbilityCoolDownDictionary[(Useable as IAbility).DisplayName].MyRemainingCoolDown;
                        initialCoolDown = PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.MyAbilityCoolDownDictionary[(Useable as IAbility).DisplayName].MyInitialCoolDown;
                    } else {
                        initialCoolDown = (Useable as BaseAbility).abilityCoolDown;
                    }
                    //float globalCoolDown
                    float fillAmount = Mathf.Max(abilityCoolDown, PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.MyRemainingGlobalCoolDown) /
                        (abilityCoolDown > PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.MyRemainingGlobalCoolDown ? initialCoolDown : PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.MyInitialGlobalCoolDown);
                    //Debug.Log("Setting fill amount to: " + fillAmount);
                    if (coolDownIcon.fillAmount != fillAmount) {
                        coolDownIcon.fillAmount = fillAmount;
                    }
                } else {
                    if (coolDownIcon.sprite != null) {
                        coolDownIcon.sprite = null;
                    }
                    if (coolDownIcon.enabled != false) {
                        coolDownIcon.enabled = false;
                    }
                }

            }

        }

        public void EnableFullCoolDownIcon() {
            if (coolDownIcon.isActiveAndEnabled == false) {
                coolDownIcon.enabled = true;
            }
            if (coolDownIcon.sprite != MyIcon.sprite) {
                //Debug.Log("Setting coolDownIcon to match MyIcon");
                coolDownIcon.sprite = MyIcon.sprite;
                coolDownIcon.color = new Color32(0, 0, 0, 150);
                coolDownIcon.fillMethod = Image.FillMethod.Radial360;
                //coolDownIcon.fillOrigin = Image.Origin360.Top;
                coolDownIcon.fillClockwise = false;
            }
            float fillAmount = 1f;
            if (coolDownIcon.fillAmount != fillAmount) {
                coolDownIcon.fillAmount = fillAmount;
            }
        }

        public void UpdateItemCount(Item item) {

            if (item is IUseable) {
                UpdateVisual();
            }
        }

        public void OnPointerEnter(PointerEventData eventData) {
            //Debug.Log(gameObject + ".ActionButton.OnPointerEnter()");
            IDescribable tmp = null;

            if (Useable != null && Useable is IDescribable) {
                tmp = (IDescribable)Useable;
                //UIManager.MyInstance.ShowToolTip(transform.position);
            }// else if (MyUseables.Count > 0) {
             //UIManager.MyInstance.ShowToolTip(transform.position);
             //}
            if (tmp != null) {
                UIManager.MyInstance.ShowToolTip(transform.position, tmp);
            }
        }

        public void OnPointerExit(PointerEventData eventData) {
            UIManager.MyInstance.HideToolTip();
        }

        public void UnsubscribeFromCombatEvents() {
            if (Useable != null && Useable is BaseAbility && (Useable as BaseAbility).RequireOutOfCombat == true) {
                PlayerManager.MyInstance.MyCharacter.CharacterCombat.OnEnterCombat -= HandleEnterCombat;
                PlayerManager.MyInstance.MyCharacter.CharacterCombat.OnDropCombat -= HandleDropCombat;
            }
        }

        public void ClearUseable() {
            //Debug.Log("ActionButton.ClearUseable()");

            UnsubscribeFromCombatEvents();
            if (Useable != null) {
                savedUseable = Useable;
            }
            Useable = null;
            UpdateVisual();
        }
    }

}