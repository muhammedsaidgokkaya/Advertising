import requests
import sys
import json

def get_facebook_campaigns(access_token):
    api_version = "v21.0"
    url = f"https://graph.facebook.com/{api_version}/me/businesses"
    parameters = {
        "access_token": access_token
    }
    response = requests.get(url, params=parameters)

    if response.status_code == 200:
        return response.json()
    else:
        return {"error": response.json()}

if __name__ == "__main__":
    access_token = sys.argv[1]
    result = get_facebook_campaigns(access_token)
    print(json.dumps(result))