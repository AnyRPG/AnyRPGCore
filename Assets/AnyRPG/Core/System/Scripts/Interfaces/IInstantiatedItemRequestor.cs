using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public interface IInstantiatedItemRequestor {

        void InitializeItem(InstantiatedItem instantiatedItem);
    }

}