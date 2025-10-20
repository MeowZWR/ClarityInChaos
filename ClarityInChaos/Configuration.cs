using Dalamud.Configuration;
using Dalamud.Game.Config;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using System;

namespace ClarityInChaos
{
  [Serializable]
  public unsafe class Configuration : IPluginConfiguration
  {
    public int Version { get; set; } = 0;

    public bool IsVisible { get; set; } = true;

    public bool Enabled { get; set; } = true;

    public ConfigForBackup Backup { get; init; }
    public ConfigForSolo Solo { get; init; }
    public ConfigForLightParty LightParty { get; init; }
    public ConfigForFullParty FullParty { get; init; }
    public ConfigForAlliance Alliance { get; init; }
    public ConfigForPvP PvP { get; init; }

    public bool DebugMessages = false;

    public bool DebugForcePartySize = false;
    public int DebugPartySize = 0;
    public bool DebugForceInDuty = false;
    public bool DebugForceInPvP = false;

    // the below exist just to make saving less cumbersome
    [NonSerialized]
    private IDalamudPluginInterface? pluginInterface;

    public Configuration(bool isFresh = false)
    {
      Backup = new ConfigForBackup();
      Solo = new ConfigForSolo();
      LightParty = new ConfigForLightParty();
      FullParty = new ConfigForFullParty();
      Alliance = new ConfigForAlliance();
      PvP = new ConfigForPvP();

      if (isFresh)
      {
        ApplyDefaultConfig(Backup);
        ApplyDefaultConfig(Solo);
        ApplyDefaultConfig(LightParty);
        ApplyDefaultConfig(FullParty);
        ApplyDefaultConfig(Alliance);
        ApplyDefaultConfig(PvP);
      }
    }

    public void Initialize(IDalamudPluginInterface pluginInterface)
    {
      this.pluginInterface = pluginInterface;
    }

    public void Save()
    {
      pluginInterface!.SavePluginConfig(this);
    }

    private void ApplyDefaultConfig(ConfigForGroupingSize config)
    {
      Service.GameConfig.TryGet(UiConfigOption.BattleEffectSelf, out uint beSelf);
      config.Self = (BattleEffect)beSelf;
      Service.GameConfig.TryGet(UiConfigOption.BattleEffectParty, out uint beParty);
      config.Party = (BattleEffect)beParty;
      Service.GameConfig.TryGet(UiConfigOption.BattleEffectOther, out uint beOther);
      config.Other = (BattleEffect)beOther;

      Service.GameConfig.TryGet(UiConfigOption.NamePlateDispTypeSelf, out uint npSelf);
      config.OwnNameplate = (NameplateVisibility)npSelf;
      Service.GameConfig.TryGet(UiConfigOption.NamePlateDispTypeParty, out uint npParty);
      config.PartyNameplate = (NameplateVisibility)npParty;
      Service.GameConfig.TryGet(UiConfigOption.NamePlateDispTypeAlliance, out uint npAlliance);
      config.AllianceNameplate = (NameplateVisibility)npAlliance;
      Service.GameConfig.TryGet(UiConfigOption.NamePlateDispTypeOther, out uint npOthers);
      config.OthersNameplate = (NameplateVisibility)npOthers;
      Service.GameConfig.TryGet(UiConfigOption.NamePlateDispTypeFriend, out uint npFriends);
      config.FriendsNameplate = (NameplateVisibility)npFriends;
      Service.GameConfig.TryGet(UiConfigOption.NamePlateDispTypeEngagedEnemy, out uint npEnemy);
      config.EngagedEnemyNameplate = (EngagedEnemyNameplateVisibility)npEnemy;

      Service.GameConfig.TryGet(UiConfigOption.NamePlateHpTypeSelf, out uint hpSelf);
      config.OwnHpBar = (NameplateHpBarVisibility)hpSelf;
      Service.GameConfig.TryGet(UiConfigOption.NamePlateHpTypeParty, out uint hpParty);
      config.PartyHpBar = (NameplateHpBarVisibility)hpParty;
      Service.GameConfig.TryGet(UiConfigOption.NamePlateHpTypeAlliance, out uint hpAlliance);
      config.AllianceHpBar = (NameplateHpBarVisibility)hpAlliance;
      Service.GameConfig.TryGet(UiConfigOption.NamePlateHpTypeOther, out uint hpOthers);
      config.OthersHpBar = (NameplateHpBarVisibility)hpOthers;
      Service.GameConfig.TryGet(UiConfigOption.NamePlateHpTypeFriend, out uint hpFriends);
      config.FriendsHpBar = (NameplateHpBarVisibility)hpFriends;
      Service.GameConfig.TryGet(UiConfigOption.NamePlateHpTypeEngagedEmemy, out uint hpEnemy);
      config.EngagedEnemyHpBar = (EngagedEnemyHpBarVisibility)hpEnemy;
    }

    private ConfigForGroupingSize GetConfigForGroupingSize(GroupingSize size)
    {
      return size switch
      {
        GroupingSize.Solo => Solo,
        GroupingSize.LightParty => LightParty,
        GroupingSize.FullParty => FullParty,
        GroupingSize.Alliance => Alliance,
        GroupingSize.PvP => PvP,
        _ => Backup,
      };
    }

    private ConfigForGroupingSize GetConfigForGroupingSizeNotInDuty(GroupingSize size)
    {
      var config = GetConfigForGroupingSize(size);
      if (config.OnlyInDuty)
      {
        if (size == GroupingSize.Backup)
        {
          return Backup;
        }
        else
        {
          return GetConfigForGroupingSizeNotInDuty(size - 1);
        }
      }
      return config;
    }

    public ConfigForGroupingSize GetConfigForGroupingSize(GroupingSize size, bool inDuty)
    {
      if (inDuty)
      {
        return GetConfigForGroupingSize(size);
      }
      else
      {
        return GetConfigForGroupingSizeNotInDuty(size);
      }
    }
  }

  public abstract class ConfigForGroupingSize
  {
    public abstract GroupingSize Size { get; }
    public BattleEffect Self { get; set; }
    public BattleEffect Party { get; set; }
    public BattleEffect Other { get; set; }

    public NameplateVisibility OwnNameplate { get; set; }
    public NameplateVisibility PartyNameplate { get; set; }
    public NameplateVisibility AllianceNameplate { get; set; }
    public NameplateVisibility OthersNameplate { get; set; }
    public NameplateVisibility FriendsNameplate { get; set; }
    public EngagedEnemyNameplateVisibility EngagedEnemyNameplate { get; set; }

    public NameplateHpBarVisibility OwnHpBar { get; set; }
    public NameplateHpBarVisibility PartyHpBar { get; set; }
    public NameplateHpBarVisibility AllianceHpBar { get; set; }
    public NameplateHpBarVisibility OthersHpBar { get; set; }
    public NameplateHpBarVisibility FriendsHpBar { get; set; }
    public EngagedEnemyHpBarVisibility EngagedEnemyHpBar { get; set; }

    public ObjectHighlightColor OwnHighlight { get; set; }
    public ObjectHighlightColor PartyHighlight { get; set; }
    public ObjectHighlightColor OthersHighlight { get; set; }

    public bool OnlyInDuty { get; set; }
  }

  public class ConfigForBackup : ConfigForGroupingSize
  {
    public override GroupingSize Size => GroupingSize.Backup;
  }

  public class ConfigForSolo : ConfigForGroupingSize
  {
    public override GroupingSize Size => GroupingSize.Solo;
  }

  public class ConfigForLightParty : ConfigForGroupingSize
  {
    public override GroupingSize Size => GroupingSize.LightParty;
  }

  public class ConfigForFullParty : ConfigForGroupingSize
  {
    public override GroupingSize Size => GroupingSize.FullParty;
  }

  public class ConfigForAlliance : ConfigForGroupingSize
  {
    public override GroupingSize Size => GroupingSize.Alliance;
  }

  public class ConfigForPvP : ConfigForGroupingSize
  {
    public override GroupingSize Size => GroupingSize.PvP;
  }
}
