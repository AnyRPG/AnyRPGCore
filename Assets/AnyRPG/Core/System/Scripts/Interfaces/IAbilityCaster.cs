using UnityEngine;

namespace AnyRPG {
    public interface IAbilityCaster {

        IAbilityManager AbilityManager { get; }
        Transform transform { get; }
        GameObject gameObject { get; }
        MonoBehaviour MonoBehaviour {  get; }
        PhysicsScene PhysicsScene { get; }
    }

}