using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class CharacterRecipeManager : MonoBehaviour {

        protected BaseCharacter baseCharacter;

        protected Dictionary<string, Recipe> recipeList = new Dictionary<string, Recipe>();

        public BaseCharacter MyBaseCharacter {
            get => baseCharacter;
            set => baseCharacter = value;
        }

        public Dictionary<string, Recipe> RecipeList { get => recipeList; }

        //public List<string> MyRecipeList { get => recipeList;}

        protected bool eventSubscriptionsInitialized = false;

        protected virtual void Awake() {
            //Debug.Log("CharacteRecipeManager.Awake()");
            baseCharacter = GetComponent<BaseCharacter>();
        }

        protected virtual void Start() {
            //Debug.Log("CharacterRecipeManager.Start()");
            CreateEventSubscriptions();
            UpdateRecipeList(baseCharacter.CharacterStats.Level);
        }

        public virtual void OnDisable() {
            CleanupEventSubscriptions();
        }

        public virtual void CreateEventSubscriptions() {
        }

        public virtual void CleanupEventSubscriptions() {
        }

        /*
        public List<string> GetRecipeList() {

            return recipeList.Keys;
        }
        */

        public virtual void UpdateRecipeList(int newLevel) {
            //Debug.Log("CharacterRecipeManager.UpdateRecipeList(" + newLevel + ")");
            foreach (Recipe recipe in SystemRecipeManager.MyInstance.GetResourceList()) {
                //Debug.Log("CharacterRecipeManager.UpdateRecipeList(" + newLevel + "): evaluating recipe: " + recipe.MyName);
                foreach (Skill skill in baseCharacter.CharacterSkillManager.MySkillList.Values) {
                    //Debug.Log("CharacterRecipeManager.UpdateRecipeList(" + newLevel + "): recipe: " + recipe.MyName + "evaluating skill: " + skill.MyName);
                    if (!HasRecipe(recipe) && recipe.RequiredLevel <= newLevel && recipe.AutoLearn == true && skill.MyAbilityList.Contains(recipe.CraftAbility as BaseAbility)) {
                        LearnRecipe(recipe);
                    }/*
                    else {
                        Debug.Log("CharacterRecipeManager.UpdateRecipeList(" + newLevel + "): recipe: " +
                            recipe.MyName + "evaluating skill: " + skill.MyName + 
                            "autolearn: " + recipe.AutoLearn +
                            "level: " + recipe.RequiredLevel + 
                            "hasrecipe: " + HasRecipe(recipe) + 
                            "skillcontainsability: " + skill.MyAbilityList.Contains(recipe.CraftAbility));
                    }
                    */
                }
            }
        }

        public bool HasRecipe(Recipe checkRecipe) {
            //Debug.Log(gameObject.name + ".CharacterRecipeManager.HasRecipe(" + checkRecipe.MyName + ")");
            if (recipeList.ContainsValue(checkRecipe)) {
                return true;
            }
            return false;
        }

        public void LearnRecipe(Recipe newRecipe) {
            //Debug.Log("CharacterRecipeManager.LearnRecipe(" + newRecipe.name + ")");
            if (!recipeList.ContainsValue(newRecipe)) {
                recipeList[SystemResourceManager.prepareStringForMatch(newRecipe.MyDisplayName)] = newRecipe;
                EventParamProperties eventParamProperties = new EventParamProperties();
                eventParamProperties.simpleParams.StringParam = newRecipe.MyDisplayName;
                SystemEventManager.TriggerEvent("OnRecipeListChanged", eventParamProperties);
            }
        }

        public void LoadRecipe(string recipeName) {
            //Debug.Log("CharacterRecipeManager.LoadRecipe(" + recipeName + ")");
            string keyName = SystemResourceManager.prepareStringForMatch(recipeName);
            if (!recipeList.ContainsKey(keyName)) {
                recipeList[keyName] = SystemRecipeManager.MyInstance.GetResource(recipeName);
            }
        }


        public void UnlearnRecipe(Recipe oldRecipe) {
            if (recipeList.ContainsValue(oldRecipe)) {
                recipeList.Remove(SystemResourceManager.prepareStringForMatch(oldRecipe.MyDisplayName));
            }
        }


    }

}