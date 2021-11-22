﻿using pkuManager.Alerts;
using pkuManager.Common;
using pkuManager.Formats.Modules;
using pkuManager.Formats.pkx;
using pkuManager.pku;
using pkuManager.Utilities;
using System;
using System.Collections.Generic;
using static pkuManager.Alerts.Alert;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.showdown;

/// <summary>
/// Exports a <see cref="pkuObject"/> to a <see cref="ShowdownObject"/>.
/// </summary>
public class ShowdownExporter : Exporter, BattleStatOverride_E
{
    public override string FormatName => "Showdown";

    /// <summary>
    /// Creates an exporter that will attempt to export <paramref name="pku"/>
    /// to a .txt (Showdown!) file, encoded in UTF-8, with the given <paramref name="globalFlags"/>.
    /// </summary>
    /// <inheritdoc cref="Exporter(pkuObject, GlobalFlags, FormatObject)"/>
    public ShowdownExporter(pkuObject pku, GlobalFlags globalFlags) : base(pku, globalFlags) { }

    protected override ShowdownObject Data { get; } = new();

    public override (bool, string) CanPort()
    {
        //Showdown doesn't support eggs (they can't exactly battle...).
        if (pku.IsEgg())
            return (false, "Cannot be an Egg.");

        // Only Pokemon with a valid Showdown name are allowed.
        return (ShowdownObject.GetShowdownName(pku) is not null, "Species/Form/Appearance doesn't exist in Showdown.");
    }


    /* ------------------------------------
     * Pre-Processing Methods
     * ------------------------------------
    */
    // Format Override
    [PorterDirective(ProcessingPhase.FormatOverride)]
    protected virtual void ProcessFormatOverride()
        => pku = pkuObject.MergeFormatOverride(pku, FormatName);


    /* ------------------------------------
     * Tag Processing Methods
     * ------------------------------------
    */
    // Showdown Name
    [PorterDirective(ProcessingPhase.FirstPass)]
    protected virtual void ProcessShowdownName()
    {
        // Notes:
        //  - Combination of species and form tags (and gender for Meowstic & Indeedee)

        Data.ShowdownName = ShowdownObject.GetShowdownName(pku);
    }

    // Nickname
    [PorterDirective(ProcessingPhase.FirstPass)]
    protected virtual void ProcessNickname()
    {
        // Notes:
        //  - Practically no character limit
        //  - Can use parenthesis (only checks at the end of first line)
        //  - Empty nickname interpreted as no nickname
        //  - Leading spaces are ignored

        if (pku.Nickname is "") //empty counts as null
            Data.Nickname = null;
        else
            Data.Nickname = pku.Nickname;

        if (Data.Nickname?.Length > 0 && Data.Nickname[0] is ' ') //if first character is a space
            Warnings.Add(GetNicknameAlert(AlertType.INVALID));
    }

    // Gender
    [PorterDirective(ProcessingPhase.FirstPass)]
    protected virtual void ProcessGender()
    {
        // Notes:
        //  - Illegal genders are ignored in legal rulesets, but used in illegal ones.
        //  - Genderless is denoted by no gender.

        Data.Gender = pku.Gender.ToEnum<Gender>();
    }

    // Item
    [PorterDirective(ProcessingPhase.FirstPass)]
    protected virtual void ProcessItem()
    {
        bool itemValid = ITEM_DEX.ExistsIn(FormatName, pku.Item);
        if (pku.Item is not null && !itemValid) //check for invalid alert
            Warnings.Add(pkxUtil.ExportAlerts.GetItemAlert(AlertType.INVALID, pku.Item));

        if (itemValid)
            Data.Item = pku.Item;
    }

    // Ability
    [PorterDirective(ProcessingPhase.FirstPass)]
    protected virtual void ProcessAbility()
    {
        bool abilityValid = ABILITY_DEX.ExistsIn(FormatName, pku.Ability);
        if (pku.Ability is not null && !abilityValid) //check for invalid alert
            Warnings.Add(pkxUtil.ExportAlerts.GetAbilityAlert(AlertType.INVALID, pku.Ability, "None (Showdown will pick one)"));
        else
            Data.Ability = pku.Ability;
    }

    // Level
    [PorterDirective(ProcessingPhase.FirstPass)]
    protected virtual void ProcessLevel()
    {
        var (level, alert) = pkxUtil.ExportTags.ProcessNumericTag(pku.Level, pkxUtil.ExportAlerts.GetLevelAlert, false, 100, 1, 100);
        Data.Level = (byte)level;
        Warnings.Add(alert);
    }

    // Friendship
    [PorterDirective(ProcessingPhase.FirstPass)]
    protected virtual void ProcessFriendship()
    {
        var (friendship, alert) = pkxUtil.ExportTags.ProcessNumericTag(pku.Friendship, GetFriendshipAlert, false, 255, 0, 255);
        Data.Friendship = (byte)friendship;
        Warnings.Add(alert);
    }

    // IVs
    [PorterDirective(ProcessingPhase.FirstPass)]
    protected virtual void ProcessIVs()
        => Warnings.Add(pkxUtil.ExportTags.ProcessMultiNumericTag(pku.IVs_Array, Data.IVs, pkxUtil.ExportAlerts.GetIVsAlert, 31, false));

    // EVs
    [PorterDirective(ProcessingPhase.FirstPass)]
    protected virtual void ProcessEVs()
        => Warnings.Add(pkxUtil.ExportTags.ProcessEVs(pku, Data.EVs));

    // Nature
    [PorterDirective(ProcessingPhase.FirstPass)]
    protected virtual void ProcessNature()
    {
        Data.Nature = pku.Nature.ToEnum<Nature>();
        if (Data.Nature is null)
        {
            if (pku.Nature.IsNull)
                Warnings.Add(GetNatureAlert(AlertType.UNSPECIFIED));
            else
                Warnings.Add(GetNatureAlert(AlertType.INVALID, pku.Nature));
        }
    }

    // Gigantamax Factor
    [PorterDirective(ProcessingPhase.FirstPass)]
    protected virtual void ProcessGigantamaxFactor()
        => Data.Gigantamax_Factor = pku.Gigantamax_Factor is true;

    // Moves
    [PorterDirective(ProcessingPhase.FirstPass)]
    protected virtual void ProcessMoves()
    {
        //doesnt get gmax moves, but showdown doesn't allow them either
        List<string> moves = new();
        (_, int[] moveIndices, Alert alert) = pkxUtil.ExportTags.ProcessMoves(pku, FormatName, true);
        foreach (int id in moveIndices)
            moves.Add(pku.Moves[id].Name);
        Data.Moves = moves.ToArray();
        Warnings.Add(alert);
    }


    /* ------------------------------------
     * Showdown Exporting Alerts
     * ------------------------------------
    */
    public static Alert GetNicknameAlert(AlertType at) => at switch
    {
        AlertType.INVALID => new Alert("Nickname", $"Showdown does not recoginize leading spaces in nicknames."),
        _ => throw InvalidAlertType(at)
    };

    public static Alert GetLevelAlert(AlertType at) => at switch
    {
        //override pkx's unspecified level of 1 to 100
        AlertType.UNSPECIFIED => new Alert("Level", "No level specified, using the default: 100."),
        _ => pkxUtil.ExportAlerts.GetLevelAlert(at)
    };

    public static Alert GetFriendshipAlert(AlertType at) => at switch
    {
        //override pkx's unspecified friendship of 0 to 255
        AlertType.UNSPECIFIED => pkxUtil.ExportAlerts.GetNumericalAlert("Friendship", at, 255),
        _ => pkxUtil.ExportAlerts.GetFriendshipAlert(at)
    };

    public static Alert GetNatureAlert(AlertType at, string invalidNature = null)
    {
        Alert a = new("Nature", $"Using the default: None (Showdown uses Serious when no nature is specified.)");
        if (at is AlertType.INVALID)
        {
            if (invalidNature is null)
                throw new ArgumentException("If INVALID AlertType given, invalidNature must also be given.");
            a.Message = $"The Nature \"{invalidNature}\" is not valid in this format. " + a.Message;
        }
        else if (at is AlertType.UNSPECIFIED)
            a.Message = $"No nature was specified. " + a.Message;
        else
            throw InvalidAlertType(at);
        return a;
    }
}