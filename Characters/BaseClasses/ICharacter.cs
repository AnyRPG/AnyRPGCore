public interface ICharacter {
    ICharacterAbilityManager MyCharacterAbilityManager { get; }
    ICharacterCombat MyCharacterCombat { get; }
    ICharacterController MyCharacterController { get; }
    string MyCharacterName { get; }
    ICharacterStats MyCharacterStats { get; }
    string MyFactionName { get; set; }
    CharacterUnit MyCharacterUnit { get; set; }
    CharacterFactionManager MyCharacterFactionManager { get; }
    void SetCharacterFaction(string newFaction);
}