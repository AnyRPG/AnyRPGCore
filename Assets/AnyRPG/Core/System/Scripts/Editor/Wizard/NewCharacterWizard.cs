using UnityEditor;

namespace AnyRPG.EditorTools {
    public class NewCharacterWizard : NewCharacterWizardBase {

        [MenuItem("Tools/AnyRPG/Wizard/New Character Wizard")]
        public static void CreateWizard() {
            ScriptableWizard.DisplayWizard<NewCharacterWizard>("New Character Wizard", "Create");
        }        

    }

}
