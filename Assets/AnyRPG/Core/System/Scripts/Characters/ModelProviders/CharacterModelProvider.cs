namespace AnyRPG {

    [System.Serializable]
    public abstract class CharacterModelProvider : ConfiguredClass {

        public abstract ModelAppearanceController GetAppearanceController(UnitController unitController, UnitModelController unitModelController, SystemGameManager systemGameManager);
    }

}

