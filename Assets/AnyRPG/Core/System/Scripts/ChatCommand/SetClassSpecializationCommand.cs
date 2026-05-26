using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Set Class Specialization Command", menuName = "AnyRPG/Chat Commands/Set Class Specialization Command")]
    public class SetClassSpecializationCommand : ChatCommand {

        [Header("Set Class Specialization Command")]

        [Tooltip("If true, all parameters will be ignored, and the class specialization will be the one listed below")]
        [SerializeField]
        private bool fixedClassSpecialization = false;

        [Tooltip("The name of the class specialization to set")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(ClassSpecialization))]
        private string classSpecializationName = string.Empty;

        private ClassSpecialization classSpecialization = null;

        public override void ExecuteCommand(string commandParameters, int accountId) {
            //Debug.Log($"{ResouceName}SetClassSpecializationCommand.ExecuteCommand({commandParameters}, {accountId})");

            // set the fixed classSpecialization
            if (fixedClassSpecialization == true && classSpecialization != null) {
                SetClassSpecialization(classSpecialization, accountId);
                return;
            }

            // the classSpecialization comes from parameters, but none were given
            if (commandParameters == string.Empty) {
                return;
            }

            classSpecialization = systemDataFactory.GetResource<ClassSpecialization>(commandParameters);
            if (classSpecialization == null) {
                return;
            }
            SetClassSpecialization(classSpecialization, accountId);
        }

        private void SetClassSpecialization(ClassSpecialization classSpecialization, int accountId) {
            //Debug.Log($"SetClassSpecializationCommand.SetClassSpecialization({accountId})");

            playerManagerServer.SetPlayerCharacterSpecialization(classSpecialization, accountId);
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            if (classSpecializationName != string.Empty) {
                classSpecialization = systemDataFactory.GetResource<ClassSpecialization>(classSpecializationName);
                if (classSpecialization == null) {
                    Debug.LogError($"SetClassSpecializationCommand.SetupScriptableObjects(): Could not find classSpecialization {classSpecializationName} for command {ResourceName}");
                }
            }
        }
    }

}