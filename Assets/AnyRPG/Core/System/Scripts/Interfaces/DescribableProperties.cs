using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class DescribableProperties : IDescribable {
        
        public Sprite Icon {
            get => icon;
            set => icon = value;
        }
        
        public string DisplayName { 
            get => displayName;
            set => displayName = value;
        }

        public string Description {
            get => description;
            set => description = value;
        }

        private Sprite icon = null;
        private string displayName = string.Empty;
        private string description = string.Empty;
        private string summary = string.Empty;

        public DescribableProperties(IDescribable describable) {
            this.icon = describable.Icon;
            this.displayName = describable.DisplayName;
            this.description = describable.Description;
        }

        public DescribableProperties(Sprite icon, string displayName) {
            this.icon = icon;
            this.displayName = displayName;
        }

        public string GetSummary() {
            return string.Format("{0}\n{1}", DisplayName, GetDescription());
        }

        public string GetDescription() {
            return description;
        }
    }

}