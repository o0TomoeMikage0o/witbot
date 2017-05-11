using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Collections.Generic;

namespace WitBotTest.WitBotAPI
{
    public class WitBot
    {
        const string DEFAULT_API_VERSION = "20170307";
        const string authValue = "Bearer WLQNG6NO3EZGHEI7S7TCL5PJH535Z6NW";

        public static Dictionary<string, string> MakeRequest(string q, string msg_id = null, string thread_id = null) {
            var client = new RestClient("https://api.wit.ai");

            var request = new RestRequest("message", Method.GET);
            request.AddQueryParameter("v", DEFAULT_API_VERSION);
            request.AddQueryParameter("q", q);
            if (msg_id != null)
                request.AddQueryParameter("msg_id", msg_id);
            if (thread_id != null)
                request.AddQueryParameter("thread_id", thread_id);

            request.AddHeader("Authorization", authValue);
            
            IRestResponse response = client.Execute(request);
            var json = (JObject)JsonConvert.DeserializeObject(response.Content);

            Dictionary<string, string> resultDict = new Dictionary<string, string>();
            resultDict.Add("msg_id", (string)json["msg_id"]);
            resultDict.Add("text", (string)json["_text"]);
            //resultDict.Add("entities", );
            var entities = json["entities"].Value<JObject>();
            foreach (var entity in entities)
            {
                var key = entity.Key;
                foreach (var item in (entity.Value)[0].Value<JObject>())
                {
                    var temp = item.Key;
                    resultDict.Add(entity.Key + "_" + item.Key, item.Value.ToString());
                }

            }

            return resultDict;        
        }
    }
}