namespace AnyRPG {
    public class MovementRidingState : IMovementState {

        private UnitController unitController;
        private UnitMovementController unitMovementController;

        public MovementRidingState(UnitMovementController unitMovementController, UnitController unitController) {
            this.unitController = unitController;
            this.unitMovementController = unitMovementController;
        }

        public void Enter(bool isReplay, bool isSilent) {
            //Debug.Log($"{unitController.gameObject.name}.MovementRidingState.Enter(isReplay: {isReplay}, isSilent: {isSilent}) frame: {Time.frameCount} tick: {(isSilent ? "N/A" : unitMovementController.CurrentMovementData.SimulatedTick)} tposition: {unitController.transform.position} rposition: {unitController.UnitMotor.MovementBody.GetPosition()} mposition: {unitController.UnitModelController.UnitModel.transform.position} velocity: {unitController.UnitMotor.MovementBody.GetLinearVelocity()}");
        }

        public void Exit(bool isReplay, bool isSilent) {
            //Debug.Log($"{unitController.gameObject.name}.MovementRidingState.Exit(isReplay: {isReplay}, isSilent: {isSilent}) frame: {Time.frameCount} tick: {(isSilent ? "N/A" : unitMovementController.CurrentMovementData.SimulatedTick)} tposition: {unitController.transform.position} rposition: {unitController.UnitMotor.MovementBody.GetPosition()} mposition: {unitController.UnitModelController.UnitModel.transform.position} velocity: {unitController.UnitMotor.MovementBody.GetLinearVelocity()}");
        }

        public void Update(bool isReplay, double timeInterval) {
            //Debug.Log($"{unitController.gameObject.name}.MovementRidingState.Update(isReplay: {isReplay}) frame: {Time.frameCount} tick: {unitMovementController.CurrentMovementData.SimulatedTick} tposition: {unitController.transform.position} rposition: {unitController.UnitMotor.MovementBody.GetPosition()}");
            
            if (unitController.transform.parent == null) {
                return;
            }
            //Debug.Log($"{unitController.gameObject.name}.MovementRidingState.Update(): position: {unitController.transform.parent.position} modelPosition: {unitController.UnitModelController.UnitModel.transform.position} parentPosition: {unitController.transform.parent.position}");
            //unitController.UnitMotor.SetPosition(unitController.transform.parent.position);
            unitController.transform.position = unitController.transform.parent.position;
            unitController.UnitModelController.UnitModel.transform.position = unitController.transform.parent.position;
        }
    }

}