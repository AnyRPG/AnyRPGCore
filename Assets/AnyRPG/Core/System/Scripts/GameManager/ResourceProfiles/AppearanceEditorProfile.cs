using System;
using UnityEngine;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Appearance Editor Profile", menuName = "AnyRPG/AppearanceEditorProfile")]
    public class AppearanceEditorProfile : DescribableResource {

        [Header("Prefab")]

        [Tooltip("The appearance panel prefab to spawn")]
        [SerializeField]
        private GameObject prefab = null;

        [Tooltip("The name of the class that controls the model type")]
        [SerializeField]
        private string modelProviderType = null;

        private Type modelProviderTypeRef = null;

        public Type ModelProviderType { get => modelProviderTypeRef; }
        public GameObject Prefab { get => prefab; }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            modelProviderTypeRef = Type.GetType(modelProviderType);
        }
    }


}