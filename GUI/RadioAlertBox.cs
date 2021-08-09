using pkuManager.Alerts;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace pkuManager.GUI
{
    public class RadioAlertBox : AlertBox
    {
        // A dictionary of all the radio buttons created for each choice.
        protected List<RadioButton> radioButtons = new List<RadioButton>();

        // Creates an AlertBox with n RadioButtons for getting xor input form the user. Should be used for error boxes.
        public RadioAlertBox(RadioButtonAlert alert) : base(alert)
        {
            int xOffset = messageTextbox.Location.X;
            int yOffset = messageTextbox.Location.Y + messageTextbox.Size.Height;
            for (int i = 0; i < alert.choices.Length; i++)
            {
                RadioButton rb = new RadioButton();
                rb.Text = alert.choices[i].Item1;
                rb.Font = new Font(rb.Font, FontStyle.Underline);
                RichTextBox tb = newRichTextBox(alert.choices[i].Item2, 135);

                rb.AutoSize = true;
                //rb.Padding = new Padding(0, 0, 0, 0);

                rb.Location = new Point(xOffset, yOffset);
                yOffset += rb.Size.Height - 7;
                tb.Location = new Point(xOffset + 20, yOffset);
                yOffset += tb.Size.Height;

                radioButtons.Add(rb);

                rb.CheckedChanged += (control, e) =>
                {
                    alert.setSelectedIndex(whichChoice());
                };

                Controls.Add(rb);
                Controls.Add(tb);
            }

            // Check the first option so at leat one option is alwys checked.
            radioButtons[alert.getSelectedIndex()].Checked = true;
        }

        // Returns which choice is currently selected.
        private int whichChoice()
        {
            int index = radioButtons.TakeWhile(r => !r.Checked).Count();
            return index;
        }
    }
}
