from googleapiclient.discovery import build
from google.oauth2.credentials import Credentials
import sys
import json
from datetime import datetime, timedelta

def get_google_search_console_metrics(access_token, site_url, start_date, end_date):
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

    row = response.get('rows', [{}])[0]
    metrics = {
        "total_clicks": row.get('clicks', 0),
        "total_impressions": row.get('impressions', 0),
        "average_ctr": row.get('ctr', 0),
        "average_position": row.get('position', 0)
    }

    return metrics

def calculate_change(previous, current):
    if previous == 0:
        return 0 if current == 0 else 0
    return ((current - previous) / previous) * 100

if __name__ == "__main__":
    access_token = sys.argv[1]
    site_url = sys.argv[2]
    start_date = sys.argv[3]
    end_date = sys.argv[4]

    start_date_obj = datetime.strptime(start_date, '%Y-%m-%d')
    end_date_obj = datetime.strptime(end_date, '%Y-%m-%d')

    delta = end_date_obj - start_date_obj
    previous_start_date = (start_date_obj - delta).strftime('%Y-%m-%d')
    previous_end_date = (end_date_obj - delta).strftime('%Y-%m-%d')

    current_metrics = get_google_search_console_metrics(access_token, site_url, start_date, end_date)
    previous_metrics = get_google_search_console_metrics(access_token, site_url, previous_start_date, previous_end_date)

    clicks_change = calculate_change(previous_metrics["total_clicks"], current_metrics["total_clicks"])
    impressions_change = calculate_change(previous_metrics["total_impressions"], current_metrics["total_impressions"])
    ctr_change = calculate_change(previous_metrics["average_ctr"], current_metrics["average_ctr"])
    position_change = calculate_change(previous_metrics["average_position"], current_metrics["average_position"])

    result = {
        "total_clicks": current_metrics["total_clicks"],
        "total_impressions": current_metrics["total_impressions"],
        "average_ctr": current_metrics["average_ctr"],
        "average_position": current_metrics["average_position"],
        "clicks_change": clicks_change,
        "impressions_change": impressions_change,
        "ctr_change": ctr_change,
        "position_change": position_change
    }

    print(json.dumps(result, indent=2, ensure_ascii=False))