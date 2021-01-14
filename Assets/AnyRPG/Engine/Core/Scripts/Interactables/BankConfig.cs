using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Bank Config", menuName = "AnyRPG/Interactable/BankConfig")]
    public class BankConfig : InteractableOptionConfig {

        [SerializeField]
        private BankProps interactableOptionProps = new BankProps();

        public override InteractableOptionProps InteractableOptionProps { get => interactableOptionProps; }
    }

}