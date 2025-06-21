namespace AdminPanel.Models.Organization.Plan
{
    public class AddNewCardSubscription
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Identity { get; set; }
        public bool IsYearly { get; set; }
        public string Mail { get; set; }
        public string Name { get; set; }
        public string OrgAddress { get; set; }
        public string Phone { get; set; }
        public int PlanId { get; set; }
        public string Zip { get; set; }
        public string CardHolder { get; set; }
        public string CardNumber { get; set; }
        public string Cvv { get; set; }
        public string ExpirationDate { get; set; }
    }
}
