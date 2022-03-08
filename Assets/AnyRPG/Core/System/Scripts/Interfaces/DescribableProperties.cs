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

        private Sprite icon = null;
        private string displayName = string.Empty;
        private string description = string.Empty;
        private string summary = string.Empty;

        public DescribableProperties(Sprite icon, string displayName) {
            this.icon = icon;
            this.displayName = displayName;
        }

        public string GetDescription() {
            return description;
        }

        public string GetSummary() {
            return summary;
        }
    }

}