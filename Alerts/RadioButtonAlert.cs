using System;

namespace pkuManager.Alerts
{
    public class RadioButtonAlert : Alert
    {
        private int selectedIndex;
        public (string, string)[] choices { get; set; }

        /// <summary>
        /// Constructor for a RadioButtonAlert.
        /// </summary>
        /// <param name="title">Title of the alert</param>
        /// <param name="message">Body of the alert</param>
        /// <param name="choices">An array of string doubles. The first index represent the option name, the second is the option description.</param>
        /// <param name="initialChoice">The index of the choice that is set initially. By default it is the first (i.e. 0).</param>
        public RadioButtonAlert(string title, string message, (string, string)[] choices, int initialChoice = 0) : base(title, message)
        {
            // Check if there is at least 1 choice
            if (choices == null || choices.Length == 0)
                throw new Exception("RadioButtonAlerts must have at least 1 choice (preferably 2).");

            this.choices = choices;
            setSelectedIndex(initialChoice);
        }

        /// <summary>
        /// Returns the index of the currently selected choice.
        /// </summary>
        /// <returns></returns>
        public int getSelectedIndex()
        {
            return selectedIndex;
        }

        /// <summary>
        /// Select the <i>index</i>-th choice.
        /// </summary>
        /// <param name="index">The index of the choice to select</param>
        public void setSelectedIndex(int index)
        {
            if (index > choices.Length || index < 0)
                throw new Exception("This RadioButtonAlert only has " + choices.Length + " choices. Choice " + index + " is not valid.");

            selectedIndex = index;
        }
    }
}
