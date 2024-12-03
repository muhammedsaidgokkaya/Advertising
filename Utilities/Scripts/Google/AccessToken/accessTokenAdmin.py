import requests
import sys
import json

def get_access_token(client_id, client_secret, redirect_uri, authorization_code):
    """
    Google OAuth 2.0 ile access token alma.
    """
    url = "https://oauth2.googleapis.com/token"
    data = {
        "code": authorization_code,
        "client_id": client_id,
        "client_secret": client_secret,
        "redirect_uri": redirect_uri,
        "grant_type": "authorization_code",
        "access_type": "offline",
        "prompt": "consent"
    }

    response = requests.post(url, data=data)
    if response.status_code == 200:
        return response.json()
    else:
        return {"error": response.json()}

if __name__ == "__main__":
    client_id = sys.argv[1]
    client_secret = sys.argv[2]
    redirect_url = sys.argv[3]
    authorization_code = sys.argv[4]
    result = get_access_token(client_id, client_secret, redirect_url, authorization_code)
    print(json.dumps(result))