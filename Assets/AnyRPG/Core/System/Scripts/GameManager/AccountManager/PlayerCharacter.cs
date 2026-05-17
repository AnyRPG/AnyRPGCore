namespace AnyRPG {
    public class PlayerCharacter {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string Name { get; set; }
        public string SaveData { get; set; }

        public PlayerCharacter() {
            Name = string.Empty;
            SaveData = string.Empty;
        }
    }
}