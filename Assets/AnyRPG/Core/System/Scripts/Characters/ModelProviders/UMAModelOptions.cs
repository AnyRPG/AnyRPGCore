using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    // ===============================================================================
    // THIS CLASS DOESN'T REQUIRE THE UMA NAMESPACE SO IT IS SAFE TO EXIST IN THE CORE
    // ===============================================================================

    [System.Serializable]
    public class UMAModelOptions : ConfiguredClass {

        [SerializeField]
        private List<UMAModelSlotOptions> slotOptions = new List<UMAModelSlotOptions>();

        private Dictionary<string, UMAModelSlotOptions> slotOptionDictionary = new Dictionary<string, UMAModelSlotOptions>();

        public Dictionary<string, UMAModelSlotOptions> SlotOptions { get => slotOptionDictionary; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            foreach (UMAModelSlotOptions slotOption in slotOptions) {
                if (slotOption.SlotName != string.Empty) {
                    slotOptionDictionary.Add(slotOption.SlotName, slotOption);
                }
            }
        }

    }

}

