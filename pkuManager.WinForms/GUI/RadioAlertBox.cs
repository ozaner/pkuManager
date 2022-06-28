using pkuManager.WinForms.Alerts;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static pkuManager.WinForms.Alerts.ChoiceAlert;

namespace pkuManager.WinForms.GUI;

public class RadioAlertBox : AlertBox
{
    // A dictionary of all the radio buttons created for each choice.
    protected List<RadioButton> radioButtons = new();

    // Creates an AlertBox with n RadioButtons for getting xor input form the user. Should be used for error boxes.
    public RadioAlertBox(ChoiceAlert alert, int containerWidth) : base(alert, containerWidth)
    {
        // add each choice
        foreach (SingleChoice rbac in alert.Choices)
        {
            // create radio button + title
            RadioButton rb = new()
            {
                Text = rbac.Title,
                Font = new(Font, FontStyle.Underline),
                AutoSize = true,
            };
            Controls.Add(rb);
            radioButtons.Add(rb);

            // add message if it exists
            if (rbac.Message is not null)
            {
                RichTextBox tb = newRichTextBox(rbac.Message);
                Controls.Add(tb);
            }

            // add text entry box if it has one
            if (rbac.HasTextEntry)
            {
                TextBox eb = new();
                Controls.Add(eb);

                //update underlying rba choice whenever text entry changed.
                eb.TextChanged += (s, e) => rbac.TextEntry = eb.Text?.Length is 0 ? null : eb.Text;
            }

            //update underlying rba alert whenever selection changed.
            rb.CheckedChanged += (control, e) => alert.SelectedIndex = WhichChoice();
        }

        //check the default option
        radioButtons[alert.SelectedIndex].Checked = true;
    }

    // Returns which choice is currently selected.
    protected int WhichChoice()
    {
        int index = radioButtons.TakeWhile(r => !r.Checked).Count();
        return index;
    }
}
