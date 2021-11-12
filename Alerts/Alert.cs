using System;

namespace pkuManager.Alerts;

/// <summary>
/// An object that holds information about warnings/errors/notes in imported/exported format values.
/// </summary>
public class Alert
{
    /// <summary>
    /// The title of the alert.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// The message body of the tag.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Creates an Alert with the given title and message.
    /// </summary>
    /// <param name="title">The title of the alert.</param>
    /// <param name="message">The body of the alert.</param>
    public Alert(string title, string message)
    {
        Title = title;
        Message = message;
    }

    /// <summary>
    /// A reason for generating an Alert.
    /// </summary>
    public enum AlertType
    {
        /// <summary>
        /// When there is no alert to throw, but an <see cref="AlertType"/> is still demanded.<br/>
        /// The default <see cref="AlertType"/>.
        /// </summary>
        NONE,

        /// <summary>
        /// When a numerical value is too large, or a string is too long.
        /// </summary>
        OVERFLOW,

        /// <summary>
        /// When a numerical value is too small, or a string is too short.
        /// </summary>
        UNDERFLOW,

        /// <summary>
        /// When a value hasn't been specified.
        /// </summary>
        UNSPECIFIED,

        /// <summary>
        /// When a value is invalid in the given context.<br/>
        /// More general than <see cref="OVERFLOW"/> or <see cref="UNDERFLOW"/>.
        /// </summary>
        INVALID,

        /// <summary>
        /// When two different values conflict.
        /// </summary>
        MISMATCH,

        /// <summary>
        /// For values that can exist only in-battle.
        /// </summary>
        IN_BATTLE,

        /// <summary>
        /// For when a value (e.g. form) was casted to another.
        /// </summary>
        CASTED
    }

    /// <summary>
    /// Generates an exception to be thrown when an <see cref="AlertType"/><br/>
    /// was given to an Alert generating method that doesn't support it.
    /// </summary>
    /// <param name="at">The <see cref="AlertType"/> that was passed to a method
    ///                  that doesn't support it. Null by default.</param>
    /// <returns>An exception noting that the passed <see cref="AlertType"/> is unsupported.<br/>
    ///          If <paramref name="at"/> is null, just notes that no valid <see cref="AlertType"/>(s) were given.</returns>
    public static ArgumentException InvalidAlertType(AlertType? at = null) => new(at is null ?
        $"No valid AlertTypes were given to this alert method." :
        $"This alert method does not support the {at} AlertType");
}