using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class FactionChangeProps : InteractableOptionProps {

        [Tooltip("the faction that this interactable option offers")]
        [SerializeField]
        private string factionName = string.Empty;

        private Faction faction;

        public override Sprite Icon { get => (SystemConfigurationManager.MyInstance.MyFactionChangeInteractionPanelImage != null ? SystemConfigurationManager.MyInstance.MyFactionChangeInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (SystemConfigurationManager.MyInstance.MyFactionChangeNamePlateImage != null ? SystemConfigurationManager.MyInstance.MyFactionChangeNamePlateImage : base.NamePlateImage); }
        public Faction Faction { get => faction; set => faction = value; }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable) {
            return new FactionChangeComponent(interactable, this);
        }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();
            if (factionName != null && factionName != string.Empty) {
                Faction tmpFaction = SystemFactionManager.MyInstance.GetResource(factionName);
                if (tmpFaction != null) {
                    faction = tmpFaction;
                } else {
                    Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find faction : " + factionName + " while inititalizing.  CHECK INSPECTOR");
                }

            }

        }
    }

}