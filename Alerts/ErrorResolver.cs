using System;

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
        /// The values of the different choices the <see cref="rba"/> contains.
        /// </summary>
        protected readonly T[] options;

        /// <summary>
        /// An action that can be used to set the chosen value.
        /// </summary>
        protected Action<T> setter;

        /// <summary>
        /// Creates an ErrorResolver object.
        /// </summary>
        /// <param name="alert">The relevant alert.</param>
        /// <param name="options">An array of values coresponding to the <paramref name="alert"/>.<br/>
        ///                       Should only have one value if <paramref name="alert"/>
        ///                       is not a <see cref="RadioButtonAlert"/>.</param>
        /// <param name="setter">A function that sets a value to the desired property.</param>
        public ErrorResolver(Alert alert, T[] options, Action<T> setter)
        {
            this.options = options;
            this.setter = setter;

            if (alert is RadioButtonAlert) //A RadioButtonAlert, add to errors and initalize decision variables.
            {
                rba = alert as RadioButtonAlert;
                if (rba.choices.Length != options.Length)
                    throw new ArgumentException("Number of RadioButtonAlert choices should equal number of option values.", nameof(options));
            }
            else //Not a RadioButtonAlert, add to warnings.
            {
                if (options.Length is not 1)
                    throw new ArgumentException("Can only have 1 option for non-error Alerts.");
            }
        }

        /// <summary>
        /// Finalizes and sets the value corresponding to the chosen option.
        /// </summary>
        public void DecideValue()
        {
            if (rba is not null)
                setter(options[rba.getSelectedIndex()]); //get the option corresponding to the currently selected alert choice.
            else
                setter(options[0]); //Not an error so only one option.
        }
    }
}
