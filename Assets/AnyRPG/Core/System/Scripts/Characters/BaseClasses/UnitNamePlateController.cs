using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace AnyRPG {

    [System.Serializable]
    public class UnitNamePlateController : BaseNamePlateController {

        public override event System.Action OnInitializeNamePlate = delegate { };
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

        public override Transform NamePlateTransform {
            get {
                //Debug.Log($"{unitController.gameObject.name}.UnitNamePlateController.NamePlateTransform() mount: {(unitController.UnitMountManager.MountUnitController == null ? "null" : unitController.UnitMountManager.MountUnitController.gameObject.name)}");
                if (unitController.IsMounted == true
                    && unitController.UnitMountManager.MountUnitController != null
                    && unitController.UnitMountManager.MountUnitController.NamePlateController != null
                    && unitController.UnitMountManager.MountUnitController.NamePlateController.NamePlateTransform != null) {
                    return unitController.UnitMountManager.MountUnitController.NamePlateController.NamePlateTransform;
                }
                return base.NamePlateTransform;
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

        public UnitNamePlateController(NamePlateUnit namePlateUnit, SystemGameManager systemGameManager) : base(namePlateUnit, systemGameManager) {
            if ((namePlateUnit as UnitController) is UnitController) {
                unitController = (namePlateUnit as UnitController);
            }
        }

        /*
        public override void InitializeNamePlate() {
            //Debug.Log($"{unitController.gameObject.name}.UnitNamePlateController.InitializeNamePlate()");
            if (SuppressNamePlate == true) {
                //Debug.Log($"{unitController.gameObject.name}.UnitNamePlateController.InitializeNamePlate(): namePlate suppressed.  Returning.");
                return;
            }
            if (unitController != null) {
                if (unitController.CharacterStats != null
                    && unitController.CharacterStats.IsAlive == false
                    && unitController.BaseCharacter.SpawnDead == false) {
                    // if this is not a character that spawns dead, and is currently dead, then there is no reason to display a nameplate as dead characters usually cannot have nameplates
                    return;
                }
                SetNamePlatePosition();
                
                BroadcastInitializeNamePlate();
            } else {
                //Debug.Log($"{gameObject.name}.CharacterUnit.InitializeNamePlate(): Character is null or start has not been run yet. exiting.");
                return;
            }
        }
        */

        public override NamePlateController AddNamePlate() {
            return namePlateManager.AddNamePlate(unitController, (unitController.UnitComponentController.NamePlateTransform == null ? true : false));
        }

        public override void RemoveNamePlate() {
            unitController.UnitEventController.OnResourceAmountChanged -= HandleResourceAmountChanged;
            unitController.UnitEventController.OnTitleChange -= HandleTitleChange;
            unitController.UnitEventController.OnNameChange -= HandleNameChange;
            unitController.UnitEventController.OnReputationChange -= HandleReputationChange;
            unitController.UnitEventController.OnSetGuildId -= HandleSetGuildId;

            base.RemoveNamePlate();
        }

        public override void SetupNamePlate() {
            //Debug.Log($"{unitController.gameObject.name}.UnitNamePlateController.SetupNamePlate()");
            // intentionally not calling base because it disables the healthbar
            if (HasHealth() == true) {
                namePlate.ProcessHealthChanged(MaxHealth(), CurrentHealth());
            }
            unitController.UnitEventController.OnResourceAmountChanged += HandleResourceAmountChanged;
            unitController.UnitEventController.OnTitleChange += HandleTitleChange;
            unitController.UnitEventController.OnNameChange += HandleNameChange;
            unitController.UnitEventController.OnReputationChange += HandleReputationChange;
            unitController.UnitEventController.OnSetGuildId += HandleSetGuildId;
        }

        private void HandleSetGuildId(int guildId, string guildName) {
            //Debug.Log($"{unitController.gameObject.name}.UnitNamePlateController.HandleSetGuildId({guildId}, {guildName})");

            namePlate.HandleSetGuildId();
        }

        public void HandleReputationChange(UnitController sourceUnitController) {
            namePlate.HandleReputationChange();
        }

        public void HandleTitleChange(string newTitle) {
            namePlate.HandleTitleChange(newTitle);
        }

        public void HandleNameChange(string newName) {
            namePlate.HandleNameChange(newName);
        }



        public void HandleResourceAmountChanged(PowerResource powerResource, int maxAmount, int currentAmount) {
            //Debug.Log($"{unitController.gameObject.name}.UnitNamePlateController.HandleResourceAmountChanged()");
            namePlate.HandleResourceAmountChanged(powerResource, maxAmount, currentAmount);
        }

        public override bool CanSpawnNamePlate() {
            //Debug.Log((unitController == null ? "null" : unitController.gameObject.name) + ".UnitNamePlateController.CanSpawnNamePlate()");
            if (unitController == null) {
                return false;
            }
            if (unitController.CharacterStats != null
                                && unitController.CharacterStats.IsAlive == false
                                && unitController.BaseCharacter.SpawnDead == false) {
                // if this is not a character that spawns dead, and is currently dead, then there is no reason to display a nameplate as dead characters usually cannot have nameplates
                return false;
            }
            return base.CanSpawnNamePlate();
        }

        public override void BroadcastInitializeNamePlate() {
            OnInitializeNamePlate();

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
            //Debug.Log($"{gameObject.name}.CharacterUnit.HasHealth(): return true");
            if (unitController == null) {
                Debug.LogWarning("UnitNamePlateController.HasHealth(): unitcontroller is null");
            }
            if (unitController.CharacterUnit == null) {
                Debug.LogWarning($"{unitController.gameObject.name}.UnitNamePlateController.HasHealth(): unitcontroller.CharacterUnit is null");
            }
            if (unitController.BaseCharacter != null && unitController.CharacterStats != null) {
                return unitController.CharacterStats.HasHealthResource;
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

            // player is always green
            if (playerManager.UnitController != null && unitController == playerManager.UnitController) {
                return Color.green;
            }

            // this is not the player, check for faction
            if (Faction == null) {
                return Color.white;
            }

            // faction is not null, check for cutscene
            if (uIManager.CutSceneBarController.CurrentCutscene == null) {
                return Faction.GetFactionColor(playerManager, NamePlateUnit);
            }

            // cutscene is not null, check for faction color setting
            if (uIManager.CutSceneBarController.CurrentCutscene.UseDefaultFactionColors == true) {
                return Faction.GetFactionColor();
            }

            return Color.white;
        }

        public override string GetNamePlateString() {

            Color textColor = GetTextColor();

                string nameString = string.Empty;
                string tagString = string.Empty;
                if (playerManager.UnitController == null ||unitController != playerManager.UnitController || PlayerPrefs.GetInt("ShowPlayerName") == 1) {
                    // player is not spawned, or this is not the player, or player is allowed to show name
                    nameString = UnitDisplayName;
                }

                // faction is lowest priority
                if (playerManager.UnitController == null || unitController != playerManager.UnitController || PlayerPrefs.GetInt("ShowPlayerFaction") == 1) {
                    if (SuppressFaction == false) {
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
                    //Debug.Log(namePlateUnit.DisplayName + ".NamePlateController.SetCharacterName(): faction and name are both not empty");
                }

                //Debug.Log($"{unitNamePlateController.NamePlateUnit.DisplayName}.NamePlateController.SetCharacterName(): setting character name text: {nameString}{newLineString}{factionString}");
                return $"<color=#{ColorUtility.ToHtmlStringRGB(textColor)}>{nameString}{newLineString}{tagString}</color>";
        }


    }


}