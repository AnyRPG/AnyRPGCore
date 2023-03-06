using System.Collections;
using System.Collections.Generic;
using TMPro;
using UMA;
using UMA.CharacterSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {

    public class UMADNASlider: NavigableSlider {

		string dnaName;
		int index;
		UMADnaBase umaDnaBase;   // different DNA 
		protected DynamicCharacterAvatar dynamicCharacterAvatar;
		protected DNARangeAsset dnaRangeAsset;

		public void Initialize(string name, int index, UMADnaBase owner, DynamicCharacterAvatar avatar, float currentValue) {
			dnaName = name;
			text.text = name;
			this.index = index;
			umaDnaBase = owner;
			dynamicCharacterAvatar = avatar;
			slider.value = currentValue;

			DNARangeAsset[] dnaRangeAssets = avatar.activeRace.data.dnaRanges;
			foreach (DNARangeAsset d in dnaRangeAssets) {
				if (d.ContainsDNARange(this.index, dnaName)) {
                    dnaRangeAsset = d;
					return;
				}
			}
		}

		public void ChangeValue(float value) {
			if (dnaRangeAsset == null) //No specified DNA Range Asset for this DNA
			{
				umaDnaBase.SetValue(index, value);
				dynamicCharacterAvatar.ForceUpdate(true, false, false);
				return;
			}

			if (dnaRangeAsset.ValueInRange(index, value)) {
				umaDnaBase.SetValue(index, value);
				dynamicCharacterAvatar.ForceUpdate(true, false, false);
				return;
			} else {
				//Debug.LogWarning ("DNA Value out of range!");
			}
		}

	}

}