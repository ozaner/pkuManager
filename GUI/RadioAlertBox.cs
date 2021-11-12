using pkuManager.Alerts;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static pkuManager.Alerts.RadioButtonAlert;

namespace pkuManager.GUI;

public class RadioAlertBox : AlertBox
{
    // A dictionary of all the radio buttons created for each choice.
    protected List<RadioButton> radioButtons = new();

    // Creates an AlertBox with n RadioButtons for getting xor input form the user. Should be used for error boxes.
    public RadioAlertBox(RadioButtonAlert alert) : base(alert)
    {
        // define positional variables
        int xOffset = messageTextbox.Location.X;
        int yOffset = messageTextbox.Location.Y + messageTextbox.Size.Height;

        // add each choice
        foreach (RBAChoice rbac in alert.Choices)
        {
            // create radio button + title
            RadioButton rb = new()
            {
                Text = rbac.Title,
                Font = new(Font, FontStyle.Underline),
                AutoSize = true,
                Location = new(xOffset, yOffset)
            };
            yOffset += rb.Size.Height - 7;
            Controls.Add(rb);
            radioButtons.Add(rb);

            //update underlying rba alert whenever selection changed.
            rb.CheckedChanged += (control, e) => alert.SelectedIndex = WhichChoice(); 

            // add message if it exists
            if (rbac.Message is not null)
            {
                RichTextBox tb = newRichTextBox(rbac.Message, 135);
                tb.Location = new(xOffset + 20, yOffset);
                yOffset += tb.Size.Height;
                Controls.Add(tb);
            }

            // add text entry box if it has one
            if (rbac.HasTextEntry)
            {
                TextBox eb = new();
                int extraY = rbac.Message is null ? 5 : 0;
                eb.Location = new(xOffset + 20, yOffset + extraY);
                yOffset += eb.Size.Height + extraY;
                Controls.Add(eb);

                //update underlying rba choice whenever text entry changed.
                eb.TextChanged += (s, e) => rbac.TextEntry = eb.Text?.Length is 0 ? null : eb.Text;
            }
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
