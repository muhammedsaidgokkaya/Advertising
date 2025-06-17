namespace AdminPanel.Models.Organization.Plan
{
    public class AddSubscription
    {
        public int Amount { get; set; }
        public string Code { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Identity { get; set; }
        public bool IsPayment { get; set; }
        public bool IsYearly { get; set; }
        public string Mail { get; set; }
        public string Name { get; set; }
        public string OrgAddress { get; set; }
        public string Phone { get; set; }
        public int PlanId { get; set; }
        public string Zip { get; set; }
    }
}
