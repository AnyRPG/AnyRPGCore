using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Craft Ability", menuName = "Abilities/Effects/CraftAbility")]
public class CraftAbility : DirectAbility {

    public override void Cast(BaseCharacter source, GameObject target, Vector3 GroundTarget) {
        //Debug.Log("CraftAbility.Cast(" + source.name + ", " + target.name + ")");
        PopupWindowManager.MyInstance.craftingWindow.MyCloseableWindowContents.OnOpenWindowHandler += InitWindow;
        PopupWindowManager.MyInstance.craftingWindow.OpenWindow();
    }

    public void InitWindow(ICloseableWindowContents craftingUI) {
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
