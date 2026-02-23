using FishNet.Object.Prediction;

namespace AnyRPG {
    public struct ReplicateData : IReplicateData {
        
        public MovementData MovementData { get; private set; }

        public ReplicateData(MovementData movementData) : this() {
            MovementData = movementData;
        }

        private uint _tick;
        public void Dispose() { }
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;
    }
}