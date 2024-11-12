import requests
import sys
import json

def get_facebook_insights_filter(access_token, ad_account_id, start_date, end_date, filterParams):
    api_version = "v21.0"
    url = f"https://graph.facebook.com/{api_version}/act_{ad_account_id}/insights"
    parameters = {
        "access_token": access_token,
        'fields': 'impressions,reach,frequency,spend,clicks,cpc,ctr',
        'breakdowns': filterParams,
        "time_range": f'{{"since":"{start_date}","until":"{end_date}"}}'
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
    filterParams = sys.argv[5]
    result = get_facebook_insights_filter(access_token, ad_account_id, start_date, end_date, filterParams)
    print(json.dumps(result))