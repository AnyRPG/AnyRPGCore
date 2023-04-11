using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace AnyRPG {
    public class NewGameWizard : NewGameWizardBase {

        private const string pathToPlayerUnitsTemplate = "/AnyRPG/Core/Content/TemplatePackages/UnitProfile/Player/MecanimHumanPlayerUnitsTemplatePackage.asset";

        public override string PathToPlayerUnitsTemplate { get => pathToPlayerUnitsTemplate; }

        [MenuItem("Tools/AnyRPG/Wizard/New Game Wizard")]
        public static void CreateWizard() {
            ScriptableWizard.DisplayWizard<NewGameWizard>("New Game Wizard", "Create");
        }

        protected override void SetDefaultPlayerUnitProfileName(SystemConfigurationManager systemConfigurationManager) {
            base.SetDefaultPlayerUnitProfileName(systemConfigurationManager);

            systemConfigurationManager.DefaultPlayerUnitProfileName = "Mecanim Human Male";
        }
    }

}
