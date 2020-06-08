using AnyRPG;
using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Resource Profile", menuName = "AnyRPG/ResourceProfile")]
    public abstract class ResourceProfile : ScriptableObject, IDescribable {

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

        public Sprite MyIcon { get => icon; set => icon = value; }
        //public string MyName { get => displayName; set => displayName = value; }
        public string MyName {
            get {
                return resourceName;
            }
        }
        public string RawDisplayName {
            get {
                return displayName;
            }
            set => displayName = value;
        }

        public string MyDisplayName {
            get {
                if (displayName != null && displayName != string.Empty) {
                    return displayName;
                }
                return resourceName;
            }
            set => displayName = value;
        }
        public string MyDescription { get => description; set => description = value; }
        public Sprite IconBackgroundImage { get => iconBackgroundImage; set => iconBackgroundImage = value; }

        public virtual string GetDescription() {
            return string.Format("<color=yellow>{0}</color>\n{1}", MyDisplayName, GetSummary());
        }

        public virtual string GetSummary() {
            return string.Format("{0}", description);
        }

        public virtual void SetupScriptableObjects() {
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