namespace AdminPanel.Models.Organization.Plan
{
    public class Subscription
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int PlanId { get; set; }
        public bool IsYearly { get; set; }
        public string Code { get; set; }
    }
}
