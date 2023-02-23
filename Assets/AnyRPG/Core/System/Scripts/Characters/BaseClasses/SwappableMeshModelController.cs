using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AnyRPG {
    public class SwappableMeshModelController : ModelAppearanceController {

        protected SwappableMeshModelOptions modelOptions = null;

        private Dictionary<string, SwappableMeshOptionGroup> optionGroups = new Dictionary<string, SwappableMeshOptionGroup>();
        private Dictionary<string, List<string>> optionGroupMembers = new Dictionary<string, List<string>>();


        public SwappableMeshModelController(UnitController unitController, UnitModelController unitModelController, SystemGameManager systemGameManager, SwappableMeshModelOptions modelOptions)
            : base(unitController, unitModelController, systemGameManager) {
            this.modelOptions = modelOptions;

            // populate the group dictionaries
            foreach (SwappableMeshOptionGroup modelGroup in ModelOptions.MeshGroups) {
                // option group dictionary
                optionGroups.Add(modelGroup.GroupName, modelGroup);

                // option group members dictionary
                List<string> groupMembers = new List<string>();
                foreach (SwappableMeshOptionChoice optionChoice in modelGroup.Meshes) {
                    groupMembers.Add(optionChoice.MeshName);
                }
                optionGroupMembers.Add(modelGroup.GroupName, groupMembers);
            }
        }

        public SwappableMeshModelOptions ModelOptions { get => modelOptions; }

        public override T GetModelAppearanceController<T>() {
            return this as T;
        }

        public void SetGroupChoice(string groupName, string optionChoice) {
            
            if (optionGroups.ContainsKey(groupName) == false) {
                // option group did not exist
                return;
            }

            if (unitModelController.UnitModel == null) {
                // could not find the model to search
                return;
            }

            // enable correct mesh
            foreach (Transform child in unitModelController.UnitModel.transform) {
                if (child.name == optionChoice) {
                    // enable chosen mesh
                    child.gameObject.SetActive(true);
                } else if (optionGroupMembers[groupName].Contains(child.name)) {
                    // disable mesh that is in group but not chosen
                    child.gameObject.SetActive(false);
                }
            }
        }



        /*
        public void SetModelOptions(SwappableMeshModelOptions modelOptions) {
            this.modelOptions = modelOptions;
        }
        */


    }

}