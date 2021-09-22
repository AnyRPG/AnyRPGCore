using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class CharacterFactionManager {

        public event System.Action OnReputationChange = delegate { };

        private BaseCharacter baseCharacter;

        //[SerializeField]
        protected List<FactionDisposition> dispositionDictionary = new List<FactionDisposition>();

        //public Dictionary<Faction, float> dispositionDictionary = new Dictionary<Faction, float>();
        public List<FactionDisposition> DispositionDictionary {
            get {
                return dispositionDictionary;
            }
        }

        public CharacterFactionManager(BaseCharacter baseCharacter) {
            this.baseCharacter = baseCharacter;
        }

        public virtual void NotifyOnReputationChange() {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterFactionmanager.NotifyOnReputationChange()");
            OnReputationChange();
            if (baseCharacter.UnitController != null) {
                baseCharacter.UnitController.NotifyOnReputationChange();
            }
        }

        // ignores if existing, otherwise sets to amount.  This allows leaving and re-joining factions without losing reputation with them
        public virtual void SetReputation(Faction newFaction) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterFactionmanager.SetReputation(" + (newFaction == null ? "null" : newFaction.DisplayName) + ")");
            foreach (FactionDisposition factionDisposition in DispositionDictionary) {
                if (factionDisposition.Faction == newFaction) {
                    return;
                }
            }
            FactionDisposition _factionDisposition = new FactionDisposition();
            _factionDisposition.Faction = newFaction;
            _factionDisposition.disposition = Faction.RelationWith(baseCharacter, newFaction);
            DispositionDictionary.Add(_factionDisposition);
            NotifyOnReputationChange();
        }

        // loads reputation values from a save file, ignoring all existing values
        public virtual void LoadReputation(Faction faction, int reputationAmount) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterFactionmanager.LoadReputation(" + faction.DisplayName + ", " + reputationAmount + ")");
            bool foundReputation = false;
            foreach (FactionDisposition factionDisposition in DispositionDictionary) {
                //Debug.Log(gameObject.name + ".PlayerFactionManager.AddReputation(" + realFaction.DisplayName + ", " + reputationAmount + "): checking a disposition in my dictionary");
                if (factionDisposition.Faction == faction) {
                    //Debug.Log(baseCharacter.gameObject.name + ".PlayerFactionManager.AddReputation(" + faction.DisplayName + ", " + reputationAmount + ") existing reputation: " + factionDisposition.disposition);
                    factionDisposition.disposition = (float)reputationAmount;
                    foundReputation = true;
                    break;
                }
            }
            if (!foundReputation) {
                FactionDisposition _factionDisposition = new FactionDisposition();
                _factionDisposition.Faction = faction;
                _factionDisposition.disposition = (float)reputationAmount;
                DispositionDictionary.Add(_factionDisposition);
            }
        }

        // adds to existing amount or sets to amount if not existing
        public virtual void AddReputation(Faction faction, int reputationAmount, bool notify = true) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterFactionmanager.AddReputation(" + faction.DisplayName + ", " + reputationAmount + ", " + notify + ")");
            bool foundReputation = false;
            foreach (FactionDisposition factionDisposition in DispositionDictionary) {
                //Debug.Log(gameObject.name + ".PlayerFactionManager.AddReputation(" + realFaction.DisplayName + ", " + reputationAmount + "): checking a disposition in my dictionary");
                if (factionDisposition.Faction == faction) {
                    //Debug.Log(baseCharacter.gameObject.name + ".PlayerFactionManager.AddReputation(" + faction.DisplayName + ", " + reputationAmount + ") existing reputation: " + factionDisposition.disposition);
                    factionDisposition.disposition += (float)reputationAmount;
                    foundReputation = true;
                    break;
                }
            }
            if (!foundReputation) {
                FactionDisposition _factionDisposition = new FactionDisposition();
                _factionDisposition.Faction = faction;
                _factionDisposition.disposition = Faction.RelationWith(baseCharacter, faction) + (float)reputationAmount;
                DispositionDictionary.Add(_factionDisposition);
            }
            if (notify) {
                NotifyOnReputationChange();
            }
        }

        public bool HasReputationModifier(Faction faction) {
            if (faction == null) {
                return false;
            }

            // checking dictionary first
            List<FactionDisposition> usedDictionary = DispositionDictionary;
            if (baseCharacter.UnitController != null && baseCharacter.UnitController.UnderControl == true) {
                usedDictionary = baseCharacter.UnitController.MasterUnit.CharacterFactionManager.DispositionDictionary;
            }
            foreach (FactionDisposition factionDisposition in usedDictionary) {
                if (factionDisposition.Faction == faction) {
                    return true;
                }
            }

            // checking status effects next
            if (baseCharacter != null && baseCharacter.CharacterStats != null && baseCharacter.CharacterStats.StatusEffects != null) {
                foreach (StatusEffectNode statusEffectNode in baseCharacter.CharacterStats.StatusEffects.Values) {
                    if (statusEffectNode != null && statusEffectNode.StatusEffect != null && statusEffectNode.StatusEffect.FactionModifiers != null) {
                        foreach (FactionDisposition factionDisposition in statusEffectNode.StatusEffect.FactionModifiers) {
                            if (factionDisposition.Faction == faction) {
                                return true;
                            }
                        }

                    }
                }
            }
            return false;
        }

        public float GetReputationValue(Faction faction) {
            //Debug.Log(gameObject.name + ".CharacterFactionManager.RelationWith(" + faction.DisplayName + "): checking personal status dictionary and status effects to get special dispositions toward faction");

            List<FactionDisposition> usedDictionary = DispositionDictionary;
            if (baseCharacter?.UnitController?.UnderControl == true) {
                usedDictionary = baseCharacter.UnitController.MasterUnit.CharacterFactionManager.DispositionDictionary;
            }

            // checking personal dictionary before status effects?
            foreach (FactionDisposition factionDisposition in usedDictionary) {
                if (factionDisposition.Faction == faction) {
                    //Debug.Log("CharacterFactionManager.RelationWith(" + faction.DisplayName + "): dictionary contained: " + faction.DisplayName + "; returning value: " + factionDisposition.disposition);
                    return factionDisposition.disposition;
                }
            }

            if (baseCharacter != null && baseCharacter.CharacterStats != null && baseCharacter.CharacterStats.StatusEffects != null) {
                // checking status effect disposition modifiers
                foreach (StatusEffectNode statusEffectNode in baseCharacter.CharacterStats.StatusEffects.Values) {
                    foreach (FactionDisposition factionDisposition in statusEffectNode.StatusEffect.FactionModifiers) {
                        if (factionDisposition.Faction == faction) {
                            return factionDisposition.disposition;
                        }
                    }
                }
            }

            // hmm, should this return Faction.getreputationvalue instead?
            return 0f;
        }


    }

}