import aiohttp
import asyncio
import json

async def get_ad_accounts(session, access_token):
    url = "https://graph.facebook.com/v21.0/me/adaccounts"
    params = {
        "access_token": access_token,
        "fields": "id,name"
    }
    async with session.get(url, params=params) as response:
        if response.status == 200:
            data = await response.json()
            return [account["id"] for account in data.get("data", [])]
        else:
            return []

async def fetch_metric(session, access_token, ad_account_id, metric):
    url = f"https://graph.facebook.com/v21.0/{ad_account_id}/insights"
    params = {
        "access_token": access_token,
        "level": "account",
        "fields": metric,
        "date_preset": "maximum"
    }

    try:
        async with session.get(url, params=params) as response:
            if response.status == 200:
                data = await response.json()
                total = 0
                for row in data.get("data", []):
                    value = row.get(metric)
                    total += float(value) if value is not None else 0
                return {ad_account_id: round(total, 2)}
            else:
                error = await response.json()
                return {ad_account_id: {"error": error}}
    except Exception as e:
        return {ad_account_id: {"error": str(e)}}

async def fetch_all_metrics(access_token, metric):
    async with aiohttp.ClientSession() as session:
        ad_accounts = await get_ad_accounts(session, access_token)
        tasks = [fetch_metric(session, access_token, acc_id, metric) for acc_id in ad_accounts]
        responses = await asyncio.gather(*tasks)

    total_results = {}
    total_value = 0.0

    for r in responses:
        for acc_id, value in r.items():
            total_results[acc_id] = value
            if isinstance(value, (int, float)):
                total_value += value

    total_results[f"total_{metric}"] = round(total_value, 2)
    return total_results

async def main(access_token):
    results = await asyncio.gather(
        fetch_all_metrics(access_token, "spend"),
        fetch_all_metrics(access_token, "impressions"),
        fetch_all_metrics(access_token, "clicks")
    )

    spend_result, impressions_result, clicks_result = results

    combined_result = {
        "totalMeta": {
            "spend": spend_result.get("total_spend", 0),
            "impressions": impressions_result.get("total_impressions", 0),
            "clicks": clicks_result.get("total_clicks", 0)
        }
    }

    print(json.dumps(combined_result, indent=4))

if __name__ == "__main__":
    import sys
    access_token = sys.argv[1]

    asyncio.run(main(access_token))
