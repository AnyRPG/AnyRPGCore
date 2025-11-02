namespace AnyRPG {

    public class SystemAchievementManager : ConfiguredClass {

        public void AcceptAchievements(UnitController sourceUnitController) {
            //Debug.Log($"SystemAchievementManager.AcceptAchievements({sourceUnitController.gameObject.name})");

            foreach (Achievement resource in systemDataFactory.GetResourceList<Achievement>()) {
                if (resource.TurnedIn(sourceUnitController) == false && resource.IsComplete(sourceUnitController) == false) {
                    sourceUnitController.CharacterQuestLog.AcceptAchievement(resource);
                }
            }
        }

       
    }

}