namespace pkuManager.Alerts
{
    public class Alert
    {
        public string title { get; set; }
        public string message { get; set; }

        public Alert(string title, string message)
        {
            this.title = title;
            this.message = message;
        }
    }
}
