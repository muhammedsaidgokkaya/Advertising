from googleapiclient.discovery import build
from google.oauth2.credentials import Credentials
import sys
import json

def get_google_search_console_metrics_without_dimensions(access_token, site_url, start_date, end_date):
    creds = Credentials(token=access_token)

    service = build('searchconsole', 'v1', credentials=creds)

    response = service.searchanalytics().query(
        siteUrl=site_url,
        body={
            'startDate': start_date,
            'endDate': end_date,
            'rowLimit': 1
        }
    ).execute()

    total_clicks = response.get('rows', [{}])[0].get('clicks', 0)
    total_impressions = response.get('rows', [{}])[0].get('impressions', 0)
    avg_ctr = response.get('rows', [{}])[0].get('ctr', 0)
    avg_position = response.get('rows', [{}])[0].get('position', 0)

    metrics = {
        "total_clicks": total_clicks,
        "total_impressions": total_impressions,
        "average_ctr": avg_ctr,
        "average_position": avg_position
    }

    return metrics

if __name__ == "__main__":
    access_token = sys.argv[1]
    site_url = sys.argv[2]
    start_date = sys.argv[3]
    end_date = sys.argv[4]

    result = get_google_search_console_metrics_without_dimensions(access_token, site_url, start_date, end_date)
    print(json.dumps(result, indent=2, ensure_ascii=False))