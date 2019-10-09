using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Inventory/Equipment/Weapon", order = 3)]
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
