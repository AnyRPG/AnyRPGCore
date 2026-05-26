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

        protected override void ConfigureGameOptions(SystemConfigurationManager systemConfigurationManager) {
            base.ConfigureGameOptions(systemConfigurationManager);

            systemConfigurationManager.DefaultPlayerUnitProfileName = "Mecanim Human Male";
        }

        
        public override void CreateFirstScene(string gameParentFolder, string gameName, string sceneName, bool copyExistingScene, SceneAsset existingScene, AudioClip newSceneDayAmbientSounds, AudioClip newSceneNightAmbientSounds, AudioClip newSceneMusic, ICreateSceneRequestor createSceneRequestor) {
            // create first scene
            NewSceneWizardBase.CreateScene(gameParentFolder, gameName, firstSceneName, copyExistingScene, existingScene, firstSceneDayAmbientSounds, firstSceneNightAmbientSounds, firstSceneMusic, this, NewSceneWizard.PortalTemplatePath);
        }

        public override bool CheckRequiredTemplatesExist() {
            return NewSceneWizard.CheckRequiredTemplatesExistStatic();
        }

    }

}
