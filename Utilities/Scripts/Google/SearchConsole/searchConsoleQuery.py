from googleapiclient.discovery import build
from google.oauth2.credentials import Credentials
import requests
import sys
import json

def get_google_search_console_sites_query(access_token, site_url, rows, dimensions, start_date, end_date):
    creds = Credentials(token=access_token)

    service = build('searchconsole', 'v1', credentials=creds)

    response = service.searchanalytics().query(
        siteUrl=site_url,
        body={
            'startDate': start_date,
            'endDate': end_date,
            'dimensions': [dimensions],
            'rowLimit': rows
        }
    ).execute()

    return response
    
if __name__ == "__main__":
    access_token = sys.argv[1]
    site_url = sys.argv[2]
    rows = sys.argv[3]
    dimensions = sys.argv[4] # query, page, country, device, searchAppearance, date
    start_date = sys.argv[5]
    end_date = sys.argv[6]
    result = get_google_search_console_sites_query(access_token, site_url, rows, dimensions, start_date, end_date)
    print(json.dumps(result))