using pkuManager.Formats.pku;
using System.Collections.Generic;
using System.Windows.Forms;

namespace pkuManager.GUI;

public partial class FormatChooser : Form
{
    private FormatChooser()
        => InitializeComponent();

    private static string GetSelectPrompt(bool isImport)
        => $"Select a format to {(isImport ? "im" : "ex")}port to:";

    private static string GetNotice(bool isImport)
        => $"If a format does not appear, then the pku cannot be {(isImport ? "im" : "ex")}ported to it.";

    private static string ChoosePortFormat(List<string> formats, bool isImport)
    {
        FormatChooser fc = new();
        fc.selectionPromptLabel.Text = GetSelectPrompt(isImport);
        fc.invalidFormatNoticeLabel.Text = GetNotice(isImport);
        if (formats.Count is 0)
        {
            MessageBox.Show("This pku cannot be exported to any currently supported format.", "No valid formats");
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

    public static string ChooseImportFormat()
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
        return ChoosePortFormat(formats, true);
    }

    public static string ChooseExportFormat(pkuObject pku, GlobalFlags flags)
    {
        List<string> formats = new();
        foreach (var kvp in Registry.FORMATS)
        {
            //pku can't be ported to pku
            if (kvp.Key is "pku")
                continue;

            //can export pku to formats w/ an exporter & pass CanExport
            if (kvp.Value.Exporter is not null && ExportingWindow.CanExport(kvp.Value, pku, flags))
                formats.Add(kvp.Key);
        }
        return ChoosePortFormat(formats, false);
    }
}
