using System;
using System.Linq;

namespace pkuManager.Alerts
{
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
        protected RadioButtonAlert rba;

        /// <summary>
        /// Functions that return the values of the different choices the <see cref="rba"/> contains.
        /// </summary>
        protected readonly Func<T>[] Options;

        /// <summary>
        /// An action that can be used to set the chosen value.
        /// </summary>
        protected Action<T> Setter;

        /// <summary>
        /// Creates an ErrorResolver object.
        /// </summary>
        /// <param name="alert">The relevant alert.</param>
        /// <param name="options">An array of functions that return values coresponding to the <paramref name="alert"/>.<br/>
        ///                       Should only have one value if <paramref name="alert"/>
        ///                       is not a <see cref="RadioButtonAlert"/>.</param>
        /// <param name="setter">A function that sets a value to the desired property.</param>
        public ErrorResolver(Alert alert, Func<T>[] options, Action<T> setter)
        {
            Options = options;
            Setter = setter;

            if (alert is RadioButtonAlert) //A RadioButtonAlert, initalize decision variables.
            {
                rba = alert as RadioButtonAlert;
                if (rba.Choices.Length != options.Length)
                    throw new ArgumentException("Number of RadioButtonAlert choices should equal number of option values.", nameof(options));
            }
            else //Not a RadioButtonAlert.
            {
                if (options.Length is not 1)
                    throw new ArgumentException("Can only have 1 option for non-error Alerts.");
            }
        }

        /// <param name="options">An array of values coresponding to the <paramref name="alert"/>.<br/>
        ///                       Should only have one value if <paramref name="alert"/>
        ///                       is not a <see cref="RadioButtonAlert"/>.</param>
        /// <inheritdoc cref="ErrorResolver(Alert, Func{T}[], Action{T})"/>
        public ErrorResolver(Alert alert, T[] options, Action<T> setter) : this(alert, options.Select<T, Func<T>>(x => () => x).ToArray(), setter) { }

        /// <summary>
        /// Finalizes and sets the value corresponding to the chosen option.
        /// </summary>
        public void DecideValue()
        {
            if (rba is not null)
                Setter(Options[rba.SelectedIndex].Invoke()); //get the option corresponding to the currently selected alert choice.
            else
                Setter(Options[0].Invoke()); //Not an error so only one option.
        }
    }
}
