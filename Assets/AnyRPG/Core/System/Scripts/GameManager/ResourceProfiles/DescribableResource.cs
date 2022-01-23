using AnyRPG;
using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;

namespace AnyRPG {
    public abstract class DescribableResource : ResourceProfile {

        [Header("Override Name, Icon, and Description")]

        [Tooltip("If true, look for the resource description with the same name as this resource.")]
        [SerializeField]
        private bool useRegionalDescription = false;

        [Tooltip("Manually set a resource description to be used.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(ResourceDescription))]
        protected string resourceDescriptionProfile;

        protected ResourceDescription resourceDescription = null;

        [Tooltip("If true, the regional override is optional, and will not cause an error if it doesn't exist.")]
        [SerializeField]
        protected bool optionalOverride = false;

        // game manager references

        protected SystemDataFactory systemDataFactory = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            systemDataFactory = systemGameManager.SystemDataFactory;
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            // get the description profile if it exists, and then overwrite any local properties that are not null in that profile
            resourceDescription = null;
            if (useRegionalDescription == true) {
                resourceDescriptionProfile = DisplayName;
            }
            if (resourceDescriptionProfile != null && resourceDescriptionProfile != string.Empty) {
                ResourceDescription tmpResourceDescription = systemDataFactory.GetResource<ResourceDescription>(resourceDescriptionProfile);
                if (tmpResourceDescription != null) {
                    if (tmpResourceDescription.RawDisplayName != null && tmpResourceDescription.RawDisplayName != string.Empty) {
                        //Debug.Log("setting resource name to: " + tmpResourceDescription.MyDisplayName);
                        displayName = tmpResourceDescription.RawDisplayName;
                    }
                    if (tmpResourceDescription.Icon != null) {
                        icon = tmpResourceDescription.Icon;
                    }
                    if (tmpResourceDescription.IconBackgroundImage != null) {
                        iconBackgroundImage = tmpResourceDescription.IconBackgroundImage;
                    }
                    if (tmpResourceDescription.Description != null && tmpResourceDescription.Description != string.Empty) {
                        description = tmpResourceDescription.Description;
                    }
                } else {
                    if (optionalOverride == false) {
                        Debug.LogError("DescribableResource.SetupScriptableObjects(): Could Not Find " + resourceDescriptionProfile + " resource description while processing " + DisplayName + ". CHECK INSPECTOR!");
                    }
                }
            }
        }


    }

}