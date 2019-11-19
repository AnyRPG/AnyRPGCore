using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class CharacterFactionManager : MonoBehaviour {

        private BaseCharacter baseCharacter;

        //public Dictionary<Faction, float> dispositionDictionary = new Dictionary<Faction, float>();
        public List<FactionDisposition> MyDispositionDictionary;

        protected void Awake() {
            //Debug.Log(gameObject.name + ".PlayerFactionManager.Awake()");
            baseCharacter = GetComponent<BaseCharacter>();
        }

        protected void Start() {
        }

        // ignores if exiting, otherwise sets to amount.  This allows leaving and re-joining factions without losing reputation with them
        public void SetReputation(string newFactionName) {
            //Debug.Log(gameObject.name + ".PlayerFactionManager.SetReputation(" + newFaction + ")");
            foreach (FactionDisposition factionDisposition in MyDispositionDictionary) {
                if (factionDisposition.factionName == newFactionName) {
                    return;
                }
            }
            FactionDisposition _factionDisposition = new FactionDisposition();
            _factionDisposition.factionName = newFactionName;
            _factionDisposition.disposition = Faction.RelationWith(baseCharacter, newFactionName);
            MyDispositionDictionary.Add(_factionDisposition);
            SystemEventManager.MyInstance.NotifyOnReputationChange();
        }

        // adds to existing amount or sets to amount if not existing
        public void AddReputation(string factionName, int reputationAmount) {
            //Debug.Log(gameObject.name + ".PlayerFactionManager.AddReputation(" + realFaction.MyName + ", " + reputationAmount + ")");
            //bool foundReputation = false;
            foreach (FactionDisposition factionDisposition in MyDispositionDictionary) {
                //Debug.Log(gameObject.name + ".PlayerFactionManager.AddReputation(" + realFaction.MyName + ", " + reputationAmount + "): checking a disposition in my dictionary");
                if (SystemResourceManager.MatchResource(factionDisposition.factionName, factionName)) {
                    //Debug.Log(gameObject.name + ".PlayerFactionManager.AddReputation(" + realFaction.MyName + ", " + reputationAmount + "): checking a disposition in my dictionary MATCHED: adding reputation");
                    factionDisposition.disposition += (float)reputationAmount;
                    return;
                }
            }
            FactionDisposition _factionDisposition = new FactionDisposition();
            _factionDisposition.factionName = factionName;
            _factionDisposition.disposition = Faction.RelationWith(baseCharacter, factionName) + (float)reputationAmount;
            MyDispositionDictionary.Add(_factionDisposition);
            SystemEventManager.MyInstance.NotifyOnReputationChange();
        }

        public bool HasReputationModifier(Faction faction) {
            if (faction == null) {
                return false;
            }
            //Debug.Log(gameObject.name + ".CharacterFactionManager.HasReputationModifer(" + faction.MyName + "): searching for reputation modifer");

            // checking dictionary first
            //Debug.Log(gameObject.name + ".CharacterFactionManager.HasReputationModifer(" + faction.MyName + "): checking local disposition dictionary");
            foreach (FactionDisposition factionDisposition in MyDispositionDictionary) {
                if (SystemResourceManager.MatchResource(factionDisposition.factionName, faction.MyName)) {
                    //Debug.Log(gameObject.name + ".CharacterFactionManager.HasReputationModifer(" + faction.MyName + "): name matched a disposition in local dictionary");
                    return true;
                }
            }

            // checking status effects next
            //Debug.Log(gameObject.name + ".CharacterFactionManager.HasReputationModifer(" + faction.MyName + "): no match disposition dictionary, checking status effect buffs");
            if (baseCharacter != null && baseCharacter.MyCharacterStats != null && baseCharacter.MyCharacterStats.MyStatusEffects != null) {
                foreach (StatusEffectNode statusEffectNode in baseCharacter.MyCharacterStats.MyStatusEffects.Values) {
                    foreach (FactionDisposition factionDisposition in statusEffectNode.MyStatusEffect.MyFactionModifiers) {
                        //Debug.Log(gameObject.name + "Faction.RelationWith(" + faction.MyName + "): " + statusEffect.MyName + " had disposition: " + factionDisposition.factionName + ": " + factionDisposition.disposition);
                        if (SystemResourceManager.MatchResource(factionDisposition.factionName, faction.MyName)) {
                            //Debug.Log(gameObject.name + "Faction.RelationWith(" + faction.MyName + "): found special disposition in status effects and it matches the requested faction: " + factionDisposition.factionName + ": " + factionDisposition.disposition);
                            return true;
                        }
                    }
                }
            }
            //Debug.Log(gameObject.name + ".CharacterFactionManager.HasReputationModifer(" + faction.MyName + "): no match disposition dictionary or buffs, return false");
            return false;
        }

        public float GetReputationValue(Faction faction) {
            //Debug.Log(gameObject.name + ".CharacterFactionManager.RelationWith(" + faction.MyName + "): checking personal status dictionary and status effects to get special dispositions toward faction");

            // checking personal dictionary before status effects?
            foreach (FactionDisposition factionDisposition in MyDispositionDictionary) {
                if (SystemResourceManager.MatchResource(factionDisposition.factionName, faction.MyName)) {
                    //Debug.Log("CharacterFactionManager.RelationWith(" + faction.MyName + "): dictionary contained: " + faction.MyName + "; returning value: " + factionDisposition.disposition);
                    return factionDisposition.disposition;
                }
            }

            if (baseCharacter != null && baseCharacter.MyCharacterStats != null && baseCharacter.MyCharacterStats.MyStatusEffects != null) {
                // checking status effect disposition modifiers
                foreach (StatusEffectNode statusEffectNode in baseCharacter.MyCharacterStats.MyStatusEffects.Values) {
                    foreach (FactionDisposition factionDisposition in statusEffectNode.MyStatusEffect.MyFactionModifiers) {
                        //Debug.Log(gameObject.name + "Faction.RelationWith(" + faction.MyName + "): " + statusEffect.MyName + " had disposition: " + factionDisposition.factionName + ": " + factionDisposition.disposition);
                        if (SystemResourceManager.MatchResource(factionDisposition.factionName, faction.MyName)) {
                            //Debug.Log(gameObject.name + "Faction.RelationWith(" + faction.MyName + "): found special disposition in status effects and it matches the requested faction: " + factionDisposition.factionName + ": " + factionDisposition.disposition);
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