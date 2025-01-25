import aiohttp
import asyncio 
import sys  
import json
sys.stdout.reconfigure(encoding='utf-8')

async def get_facebook_audiences(access_token, ad_account_id):  
    api_version = "v21.0"
    custom_audiences_url = f"https://graph.facebook.com/{api_version}/{ad_account_id}/customaudiences"
    
    params = {
        "access_token": access_token,
        "fields": "id,name,approximate_count_upper_bound,approximate_count_lower_bound,lookalike_spec,time_created,time_updated"
    }
    
    async with aiohttp.ClientSession() as session:
        async with session.get(custom_audiences_url, params=params) as response:
            if response.status != 200:
                return {"error": await response.json()}
            
            custom_audiences = (await response.json()).get("data", [])
            
            for audience in custom_audiences:
                if audience.get("lookalike_spec") is not None:
                    audience["audienceType"] = 'lookalike'
                else:
                    audience["audienceType"] = 'custom'

    return custom_audiences

async def main():
    access_token, ad_account_id = sys.argv[1], sys.argv[2]
    audiences = await get_facebook_audiences(access_token, ad_account_id)
    print(json.dumps(audiences, indent=4, ensure_ascii=False))

if __name__ == "__main__":
    asyncio.run(main())