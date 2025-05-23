# Set CLIENT_SECRET and VERIFY_TOKEN environment variables

# List of existing push subscriptions 
curl -X GET "https://www.strava.com/api/v3/push_subscriptions?client_id=115985&client_secret=CLIENT_SECRET"

# Delete an existing push subscription by SUBSCRIPTION_ID
curl -X DELETE "https://www.strava.com/api/v3/push_subscriptions/SUBSCRIPTION_ID?client_id=115985&client_secret=CLIENT_SECRET"

# Create a new push subscription
curl -X POST https://www.strava.com/api/v3/push_subscriptions \
      -F client_id=115985 \
      -F client_secret=CLIENT_SECRET \
      -F callback_url=https://fitsynchubfunctions.azurewebsites.net/strava/webhook \
      -F verify_token=VERIFY_TOKEN

# Link to authorize the app
http://www.strava.com/oauth/authorize?client_id=115985&response_type=code&redirect_uri=https://fitsynchubfunctions.azurewebsites.net/strava/exchange_token&approval_prompt=force&scope=read,read_all,profile:read_all,profile:write,activity:read,activity:read_all,activity:write