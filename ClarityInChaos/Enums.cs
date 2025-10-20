namespace ClarityInChaos
{
  public enum GroupingSize
  {
    Backup,
    Solo,
    LightParty,
    FullParty,
    Alliance,
    PvP,
  }

  public enum BattleEffect
  {
    All,
    Limited,
    None
  }

  public enum NameplateVisibility
  {
    Always,
    DuringBattle,
    WhenTargeted,
    Never,
    OutofBattle,
  }

  public enum HighlightColor
  {
    None,
    Red,
    Green,
    Blue,
    Yellow,
    Orange,
    Magenta,
    Black
  }

  public enum NameplateHpBarVisibility
  {
    Always,
    DuringBattle,
    WhenHpNotFull,
    Never
  }

  public enum EngagedEnemyNameplateVisibility
  {
    Always,
    WhenTargeted,
    Never
  }

  public enum EngagedEnemyHpBarVisibility
  {
    Always,
    WhenHpNotFull,
    Never
  }
}
