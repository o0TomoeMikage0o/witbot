using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Text;

namespace WitBotTest.Location
{
    public class WitBotLocation
    {
        public static Tuple<double, double> GetCityLocation(string cityName)
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
            var lat = (double)location["lat"];
            var lon = (double)location["lon"];

            return new Tuple<double, double>(lat, lon);
        }
    }
}