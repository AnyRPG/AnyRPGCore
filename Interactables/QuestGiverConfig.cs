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

        [Header("QuestGiver")]

        [SerializeField]
        private List<string> questGiverProfileNames = new List<string>();

        public override Sprite Icon { get => (SystemConfigurationManager.MyInstance.MyQuestGiverInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MyQuestGiverInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (SystemConfigurationManager.MyInstance.MyQuestGiverNamePlateImage != null ? SystemConfigurationManager.MyInstance.MyQuestGiverNamePlateImage : base.NamePlateImage); }

        public List<string> QuestGiverProfileNames { get => questGiverProfileNames; set => questGiverProfileNames = value; }

    }

}