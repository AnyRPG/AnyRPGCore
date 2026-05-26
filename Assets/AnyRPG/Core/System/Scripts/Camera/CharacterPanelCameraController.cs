namespace AnyRPG {

    public class CharacterPanelCameraController : PreviewCameraController {

        //public event System.Action OnTargetReady = delegate { };
        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            currentCamera = cameraManager.CharacterPanelCamera;
        }

    }

}