using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Set Character Class Command", menuName = "AnyRPG/Chat Commands/Set Character Class Command")]
    public class SetCharacterClassCommand : ChatCommand {

        [Header("Set Character Class Command")]

        [Tooltip("If true, all parameters will be ignored, and the character class will be the one listed below")]
        [SerializeField]
        private bool fixedCharacterClass = false;

        [Tooltip("The name of the character class to set")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(CharacterClass))]
        private string characterClassName = string.Empty;

        private CharacterClass characterClass = null;

        public override void ExecuteCommand(string commandParameters, int accountId) {
            //Debug.Log($"{ResouceName}SetCharacterClassCommand.ExecuteCommand({commandParameters}, {accountId})");

            // set the fixed characterClass
            if (fixedCharacterClass == true && characterClass != null) {
                SetCharacterClass(characterClass, accountId);
                return;
            }

            // the characterClass comes from parameters, but none were given
            if (commandParameters == string.Empty) {
                return;
            }

            characterClass = systemDataFactory.GetResource<CharacterClass>(commandParameters);
            if (characterClass == null) {
                return;
            }
            SetCharacterClass(characterClass, accountId);
        }

        private void SetCharacterClass(CharacterClass characterClass, int accountId) {
            //Debug.Log($"SetCharacterClassCommand.SetCharacterClass({accountId})");

            playerManagerServer.SetPlayerCharacterClass(characterClass, accountId);
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            if (characterClassName != string.Empty) {
                characterClass = systemDataFactory.GetResource<CharacterClass>(characterClassName);
                if (characterClass == null) {
                    Debug.LogError($"SetCharacterClassCommand.SetupScriptableObjects(): Could not find characterClass {characterClassName} for command {ResourceName}");
                }
            }
        }
    }

}