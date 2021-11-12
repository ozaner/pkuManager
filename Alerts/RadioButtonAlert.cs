using System;

namespace pkuManager.Alerts;

/// <summary>
/// An <see cref="Alert"/> with a set of exclusive options that the user must choose form to resolve it.
/// </summary>
public class RadioButtonAlert : Alert
{
    /// <summary>
    /// All the different choices associated with this alert.
    /// </summary>
    public RBAChoice[] Choices { get; }

    /// <summary>
    /// The index of the currently selected choice in <see cref="Choices"/>.
    /// </summary>
    public int SelectedIndex { get; set; }

    /// <summary>
    /// Constructs an <see cref="RadioButtonAlert"/> object.
    /// </summary>
    /// <param name="choices">An array of choices defining this alert. Must have at least one entry.</param>
    /// <param name="initialChoice">The index of the choice that is set initially. 0 by default.</param>
    /// <inheritdoc cref="Alert(string, string)"/>
    public RadioButtonAlert(string title, string message, RBAChoice[] choices, int initialChoice = 0) : base(title, message)
    {
        // Check if there is at least 1 choice
        if (choices?.Length is not > 0)
            throw new ArgumentException($"{nameof(choices)} must have at least 1 entry.", nameof(choices));

        Choices = choices;
        SelectedIndex = initialChoice;
    }

    /// <summary>
    /// The data defining a single choice in a <see cref="RadioButtonAlert"/>.
    /// </summary>
    public class RBAChoice : Alert
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
        /// Constructs an <see cref="RBAChoice"/> object.
        /// </summary>
        /// <param name="hasTextEntry">Whether this choice should have text entry associated with it.</param>
        /// <inheritdoc cref="Alert(string, string)"/>
        public RBAChoice(string title, string message, bool hasTextEntry = false) : base(title, message)
            => HasTextEntry = hasTextEntry;
    }
}