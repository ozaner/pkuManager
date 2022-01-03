using pkuManager.Alerts;
using pkuManager.Formats.pku;
using pkuManager.Formats.pkx;
using pkuManager.Utilities;
using System.Collections.Generic;
using System.Linq;
using static pkuManager.Formats.PorterDirective;

namespace pkuManager.Formats.Modules;

public interface BattleStatOverride_E
{
    public pkuObject pku { get; }
    public GlobalFlags GlobalFlags { get; }
    public List<Alert> Notes { get; }

    [PorterDirective(ProcessingPhase.PreProcessing)]
    public virtual void ProcessBattleStatOverride()
    {
        if (GlobalFlags.Battle_Stat_Override)
        {
            Alert alert = GetBattleStatAlert(); //generate alert, BEFORE modifying pku

            //If stat nature is specified, replace nature with it
            if (!pku.Stat_Nature.IsNull)
                pku.Nature.Set(pku.Stat_Nature);

            //If any hyper training is specified, replace corresponding IVs with 31
            if (pku.Hyper_Training.HP == true)
                pku.IVs.HP.Set(31);
            if (pku.Hyper_Training.Attack == true)
                pku.IVs.Attack.Set(31);
            if (pku.Hyper_Training.Defense == true)
                pku.IVs.Defense.Set(31);
            if (pku.Hyper_Training.Sp_Attack == true)
                pku.IVs.Sp_Attack.Set(31);
            if (pku.Hyper_Training.Sp_Defense == true)
                pku.IVs.Sp_Defense.Set(31);
            if (pku.Hyper_Training.Speed == true)
                pku.IVs.Speed.Set(31);

            Notes.Add(alert);
        }
    }

    protected Alert GetBattleStatAlert()
    {
        string msg = "";

        //Deal with stat nature override
        if (!pku.Stat_Nature.IsNull)
            msg += $"The pku's Nature " +
                   (pku.Nature.IsNull ? "is unspecified, replacing it" : $"{pku.Nature}, was replaced") +
                   $" with it's Stat Nature ({pku.Stat_Nature}).";

        if (pku.Hyper_Training_Array.Any(x => x == true)) //at least one hyper trained IV
        {
            if (!pku.Stat_Nature.IsNull)
                msg += DataUtil.Newline(2);
            msg += "Replacing the pku's ";
            for (int i = 0; i < 6; i++)
            {
                if (pku.Hyper_Training_Array[i] == true)
                    msg += (pku.IVs_Array[i].IsNull ? $"unspecified" : $"{pku.IVs_Array[i]}") + $" {pkxUtil.STAT_NAMES[i]} IV, ";
            }
            msg += "with 31s as they are Hyper Trained.";
        }

        return msg is "" ? null : new("Battle Stat Override", msg);
    }
}