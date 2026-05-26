using System;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    public class UnitNameplateController : BaseNameplateController {

        public override event System.Action OnInitializeNameplate = delegate { };
        public override event Action OnNameChange = delegate { };

        private UnitController unitController = null;

        public override string UnitDisplayName {
            get {
                return (unitController.BaseCharacter != null ? unitController.BaseCharacter.CharacterName : base.UnitDisplayName);
            }
        }
        public override Faction Faction {
            get {
                if (unitController.BaseCharacter != null) {
                    return unitController.BaseCharacter.Faction;
                }
                return base.Faction;
            }
        }
        public override string Title {
            get {
                return (unitController.BaseCharacter != null ? unitController.BaseCharacter.Title : base.Title); }
        }

        public override Vector3 NameplatePosition {
            get {
                //Debug.Log($"{unitController.gameObject.name}.UnitNameplateController.NameplateTransform() mount: {(unitController.UnitMountManager.MountUnitController == null ? "null" : unitController.UnitMountManager.MountUnitController.gameObject.name)}");
                if (unitController.IsMounted == true
                    && unitController.UnitMountManager.MountUnitController != null) {
                    return unitController.UnitMountManager.MountUnitController.GetNameplatePosition();
                }
                return base.NameplatePosition;
            }
        }
        public override int Level {
            get {
                if (unitController.BaseCharacter != null && unitController.CharacterStats != null) {
                    return unitController.CharacterStats.Level;
                }
                return base.Level;
            }
        }
        public override List<PowerResource> PowerResourceList {
            get {
                if (unitController != null && unitController.BaseCharacter != null) {
                    return unitController.CharacterStats.PowerResourceList;
                }
                return base.PowerResourceList;
            }
        }


        public UnitController UnitController { get => unitController; set => unitController = value; }

        public UnitNameplateController(UnitController unitController, SystemGameManager systemGameManager) : base(unitController, systemGameManager) {
            this.unitController = unitController;
        }

        /*
        public override void InitializeNameplate() {
            //Debug.Log($"{unitController.gameObject.name}.UnitNameplateController.InitializeNameplate()");
            if (SuppressNameplate == true) {
                //Debug.Log($"{unitController.gameObject.name}.UnitNameplateController.InitializeNameplate(): namePlate suppressed.  Returning.");
                return;
            }
            if (unitController != null) {
                if (unitController.CharacterStats != null
                    && unitController.CharacterStats.IsAlive == false
                    && unitController.BaseCharacter.SpawnDead == false) {
                    // if this is not a character that spawns dead, and is currently dead, then there is no reason to display a nameplate as dead characters usually cannot have nameplates
                    return;
                }
                SetNameplatePosition();
                
                BroadcastInitializeNameplate();
            } else {
                //Debug.Log($"{gameObject.name}.CharacterUnit.InitializeNameplate(): Character is null or start has not been run yet. exiting.");
                return;
            }
        }
        */

        public override NameplateController AddNameplate() {
            //return namePlateManager.AddNameplate(unitController, (unitController.ComponentController.NameplateTransform == null ? true : false));
            return namePlateManager.AddNameplate(unitController, false);
        }

        public override void RemoveNameplate() {
            unitController.UnitEventController.OnResourceAmountChanged -= HandleResourceAmountChanged;
            unitController.UnitEventController.OnTitleChange -= HandleTitleChange;
            unitController.UnitEventController.OnNameChange -= HandleNameChange;
            unitController.UnitEventController.OnReputationChange -= HandleReputationChange;
            unitController.UnitEventController.OnSetGuildId -= HandleSetGuildId;

            base.RemoveNameplate();
        }

        public override void SetupNameplate() {
            //Debug.Log($"{unitController.gameObject.name}.UnitNameplateController.SetupNameplate()");
            // intentionally not calling base because it disables the healthbar
            if (HasHealth() == true) {
                nameplate.ProcessHealthChanged(MaxHealth(), CurrentHealth());
            }
            unitController.UnitEventController.OnResourceAmountChanged += HandleResourceAmountChanged;
            unitController.UnitEventController.OnTitleChange += HandleTitleChange;
            unitController.UnitEventController.OnNameChange += HandleNameChange;
            unitController.UnitEventController.OnReputationChange += HandleReputationChange;
            unitController.UnitEventController.OnSetGuildId += HandleSetGuildId;
        }

        private void HandleSetGuildId(int guildId, string guildName) {
            //Debug.Log($"{unitController.gameObject.name}.UnitNameplateController.HandleSetGuildId({guildId}, {guildName})");

            nameplate.HandleSetGuildId();
        }

        public void HandleReputationChange(UnitController sourceUnitController) {
            nameplate.HandleReputationChange();
        }

        public void HandleTitleChange(string newTitle) {
            nameplate.HandleTitleChange(newTitle);
        }

        public void HandleNameChange(string newName) {
            nameplate.HandleNameChange(newName);
        }



        public void HandleResourceAmountChanged(PowerResource powerResource, int maxAmount, int currentAmount) {
            //Debug.Log($"{unitController.gameObject.name}.UnitNameplateController.HandleResourceAmountChanged()");
            nameplate.HandleResourceAmountChanged(powerResource, maxAmount, currentAmount);
        }

        public override bool CanSpawnNameplate() {
            //Debug.Log((unitController == null ? "null" : unitController.gameObject.name) + ".UnitNameplateController.CanSpawnNameplate()");
            if (unitController == null) {
                return false;
            }
            if (unitController.CharacterStats != null
                                && unitController.CharacterStats.IsAlive == false
                                && unitController.BaseCharacter.SpawnDead == false) {
                // if this is not a character that spawns dead, and is currently dead, then there is no reason to display a nameplate as dead characters usually cannot have nameplates
                return false;
            }
            return base.CanSpawnNameplate();
        }

        public override void BroadcastInitializeNameplate() {
            OnInitializeNameplate();

        }

        public override int CurrentHealth() {
            if (unitController.BaseCharacter != null && unitController.CharacterStats != null) {
                return unitController.CharacterStats.CurrentPrimaryResource;
            }
            return base.CurrentHealth();
        }

        public override int MaxHealth() {
            //Debug.Log($"{gameObject.name}.CharacterUnit.MaxHealth()");
            if (unitController.BaseCharacter != null && unitController.CharacterStats != null) {
                return unitController.CharacterStats.MaxPrimaryResource;
            }
            return base.MaxHealth();
        }

        public override bool HasPrimaryResource() {
            //Debug.Log($"{gameObject.name}.CharacterUnit.HasHealth(): return true");
            if (unitController.BaseCharacter != null && unitController.CharacterStats != null) {
                return unitController.CharacterStats.HasPrimaryResource;
            }
            return base.HasPrimaryResource();
        }

        public override bool HasSecondaryResource() {
            //Debug.Log($"{gameObject.name}.CharacterUnit.HasHealth(): return true");
            if (unitController.BaseCharacter != null && unitController.CharacterStats != null) {
                return unitController.CharacterStats.HasSecondaryResource;
            }
            return base.HasSecondaryResource();
        }

        public override bool HasHealth() {
            //Debug.Log($"UnitNameplateController.HasHealth()");

            if (unitController == null) {
                Debug.LogWarning("UnitNameplateController.HasHealth(): unitcontroller is null");
            }
            if (unitController.CharacterUnit == null) {
                Debug.LogWarning($"{unitController.gameObject.name}.UnitNameplateController.HasHealth(): unitcontroller.CharacterUnit is null");
            }
            if (unitController.BaseCharacter != null && unitController.CharacterStats != null) {
                return unitController.CharacterStats.HasHealthResource;
            } else {
                Debug.LogWarning($"{unitController.gameObject.name}.UnitNameplateController.HasHealth(): unitcontroller.BaseCharacter or CharacterStats is null");
            }
            return base.HasHealth();
        }

        public override float GetPowerResourceMaxAmount(PowerResource powerResource) {
            if (unitController != null && unitController.BaseCharacter != null) {
                return unitController.CharacterStats.GetPowerResourceMaxAmount(powerResource);
            }
            return base.GetPowerResourceMaxAmount(powerResource);
        }

        public override float GetPowerResourceAmount(PowerResource powerResource) {
            if (unitController != null && unitController.BaseCharacter != null) {
                return unitController.CharacterStats.GetPowerResourceAmount(powerResource);
            }
            return base.GetPowerResourceAmount(powerResource);
        }

        public Color32 GetTextColor() {
            //Debug.Log($"{unitController.gameObject.name}.UnitNameplateController.GetTextColor()");

            // player is always green
            if (playerManagerClient.UnitController != null && unitController == playerManagerClient.UnitController) {
                return Color.green;
            }

            // this is not the player, check for faction
            if (Faction == null) {
                return Color.white;
            }

            // faction is not null, check for cutscene
            if (uIManager.CutSceneBarController.CurrentCutscene == null) {
                return Faction.GetFactionColor(playerManagerClient, interactable);
            }

            // cutscene is not null, check for faction color setting
            if (uIManager.CutSceneBarController.CurrentCutscene.UseDefaultFactionColors == true) {
                return Faction.GetFactionColor();
            }

            return Color.white;
        }

        public override string GetNameplateString() {
            //Debug.Log($"{unitController.gameObject.name}.UnitNameplateController.GetNameplateString()");

            Color textColor = GetTextColor();

                string nameString = string.Empty;
                string tagString = string.Empty;
                if (playerManagerClient.UnitController == null ||unitController != playerManagerClient.UnitController || PlayerPrefs.GetInt("ShowPlayerName") == 1) {
                    // player is not spawned, or this is not the player, or player is allowed to show name
                    nameString = UnitDisplayName;
                }

                // faction is lowest priority
                if (playerManagerClient.UnitController == null || unitController != playerManagerClient.UnitController || PlayerPrefs.GetInt("ShowPlayerFaction") == 1) {
                    if (unitController.UnitProfile.SuppressNameplateFaction == false) {
                        tagString = $"<{Faction.DisplayName}>";
                    }
                }

                // title is higher priority than faction
                if (Title != string.Empty) {
                    tagString = $"<{Title}>";
                }

                // guild is the highest priority
                if (unitController.CharacterGuildManager.IsInGuild()) {
                    tagString = $"<{unitController.CharacterGuildManager.GuildName}>";
                }

                string newLineString = string.Empty;
                if (tagString != string.Empty && nameString != string.Empty) {
                    newLineString = "\n";
                    //Debug.Log(namePlateUnit.DisplayName + ".NameplateController.SetCharacterName(): faction and name are both not empty");
                }

                //Debug.Log($"{unitNameplateController.NameplateUnit.DisplayName}.NameplateController.SetCharacterName(): setting character name text: {nameString}{newLineString}{factionString}");
                return $"<color=#{ColorUtility.ToHtmlStringRGB(textColor)}>{nameString}{newLineString}{tagString}</color>";
        }


    }


}