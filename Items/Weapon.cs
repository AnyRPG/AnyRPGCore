using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
[CreateAssetMenu(fileName = "New Weapon",menuName = "AnyRPG/Inventory/Equipment/Weapon", order = 3)]
public class Weapon : Equipment {

    [SerializeField]
    protected AnimationProfile defaultAttackAnimationProfile;

    /// <summary>
    /// The ability to cast when the weapon hits a target
    /// </summary>
    [SerializeField]
    private InstantEffectAbility onHitAbility;

    [SerializeField]
    private AnyRPGWeaponAffinity weaponAffinity;

    [SerializeField]
    private AudioClip defaultHitSoundEffect;

    public InstantEffectAbility OnHitAbility
    {
        get
        {
            return onHitAbility;
        }
    }

    public AnimationProfile MyDefaultAttackAnimationProfile { get => defaultAttackAnimationProfile; set => defaultAttackAnimationProfile = value; }
    public AnyRPGWeaponAffinity MyWeaponAffinity { get => weaponAffinity; set => weaponAffinity = value; }
    public AudioClip MyDefaultHitSoundEffect { get => defaultHitSoundEffect; set => defaultHitSoundEffect = value; }

        public override int MyDamageModifier {
            get {
                if (weaponAffinity == AnyRPGWeaponAffinity.Bow || weaponAffinity == AnyRPGWeaponAffinity.Staff || weaponAffinity == AnyRPGWeaponAffinity.Mace2H || weaponAffinity == AnyRPGWeaponAffinity.Sword2H) {
                    return base.MyDamageModifier * 2;
                }
                return base.MyDamageModifier;
            }
            set => base.MyDamageModifier = value;
        }
        public override int MyArmorModifier {
            get {
                if (weaponAffinity == AnyRPGWeaponAffinity.Bow || weaponAffinity == AnyRPGWeaponAffinity.Staff || weaponAffinity == AnyRPGWeaponAffinity.Mace2H || weaponAffinity == AnyRPGWeaponAffinity.Sword2H) {
                    return base.MyArmorModifier * 2;
                }
                return base.MyArmorModifier;
            }
            set => base.MyArmorModifier = value;
        }

        public override int MyIntellectModifier {
            get {
                if (weaponAffinity == AnyRPGWeaponAffinity.Bow || weaponAffinity == AnyRPGWeaponAffinity.Staff || weaponAffinity == AnyRPGWeaponAffinity.Mace2H || weaponAffinity == AnyRPGWeaponAffinity.Sword2H) {
                    return base.MyIntellectModifier * 2;
                }
                return base.MyIntellectModifier;
            }
            set => base.MyIntellectModifier = value;
        }
        public override int MyStaminaModifier {
            get {
                if (weaponAffinity == AnyRPGWeaponAffinity.Bow || weaponAffinity == AnyRPGWeaponAffinity.Staff || weaponAffinity == AnyRPGWeaponAffinity.Mace2H || weaponAffinity == AnyRPGWeaponAffinity.Sword2H) {
                    return base.MyStaminaModifier * 2;
                }
                return base.MyStaminaModifier;
            }
            set => base.MyStaminaModifier = value;
        }
        public override int MyStrengthModifier {
            get {
                if (weaponAffinity == AnyRPGWeaponAffinity.Bow || weaponAffinity == AnyRPGWeaponAffinity.Staff || weaponAffinity == AnyRPGWeaponAffinity.Mace2H || weaponAffinity == AnyRPGWeaponAffinity.Sword2H) {
                    return base.MyDamageModifier * 2;
                }
                return base.MyStrengthModifier;
            }
            set => base.MyStrengthModifier = value;
        }
        public override int MyAgilityModifier {
            get {
                if (weaponAffinity == AnyRPGWeaponAffinity.Bow || weaponAffinity == AnyRPGWeaponAffinity.Staff || weaponAffinity == AnyRPGWeaponAffinity.Mace2H || weaponAffinity == AnyRPGWeaponAffinity.Sword2H) {
                    return base.MyDamageModifier * 2;
                }
                return base.MyAgilityModifier;
            }
            set => base.MyAgilityModifier = value;
        }

        public override string GetSummary() {

        List<string> abilitiesList = new List<string>();

        if (onHitAbility != null ) {
            abilitiesList.Add(string.Format("<color=green>Cast On Hit: {0}</color>", onHitAbility.MyName));
        }
        string abilitiesString = string.Empty;
        if (abilitiesList.Count > 0) {
            abilitiesString = "\n" + string.Join("\n", abilitiesList);
        }
        return base.GetSummary() + abilitiesString;
    }

}

}