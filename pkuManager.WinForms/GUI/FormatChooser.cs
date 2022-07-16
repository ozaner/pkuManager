using pkuManager.WinForms.Formats.pku;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace pkuManager.WinForms.GUI;

public partial class FormatChooser : Form
{
    private FormatChooser()
        => InitializeComponent();

    private static string GetSelectPrompt(FormatComponent comp, bool check) => comp switch
    {
        FormatComponent.Exporter => $"Select a format to {(check ? "check-out" : "export")} to:",
        FormatComponent.Importer => $"Select a format to {(check ? "check-in" : "import")} from:",
        FormatComponent.Collection => "Select the type of save file you wish to open:",
        _ => throw new NotImplementedException()
    };

    private static string GetNotice(FormatComponent comp) => "If a format does not appear, then " + comp switch
    {
        FormatComponent.Exporter => "the pku cannot be exported to it, or it is currently unsupported.",
        FormatComponent.Importer => "the pku cannot be imported to it, or it is currently unsupported.",
        FormatComponent.Collection => "it does not have save support.",
        _ => throw new NotImplementedException()
    };

    private static string GetEmptyNotice(FormatComponent comp) => comp switch
    {
        FormatComponent.Exporter => "This pku cannot be exported to any currently supported format.",
        FormatComponent.Importer => "There are currently no formats that support importing...",
        FormatComponent.Collection => "There are currently no supported save file types...",
        _ => throw new NotImplementedException()
    };

    private enum FormatComponent
    {
        Exporter,
        Importer,
        Collection
    }

    private static string ChoosePortFormat(List<string> formats, FormatComponent comp, bool check = false)
    {
        FormatChooser fc = new();
        fc.selectionPromptLabel.Text = GetSelectPrompt(comp, check);
        fc.invalidFormatNoticeLabel.Text = GetNotice(comp);
        if (formats.Count is 0)
        {
            MessageBox.Show(GetEmptyNotice(comp), "No valid formats");
            return null;
        }
        fc.formatComboBox.Items.AddRange(formats.ToArray());
        fc.formatComboBox.SelectedIndex = 0; //make sure that some index is selected

        DialogResult result = fc.ShowDialog();
        if (result is DialogResult.OK)
            return fc.formatComboBox.SelectedItem.ToString();
        else
            return null;
    }

    public static string ChooseImportFormat(bool check)
    {
        List<string> formats = new();
        foreach (var kvp in Registry.FORMATS)
        {
            //pku can't be imported
            if (kvp.Key is "pku")
                continue;

            //can import any format w/ an importer
            if (kvp.Value.Importer is not null)
                formats.Add(kvp.Key);
        }
        return ChoosePortFormat(formats, FormatComponent.Importer, check);
    }

    public static string ChooseExportFormat(pkuObject pku, GlobalFlags flags, bool check)
    {
        List<string> formats = new();
        foreach (var kvp in Registry.FORMATS)
        {
            //pku can't be ported to pku
            if (kvp.Key is "pku")
                continue;

            //can export pku to formats w/ an exporter & pass CanExport
            if (kvp.Value.Exporter is not null && PortingWindow.CanExport(kvp.Value, pku, flags, check))
                formats.Add(kvp.Key);
        }
        return ChoosePortFormat(formats, FormatComponent.Exporter, check);
    }

    public static string ChooseCollectionFormat()
    {
        List<string> formats = new();
        foreach (var kvp in Registry.FORMATS)
        {
            //pkuCollections must be opened another way
            if (kvp.Key is "pku")
                continue;

            //can import any format w/ a collection
            if (kvp.Value.Collection is not null)
                formats.Add(kvp.Key);
        }
        return ChoosePortFormat(formats, FormatComponent.Collection);
    }
}
