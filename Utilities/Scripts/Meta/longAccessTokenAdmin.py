import requests
import sys
import json

def get_long_lived_token(app_id, app_secret, short_lived_token):
    token_url = f"https://graph.facebook.com/v21.0/oauth/access_token"
    
    params = {
        'grant_type': 'fb_exchange_token',
        'client_id': app_id,
        'client_secret': app_secret,
        'fb_exchange_token': short_lived_token
    }
    
    response = requests.get(token_url, params=params)
    
    if response.status_code == 200:
        return response.json()
    else:
        return {"error": response.json()}
    
if __name__ == "__main__":
    app_id = sys.argv[1]
    app_secret = sys.argv[2]
    short_lived_token = sys.argv[3]
    
    long_lived_token = get_long_lived_token(app_id, app_secret, short_lived_token)
    print(json.dumps(long_lived_token))