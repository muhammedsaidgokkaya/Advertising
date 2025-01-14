using AdminPanel.Models.Google.Analytics.Summary;
using AdminPanel.Models.Google.SearchConsole.Site;
using AdminPanel.Models.Meta.AdvertisingAccount;

namespace AdminPanel.Models.Organization.User
{
    public class AddAccountSetting
    {
        public List<Sites> SelectedSites { get; set; }
        public List<AccountSummary> SelectedAnalytics { get; set; }
        public List<AdvertisingAccount> SelectedAdvertisingAccount { get; set; }
    }
}
