namespace AnyRPG {

    public class CharacterPreviewCameraController : PreviewCameraController {

        //public event System.Action OnTargetReady = delegate { };
        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            currentCamera = cameraManager.CharacterPreviewCamera;
        }

    }

}