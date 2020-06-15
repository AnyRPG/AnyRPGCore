using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class CharacterFactionManager : MonoBehaviour {

        public event System.Action OnReputationChange = delegate { };

        private BaseCharacter baseCharacter;

        //[SerializeField]
        protected List<FactionDisposition> dispositionDictionary = new List<FactionDisposition>();

        //public Dictionary<Faction, float> dispositionDictionary = new Dictionary<Faction, float>();
        public List<FactionDisposition> MyDispositionDictionary {
            get {
                return dispositionDictionary;
            }
        }

        protected void Awake() {
            //Debug.Log(gameObject.name + ".PlayerFactionManager.Awake()");
            baseCharacter = GetComponent<BaseCharacter>();
        }

        protected void Start() {
        }

        public virtual void NotifyOnReputationChange() {
            OnReputationChange();
        }


        // ignores if exiting, otherwise sets to amount.  This allows leaving and re-joining factions without losing reputation with them
        public virtual void SetReputation(Faction newFaction) {
            //Debug.Log(gameObject.name + ".PlayerFactionManager.SetReputation(" + newFaction + ")");
            foreach (FactionDisposition factionDisposition in MyDispositionDictionary) {
                if (factionDisposition.MyFaction == newFaction) {
                    return;
                }
            }
            FactionDisposition _factionDisposition = new FactionDisposition();
            _factionDisposition.MyFaction = newFaction;
            _factionDisposition.disposition = Faction.RelationWith(baseCharacter, newFaction);
            MyDispositionDictionary.Add(_factionDisposition);
            OnReputationChange();
        }

        // adds to existing amount or sets to amount if not existing
        public virtual void AddReputation(Faction faction, int reputationAmount, bool notify = true) {
            //Debug.Log(gameObject.name + ".PlayerFactionManager.AddReputation(" + realFaction.MyName + ", " + reputationAmount + ")");
            //bool foundReputation = false;
            foreach (FactionDisposition factionDisposition in MyDispositionDictionary) {
                //Debug.Log(gameObject.name + ".PlayerFactionManager.AddReputation(" + realFaction.MyName + ", " + reputationAmount + "): checking a disposition in my dictionary");
                if (factionDisposition.MyFaction == faction) {
                    //Debug.Log(gameObject.name + ".PlayerFactionManager.AddReputation(" + realFaction.MyName + ", " + reputationAmount + "): checking a disposition in my dictionary MATCHED: adding reputation");
                    factionDisposition.disposition += (float)reputationAmount;
                    return;
                }
            }
            FactionDisposition _factionDisposition = new FactionDisposition();
            _factionDisposition.MyFaction = faction;
            _factionDisposition.disposition = Faction.RelationWith(baseCharacter, faction) + (float)reputationAmount;
            MyDispositionDictionary.Add(_factionDisposition);
            if (notify) {
                OnReputationChange();
            }
        }

        public bool HasReputationModifier(Faction faction) {
            if (faction == null) {
                return false;
            }
            //Debug.Log(gameObject.name + ".CharacterFactionManager.HasReputationModifer(" + faction.MyName + "): searching for reputation modifer");

            // checking dictionary first
            //Debug.Log(gameObject.name + ".CharacterFactionManager.HasReputationModifer(" + faction.MyName + "): checking local disposition dictionary");
            foreach (FactionDisposition factionDisposition in MyDispositionDictionary) {
                if (factionDisposition.MyFaction == faction) {
                    //Debug.Log(gameObject.name + ".CharacterFactionManager.HasReputationModifer(" + faction.MyName + "): name matched a disposition in local dictionary");
                    return true;
                }
            }

            // checking status effects next
            //Debug.Log(gameObject.name + ".CharacterFactionManager.HasReputationModifer(" + faction.MyName + "): no match disposition dictionary, checking status effect buffs");
            if (baseCharacter != null && baseCharacter.CharacterStats != null && baseCharacter.CharacterStats.StatusEffects != null) {
                foreach (StatusEffectNode statusEffectNode in baseCharacter.CharacterStats.StatusEffects.Values) {
                    /*
                    if (statusEffectNode.MyStatusEffect == null) {
                        Debug.LogError("STATUS EFFECT IS NULL");
                    }
                    if (statusEffectNode.MyStatusEffect.MyFactionModifiers == null) {
                        Debug.LogError("FACTION MODIFIERS IS NULL");
                    }
                    */
                    if (statusEffectNode != null && statusEffectNode.MyStatusEffect != null && statusEffectNode.MyStatusEffect.MyFactionModifiers != null) {
                        foreach (FactionDisposition factionDisposition in statusEffectNode.MyStatusEffect.MyFactionModifiers) {
                            //Debug.Log(gameObject.name + "Faction.RelationWith(" + faction.MyName + "): " + statusEffect.MyName + " had disposition: " + factionDisposition.factionName + ": " + factionDisposition.disposition);
                            if (factionDisposition.MyFaction == faction) {
                                //Debug.Log(gameObject.name + "Faction.RelationWith(" + faction.MyName + "): found special disposition in status effects and it matches the requested faction: " + factionDisposition.factionName + ": " + factionDisposition.disposition);
                                return true;
                            }
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
                if (factionDisposition.MyFaction == faction) {
                    //Debug.Log("CharacterFactionManager.RelationWith(" + faction.MyName + "): dictionary contained: " + faction.MyName + "; returning value: " + factionDisposition.disposition);
                    return factionDisposition.disposition;
                }
            }

            if (baseCharacter != null && baseCharacter.CharacterStats != null && baseCharacter.CharacterStats.StatusEffects != null) {
                // checking status effect disposition modifiers
                foreach (StatusEffectNode statusEffectNode in baseCharacter.CharacterStats.StatusEffects.Values) {
                    foreach (FactionDisposition factionDisposition in statusEffectNode.MyStatusEffect.MyFactionModifiers) {
                        //Debug.Log(gameObject.name + "Faction.RelationWith(" + faction.MyName + "): " + statusEffect.MyName + " had disposition: " + factionDisposition.factionName + ": " + factionDisposition.disposition);
                        if (factionDisposition.MyFaction == faction) {
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