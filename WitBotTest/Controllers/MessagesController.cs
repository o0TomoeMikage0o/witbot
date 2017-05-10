using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using com.valgut.libs.bots.Wit;
using com.valgut.libs.bots.Wit.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace WitBotTest
{
    //[BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                StringBuilder reply_string = new StringBuilder();
                WitClient client = new WitClient("WLQNG6NO3EZGHEI7S7TCL5PJH535Z6NW");
                Message message = client.GetMessage(activity.Text);
                var messageType = ((JValue)message.entities["intent"][0].value).Value.ToString();
                if (messageType == "weather")
                {
                    var city = ((JValue)message.entities["location"][0].value).Value.ToString();
                    var cityLocation = GetCityLocation(city);
                    //var cityLocation = new Tuple<double, double>(49.2320162, 28.467975);
                    var duration = " ";
                    DateTime myDate = new DateTime();
                    try
                    {
                        duration = ((JValue)message.entities["datetime"][0].value).Value.ToString();
                        myDate = DateTime.Parse(duration);
                    }
                    catch
                    {}
                    
                    var weather = GetWeatherFromLocation(cityLocation);

                    var cityName = weather.Item1;
                    reply_string.AppendLine($" Weather in {weather.Item1} for {duration} days. \r\n");

                    var forecast = weather.Item2;
                    foreach (var dayilyForecast in forecast)
                    {
                        reply_string.AppendLine($"{dayilyForecast.Item1.ToShortDateString()} Daily temperature is {dayilyForecast.Item2} degrees. \r\n");
                        reply_string.AppendLine($"Minimum temperature is {dayilyForecast.Item3} and maximum is {dayilyForecast.Item4}. \r\n");
                    }
                    
                }
                else if (messageType == "greetings")
                {
                    reply_string.AppendLine("Hello!");
                }
                else
                {
                    reply_string.AppendLine("There is no simmilar command.");
                }
                Activity reply = activity.CreateReply($"{reply_string}");
                await connector.Conversations.ReplyToActivityAsync(reply);

            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private static Tuple<double, double> GetCityLocation(string cityName)
        {
            var url = string.Format("http://nominatim.openstreetmap.org/search.php?q={0}&format=json&limit=1", cityName);

            WebClient client = new WebClient { Encoding = Encoding.UTF8 };
            string reply = client.DownloadString(url);
            var json = (JArray)JsonConvert.DeserializeObject(reply);

            if (json.Count == 0)
            {
                throw new Exception("City not found.");
            }

            var location = (JObject)json[0];
            var lat = (double) location["lat"];
            var lon = (double) location["lon"];

            return new Tuple<double, double>(lat, lon);
        }

        private static Tuple<string, List<Tuple<DateTime, double, double, double>>> GetWeatherFromLocation(Tuple<double, double> location, int days = 3) {

            //var reqestUrl = string.Format("http://api.openweathermap.org/data/2.5/weather?lat={0}&lon={1}&units=metric&APPID=7eac9d42bc68621183847bb4846d3bb3", location.Item1, location.Item2);
            var reqestUrl = string.Format("http://api.openweathermap.org/data/2.5/forecast/daily?lat={0}&lon={1}&units=metric&cnt={2}&APPID=7eac9d42bc68621183847bb4846d3bb3", location.Item1, location.Item2, days);

            WebClient client = new WebClient { Encoding = Encoding.UTF8 };
            string reply = client.DownloadString(reqestUrl);
            var json = (JObject) JsonConvert.DeserializeObject(reply);
            var cityName = (string)json["city"]["name"];
            var forecastList = json["list"];
            var forecast = new List<Tuple<DateTime, double, double, double>>();
            foreach (var forecastListItem in forecastList)
            {
                DateTime date = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds((int)forecastListItem["dt"]);
                var currentTemp = Math.Round((double)forecastListItem["temp"]["day"]);
                var minTemp = Math.Round((double)forecastListItem["temp"]["min"]);
                var maxTemp = Math.Round((double)forecastListItem["temp"]["max"]);
                var dailyForecast = new Tuple<DateTime, double, double, double>(date, currentTemp, minTemp, maxTemp);
                forecast.Add(dailyForecast);
            }

            return new Tuple<string, List<Tuple<DateTime, double, double, double>>> (cityName, forecast);
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}