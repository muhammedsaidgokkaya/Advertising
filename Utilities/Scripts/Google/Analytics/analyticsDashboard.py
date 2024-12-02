import sys
import json
from google.oauth2.credentials import Credentials
from google.analytics.data_v1beta import BetaAnalyticsDataClient
from google.analytics.data_v1beta.types import RunReportRequest, DateRange, Dimension, Metric

def get_google_analytics_dashboard(access_token, property_id, start_date, end_date):
    credentials = Credentials(token=access_token)

    client = BetaAnalyticsDataClient(credentials=credentials)

    request = RunReportRequest(
        property=f"properties/{property_id}",
        dimensions=[],
        metrics=[  
            Metric(name="activeUsers"),
            Metric(name="eventCount"),
            Metric(name="newUsers"),
            Metric(name="engagedSessions")
        ],
        date_ranges=[DateRange(start_date=start_date, end_date=end_date)],
    )

    response = client.run_report(request)

    result = []
    for row in response.rows:
        record = {}
        for i, metric in enumerate(response.metric_headers):
            record[metric.name] = row.metric_values[i].value
        result.append(record)

    return result

if __name__ == "__main__":
    access_token = sys.argv[1]
    property_id = sys.argv[2]
    start_date = sys.argv[3]
    end_date = sys.argv[4]
    try:
        result = get_google_analytics_dashboard(access_token, property_id, start_date, end_date)
        print(json.dumps(result))
    except Exception as e:
        print(json.dumps({"error": str(e)}), file=sys.stderr)