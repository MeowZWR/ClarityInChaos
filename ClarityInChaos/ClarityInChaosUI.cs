using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using Dalamud.Bindings.ImGui;
using System;
using System.Numerics;

namespace ClarityInChaos
{
  public unsafe class ClarityInChaosUI : Window, IDisposable
  {
    private readonly ClarityInChaosPlugin plugin;

    public ClarityInChaosUI(ClarityInChaosPlugin plugin)
      : base(
        "Clarity In Chaos##ConfigWindow",
        ImGuiWindowFlags.AlwaysAutoResize
        | ImGuiWindowFlags.NoResize
        | ImGuiWindowFlags.NoCollapse
      )
    {
      this.plugin = plugin;

      SizeConstraints = new WindowSizeConstraints()
      {
        MinimumSize = new Vector2(488, 0),
        MaximumSize = new Vector2(1000, 1000)
      };
    }

    public void Dispose()
    {
      GC.SuppressFinalize(this);
    }

    public override void OnClose()
    {
      base.OnClose();
      plugin.Configuration.IsVisible = false;
      plugin.Configuration.Save();
    }

    private void PrettyEffect(string label, BattleEffect effect)
    {
      ImGui.Text($"{label}:");
      ImGui.SameLine();
      ImGui.TextColored(ColorForEffect(effect), effect.ToString());
    }

    private Vector4 ColorForEffect(BattleEffect effect)
    {
      Vector4 color = new Vector4(255, 255, 255, 255);
      switch (effect)
      {
        case BattleEffect.All:
          color = new Vector4(0, 255, 0, 255);
          break;
        case BattleEffect.Limited:
          color = new Vector4(255, 255, 0, 255);
          break;
        case BattleEffect.None:
          color = new Vector4(255, 0, 0, 255);
          break;
      }
      return color;
    }

    private void DrawSectionMasterEnable(ConfigForGroupingSize activeConfig)
    {
      // can't ref a property, so use a local copy
      var enabled = plugin.Configuration.Enabled;
      if (ImGui.Checkbox("总开关", ref enabled))
      {
        plugin.Configuration.Enabled = enabled;
        plugin.Configuration.Save();
      }

      ImGui.Separator();

      var green = new Vector4(0, 255, 0, 255);

      if (plugin.Configuration.Enabled)
      {
        ImGui.TextColored(green, $"当前战斗特效设置: {activeConfig.Size}");
      }
      else
      {
        ImGui.Text($"当前战斗特效设置: 游戏内保存的设置");
      }

      if (plugin.BattleEffectsConfigurator.IsTerritoryAllianceLike())
      {
        ImGui.SameLine();
        ImGui.TextColored(green, $"(检测到特殊副本)");
      }

      if (plugin.InPvP)
      {
        ImGui.SameLine();
        var yellow = new Vector4(255, 255, 0, 255);
        ImGui.TextColored(yellow, $"(检测到PvP)");
      }

      ImGui.Indent();
      PrettyEffect("自己", plugin.BattleEffectsConfigurator.BattleEffectSelf);
      PrettyEffect("小队", plugin.BattleEffectsConfigurator.BattleEffectParty);
      PrettyEffect("他人", plugin.BattleEffectsConfigurator.BattleEffectOther);
      ImGui.Unindent();
    }

    private void DrawBattleEffectsTableHeader()
    {
      ImGui.TableNextRow();

      ImGui.TableSetColumnIndex(1);
      ImGui.Text("完全显示");

      ImGui.TableSetColumnIndex(2);
      ImGui.Text("简单显示");

      ImGui.TableSetColumnIndex(3);
      ImGui.Text("不显示");
    }

    private bool DrawBattleEffectsTable(ref ConfigForGroupingSize config)
    {
      var changed = false;
      ImGui.BeginTable("Table", 4);

      var self = config.Self;
      var party = config.Party;
      var other = config.Other;

      DrawBattleEffectsTableHeader();

      if (DrawBfxRadiosLine($"自己", ref self))
      {
        config.Self = self;
        changed = true;
      }
      if (DrawBfxRadiosLine($"小队", ref party))
      {
        config.Party = party;
        changed = true;
      }
      if (DrawBfxRadiosLine($"他人", ref other))
      {
        config.Other = other;
        changed = true;
      }
      ImGui.EndTable();
      return changed;
    }

    private void DrawNameplatesTableHeader()
    {
      ImGui.TableNextRow();

      ImGui.TableSetColumnIndex(1);
      ImGui.Text("一直显示");

      ImGui.TableSetColumnIndex(2);
      ImGui.Text("战斗时显示");

      ImGui.TableSetColumnIndex(3);
      ImGui.Text("非战斗时显示");

      ImGui.TableSetColumnIndex(4);
      ImGui.Text("选为目标时显示");

      ImGui.TableSetColumnIndex(5);
      ImGui.Text("不显示");
    }

    private bool DrawNameplatesTable(ref ConfigForGroupingSize config)
    {
      var changed = false;
      ImGui.BeginTable("Table", 6);

      var own = config.OwnNameplate;
      var party = config.PartyNameplate;
      var alliance = config.AllianceNameplate;
      var others = config.OthersNameplate;
      var friends = config.FriendsNameplate;
      var enemy = config.EngagedEnemyNameplate;

      DrawNameplatesTableHeader();

      if (DrawNameplatesRadiosLine($"自己", ref own))
      {
        config.OwnNameplate = own;
        changed = true;
      }

      if (DrawNameplatesRadiosLine($"小队", ref party))
      {
        config.PartyNameplate = party;
        changed = true;
      }

      if (DrawNameplatesRadiosLine($"团队", ref alliance))
      {
        config.AllianceNameplate = alliance;
        changed = true;
      }

      if (DrawNameplatesRadiosLine($"他人", ref others))
      {
        config.OthersNameplate = others;
        changed = true;
      }

      if (DrawNameplatesRadiosLine($"好友", ref friends))
      {
        config.FriendsNameplate = friends;
        changed = true;
      }

      if (DrawEngagedEnemyNameplateRadiosLine($"自己占有的敌人", ref enemy))
      {
        config.EngagedEnemyNameplate = enemy;
        changed = true;
      }

      ImGui.EndTable();
      return changed;
    }

    private bool DrawEngagedEnemyNameplateRadiosLine(string label, ref EngagedEnemyNameplateVisibility visibility)
    {
      var changed = false;

      ImGui.TableNextRow();
      ImGui.TableSetColumnIndex(0);
      ImGui.Text(label);

      ImGui.PushID(label);

      ImGui.TableSetColumnIndex(1);
      ImGui.SetCursorPosX(ImGui.GetCursorPos().X + (ImGui.GetContentRegionAvail().X - 24) / 2f);
      if (ImGui.RadioButton($"##Always", visibility is EngagedEnemyNameplateVisibility.Always))
      {
        visibility = EngagedEnemyNameplateVisibility.Always;
        changed = true;
      }

      ImGui.TableSetColumnIndex(2);
      // Skip "During Battle" column

      ImGui.TableSetColumnIndex(3);
      // Skip "Out of Battle" column

      ImGui.TableSetColumnIndex(4);
      ImGui.SetCursorPosX(ImGui.GetCursorPos().X + (ImGui.GetContentRegionAvail().X - 24) / 2f);
      if (ImGui.RadioButton($"##When Targeted", visibility is EngagedEnemyNameplateVisibility.WhenTargeted))
      {
        visibility = EngagedEnemyNameplateVisibility.WhenTargeted;
        changed = true;
      }

      ImGui.TableSetColumnIndex(5);
      ImGui.SetCursorPosX(ImGui.GetCursorPos().X + (ImGui.GetContentRegionAvail().X - 24) / 2f);
      if (ImGui.RadioButton($"##Never", visibility is EngagedEnemyNameplateVisibility.Never))
      {
        visibility = EngagedEnemyNameplateVisibility.Never;
        changed = true;
      }

      ImGui.PopID();

      return changed;
    }

    private void DrawHighlightsTableHeader()
    {
      ImGui.TableNextRow();

      ImGui.TableSetColumnIndex(1);
      ImGui.Text("高亮显示                           ");
    }

    private bool DrawHighlightsTable(ref ConfigForGroupingSize config)
    {
      var changed = false;
      ImGui.BeginTable("Table", 2);

      var own = config.OwnHighlight;
      var party = config.PartyHighlight;
      var others = config.OthersHighlight;

      DrawHighlightsTableHeader();

      if (DrawHighlightsSelectLine($"自己", ref own))
      {
        config.OwnHighlight = own;
        changed = true;
      }

      if (DrawHighlightsSelectLine($"小队", ref party))
      {
        config.PartyHighlight = party;
        changed = true;
      }

      if (DrawHighlightsSelectLine($"他人", ref others))
      {
        config.OthersHighlight = others;
        changed = true;
      }

      ImGui.EndTable();
      return changed;
    }

    private void DrawHpBarsTableHeader()
    {
      ImGui.TableNextRow();

      ImGui.TableSetColumnIndex(1);
      ImGui.Text("一直显示");

      ImGui.TableSetColumnIndex(2);
      ImGui.Text("战斗时显示");

      ImGui.TableSetColumnIndex(3);
      ImGui.Text("体力减少时显示");

      ImGui.TableSetColumnIndex(4);
      ImGui.Text("不显示");
    }

    private bool DrawHpBarsTable(ref ConfigForGroupingSize config)
    {
      var changed = false;
      ImGui.BeginTable("Table", 5);

      var own = config.OwnHpBar;
      var party = config.PartyHpBar;
      var alliance = config.AllianceHpBar;
      var others = config.OthersHpBar;
      var friends = config.FriendsHpBar;
      var enemy = config.EngagedEnemyHpBar;

      DrawHpBarsTableHeader();

      if (DrawHpBarsRadiosLine($"自己", ref own))
      {
        config.OwnHpBar = own;
        changed = true;
      }

      if (DrawHpBarsRadiosLine($"小队", ref party))
      {
        config.PartyHpBar = party;
        changed = true;
      }

      if (DrawHpBarsRadiosLine($"团队", ref alliance))
      {
        config.AllianceHpBar = alliance;
        changed = true;
      }

      if (DrawHpBarsRadiosLine($"他人", ref others))
      {
        config.OthersHpBar = others;
        changed = true;
      }

      if (DrawHpBarsRadiosLine($"好友", ref friends))
      {
        config.FriendsHpBar = friends;
        changed = true;
      }

      if (DrawEngagedEnemyHpBarRadiosLine($"自己占有的敌人", ref enemy))
      {
        config.EngagedEnemyHpBar = enemy;
        changed = true;
      }

      ImGui.EndTable();
      return changed;
    }

    private bool DrawEngagedEnemyHpBarRadiosLine(string label, ref EngagedEnemyHpBarVisibility visibility)
    {
      var changed = false;

      ImGui.TableNextRow();
      ImGui.TableSetColumnIndex(0);
      ImGui.Text(label);

      ImGui.PushID(label);

      ImGui.TableSetColumnIndex(1);
      ImGui.SetCursorPosX(ImGui.GetCursorPos().X + (ImGui.GetContentRegionAvail().X - 24) / 2f);
      if (ImGui.RadioButton($"##Always", visibility is EngagedEnemyHpBarVisibility.Always))
      {
        visibility = EngagedEnemyHpBarVisibility.Always;
        changed = true;
      }

      ImGui.TableSetColumnIndex(2);
      // Skip "During Battle" column

      ImGui.TableSetColumnIndex(3);
      ImGui.SetCursorPosX(ImGui.GetCursorPos().X + (ImGui.GetContentRegionAvail().X - 24) / 2f);
      if (ImGui.RadioButton($"##When HP Not Full", visibility is EngagedEnemyHpBarVisibility.WhenHpNotFull))
      {
        visibility = EngagedEnemyHpBarVisibility.WhenHpNotFull;
        changed = true;
      }

      ImGui.TableSetColumnIndex(4);
      ImGui.SetCursorPosX(ImGui.GetCursorPos().X + (ImGui.GetContentRegionAvail().X - 24) / 2f);
      if (ImGui.RadioButton($"##Never", visibility is EngagedEnemyHpBarVisibility.Never))
      {
        visibility = EngagedEnemyHpBarVisibility.Never;
        changed = true;
      }

      ImGui.PopID();

      return changed;
    }

    private bool DrawHpBarsRadiosLine(string label, ref NameplateHpBarVisibility visibility)
    {
      var changed = false;

      ImGui.TableNextRow();
      ImGui.TableSetColumnIndex(0);
      ImGui.Text(label);

      ImGui.PushID(label);

      ImGui.TableSetColumnIndex(1);
      ImGui.SetCursorPosX(ImGui.GetCursorPos().X + (ImGui.GetContentRegionAvail().X - 24) / 2f);
      if (ImGui.RadioButton($"##Always", visibility is NameplateHpBarVisibility.Always))
      {
        visibility = NameplateHpBarVisibility.Always;
        changed = true;
      }

      ImGui.TableSetColumnIndex(2);
      ImGui.SetCursorPosX(ImGui.GetCursorPos().X + (ImGui.GetContentRegionAvail().X - 24) / 2f);
      if (ImGui.RadioButton($"##During Battle", visibility is NameplateHpBarVisibility.DuringBattle))
      {
        visibility = NameplateHpBarVisibility.DuringBattle;
        changed = true;
      }

      ImGui.TableSetColumnIndex(3);
      ImGui.SetCursorPosX(ImGui.GetCursorPos().X + (ImGui.GetContentRegionAvail().X - 24) / 2f);
      if (ImGui.RadioButton($"##When HP Not Full", visibility is NameplateHpBarVisibility.WhenHpNotFull))
      {
        visibility = NameplateHpBarVisibility.WhenHpNotFull;
        changed = true;
      }

      ImGui.TableSetColumnIndex(4);
      ImGui.SetCursorPosX(ImGui.GetCursorPos().X + (ImGui.GetContentRegionAvail().X - 24) / 2f);
      if (ImGui.RadioButton($"##Never", visibility is NameplateHpBarVisibility.Never))
      {
        visibility = NameplateHpBarVisibility.Never;
        changed = true;
      }

      ImGui.PopID();

      return changed;
    }

    private bool DrawTableGroup(ref ConfigForGroupingSize config)
    {
      var changed = false;

      ImGui.BeginTabBar("TabBar");

      if (ImGui.BeginTabItem("战斗特效"))
      {
        ImGui.Indent();
        changed |= DrawBattleEffectsTable(ref config);
        ImGui.Unindent();
        ImGui.EndTabItem();
      }

      if (ImGui.BeginTabItem("名牌显示"))
      {
        ImGui.Indent();
        changed |= DrawNameplatesTable(ref config);
        ImGui.Unindent();
        ImGui.EndTabItem();
      }

      if (ImGui.BeginTabItem("高亮"))
      {
        ImGui.Indent();
        changed |= DrawHighlightsTable(ref config);
        ImGui.Unindent();
        ImGui.EndTabItem();
      }

      if (ImGui.BeginTabItem("体力栏"))
      {
        ImGui.Indent();
        changed |= DrawHpBarsTable(ref config);
        ImGui.Unindent();
        ImGui.EndTabItem();
      }

      ImGui.BeginTable("Table2", 4);
      var onlyInDuty = config.OnlyInDuty;
      if (config.Size != GroupingSize.Backup && config.Size != GroupingSize.Alliance && DrawOnlyInDutyCheckbox($"仅在副本中生效", ref onlyInDuty))
      {
        config.OnlyInDuty = onlyInDuty;
        changed = true;
      }

      ImGui.EndTable();

      ImGui.EndTabBar();

      return changed;
    }

    private bool DrawOnlyInDutyCheckbox(string label, ref bool onlyInDuty)
    {
      var changed = false;

      ImGui.TableNextRow();
      ImGui.TableSetColumnIndex(0);
      ImGui.Text(label);

      ImGui.TableSetColumnIndex(1);
      if (ImGui.Checkbox($"##{label}", ref onlyInDuty))
      {
        changed = true;
      }

      return changed;
    }

    private bool DrawBfxRadiosLine(string label, ref BattleEffect effect)
    {
      var changed = false;

      ImGui.TableNextRow();
      ImGui.TableSetColumnIndex(0);
      ImGui.Text(label);

      ImGui.PushID(label);

      ImGui.TableSetColumnIndex(1);
      ImGui.SetCursorPosX(ImGui.GetCursorPos().X + (ImGui.GetContentRegionAvail().X - 24) / 2f);
      if (ImGui.RadioButton($"##All", effect is BattleEffect.All))
      {
        effect = BattleEffect.All;
        changed = true;
      }

      ImGui.TableSetColumnIndex(2);
      ImGui.SetCursorPosX(ImGui.GetCursorPos().X + (ImGui.GetContentRegionAvail().X - 24) / 2f);
      if (ImGui.RadioButton($"##Limited", effect is BattleEffect.Limited))
      {
        effect = BattleEffect.Limited;
        changed = true;
      }

      ImGui.TableSetColumnIndex(3);
      ImGui.SetCursorPosX(ImGui.GetCursorPos().X + (ImGui.GetContentRegionAvail().X - 24) / 2f);
      if (ImGui.RadioButton($"##None", effect is BattleEffect.None))
      {
        effect = BattleEffect.None;
        changed = true;
      }

      ImGui.PopID();

      return changed;
    }

    private bool DrawNameplatesRadiosLine(string label, ref NameplateVisibility effect)
    {
      var changed = false;

      ImGui.TableNextRow();
      ImGui.TableSetColumnIndex(0);
      ImGui.Text(label);

      ImGui.PushID(label);

      ImGui.TableSetColumnIndex(1);
      ImGui.SetCursorPosX(ImGui.GetCursorPos().X + (ImGui.GetContentRegionAvail().X - 24) / 2f);
      if (ImGui.RadioButton($"##Always", effect is NameplateVisibility.Always))
      {
        effect = NameplateVisibility.Always;
        changed = true;
      }

      ImGui.TableSetColumnIndex(2);
      ImGui.SetCursorPosX(ImGui.GetCursorPos().X + (ImGui.GetContentRegionAvail().X - 24) / 2f);
      if (ImGui.RadioButton($"##During Battle", effect is NameplateVisibility.DuringBattle))
      {
        effect = NameplateVisibility.DuringBattle;
        changed = true;
      }

      ImGui.TableSetColumnIndex(3);
      ImGui.SetCursorPosX(ImGui.GetCursorPos().X + (ImGui.GetContentRegionAvail().X - 24) / 2f);
      if (ImGui.RadioButton($"##Out of Battle", effect is NameplateVisibility.OutofBattle))
      {
        effect = NameplateVisibility.OutofBattle;
        changed = true;
      }

      ImGui.TableSetColumnIndex(4);
      ImGui.SetCursorPosX(ImGui.GetCursorPos().X + (ImGui.GetContentRegionAvail().X - 24) / 2f);
      if (ImGui.RadioButton($"##When Targeted", effect is NameplateVisibility.WhenTargeted))
      {
        effect = NameplateVisibility.WhenTargeted;
        changed = true;
      }

      ImGui.TableSetColumnIndex(5);
      ImGui.SetCursorPosX(ImGui.GetCursorPos().X + (ImGui.GetContentRegionAvail().X - 24) / 2f);
      if (ImGui.RadioButton($"##Never", effect is NameplateVisibility.Never))
      {
        effect = NameplateVisibility.Never;
        changed = true;
      }

      ImGui.PopID();

      return changed;
    }

    private bool DrawHighlightsSelectLine(string label, ref ObjectHighlightColor color)
    {
      var changed = false;

      ImGui.TableNextRow();
      ImGui.TableSetColumnIndex(0);
      ImGui.Text(label);

      ImGui.PushID(label);

      ImGui.TableSetColumnIndex(1);
      var proximityColor = (int)color;
      if (ImGui.Combo("##color", ref proximityColor, "无\0红色\0绿色\0蓝色\0黄色\0橙色\0紫红色\0"))
      {
        color = (ObjectHighlightColor)proximityColor;
        changed = true;
      }

      ImGui.PopID();

      return changed;
    }

    private void DrawGroupingSizeGroup(ConfigForGroupingSize config, bool isActive)
    {
      if (DrawTableGroup(ref config))
      {
        if (isActive)
        {
          plugin.BattleEffectsConfigurator.UIChange(config.Size);
        }
        plugin.Configuration.Save();
      }
    }

    private void DrawPrettyHeader(ConfigForGroupingSize config, bool isActive)
    {
      string headerText = config.Size switch
      {
        GroupingSize.Solo => "单人",
        GroupingSize.LightParty => "轻型小队 (4人)",
        GroupingSize.FullParty => "满编小队 (8人)",
        GroupingSize.Alliance => "团队副本 (24人)",
        GroupingSize.PvP => "PvP",
        _ => "游戏内保存的设置",
      };

      if (isActive)
      {
        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0, 255, 0, 255));
      }

      ImGui.PushID(headerText);
      if (ImGui.CollapsingHeader(headerText))
      {
        if (isActive)
        {
          ImGui.PopStyleColor();
        }

        ImGui.Indent();

        DrawGroupingSizeGroup(config, isActive);

        ImGui.Unindent();
      }
      else
      {
        if (isActive)
        {
          ImGui.PopStyleColor();
        }
      }
      ImGui.PopID();
    }

    private void DrawBattleEffectsMatrixSection(ConfigForGroupingSize activeConfig)
    {
      DrawPrettyHeader(plugin.Configuration.PvP, plugin.Configuration.PvP == activeConfig && plugin.Configuration.Enabled);
      DrawPrettyHeader(plugin.Configuration.Solo, plugin.Configuration.Solo == activeConfig && plugin.Configuration.Enabled);
      DrawPrettyHeader(plugin.Configuration.LightParty, plugin.Configuration.LightParty == activeConfig && plugin.Configuration.Enabled);
      DrawPrettyHeader(plugin.Configuration.FullParty, plugin.Configuration.FullParty == activeConfig && plugin.Configuration.Enabled);
      DrawPrettyHeader(plugin.Configuration.Alliance, plugin.Configuration.Alliance == activeConfig && plugin.Configuration.Enabled);
      DrawPrettyHeader(plugin.Configuration.Backup, plugin.Configuration.Backup == activeConfig || !plugin.Configuration.Enabled);
    }

    public void DrawDebugSection()
    {
      if (ImGui.CollapsingHeader("调试选项"))
      {
        ImGui.Indent();

        ImGui.TextWrapped("使用这些选项来测试你的设置。");

        var psize = plugin.Configuration.DebugPartySize;
        var forcePSize = plugin.Configuration.DebugForcePartySize;
        if (ImGui.Checkbox("强制小队人数", ref forcePSize))
        {
          plugin.Configuration.DebugForcePartySize = forcePSize;
          plugin.Configuration.Save();
        }
        ImGui.SameLine();
        if (ImGui.InputInt("##psize", ref psize))
        {
          plugin.Configuration.DebugPartySize = Math.Max(psize, 0);
          plugin.Configuration.Save();
        }

        var forceInDuty = plugin.Configuration.DebugForceInDuty;
        if (ImGui.Checkbox("强制副本状态", ref forceInDuty))
        {
          plugin.Configuration.DebugForceInDuty = forceInDuty;
          plugin.Configuration.Save();
        }

        var forceInPvP = plugin.Configuration.DebugForceInPvP;
        if (ImGui.Checkbox("强制PvP状态", ref forceInPvP))
        {
          plugin.Configuration.DebugForceInPvP = forceInPvP;
          plugin.Configuration.Save();
        }

        ImGui.Unindent();
      }
    }

    public override void Draw()
    {
      var groupSize = plugin.BattleEffectsConfigurator.GetCurrentGroupingSize();
      var inDuty = groupSize == GroupingSize.PvP || plugin.BoundByDuty;
      var activeConfig = plugin.Configuration.GetConfigForGroupingSize(groupSize, inDuty);

      DrawSectionMasterEnable(activeConfig);

      ImGui.Separator();

      DrawBattleEffectsMatrixSection(activeConfig);

      ImGui.Separator();

      DrawDebugSection();
    }
  }
}
