import requests
import sys
import json

def get_google_search_console_sites(access_token):
    url = f"https://www.googleapis.com/webmasters/v3/sites"
    headers = {
        "Authorization": f"Bearer {access_token}"
    }
    response = requests.get(url, headers=headers)

    if response.status_code == 200:
        return response.json()
    else:
        try:
            error_details = response.json()
        except json.JSONDecodeError:
            error_details = {"message": "Response is not in JSON format", "content": response.text}
        return {"error": {"status_code": response.status_code, "details": error_details}}

if __name__ == "__main__":
    access_token = sys.argv[1]
    result = get_google_search_console_sites(access_token)
    print(json.dumps(result))