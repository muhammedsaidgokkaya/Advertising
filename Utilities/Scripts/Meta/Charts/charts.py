import aiohttp
import asyncio
import json
from datetime import datetime, timedelta

async def fetch_data(session, url, params, month_key):
    try:
        async with session.get(url, params=params) as response:
            if response.status == 200:
                data = await response.json()
                return {month_key: data}
            else:
                error = await response.json()
                return {month_key: {"error": error}}
    except Exception as e:
        return {month_key: {"error": str(e)}}

async def fetch_all_data(access_token, ad_account_id, date_ranges):
    api_version = "v21.0"
    url = f"https://graph.facebook.com/{api_version}/{ad_account_id}/insights"
    
    fields = "reach,frequency,spend,impressions,clicks"

    tasks = []
    async with aiohttp.ClientSession() as session:
        for month_start, month_end in date_ranges:
            params = {
                "access_token": access_token,
                "level": "account",
                "fields": fields,
                "time_range": json.dumps({
                    "since": month_start,
                    "until": month_end
                })
            }
            month_key = f"{month_start[:7]}"
            tasks.append(fetch_data(session, url, params, month_key))
        
        responses = await asyncio.gather(*tasks)
    
    results = {}
    for response in responses:
        results.update(response)
    return results

def generate_date_ranges(start_date, end_date):
    current_date = datetime.strptime(start_date, "%Y-%m-%d")
    end_date_dt = datetime.strptime(end_date, "%Y-%m-%d")
    
    date_ranges = []
    while current_date <= end_date_dt:
        month_start = current_date.replace(day=1).strftime("%Y-%m-%d")
        next_month = (current_date.replace(day=1) + timedelta(days=31)).replace(day=1)
        month_end = (next_month - timedelta(days=1)).strftime("%Y-%m-%d")
        if datetime.strptime(month_end, "%Y-%m-%d") > end_date_dt:
            month_end = end_date_dt.strftime("%Y-%m-%d")
        date_ranges.append((month_start, month_end))
        current_date = next_month
    
    return date_ranges

if __name__ == "__main__":
    import sys
    access_token = sys.argv[1]
    ad_account_id = sys.argv[2]
    start_date = sys.argv[3]
    end_date = sys.argv[4]
    
    date_ranges = generate_date_ranges(start_date, end_date)
    
    result = asyncio.run(fetch_all_data(access_token, ad_account_id, date_ranges))
    
    print(json.dumps(result, indent=4))