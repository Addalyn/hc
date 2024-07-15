public static class CharacterTypeExtensions
{
    public static string GetNameForCharacterType(CharacterType characterType, string displayName)
    {
        return characterType != CharacterType.BattleMonk
               && characterType != CharacterType.BazookaGirl
               && characterType != CharacterType.Blaster
               && characterType != CharacterType.Claymore
               && characterType != CharacterType.DigitalSorceress
               && characterType != CharacterType.Gremlins
               && characterType != CharacterType.NanoSmith
               && characterType != CharacterType.RageBeast
               && characterType != CharacterType.RobotAnimal
               && characterType != CharacterType.Scoundrel
               && characterType != CharacterType.Sniper
               && characterType != CharacterType.SpaceMarine
               && characterType != CharacterType.Spark
               && characterType != CharacterType.Tracker
               && characterType != CharacterType.Trickster
            ? characterType.ToString()
            : displayName;
    }

    public static bool IsValidForHumanGameplay(this CharacterType characterType)
    {
        return characterType > CharacterType.None
               && characterType < CharacterType.Last
               && !characterType.IsWillFill()
               && characterType != CharacterType.PunchingDummy
               && characterType != CharacterType.FemaleWillFill;
    }

    public static bool IsValidForHumanPreGameSelection(this CharacterType characterType)
    {
        return characterType > CharacterType.None
               && characterType < CharacterType.Last
               && characterType != CharacterType.PunchingDummy
               && characterType != CharacterType.FemaleWillFill;
    }

    public static bool IsWillFill(this CharacterType characterType)
    {
        return characterType == CharacterType.PendingWillFill;
    }
}