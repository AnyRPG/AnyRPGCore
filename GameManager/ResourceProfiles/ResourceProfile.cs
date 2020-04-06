using AnyRPG;
using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Resource Profile", menuName = "AnyRPG/ResourceProfile")]
    public abstract class ResourceProfile : ScriptableObject, IDescribable {

        [SerializeField]
        protected string resourceName;

        //protected string displayName = string.Empty;

        [SerializeField]
        protected Sprite icon;

        [SerializeField]
        [TextArea(10, 20)]
        protected string description;

        public Sprite MyIcon { get => icon; set => icon = value; }
        //public string MyName { get => displayName; set => displayName = value; }
        public string MyName { get => resourceName; set => resourceName = value; }
        public string MyDescription { get => description; set => description = value; }

        public virtual string GetDescription() {
            return string.Format("<color=yellow>{0}</color>\n{1}", MyName, GetSummary());
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