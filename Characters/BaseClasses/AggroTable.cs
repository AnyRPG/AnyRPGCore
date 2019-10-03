using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AggroTable {

    private BaseCharacter baseCharacter;

    private List<AggroNode> aggroNodes = new List<AggroNode>();

    public List<AggroNode> MyAggroNodes { get => aggroNodes; set => aggroNodes = value; }

    public AggroNode MyTopAgroNode {
        get {
            List<AggroNode> removeNodes = new List<AggroNode>();
            // we need to remove stale nodes on each check because we could have aggro'd a target that was already fighting with someone and not got the message it died if we didn't hit it first
            foreach (AggroNode node in aggroNodes) {
                if (node.aggroTarget == null || Faction.RelationWith(node.aggroTarget.MyCharacter, MyBaseCharacter) > -1 || node.aggroTarget.MyCharacter.MyCharacterStats.IsAlive == false) {
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
            }
            if (aggroNodes.Count == 0) {
                return null;
            }
            AggroNode topNode = aggroNodes[0];
            foreach (AggroNode node in aggroNodes) {
                if (node.aggroValue > topNode.aggroValue) {
                    topNode = node;
                }
            }
            return topNode;
        }
    }

    public BaseCharacter MyBaseCharacter { get => baseCharacter; set => baseCharacter = value; }

    public AggroTable (BaseCharacter baseCharacter) {
        //Debug.Log("AggroTable.AggroTable(" + baseCharacter.gameObject.name + ")");
        this.baseCharacter = baseCharacter;
    }


    /// <summary>
    /// Add an object to the aggro table or update its agro amount if it is already in the aggro table
    /// RETURN TRUE IF THIS IS A NEW ENTRY, FALSE IF NOT
    /// </summary>
    /// <param name="_aggroTable"></param>
    /// <param name="targetCharacterUnit"></param>
    /// <param name="aggroAmount"></param>
    /// return true if new entry to the table
    public bool AddToAggroTable(CharacterUnit targetCharacterUnit, int aggroAmount) {
        //Debug.Log("AggroTable.AddToAggroTable(): target: " + target.name + "; amount: " + aggroAmount);

        if (targetCharacterUnit.MyCharacter.MyCharacterStats.IsAlive == false) {
            return false;
        }

        bool isAlreadyInAggroTable = false;

        // if aggro table is empty, skip the table scan because the target must be added
        if (aggroNodes.Count != 0) {
            // loop through the table and see if the target is already in it.
            foreach (AggroNode aggroNode in aggroNodes) {
                if (aggroNode.aggroTarget == targetCharacterUnit) {
                    aggroNode.aggroValue += aggroAmount;
                    isAlreadyInAggroTable = true;
                    //Debug.Log(gameObject.name + " adding " + aggroAmount.ToString() + " aggro to entry: " + target.name + "; total: " + aggroNode.aggroValue.ToString());
                }
            }
        }

        if (!isAlreadyInAggroTable) {
            //Debug.Log(gameObject.name + " adding new entry " + target.name + " to aggro table");
            AggroNode aggroNode = new AggroNode();
            aggroNode.aggroTarget = targetCharacterUnit;
            aggroNode.aggroValue = aggroAmount;
            aggroNodes.Add(aggroNode);
        }
        //if he is, add the damage amount to the agro.
        //if not, add him to the table with the damage amount.

        return !isAlreadyInAggroTable;
    }

    public bool AggroTableContains(CharacterUnit target) {
        // if aggro table is empty, skip the table scan because the target must be added
        if (aggroNodes.Count != 0) {
            // loop through the table and see if the target is already in it.
            foreach (AggroNode aggroNode in aggroNodes) {
                if (aggroNode.aggroTarget == target) {
                    return true;
                    //Debug.Log(gameObject.name + " adding " + aggroAmount.ToString() + " aggro to entry: " + target.name + "; total: " + aggroNode.aggroValue.ToString());
                }
            }
        }
        return false;
    }


    /// <summary>
    /// Meant to be called internally. Remove a single object and broadcast to them to clear us from their aggro table if they have less than 0 agro
    /// </summary>
    /// <param name="targetCharacterUnit"></param>
    public void AttemptRemoveAndBroadcast(CharacterUnit targetCharacterUnit) {
        Debug.Log((baseCharacter == null ? "null" : baseCharacter.MyCharacterName) + ".AggroTable.AttemptRemoveAndBroadCast(" + targetCharacterUnit + ")");
        foreach (AggroNode aggroNode in aggroNodes.ToArray()) {
            if (aggroNode.aggroTarget == targetCharacterUnit && aggroNode.aggroValue < 0) {
                Debug.Log(baseCharacter.name + ": Removing " + targetCharacterUnit.name + " from aggro table");
                aggroNode.aggroTarget.MyCharacter.MyCharacterCombat.MyAggroTable.ClearSingleTarget(baseCharacter.MyCharacterUnit);
                aggroNodes.Remove(aggroNode);
            }
        }
        //TryToDropCombat();
    }

    /// <summary>
    /// Meant to be called internally. Remove a single object and broadcast to them to clear us from their aggro table.
    /// </summary>
    /// <param name="target"></param>
    public void RemoveAndBroadcast(CharacterUnit target) {
        foreach (AggroNode aggroNode in aggroNodes.ToArray()) {
            if (aggroNode.aggroTarget == target) {
                Debug.Log(baseCharacter.name + ": Removing " + target.name + " from aggro table");
                aggroNode.aggroTarget.MyCharacter.MyCharacterCombat.MyAggroTable.ClearSingleTarget(baseCharacter.MyCharacterUnit);
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
            Debug.Log(baseCharacter.name + ": Removing " + aggroNode.aggroTarget.name + " from aggro table");
            CharacterCombat _characterCombat = aggroNode.aggroTarget.GetComponent<CharacterUnit>().MyCharacter.MyCharacterCombat as CharacterCombat;
            if (_characterCombat != null) {
                _characterCombat.MyAggroTable.ClearSingleTarget(baseCharacter.MyCharacterUnit);
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
    /// <param name="target"></param>
    public void ClearSingleTarget(CharacterUnit target) {
        if (target == null) {
            //Debug.Log("ClearSingleTarget(); target is null");
        }
        if (aggroNodes == null) {
            //Debug.Log("ClearSingleTarget(); aggroNodes is null");
        }
        if (baseCharacter == null) {
            //Debug.Log("ClearSingleTarget(); baseCharacter is null");
        }
        //Debug.Log(baseCharacter.name + ": Clearing " + target.name + " from aggro table with size(" + aggroNodes.Count + ") due to external signal");
        foreach (AggroNode aggroNode in aggroNodes.ToArray()) {
            //Debug.Log(baseCharacter.name + ": Clearing " + target.name + " from aggro table with size(" + aggroNodes.Count + ") due to external signal: looking for target...");
            if (aggroNode.aggroTarget == target) {
                //Debug.Log(baseCharacter.name + ": Clearing " + target.name + " from aggro table with size(" + aggroNodes.Count + ") due to external signal: found target!");
                aggroNodes.Remove(aggroNode);
            }
        }
    }

}
