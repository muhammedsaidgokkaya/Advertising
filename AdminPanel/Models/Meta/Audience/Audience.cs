namespace AdminPanel.Models.Meta.Audience
{
    public class Audience
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int ApproximateCountUpperBound { get; set; }
        public int ApproximateCountLowerBound { get; set; }
        public string AudienceType { get; set; }
        public string AudienceTypeText { get; set; }
        public DateTime TimeCreated { get; set; }
        public DateTime TimeUpdated { get; set; }
        public string Gender { get; set; }
        public string AgeRange { get; set; }
        public string Countries { get; set; }
        public string TargetAudienceSize { get; set; }
    }
}
