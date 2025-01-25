import aiohttp
import asyncio
import sys
import json
sys.stdout.reconfigure(encoding='utf-8')

async def get_facebook_audiences(access_token, ad_account_id):  
    api_version = "v21.0"
    saved_audiences_url = f"https://graph.facebook.com/{api_version}/{ad_account_id}/saved_audiences"
    
    params = {
        "access_token": access_token,
        "fields": "id,name,approximate_count_upper_bound,approximate_count_lower_bound,time_created,time_updated,targeting"
    }
    
    async with aiohttp.ClientSession() as session:
        async with session.get(saved_audiences_url, params=params) as response:
            if response.status != 200:
                return {"error": await response.json()}
            
            saved_audiences = (await response.json()).get("data", [])
            for audience in saved_audiences:
                audience["audienceType"] = 'saved'
    
    return saved_audiences

async def main():
    access_token, ad_account_id = sys.argv[1], sys.argv[2]
    audiences = await get_facebook_audiences(access_token, ad_account_id)
    print(json.dumps(audiences, indent=4, ensure_ascii=False))

if __name__ == "__main__":
    asyncio.run(main())