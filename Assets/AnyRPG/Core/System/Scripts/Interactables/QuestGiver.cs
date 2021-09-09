using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class QuestGiver : InteractableOption {

        [SerializeField]
        private QuestGiverProps questGiverProps = new QuestGiverProps();

        public override InteractableOptionProps InteractableOptionProps { get => questGiverProps; }
    }

}