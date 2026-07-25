using UnityEditor;

namespace AnyRPG.EditorTools {
    public class NewCharacterWizard : NewCharacterWizardBase {

        [MenuItem("Tools/AnyRPG/Wizard/New Character/New Offline Character Wizard")]
        public static void CreateWizard() {
            ScriptableWizard.DisplayWizard<NewCharacterWizard>("New Offline Character Wizard", "Create");
        }        

    }

}
