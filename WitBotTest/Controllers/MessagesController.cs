﻿using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;
using System;
using System.Text;
using WitBotTest.WitBotAPI;
using WitBotTest.Weather;
using WitBotTest.Distance;

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
                var WitBotMessage = WitBot.MakeRequest(activity.Text);

                string messageIntent;
                WitBotMessage.TryGetValue("intent_value", out messageIntent);
                switch (messageIntent)
                {
                    case "weather":

                        reply_string.AppendLine(WitBotWeather.GetWeather(WitBotMessage));
                        break;

                    case "distance":
                        reply_string.AppendLine(WitBotDistance.GetDistance(WitBotMessage));
                        break;

                    case "some pics":
                        reply_string.AppendLine($"![image]({GetSomethingFunny()})");
                        break;

                    case "greetings":
                        reply_string.AppendLine("Hello");
                        break;

                    default:
                        reply_string.AppendLine("There is no simmilar command.");
                        break;

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

        private static string GetSomethingFunny()
        {
            var url = "https://9gag.com/funny/fresh";
            WebClient client = new WebClient {};
            string reply = client.DownloadString(url);
            var blocks = reply.Split(new string[] { "<img class=\"badge-item-img\" src=\"" }, StringSplitOptions.None);
            var i = new Random().Next(1, 10);
            var element = blocks[i].Split(new string[] { "\" alt" }, StringSplitOptions.None);
            return element[0];
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