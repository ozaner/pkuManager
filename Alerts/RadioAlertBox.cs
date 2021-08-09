using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace pkuManager.Alerts
{
    public class RadioAlertBox : AlertBox
    {
        // A dictionary of all the radio buttons created for each choice.
        protected Dictionary<string, RadioButton> radioButtons = new Dictionary<string, RadioButton>();

        // Creates an AlertBox with n RadioButtons for getting xor input form the user. Should be used for error boxes.
        public RadioAlertBox(string label, string message, string[] choices, string[] choiceMessages) : base(label, message)
        {
            if (choices.Length == 0)
                throw new Exception("There must be at least 1 string in choices!");
            if (choices.Length != choiceMessages.Length)
                throw new Exception("choices and choiceMessages must have same size!");

            int xOffset = messageTextbox.Location.X;
            int yOffset = messageTextbox.Location.Y + messageTextbox.Size.Height;
            for (int i = 0; i < choices.Length; i++)
            {
                RadioButton rb = new RadioButton();
                rb.Text = choices[i];
                rb.Font = new Font(rb.Font, FontStyle.Underline);
                TextBox tb = newTextBox(choiceMessages[i]);
                rb.AutoSize = true;
                rb.Padding = new Padding(0, 0, 0, 0);

                rb.Location = new Point(xOffset,yOffset);
                yOffset += rb.Size.Height-10;
                tb.Location = new Point(xOffset+20, yOffset);
                yOffset += tb.Size.Height;

                Controls.Add(rb);
                Controls.Add(tb);

                radioButtons.Add(choices[i], rb);
            }

            // Check the first option so at leat one option is alwys checked.
            radioButtons[radioButtons.First().Key].Checked = true;
        }

        // Returns which choice is currently selected.
        public string whichChoice()
        {
            RadioButton chosenButton = radioButtons.Values.FirstOrDefault(r => r.Checked);
            return chosenButton.Text;
        }
    }
}
