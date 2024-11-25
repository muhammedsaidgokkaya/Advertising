import requests
import sys
import json

def get_access_token(client_id, client_secret, refresh_token):
    """
    Google OAuth 2.0 ile access token alma.
    """
    url = "https://oauth2.googleapis.com/token"
    data = {
        "client_id": client_id,
        "client_secret": client_secret,
        "refresh_token": refresh_token,
        "grant_type": "refresh_token"
    }

    response = requests.post(url, data=data)
    if response.status_code == 200:
        return response.json()
    else:
        return {"error": response.json()}

if __name__ == "__main__":
    client_id = sys.argv[1]
    client_secret = sys.argv[2]
    refresh_token = sys.argv[3]
    result = get_access_token(client_id, client_secret, refresh_token)
    print(json.dumps(result))