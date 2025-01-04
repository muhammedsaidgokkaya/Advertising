import requests
import sys
import json
from datetime import datetime, timedelta

def get_facebook_insights(access_token, ad_account_id, start_date, end_date):
    api_version = "v21.0"
    url = f"https://graph.facebook.com/{api_version}/{ad_account_id}/insights"
    current_date = datetime.strptime(start_date, "%Y-%m-%d")
    end_date_dt = datetime.strptime(end_date, "%Y-%m-%d")
    
    results = {}

    while current_date <= end_date_dt:
        month_start = current_date.replace(day=1)
        next_month = (month_start + timedelta(days=31)).replace(day=1)
        month_end = (next_month - timedelta(days=1)).strftime("%Y-%m-%d")
        if datetime.strptime(month_end, "%Y-%m-%d") > end_date_dt:
            month_end = end_date_dt.strftime("%Y-%m-%d")

        parameters = {
            "access_token": access_token,
            "level": "account",
            "fields": ",".join([
                "reach",
                "frequency",
                "spend",
                "impressions",
                "clicks"
            ]),
            "action_breakdowns": ["action_type"],
            "time_range": f'{{"since":"{month_start.strftime("%Y-%m-%d")}","until":"{month_end}"}}'
        }

        response = requests.get(url, params=parameters)
        if response.status_code == 200:
            results[f"{month_start.strftime('%Y-%m')}"] = response.json()
        else:
            results[f"{month_start.strftime('%Y-%m')}"] = {"error": response.json()}
        
        current_date = next_month

    return results

if __name__ == "__main__":
    access_token = sys.argv[1]
    ad_account_id = sys.argv[2]
    start_date = sys.argv[3]
    end_date = sys.argv[4]
    result = get_facebook_insights(access_token, ad_account_id, start_date, end_date)
    print(json.dumps(result, indent=4))