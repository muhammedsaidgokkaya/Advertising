import requests
import sys
import json

def get_facebook_ads(access_token, ad_account_id, start_date, end_date):
    api_version = "v21.0"
    url = f"https://graph.facebook.com/{api_version}/act_{ad_account_id}/ads"
    parameters = {
        "access_token": access_token,
        "fields": "name,configured_status,adset{name,bid_strategy,daily_budget,updated_time},insights{reach,impressions,cpc,cpm,spend,quality_ranking,engagement_rate_ranking,conversion_rate_ranking,date_start,date_stop}",
        "since": start_date,
        "until": end_date
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
    result = get_facebook_ads(access_token, ad_account_id, start_date, end_date)
    print(json.dumps(result))