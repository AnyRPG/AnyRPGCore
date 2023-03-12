using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public interface ISaveDataOwner {

        void SetSaveData(AnyRPGSaveData saveData);
    }

}