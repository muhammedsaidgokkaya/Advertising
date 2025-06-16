namespace AdminPanel.Models.Organization.Plan
{
    public class Card
    {
        public string CardNumber { get; set; }
        public string CardHolder { get; set; }
        public string Cvv { get; set; }
        public string ExpirationDate { get; set; }
    }

    public class AddCard
    {
        public string CardNumber { get; set; }
        public string CardHolder { get; set; }
        public string Cvv { get; set; }
        public string ExpirationDate { get; set; }
    }
}
