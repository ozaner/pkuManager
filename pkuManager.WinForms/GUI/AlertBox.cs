using pkuManager.WinForms.Alerts;
using System.Drawing;
using System.Windows.Forms;

namespace pkuManager.WinForms.GUI;

/// <summary>
/// A GUI container for basic Alerts. To be used in conjunction with the porter windows.
/// </summary>
public class AlertBox : FlowLayoutPanel
{
    protected Label titleLabel;
    protected RichTextBox messageTextbox;

    private static int SCROLLBAR_SPACE = 25;
    private static int TEXT_PADDING = 14;

    // Creates a new AlertBox (which is just a pre-formatted Panel)
    public AlertBox(Alert alert, int containerWidth)
    {
        MinimumSize = new(containerWidth - SCROLLBAR_SPACE, 15); //30 height
        MaximumSize = new(containerWidth - SCROLLBAR_SPACE, int.MaxValue);
        BorderStyle = BorderStyle.FixedSingle;
        AutoSize = true;
        Padding = new(4);
        titleLabel = new()
        {
            MaximumSize = new(0, 15),
            Text = alert.Title,
        };
        titleLabel.Font = new(titleLabel.Font, FontStyle.Bold);
        titleLabel.Width = titleLabel.PreferredWidth;
        Controls.Add(titleLabel);

        messageTextbox = newRichTextBox(alert.Message);
        Controls.Add(messageTextbox);
    }

    // Creates a richtextbox formatted to look like a warning/error box
    protected RichTextBox newRichTextBox(string text)
    {
        RichTextBox tb = new()
        {
            BorderStyle = BorderStyle.None,
            Text = text,
            TabStop = false,
            ReadOnly = true,
            AutoSize = true,
            MaximumSize = new(Width - TEXT_PADDING, int.MaxValue),
            MinimumSize = new(Width - TEXT_PADDING, 0)
        };
        tb.ContentsResized += rtb_ContentsResized;
        return tb;
    }

    private static void rtb_ContentsResized(object sender, ContentsResizedEventArgs e)
    {
        ((RichTextBox)sender).Height = e.NewRectangle.Height + 5;
    }
}