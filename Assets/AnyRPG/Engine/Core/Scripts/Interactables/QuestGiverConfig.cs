using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New QuestGiver Config", menuName = "AnyRPG/Interactable/QuestGiverConfig")]
    public class QuestGiverConfig : InteractableOptionConfig {

        [SerializeField]
        private QuestGiverProps interactableOptionProps = new QuestGiverProps();

        public override InteractableOptionProps InteractableOptionProps { get => interactableOptionProps; }

    }

}