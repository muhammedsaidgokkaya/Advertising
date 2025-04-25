namespace AdminPanel.Models.Organization.User
{
	public class SelectedAdsAccounts
	{
		public List<AdsAccount> SelectedAdsAccount { get; set; }
	}

	public class AdsAccount
	{
		public long Id { get; set; }
		public string Name { get; set; }
	}
}
