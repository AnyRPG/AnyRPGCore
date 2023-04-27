using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class UMAModelProvider : CharacterModelProvider {

        [SerializeField]
        private bool useInlineOptions = true;

        [SerializeField]
        private UMAModelOptions inlineOptions = new UMAModelOptions();

        private UMAModelOptions modelOptions = null;

        [SerializeField]
        [ResourceSelector(resourceType = typeof(UMAModelProfile))]
        private string sharedOptions = string.Empty;

        //public UMAModelOptions ModelOptions { get => modelOptions; }

        public override ModelAppearanceController GetAppearanceController(UnitController unitController, UnitModelController unitModelController, SystemGameManager systemGameManager) {
            return new UMAModelController(unitController, unitModelController, systemGameManager, modelOptions);
        }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            SetupScriptableObjects(systemGameManager);
        }

        private void SetupScriptableObjects(SystemGameManager systemGameManager) {

            if (useInlineOptions == false) {
                if (sharedOptions != string.Empty) {
                    // get shared options and overwrite inline options
                    UMAModelProfile tmpProfile = systemDataFactory.GetResource<UMAModelProfile>(sharedOptions);
                    if (tmpProfile != null) {
                        modelOptions = tmpProfile.ModelOptions;
                        modelOptions.Configure(systemGameManager);
                    } else {
                        Debug.LogError("UMAModelProvider.SetupScriptableObjects(): Could not find model profile : " + sharedOptions + " while inititalizing.  CHECK INSPECTOR");
                    }
                }
            } else {
                inlineOptions.Configure(systemGameManager);
                modelOptions = inlineOptions;
            }
        }

    }

}

