using pkuManager.Alerts;
using pkuManager.Formats.Fields;
using pkuManager.Formats.pku;
using pkuManager.Utilities;
using System.Collections.Generic;
using System.Linq;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules.MetaTags;

public interface BattleStatOverride_E
{
    public pkuObject pku { get; }
    public GlobalFlags GlobalFlags { get; }
    public List<Alert> Notes { get; }

    [PorterDirective(ProcessingPhase.PreProcessing)]
    public void ProcessBattleStatOverride()
    {
        if (GlobalFlags.Battle_Stat_Override)
        {
            Alert alert = GetBattleStatAlert(); //generate alert, BEFORE modifying pku

            //If stat nature is specified, replace nature with it
            if (!pku.Stat_Nature.IsNull())
                pku.Nature.Value = pku.Stat_Nature.Value;

            //If any hyper training is specified, replace corresponding IVs with 31
            if (pku.Hyper_Training.HP.Value == true)
                pku.IVs.HP.Value = 31;
            if (pku.Hyper_Training.Attack.Value == true)
                pku.IVs.Attack.Value = 31;
            if (pku.Hyper_Training.Defense.Value == true)
                pku.IVs.Defense.Value = 31;
            if (pku.Hyper_Training.Sp_Attack.Value == true)
                pku.IVs.Sp_Attack.Value = 31;
            if (pku.Hyper_Training.Sp_Defense.Value == true)
                pku.IVs.Sp_Defense.Value = 31;
            if (pku.Hyper_Training.Speed.Value == true)
                pku.IVs.Speed.Value = 31;

            Notes.Add(alert);
        }
    }

    protected Alert GetBattleStatAlert()
    {
        string msg = "";

        //Deal with stat nature override
        if (!pku.Stat_Nature.IsNull())
            msg += $"The pku's nature " +
                   (pku.Nature.IsNull() ? "is unspecified, replacing it" : $"({pku.Nature.Value}), was replaced") +
                   $" with it's stat nature ({pku.Stat_Nature.Value}).";

        if (pku.Hyper_Training_Array.Any(x => x.Value == true)) //at least one hyper trained IV
        {
            if (!pku.Stat_Nature.IsNull())
                msg += DataUtil.Newline(2);
            List<string> list = new();
            for (int i = 0; i < 6; i++)
            {
                if (pku.Hyper_Training_Array[i].Value == true)
                    list.Add($"{TagUtil.STAT_NAMES[i]} IV ({(pku.IVs_Array[i].IsNull() ? "unspecified" : $"{pku.IVs_Array[i].Value}")})");
            }
            msg += $"Setting the pku's {list.ToArray().JoinGrammatical()} to 31 as they are Hyper Trained.";
        }

        return msg is "" ? null : new("Battle Stat Override", msg);
    }
}