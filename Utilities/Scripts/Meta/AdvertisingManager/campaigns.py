import requests
import sys
import json

def get_facebook_campaigns(access_token, ad_account_id, start_date, end_date):
    api_version = "v21.0"
    url = f"https://graph.facebook.com/{api_version}/{ad_account_id}/campaigns"
    parameters = {
        "access_token": access_token,
        "fields": (
            "id,name,status,bid_strategy,daily_budget,account_id,"
            "insights.time_range({"f"'since':'{start_date}', 'until':'{end_date}'"
            "}){reach,impressions,cpc,cpm,spend,actions},end_time"
        )
    }
    response = requests.get(url, params=parameters)

    if response.status_code == 200:
        return response.json()
    else:
        return {"error": response.json()}

if __name__ == "__main__":
    access_token = sys.argv[1]
    ad_account_id = sys.argv[2]
    start_date = sys.argv[3]
    end_date = sys.argv[4]
    result = get_facebook_campaigns(access_token, ad_account_id, start_date, end_date)
    print(json.dumps(result))