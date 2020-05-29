using AnyRPG;
using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Describable Resource", menuName = "AnyRPG/Describable Resource")]
    public abstract class DescribableResource : ResourceProfile {

        [Header("Override Name, Icon, and Description")]

        [Tooltip("If true, look for the resource description with the same name as this resource.")]
        [SerializeField]
        private bool useRegionalDescription = false;

        [Tooltip("Manually set a resource description to be used.")]
        [SerializeField]
        protected string resourceDescriptionProfile;

        protected ResourceDescription resourceDescription = null;

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();

            // get the description profile if it exists, and then overwrite any local properties that are not null in that profile
            resourceDescription = null;
            if (useRegionalDescription == true) {
                resourceDescriptionProfile = MyName;
            }
            if (resourceDescriptionProfile != null && resourceDescriptionProfile != string.Empty) {
                ResourceDescription tmpResourceDescription = SystemResourceDescriptionManager.MyInstance.GetResource(resourceDescriptionProfile);
                if (tmpResourceDescription != null) {
                    if (tmpResourceDescription.MyDisplayName != null && tmpResourceDescription.MyDisplayName != string.Empty) {
                        resourceName = tmpResourceDescription.MyDisplayName;
                    }
                    if (tmpResourceDescription.MyIcon != null) {
                        icon = tmpResourceDescription.MyIcon;
                    }
                    if (tmpResourceDescription.MyDescription != null && tmpResourceDescription.MyDescription != string.Empty) {
                        description = tmpResourceDescription.MyDescription;
                    }
                } else {
                    Debug.LogError("DescribableResource.SetupScriptableObjects(): Could Not Find " + resourceDescriptionProfile + " resource description while processing " + MyName + ". CHECK INSPECTOR!");
                }
            }
        }


    }

}