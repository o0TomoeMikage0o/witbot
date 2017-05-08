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

                var reply_string = " ";
                WitClient client = new WitClient("WLQNG6NO3EZGHEI7S7TCL5PJH535Z6NW");
                Message message = client.GetMessage(activity.Text);
                var messageType = ((JValue)message.entities["intent"][0].value).Value.ToString();
                if (messageType == "weather")
                {
                    var city = ((JValue)message.entities["location"][0].value).Value.ToString();
                    var cityLocation = GetCityLocation(city);
                    var temperature = GetWeatherFromLocation(cityLocation);
                    reply_string = string.Format($" Temperature in {city.ToString()} is {temperature.ToString()} degrees.");
                }
                else if (messageType == "greetings")
                {
                    reply_string = "Hello!";
                }
                else
                {
                    reply_string = "None.";
                }
                Activity reply = activity.CreateReply($"{reply_string}");
                await connector.Conversations.ReplyToActivityAsync(reply);
                //await Conversation.SendAsync(activity, () => new Dialogs.RootDialog());

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

            var lat = ((JObject)json[0])["lat"].Value<double>();
            var lon = ((JObject)json[0])["lon"].Value<double>();

            return new Tuple<double, double>(lat, lon);
        }

        private static Double GetWeatherFromLocation(Tuple<double, double> location) {

            var reqestUrl = string.Format("http://api.openweathermap.org/data/2.5/weather?lat={0}&lon={1}&units=metric&APPID=7eac9d42bc68621183847bb4846d3bb3", location.Item1, location.Item2);

            WebClient client = new WebClient { Encoding = Encoding.UTF8 };

            string reply = client.DownloadString(reqestUrl);

            var json = JsonConvert.DeserializeObject(reply);

            var temp = Math.Round(((JObject)json)["main"]["temp"].Value<double>());

            return temp;
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