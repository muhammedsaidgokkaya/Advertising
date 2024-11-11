import requests
import sys
import json

def get_facebook_accounts(access_token, business_id):
    api_version = "v21.0"
    url = f"https://graph.facebook.com/{api_version}/{business_id}/owned_ad_accounts"
    parameters = {
        "access_token": access_token,
        "fields": "id,name"
    }
    response = requests.get(url, params=parameters)

    if response.status_code == 200:
        return response.json()
    else:
        return {"error": response.json()}

if __name__ == "__main__":
    access_token = sys.argv[1]
    business_id = sys.argv[2]
    result = get_facebook_accounts(access_token, business_id)
    print(json.dumps(result))