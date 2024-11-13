namespace AdminTest.Models.Meta.Insight
{
    public class InsightGeneral
    {
        public int Reach { get; set; }

        public double Frequency { get; set; }

        public double Ctr { get; set; }

        public int Impressions { get; set; }

        public double Cpc { get; set; }

        public double Cpm { get; set; }

        public double Spend { get; set; }

        public int Clicks { get; set; }

        public DateTime DateStart { get; set; }

        public DateTime DateStop { get; set; }

        public List<Action.Action> OutboundClicks { get; set; }

        public List<Action.Action> OutboundClicksCtr { get; set; }

        public List<Action.Action> Actions { get; set; }
    }
}
