using UnityEditor;

namespace AnyRPG {
    public class NewSceneWizard : NewSceneWizardBase, ICreateSceneRequestor {

        private const string portalTemplatePath = "/AnyRPG/Core/Templates/Prefabs/Portal/StonePortal.prefab";

        public static string PortalTemplatePath => portalTemplatePath;

        [MenuItem("Tools/AnyRPG/Wizard/New Scene Wizard")]
        public static void CreateWizard() {
            ScriptableWizard.DisplayWizard<NewSceneWizard>("New Scene Wizard", "Create");
        }

        public static bool CheckRequiredTemplatesExistStatic() {

            if (NewSceneWizardBase.CheckRequiredBaseTemplatesExist() == false) {
                return false;
            }

            // Check for presence of portal template
            if (WizardUtilities.CheckFileExists(portalTemplatePath, "portal template") == false) {
                return false;
            }

            return true;
        }

        public override string GetPortalTemplatePath() {
            return portalTemplatePath;
        }


    }

}
