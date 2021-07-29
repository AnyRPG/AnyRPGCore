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

        public override Sprite Icon { get => (SystemGameManager.Instance.SystemConfigurationManager.FactionChangeInteractionPanelImage != null ? SystemGameManager.Instance.SystemConfigurationManager.FactionChangeInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (SystemGameManager.Instance.SystemConfigurationManager.FactionChangeNamePlateImage != null ? SystemGameManager.Instance.SystemConfigurationManager.FactionChangeNamePlateImage : base.NamePlateImage); }
        public Faction Faction { get => faction; set => faction = value; }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable, InteractableOption interactableOption = null) {
            InteractableOptionComponent returnValue = new FactionChangeComponent(interactable, this);
            if (interactableOption != null) {
                interactableOption.SetComponent(returnValue);
            }
            return returnValue;
        }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();
            if (factionName != null && factionName != string.Empty) {
                Faction tmpFaction = SystemFactionManager.Instance.GetResource(factionName);
                if (tmpFaction != null) {
                    faction = tmpFaction;
                } else {
                    Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find faction : " + factionName + " while inititalizing.  CHECK INSPECTOR");
                }

            }

        }
    }

}