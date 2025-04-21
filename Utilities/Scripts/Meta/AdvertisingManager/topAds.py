import requests
import sys
import json

def get_facebook_ad_accounts(access_token):
    url = f"https://graph.facebook.com/v21.0/me/adaccounts"
    parameters = {
        "access_token": access_token,
        "fields": "id,name"
    }
    response = requests.get(url, params=parameters)

    if response.status_code == 200:
        return response.json()['data']
    else:
        return {"error": response.json()}

def get_ads_from_account(access_token, ad_account_id):
    url = f"https://graph.facebook.com/v21.0/{ad_account_id}/ads"
    parameters = {
        "access_token": access_token,
        "fields": "id,name,creative{image_url}",
        "limit": 3
    }
    response = requests.get(url, params=parameters)

    if response.status_code == 200:
        return response.json()['data']
    else:
        return {"error": response.json()}

def get_all_ads(access_token):
    ad_accounts = get_facebook_ad_accounts(access_token)
    
    if 'error' in ad_accounts:
        return ad_accounts
    
    all_ads = []

    for account in ad_accounts:
        ad_account_id = account['id']
        ads = get_ads_from_account(access_token, ad_account_id)
        
        if 'error' not in ads:
            for ad in ads:
                ad_info = {
                    "name": ad.get("name"),
                    "image_url": ad.get("creative", {}).get("image_url")
                }
                all_ads.append(ad_info)
        else:
            all_ads.append({"error": ads["error"]})
    
    return all_ads

if __name__ == "__main__":
    access_token = sys.argv[1]
    result = get_all_ads(access_token)
    print(json.dumps(result, indent=2))