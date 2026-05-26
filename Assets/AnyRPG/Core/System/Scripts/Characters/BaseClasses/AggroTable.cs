using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class AggroTable {

        private UnitController unitController;

        private bool threatLocked = false;

        private AggroNode lockedNode = null;

        private List<AggroNode> aggroNodes = new List<AggroNode>();

        public List<AggroNode> AggroNodes { get => aggroNodes; set => aggroNodes = value; }

        // testing - don't use local variables to avoid garbage collection
        private List<AggroNode> removeNodes = new List<AggroNode>();
        private AggroNode topNode = null;

        public AggroNode TopAgroNode {
            get {
                removeNodes.Clear();
                // we need to remove stale nodes on each check because we could have aggro'd a target that was already fighting with someone and not got the message it died if we didn't hit it first
                foreach (AggroNode node in aggroNodes) {
                    if (node.aggroTarget?.gameObject == null
                        || node.aggroTarget.gameObject.activeInHierarchy == false
                        || node.aggroTarget.IsInitialized == false
                        || node.aggroTarget.CharacterStats.IsAlive == false
                        || Faction.RelationWith(node.aggroTarget, unitController) > -1) {
                        //Debug.Log($"AggroTable.TopAgroNode: isInitialized: {node.aggroTarget.IsInitialized}");
                        //Debug.Log(node.aggroTarget.name + ". alive: " + node.aggroTarget.MyCharacter.MyCharacterStats.IsAlive);
                        // we could be in combat with someone who has switched faction from a faction buff mid combat or died
                        removeNodes.Add(node);
                    } else {
                        //Debug.Log("node.aggroTarget: " + node.aggroTarget == null ? "null" : node.aggroTarget.name + "; isalive: " + node.aggroTarget.MyCharacter.MyCharacterStats.IsAlive);
                    }
                }
                foreach (AggroNode node in removeNodes) {
                    //Debug.Log("Clearning null node from aggro list");
                    aggroNodes.Remove(node);
                    if (threatLocked && node == lockedNode) {
                        UnLockAgro();
                    }
                }
                if (aggroNodes.Count == 0) {
                    return null;
                }
                topNode = aggroNodes[0];
                if (threatLocked) {
                    if (lockedNode == null) {
                        Debug.LogWarning("AggroTable.TopAgroNode.Get() about to assign null locked node");
                    }
                    topNode = lockedNode;
                } else {
                    foreach (AggroNode node in aggroNodes) {
                        if (node.aggroValue > topNode.aggroValue) {
                            topNode = node;
                        }
                    }
                }
                return topNode;
            }
        }

        public AggroTable(UnitController unitController) {
            //Debug.Log("AggroTable.AggroTable(" + baseCharacter.gameObject.name + ")");
            this.unitController = unitController;
        }

        public void LockAgro() {
            //Debug.Log("AggroTable.LockAgro()");
            lockedNode = TopAgroNode;

            // ordering matters here, have to set the locked node first
            threatLocked = true;
        }

        public void UnLockAgro() {
            //Debug.Log("AggroTable.UnLockAgro()");
            threatLocked = false;
            lockedNode = null;
        }


        /// <summary>
        /// Add an object to the aggro table or update its agro amount if it is already in the aggro table
        /// RETURN TRUE IF THIS IS A NEW ENTRY, FALSE IF NOT
        /// </summary>
        /// <param name="_aggroTable"></param>
        /// <param name="targetUnitController"></param>
        /// <param name="aggroAmount"></param>
        /// return true if new entry to the table
        public bool AddToAggroTable(UnitController targetUnitController, int aggroAmount) {

            if (targetUnitController.CharacterStats.IsAlive == false) {
                return false;
            }

            // testing clamp agro amount to 1 or above to prevent getting no credit from bosses that are immune to damage
            aggroAmount = (int)Mathf.Clamp(aggroAmount, 1f, Mathf.Infinity);

            bool isAlreadyInAggroTable = false;

            // if aggro table is empty, skip the table scan because the target must be added
            if (aggroNodes.Count != 0) {
                // loop through the table and see if the target is already in it.
                foreach (AggroNode aggroNode in aggroNodes) {
                    if (aggroNode.aggroTarget == targetUnitController) {
                        aggroNode.aggroValue += aggroAmount;
                        isAlreadyInAggroTable = true;
                    }
                }
            }

            if (!isAlreadyInAggroTable) {
                AggroNode aggroNode = new AggroNode();
                aggroNode.aggroTarget = targetUnitController;
                aggroNode.aggroValue = aggroAmount;
                aggroNodes.Add(aggroNode);
            }
            //if he is, add the damage amount to the agro.
            //if not, add him to the table with the damage amount.
            //Debug.Log(baseCharacter.gameObject.name + ".AddToAggroTable() isAlreadyInAggroTable: " + isAlreadyInAggroTable);
            return !isAlreadyInAggroTable;
        }

        public bool AggroTableContains(UnitController target) {
            // if aggro table is empty, skip the table scan because the target must be added
            if (aggroNodes.Count != 0) {
                // loop through the table and see if the target is already in it.
                foreach (AggroNode aggroNode in aggroNodes) {
                    if (aggroNode.aggroTarget == target) {
                        return true;
                        //Debug.Log($"{gameObject.name} adding " + aggroAmount.ToString() + " aggro to entry: " + target.name + "; total: " + aggroNode.aggroValue.ToString());
                    }
                }
            }
            return false;
        }


        /// <summary>
        /// Meant to be called internally. Remove a single object and broadcast to them to clear us from their aggro table if they have less than 0 agro
        /// </summary>
        /// <param name="targetUnitController"></param>
        public void AttemptRemoveAndBroadcast(UnitController targetUnitController) {
            foreach (AggroNode aggroNode in aggroNodes.ToArray()) {
                if (aggroNode.aggroTarget == targetUnitController && aggroNode.aggroValue < 0) {
                    aggroNode.aggroTarget.CharacterCombat.AggroTable.ClearSingleTarget(unitController);
                    aggroNodes.Remove(aggroNode);
                }
            }
            //TryToDropCombat();
        }

        /// <summary>
        /// Meant to be called internally. Remove a single object and broadcast to them to clear us from their aggro table.
        /// </summary>
        /// <param name="targetUnitController"></param>
        public void RemoveAndBroadcast(UnitController targetUnitController) {
            foreach (AggroNode aggroNode in aggroNodes.ToArray()) {
                if (aggroNode.aggroTarget == targetUnitController) {
                    //Debug.Log(baseCharacter.name + ": Removing " + target.name + " from aggro table");
                    aggroNode.aggroTarget.CharacterCombat.AggroTable.ClearSingleTarget(unitController);
                    aggroNodes.Remove(aggroNode);
                }
            }
            //TryToDropCombat();
        }

        /// <summary>
        /// Meant to be called internally. Clear the table and broadcast to all members to remove us from their aggro tables
        /// </summary>
        public void ClearAndBroadcast() {
            foreach (AggroNode aggroNode in aggroNodes.ToArray()) {
                if (aggroNode.aggroTarget.CharacterCombat != null) {
                    aggroNode.aggroTarget.CharacterCombat.AggroTable.ClearSingleTarget(unitController);
                }
                aggroNodes.Remove(aggroNode);
            }
            //TryToDropCombat();
        }

        public void ClearTable() {
            aggroNodes.Clear();
        }


        /// <summary>
        /// Called from external objects to drop themselves from our table (ie, they died, evaded, feigned death, vanished, invisible etc
        /// </summary>
        /// <param name="targetUnitController"></param>
        public void ClearSingleTarget(UnitController targetUnitController) {
            if (targetUnitController == null) {
                //Debug.Log("ClearSingleTarget(); target is null");
            }
            if (aggroNodes == null) {
                //Debug.Log("ClearSingleTarget(); aggroNodes is null");
            }
            if (unitController == null) {
                //Debug.Log("ClearSingleTarget(); baseCharacter is null");
            }
            //Debug.Log(baseCharacter.name + ": Clearing " + target.name + " from aggro table with size(" + aggroNodes.Count + ") due to external signal");
            foreach (AggroNode aggroNode in aggroNodes.ToArray()) {
                //Debug.Log(baseCharacter.name + ": Clearing " + target.name + " from aggro table with size(" + aggroNodes.Count + ") due to external signal: looking for target...");
                if (aggroNode.aggroTarget == targetUnitController) {
                    //Debug.Log(baseCharacter.name + ": Clearing " + target.name + " from aggro table with size(" + aggroNodes.Count + ") due to external signal: found target!");
                    aggroNodes.Remove(aggroNode);
                }
            }
        }

    }

}