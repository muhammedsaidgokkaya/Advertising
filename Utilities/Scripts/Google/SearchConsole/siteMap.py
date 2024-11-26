from googleapiclient.discovery import build
from google.oauth2.credentials import Credentials
import sys
import json

def get_sitemap_urls(access_token, site_url):
    creds = Credentials(token=access_token)

    service = build('searchconsole', 'v1', credentials=creds)

    response = service.sitemaps().list(siteUrl=site_url).execute()
    
    return response

if __name__ == "__main__":
    access_token = sys.argv[1]
    site_url = sys.argv[2]
    
    sitemap_result = get_sitemap_urls(access_token, site_url)
    
    print(json.dumps(sitemap_result))