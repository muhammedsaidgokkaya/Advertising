import sys
import json
from google.oauth2.credentials import Credentials
from google.analytics.data_v1beta import BetaAnalyticsDataClient
from google.analytics.data_v1beta.types import RunReportRequest, DateRange, Dimension, Metric
from datetime import datetime, timedelta

def get_google_analytics_dashboard_monthly(access_token, property_id, start_date, end_date):
    credentials = Credentials(token=access_token)
    client = BetaAnalyticsDataClient(credentials=credentials)

    start = datetime.strptime(start_date, "%Y-%m-%d")
    end = datetime.strptime(end_date, "%Y-%m-%d")

    current = start
    results = []

    while current <= end:
        month_start = current.replace(day=1)
        next_month = (month_start + timedelta(days=32)).replace(day=1)
        month_end = (next_month - timedelta(days=1))

        if month_end > end:
            month_end = end

        request = RunReportRequest(
            property=f"properties/{property_id}",
            dimensions=[],
            metrics=[
                Metric(name="activeUsers"),
                Metric(name="eventCount"),
                Metric(name="newUsers"),
                Metric(name="engagedSessions")
            ],
            date_ranges=[
                DateRange(start_date=month_start.strftime("%Y-%m-%d"), end_date=month_end.strftime("%Y-%m-%d"))
            ],
        )

        response = client.run_report(request)

        month_result = {
            "month": month_start.strftime("%Y-%m"),
            "data": []
        }

        for row in response.rows:
            record = {}
            for i, metric in enumerate(response.metric_headers):
                record[metric.name] = row.metric_values[i].value
            month_result["data"].append(record)

        results.append(month_result)

        current = next_month

    return results

if __name__ == "__main__":
    access_token = sys.argv[1]
    property_id = sys.argv[2]
    start_date = sys.argv[3]
    end_date = sys.argv[4]
    try:
        result = get_google_analytics_dashboard_monthly(access_token, property_id, start_date, end_date)
        print(json.dumps(result, indent=4))
    except Exception as e:
        print(json.dumps({"error": str(e)}), file=sys.stderr)
