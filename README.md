curl -X GET "https://www.strava.com/api/v3/push_subscriptions?client_id=115985&client_secret=63afb87bf950a3e6ed406a744be0c85b19c1c1d8"

curl -X DELETE "https://www.strava.com/api/v3/push_subscriptions/255408?client_id=115985&client_secret=63afb87bf950a3e6ed406a744be0c85b19c1c1d8"

curl -X POST https://www.strava.com/api/v3/push_subscriptions \
      -F client_id=115985 \
      -F client_secret=63afb87bf950a3e6ed406a744be0c85b19c1c1d8 \
      -F callback_url=https://stravawebhooksazurefunctions20240223145139.azurewebsites.net/webhook \
      -F verify_token=XFmoPfNQuwBhZByxquVsliUw2EDo0WAB7aWsHlnEShace0JoVhTrcsCgbUa26Yy4

http://www.strava.com/oauth/authorize?client_id=115985&response_type=code&redirect_uri=https://stravawebhooksazurefunctions20240223145139.azurewebsites.net/exchange_token&approval_prompt=force&scope=read,read_all,profile:read_all,profile:write,activity:read,activity:read_all,activity:write