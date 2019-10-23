using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMA;

[CreateAssetMenu(fileName = "New Equipment", menuName = "Inventory/Equipment")]
public class Equipment : Item {

    public EquipmentSlot equipSlot;
    //public UMASlot UMASlotAffinity;
    public UMA.UMATextRecipe UMARecipe = null;

    // The next 5 fiels are meant for weapons.  They are being left in the base equipment class for now in case we want to do something like attach a cape to the spine
    // However, this will likely not happen and these should probably just be moved to weapon.

    [SerializeField]
    private string holdableObjectName;

    public int armorModifier;
    public int damageModifier;

    [SerializeField]
    private int intellectModifier;
    [SerializeField]
    private int staminaModifier;
    [SerializeField]
    private int strengthModifier;
    [SerializeField]
    private int agilityModifier;

    [SerializeField]
    private BaseAbility onEquipAbility;

    [SerializeField]
    private List<BaseAbility> learnedAbilities;

    public int MyIntellectModifier { get => intellectModifier; set => intellectModifier = value; }
    public int MyStaminaModifier { get => staminaModifier; set => staminaModifier = value; }
    public int MyStrengthModifier { get => strengthModifier; set => strengthModifier = value; }
    public int MyAgilityModifier { get => agilityModifier; set => agilityModifier = value; }
    public BaseAbility MyOnEquipAbility { get => onEquipAbility; set => onEquipAbility = value; }
    public List<BaseAbility> MyLearnedAbilities { get => learnedAbilities; set => learnedAbilities = value; }
    public string MyHoldableObjectName { get => holdableObjectName; set => holdableObjectName = value; }

    public override void Start() {
        base.Start();
    }

    public override void Use() {
        if (PlayerManager.MyInstance != null && PlayerManager.MyInstance.MyCharacter != null && PlayerManager.MyInstance.MyCharacter.MyCharacterEquipmentManager != null) {
            base.Use();
            PlayerManager.MyInstance.MyCharacter.MyCharacterEquipmentManager.Equip(this);
            Remove();
        }
    }

    public override string GetSummary() {
        //string stats = string.Empty;
        List<string> abilitiesList = new List<string>();

        if (armorModifier > 0) {
            abilitiesList.Add(string.Format(" +{0} Armor", armorModifier));
        }
        if (damageModifier > 0) {
            abilitiesList.Add(string.Format(" +{0} Damage", damageModifier));
        }
        if (staminaModifier > 0) {
            abilitiesList.Add(string.Format(" +{0} Stamina", staminaModifier));
        }
        if (strengthModifier > 0) {
            abilitiesList.Add(string.Format(" +{0} Strength", strengthModifier));
        }
        if (intellectModifier > 0) {
            abilitiesList.Add(string.Format(" +{0} Intellect", intellectModifier));
        }
        if (agilityModifier > 0) {
            abilitiesList.Add(string.Format(" +{0} Agility", agilityModifier));
        }

        if (onEquipAbility != null) {
            abilitiesList.Add(string.Format("<color=green>Cast On Equip: {0}</color>", onEquipAbility.MyName));
        }
        foreach (BaseAbility learnedAbility in MyLearnedAbilities) {
            abilitiesList.Add(string.Format("<color=green>Learn On Equip: {0}</color>", learnedAbility.MyName));
        }

        return base.GetSummary() + "\n" + string.Join("\n", abilitiesList);
    }
}

public enum EquipmentSlot { Helm, Chest, Legs, MainHand, OffHand, Feet, Hands, Shoulders }
//public enum UMASlot { None, Helm, Chest, Legs, Feet, Hands }