using AnyRPG;
using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;

namespace AnyRPG {
    public abstract class ResourceProfile : ConfiguredScriptableObject, IDescribable {

        [SerializeField]
        protected string resourceName;

        [SerializeField]
        protected string displayName = string.Empty;

        [SerializeField]
        protected Sprite icon;

        [Tooltip("The image that appears behind the items when viewed in equipment manager and bags")]
        [SerializeField]
        protected Sprite iconBackgroundImage;

        [SerializeField]
        [TextArea(10, 20)]
        protected string description;

        public virtual Sprite Icon { get => icon; set => icon = value; }

        /// <summary>
        /// return the resourceName
        /// </summary>
        public string ResourceName {
            get {
                return resourceName;
            }
            set {
                resourceName = value;
            }
        }

        /// <summary>
        /// return the displayName
        /// </summary>
        public string RawDisplayName {
            get {
                return displayName;
            }
            set => displayName = value;
        }

        /// <summary>
        /// return the displayName if set, otherwise return the resourceName
        /// </summary>
        public string DisplayName {
            get {
                if (displayName != null && displayName != string.Empty) {
                    return displayName;
                }
                return resourceName;
            }
            set => displayName = value;
        }

        public string Description { get => description; set => description = value; }
        public Sprite IconBackgroundImage { get => iconBackgroundImage; set => iconBackgroundImage = value; }

        public virtual string GetName() {
            return string.Format("<color=yellow>{0}</color>", DisplayName);
        }

        public virtual string GetSummary() {
            return string.Format("{0}\n{1}", GetName(), GetDescription());
        }

        public virtual string GetDescription() {
            return string.Format("{0}", description);
        }

        public virtual void SetupScriptableObjects(SystemGameManager systemGameManager) {
            Configure(systemGameManager);
            /*
            if (displayName == null || displayName == string.Empty) {
                displayName = resourceName;
            }
            */
        }

        public virtual void CleanupScriptableObjects() {

        }


    }

}