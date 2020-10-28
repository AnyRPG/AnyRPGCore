using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Skill Trainer Config", menuName = "AnyRPG/Interactable/SkillTrainerConfig")]
    public class SkillTrainerConfig : InteractableOptionConfig {

        [SerializeField]
        private SkillTrainerProps interactableOptionProps = new SkillTrainerProps();

        [Tooltip("the skills that this interactable option offers")]
        [SerializeField]
        private List<string> skillNames = new List<string>();

        public override Sprite Icon { get => (SystemConfigurationManager.MyInstance.MySkillTrainerInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MySkillTrainerInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (SystemConfigurationManager.MyInstance.MySkillTrainerNamePlateImage != null ? SystemConfigurationManager.MyInstance.MySkillTrainerNamePlateImage : base.NamePlateImage); }

        public List<string> SkillNames { get => skillNames; set => skillNames = value; }

    }

}