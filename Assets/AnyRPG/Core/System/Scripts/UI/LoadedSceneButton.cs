using TMPro;
using UnityEngine;

namespace AnyRPG {
    public class LoadedSceneButton : ConfiguredMonoBehaviour {

        [SerializeField]
        private TextMeshProUGUI sceneNameText = null;

        [SerializeField]
        private TextMeshProUGUI instanceTypeText = null;

        [SerializeField]
        private TextMeshProUGUI playerCountText = null;

        private SceneData sceneData = null;

        // game manager references
        NetworkManagerServer networkManagerServer = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            networkManagerServer = systemGameManager.NetworkManagerServer;
        }

        public void SetScenedata(SceneData sceneData) {
            this.sceneData = sceneData;
            UpdateText();
        }

        public void UpdateText() {
            sceneNameText.text = sceneData.SceneNode.DisplayName;
            instanceTypeText.text = sceneData.SceneInstanceType.ToString();
            playerCountText.text = sceneData.ClientCount.ToString();
        }

        /*
        public void UpdatePlayerCount(int playerCount) {
            playerCountText.text = playerCount.ToString();
        }
        */

    }

}
