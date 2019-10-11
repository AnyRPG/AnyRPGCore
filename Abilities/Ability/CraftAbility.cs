using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Craft Ability", menuName = "Abilities/Effects/CraftAbility")]
public class CraftAbility : DirectAbility {

    public override bool Cast(BaseCharacter source, GameObject target, Vector3 GroundTarget) {
        //Debug.Log("CraftAbility.Cast(" + source.name + ", " + target.name + ")");
        PopupWindowManager.MyInstance.craftingWindow.MyCloseableWindowContents.OnOpenWindow += InitWindow;
        PopupWindowManager.MyInstance.craftingWindow.OpenWindow();

        // ok to always do this since crafting abilities shouldn't really require weapon affinities?
        // at least not unless we implement some kind of "you must have a fishing rod equipped to fish logic", but I prefer just needing it in your backpack or not needing it at all for now
        // and that stuff applies more to gathering than crafting anyway, unless we want to require equipping sewing needles ;)
        return true;
    }

    public void InitWindow(ICloseableWindowContents craftingUI) {
        //Debug.Log("CraftAbility.InitWindow()");
        (craftingUI as CraftingUI).ShowRecipes(this);
    }

    public override bool CanUseOn(GameObject target, BaseCharacter source) {
        return true;
    }

    public List<Recipe> GetRecipes() {
        //Debug.Log("CraftAbility.GetRecipes() this: " + this.name);
        List<Recipe> returnList = new List<Recipe>();
        foreach (Recipe recipe in SystemRecipeManager.MyInstance.GetResourceList()) {
            if (SystemResourceManager.MatchResource(recipe.MyCraftAbility.MyName, MyName)) {
                returnList.Add(recipe);
            }
        }
        return returnList;
    }

    public override GameObject ReturnTarget(BaseCharacter source, GameObject target) {
        //Debug.Log("CraftAbility.ReturnTarget()");
        if (source == null) {
            //Debug.Log("CraftAbility.ReturnTarget(): source is null");
        } else {
            //Debug.Log("CraftAbility.ReturnTarget(): source: " + source.name);
        }
        if (target == null) {
            //Debug.Log("CraftAbility.ReturnTarget(): target is null");
        }
        return source.MyCharacterUnit.gameObject;
    }


}
