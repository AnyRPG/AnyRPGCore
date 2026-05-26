namespace AnyRPG {
    public class MusicPlayerManager : InteractableOptionManager {

        private MusicPlayerProps musicPlayerProps = null;

        public MusicPlayerProps MusicPlayerProps { get => musicPlayerProps; set => musicPlayerProps = value; }

        public void SetMusicPlayerProps(MusicPlayerProps musicPlayerProps, InteractableOptionComponent interactableOptionComponent, int componentIndex, int choiceIndex) {
            this.musicPlayerProps = musicPlayerProps;

            BeginInteraction(interactableOptionComponent, componentIndex, choiceIndex);
        }

        public override void EndInteraction() {
            base.EndInteraction();

            musicPlayerProps = null;
        }

    }

}