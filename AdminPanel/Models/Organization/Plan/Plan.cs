namespace AdminPanel.Models.Organization.Plan
{
    public class Plan
    {
        public float Amount { get; set; }
        public int PlanId { get; set; }
        public bool IsYearly { get; set; }
        public bool IsPayment { get; set; }
    }

    public class AddPlan
    {
        public float Amount { get; set; }
        public int PlanId { get; set; }
        public bool IsYearly { get; set; }
        public bool IsPayment { get; set; }
    }
}
