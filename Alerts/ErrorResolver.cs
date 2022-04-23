using OneOf;
using pkuManager.Formats.Fields;
using System;

namespace pkuManager.Alerts;

/// <summary>
/// An object that can defer the setting of a property, based on the user's choice of an option in a given alert.
/// </summary>
/// <typeparam name="T">The type of the value to be resolved.</typeparam>
public class ErrorResolver<T>
{
    /// <summary>
    /// The RadioButtonAlert that this ErrorResolver will decide it's value based on.<br/>
    /// Null if any other type of alert was passed.
    /// </summary>
    protected ChoiceAlert rba;

    /// <summary>
    /// Functions that return the values of the different choices the <see cref="rba"/> contains.
    /// </summary>
    protected readonly OneOf<T, Func<T>>[] Options;

    /// <summary>
    /// The field that will store the chosen value.
    /// </summary>
    protected IField<T> Field;

    /// <summary>
    /// Creates an ErrorResolver object.
    /// </summary>
    /// <param name="alert">The relevant alert.</param>
    /// <param name="options">An array of values, or functions that return values, coresponding to
    ///                       the <paramref name="alert"/>.<br/> Should only have one value if
    ///                       <paramref name="alert"/> is not a <see cref="ChoiceAlert"/>.</param>
    /// <param name="field">A function that sets a value to the desired property.</param>
    public ErrorResolver(Alert alert, IField<T> field, params OneOf<T, Func<T>>[] options)
    {
        Options = options;
        Field = field;

        if (alert is ChoiceAlert rba)
        {
            this.rba = rba;
            if (rba.Choices.Length != options.Length)
                throw new ArgumentException("Number of RadioButtonAlert choices should equal number of option values.", nameof(options));
        }
        else //Not a RadioButtonAlert.
        {
            if (options.Length is not 1)
                throw new ArgumentException("Can only have 1 option for non-error Alerts.");
        }
    }

    /// <inheritdoc cref="ErrorResolver(Alert, IField{T}, OneOf{T, Func{T}}[])"/>
    public ErrorResolver(Alert alert, IField<T> IField, params T[] options)
        : this(alert, IField, new OneOf<T, Func<T>>[options.Length])
    {
        for (int i = 0; i < options.Length; i++)
            Options[i] = options[i];
    }

    public static implicit operator Action(ErrorResolver<T> er) => () => er.DecideValue();

    private static T ReadOption(OneOf<T, Func<T>> val)
        => val.Match(x => x, x => x.Invoke());

    /// <summary>
    /// Finalizes and sets the value corresponding to the chosen option.
    /// </summary>
    public void DecideValue()
        => Field.Value = ReadOption(rba is null ? Options[0] : Options[rba.SelectedIndex]);
}