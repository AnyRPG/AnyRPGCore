using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class SwappableMeshModelProvider : CharacterModelProvider {

        [SerializeField]
        private bool useInlineOptions = true;

        [SerializeField]
        private SwappableMeshModelOptions inlineOptions = new SwappableMeshModelOptions();

        [SerializeField]
        [ResourceSelector(resourceType = typeof(SwappableMeshModelProfile))]
        private string sharedOptions = string.Empty;

        private SwappableMeshModelOptions usedOptions = null;

        public override ModelAppearanceController GetAppearanceController(UnitController unitController, UnitModelController unitModelController, SystemGameManager systemGameManager) {
            //Debug.Log("SwappableMeshModelProvider.GetAppearanceController()");
            
            return new SwappableMeshModelController(unitController, unitModelController, systemGameManager, usedOptions);
        }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            SetupScriptableObjects();
        }

        private void SetupScriptableObjects() {
            
            if (useInlineOptions == false) {
                if (sharedOptions != string.Empty) {
                    // get shared options and overwrite inline options
                    SwappableMeshModelProfile tmpProfile = systemDataFactory.GetResource<SwappableMeshModelProfile>(sharedOptions);
                    if (tmpProfile != null) {
                        usedOptions = tmpProfile.ModelOptions;
                    } else {
                        Debug.LogError("SwappableMeshModelProvider.SetupScriptableObjects(): Could not find model profile : " + sharedOptions + " while inititalizing.  CHECK INSPECTOR");
                    }
                }
            } else {
                usedOptions = inlineOptions;
            }
        }

    }

}

