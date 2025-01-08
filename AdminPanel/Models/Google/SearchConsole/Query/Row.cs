namespace AdminPanel.Models.Google.SearchConsole.Query
{
    public class Row
    {
        public int Id { get; set; }

        public string Keys { get; set; }

        public int? Clicks { get; set; }

        public int? Impressions { get; set; }

        public double Ctr { get; set; }

        public double Position { get; set; }
    }
}
