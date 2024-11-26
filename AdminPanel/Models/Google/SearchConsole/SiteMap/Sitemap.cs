namespace AdminPanel.Models.Google.SearchConsole.SiteMap
{
    public class Sitemap
    {
        public string Path { get; set; }

        public DateTime LastSubmitted { get; set; }

        public bool IsPending { get; set; }

        public bool IsSitemapsIndex { get; set; }

        public string Type { get; set; }

        public DateTime LastDownloaded { get; set; }

        public int Warnings { get; set; }

        public int Errors { get; set; }

        public List<Content> Contents { get; set; }
    }
}
