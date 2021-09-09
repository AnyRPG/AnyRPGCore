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
        [ResourceSelector(resourceType = typeof(Faction))]
        private string factionName = string.Empty;

        private Faction faction;

        public override Sprite Icon { get => (systemConfigurationManager.FactionChangeInteractionPanelImage != null ? systemConfigurationManager.FactionChangeInteractionPanelImage : base.Icon); }
        public override Sprite NamePlateImage { get => (systemConfigurationManager.FactionChangeNamePlateImage != null ? systemConfigurationManager.FactionChangeNamePlateImage : base.NamePlateImage); }
        public Faction Faction { get => faction; set => faction = value; }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable, InteractableOption interactableOption = null) {
            InteractableOptionComponent returnValue = new FactionChangeComponent(interactable, this, systemGameManager);
            if (interactableOption != null) {
                interactableOption.SetComponent(returnValue);
            }
            return returnValue;
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);
            if (factionName != null && factionName != string.Empty) {
                Faction tmpFaction = systemDataFactory.GetResource<Faction>(factionName);
                if (tmpFaction != null) {
                    faction = tmpFaction;
                } else {
                    Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find faction : " + factionName + " while inititalizing.  CHECK INSPECTOR");
                }

            }

        }
    }

}