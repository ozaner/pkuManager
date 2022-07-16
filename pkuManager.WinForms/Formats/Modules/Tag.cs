using pkuManager.WinForms.Alerts;
using pkuManager.WinForms.Formats.pku;
using System.Collections.Generic;

namespace pkuManager.WinForms.Formats.Modules;

public interface Tag
{
    public pkuObject pku { get; }
    public FormatObject Data { get; }
    public GlobalFlags GlobalFlags { get; }
    public bool CheckMode { get; }
    public string FormatName { get; }
    public List<Alert> Warnings { get; }
    public List<Alert> Errors { get; }
    public List<Alert> Notes { get; }
}
