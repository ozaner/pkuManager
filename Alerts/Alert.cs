﻿using pkuManager.Utilities;
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

    public static Alert operator +(Alert a1, Alert a2)
    {
        if (a1 is null)
            return a2?.Clone();
        else if (a2 is null)
            return a1?.Clone();
        else //a1 & a2 not null
        {
            string title = a1.Title == a2.Title ? a1.Title : $"{a1.Title}, {a2.Title}";
            string msg;
            if (a1.Message?.Length > 0 && a2.Message?.Length > 0) //both have msgs
                msg = a1.Message + DataUtil.Newline(2) + a2.Message;
            else if (a1.Message?.Length > 0) //only a1 has message
                msg = a1.Message;
            else if (a2.Message?.Length > 0) //only a2 has message
                msg = a2.Message;
            else //neither has message
                msg = "";
            return new Alert(title, msg);
        }
    }
    
    public Alert Clone() => new(Title, Message);

    /// <summary>
    /// A reason for generating an Alert.
    /// </summary>
    [Flags]
    public enum AlertType
    {
        /// <summary>
        /// When there is no alert to throw, but an <see cref="AlertType"/> is still demanded.<br/>
        /// The default <see cref="AlertType"/>.
        /// </summary>
        NONE = 0,

        /// <summary>
        /// When a numerical value is too large.
        /// </summary>
        OVERFLOW = 1,

        /// <summary>
        /// When a numerical value is too small.
        /// </summary>
        UNDERFLOW = 2,

        /// <summary>
        /// When a value hasn't been specified.
        /// </summary>
        UNSPECIFIED = 4,

        /// <summary>
        /// When a value is invalid in the given context.<br/>
        /// More general than <see cref="OVERFLOW"/> or <see cref="UNDERFLOW"/>.
        /// </summary>
        INVALID = 8,

        /// <summary>
        /// When two different values conflict.
        /// </summary>
        MISMATCH = 16,

        /// <summary>
        /// For values that can exist only in-battle.
        /// </summary>
        IN_BATTLE = 32,

        /// <summary>
        /// For when a value (e.g. form) was casted to another.
        /// </summary>
        CASTED = 64,

        /// <summary>
        /// For when an array/string is too long.
        /// </summary>
        TOO_LONG = 128,

        /// <summary>
        /// For when an array/string is too short.
        /// </summary>
        TOO_SHORT = 256
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