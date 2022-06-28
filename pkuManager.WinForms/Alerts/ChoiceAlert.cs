using System;

namespace pkuManager.WinForms.Alerts;

/// <summary>
/// An <see cref="Alert"/> with a set of exclusive options that the user must choose form to resolve it.
/// </summary>
public class ChoiceAlert : Alert
{
    /// <summary>
    /// All the different choices associated with this alert.
    /// </summary>
    public SingleChoice[] Choices { get; }

    /// <summary>
    /// The index of the currently selected choice in <see cref="Choices"/>.
    /// </summary>
    public int SelectedIndex { get; set; }

    /// <summary>
    /// Whether or not this choice alert is displayed using radio buttons (true) or a combobox (false).
    /// </summary>
    public bool UsesRadioButtons { get; }

    /// <summary>
    /// Constructs an <see cref="ChoiceAlert"/> object.
    /// </summary>
    /// <param name="choices">An array of choices defining this alert. Must have at least one entry.</param>
    /// <param name="initialChoice">The index of the choice that is set initially. 0 by default.</param>
    /// <inheritdoc cref="Alert(string, string)"/>
    public ChoiceAlert(string title, string message, SingleChoice[] choices,
        bool usesRadioButtons, int initialChoice = 0) : base(title, message)
    {
        // Check if there is at least 1 choice
        if (choices?.Length is not > 0)
            throw new ArgumentException($"{nameof(choices)} must have at least 1 entry.", nameof(choices));

        Choices = choices;
        UsesRadioButtons = usesRadioButtons;
        SelectedIndex = initialChoice;
    }

    /// <summary>
    /// The data defining a single choice in a <see cref="ChoiceAlert"/>.
    /// </summary>
    public class SingleChoice : Alert
    {
        /// <summary>
        /// Whether or not this choice has a text entry box associated with it.
        /// </summary>
        public bool HasTextEntry { get; }

        /// <summary>
        /// The contents of the text entry box. Should be updated by the containing <see cref="GUI.RadioAlertBox"/>.<br/>
        /// <see langword="null"/> if <see cref="HasTextEntry"/> is <see langword="false"/>.
        /// </summary>
        public string TextEntry { get; set; }

        /// <summary>
        /// Constructs an <see cref="SingleChoice"/> object.
        /// </summary>
        /// <param name="hasTextEntry">Whether this choice should have text entry associated with it.</param>
        /// <inheritdoc cref="Alert(string, string)"/>
        public SingleChoice(string title, string message, bool hasTextEntry = false) : base(title, message)
            => HasTextEntry = hasTextEntry;
    }
}