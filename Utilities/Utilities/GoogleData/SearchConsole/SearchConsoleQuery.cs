using Google.Apis.Auth.OAuth2;
using Google.Apis.SearchConsole.v1.Data;
using Google.Apis.SearchConsole.v1;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Utilities.GoogleData.SearchConsole
{
	public class SearchConsoleQuery
	{
		
	}

	public class Row
	{
		public string Keys { get; set; }
		public int Clicks { get; set; }
		public int Impressions { get; set; }
		public double Ctr { get; set; }
		public double Position { get; set; }
	}
}
