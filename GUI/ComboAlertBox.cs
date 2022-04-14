using pkuManager.Alerts;
using System;
using System.Windows.Forms;
using static pkuManager.Alerts.ChoiceAlert;

namespace pkuManager.GUI;

public class ComboAlertBox : AlertBox
{
    protected ChoiceAlert alert;

    //gui elements
    protected ComboBox comboBox = new();
    protected RichTextBox currentMsgBox;
    protected TextBox currentTextEntryBox;

    // Creates an AlertBox with n RadioButtons for getting xor input form the user. Should be used for error boxes.
    public ComboAlertBox(ChoiceAlert alert, int containerWidth) : base(alert, containerWidth)
    {
        this.alert = alert;

        // add each choice
        foreach (SingleChoice rbac in alert.Choices)
            comboBox.Items.Add(rbac.Title);

        Controls.Add(comboBox);

        //to update msg and alert selection
        comboBox.SelectedIndexChanged += OnComboSwitch;

        //check the default option
        comboBox.SelectedIndex = alert.SelectedIndex;
    }

    protected void OnComboSwitch(object s, EventArgs e)
    {
        SingleChoice choice = alert.Choices[comboBox.SelectedIndex];
        Controls.Remove(currentMsgBox);
        Controls.Remove(currentTextEntryBox);

        // add message if it exists
        if (choice.Message is not null)
        {
            currentMsgBox = newRichTextBox(choice.Message);
            Controls.Add(currentMsgBox);
        }

        // add text entry box if it has one
        if (choice.HasTextEntry)
        {
            currentTextEntryBox = new();
            Controls.Add(currentTextEntryBox);

            //update underlying single choice whenever text entry changed.
            currentTextEntryBox.TextChanged += (s, e) => choice.TextEntry =
                currentTextEntryBox.Text?.Length is 0 ? null : currentTextEntryBox.Text;
        }
        alert.SelectedIndex = comboBox.SelectedIndex; //update underlying alert
    }
}
