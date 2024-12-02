import sys
import json
from googleapiclient.discovery import build
from google.oauth2.credentials import Credentials


def get_google_analytics_account(access_token):
    credentials = Credentials(token=access_token)
    service = build("analyticsadmin", "v1beta", credentials=credentials)
    response = service.accountSummaries().list().execute()
    return response
    
if __name__ == "__main__":
    access_token = sys.argv[1]
    result = get_google_analytics_account(access_token)
    print(json.dumps(result))