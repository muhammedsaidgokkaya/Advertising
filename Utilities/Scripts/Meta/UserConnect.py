import requests
import sys
import json

def get_facebook_campaigns(access_token, ad_account_id):
    api_version = "v21.0"
    url = f"https://graph.facebook.com/{api_version}/{ad_account_id}/campaigns"
    parameters = {
        "access_token": access_token,
        "fields": "name,status,objective,start_time,end_time"
    }
    response = requests.get(url, params=parameters)

    if response.status_code == 200:
        return response.json()
    else:
        return {"error": response.json()}

if __name__ == "__main__":
    access_token = sys.argv[1]
    ad_account_id = sys.argv[2]
    result = get_facebook_campaigns(access_token, ad_account_id)
    print(json.dumps(result))