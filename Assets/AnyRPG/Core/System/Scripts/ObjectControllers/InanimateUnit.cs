using AnyRPG;
using UnityEngine;

namespace AnyRPG {
    public class InanimateUnit : NamePlateUnit {


        public override void OnDisable() {
            base.OnDisable();
            //Debug.Log(gameObject.name + gameObject.GetInstanceID() + ".InanimateUnit.OnDisable()");
        }

        public override void OnDestroy() {
            base.OnDestroy();
            //Debug.Log(gameObject.name + gameObject.GetInstanceID() + ".InanimateUnit.OnDestroy()");
        }

    }

}