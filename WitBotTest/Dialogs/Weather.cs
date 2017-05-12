using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using WitBotTest.Location;

namespace WitBotTest.Weather
{
    public class WitBotWeather
    {
        public static string GetWeather(Dictionary<string, string> message)
        {
            string city;
            message.TryGetValue("location_value", out city);
            var cityLocation = WitBotLocation.GetCityLocation(city);
            var forecastType = GetForecastType(message);
            var duration = SetForecastDuration(message, forecastType);
            var weather = WeatherFromLocation(cityLocation, forecastType, duration);

            StringBuilder reply_string = new StringBuilder();
            reply_string.AppendLine($" Forecast for {weather.Item1}. \r\n");

            var forecast = weather.Item2;
            foreach (var dayilyForecast in forecast)
            {
                reply_string.AppendLine($"{dayilyForecast.Item1.ToShortDateString()} Day temperature is {dayilyForecast.Item2} and night {dayilyForecast.Item3} degrees. \r\n");
                reply_string.AppendLine($"Minimum temperature is {dayilyForecast.Item4} and maximum is {dayilyForecast.Item5}. \r\n");
            }
            return reply_string.ToString();
        }
        private static Tuple<string, List<Tuple<DateTime, double, double, double, double>>> WeatherFromLocation(Tuple<double, double> location, bool forecastType, string days)
        {

            var reqestUrl = string.Format("http://api.openweathermap.org/data/2.5/forecast/daily?lat={0}&lon={1}&units=metric&cnt={2}&APPID=7eac9d42bc68621183847bb4846d3bb3", location.Item1, location.Item2, days);

            WebClient client = new WebClient { Encoding = Encoding.UTF8 };
            string reply = client.DownloadString(reqestUrl);
            var json = (JObject)JsonConvert.DeserializeObject(reply);
            var cityName = (string)json["city"]["name"];

            var forecastList = json["list"];
            var forecast = new List<Tuple<DateTime, double, double, double, double>>();

            if (forecastType)
            {
                foreach (var forecastListItem in forecastList)
                {
                    forecast.Add(ParseForecast(forecastListItem));
                }
            }
            else { forecast.Add(ParseForecast(forecastList.Last)); }

            return new Tuple<string, List<Tuple<DateTime, double, double, double, double>>>(cityName, forecast);
        }
        private static string SetForecastDuration(Dictionary<string, string> message, bool forecastType)
        {
            string duration = "1";
            int intDuration = 1;
            try
            {
                if (forecastType)
                {
                    message.TryGetValue("duration_value", out duration);
                    string durationUnit;
                    message.TryGetValue("duration_unit", out durationUnit);
                    if ("week" == durationUnit)
                    {
                        intDuration = int.Parse(duration);
                        intDuration *= 7;
                        duration = intDuration.ToString();
                    }
                    else if ("day" == durationUnit) intDuration = int.Parse(duration);
                    else duration = "1";
                }
                else
                {
                    string datetimeValue;
                    message.TryGetValue("datetime_value", out datetimeValue);
                    DateTime datetime = DateTime.Parse(datetimeValue);
                    DateTime currentDate = DateTime.Now;
                    duration = (datetime.DayOfYear - currentDate.DayOfYear + 1).ToString();

                }
            }
            catch
            { }
            return duration;
        }
        private static Tuple<DateTime, double, double, double, double> ParseForecast(JToken Item)
        {
            DateTime date = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds((int)Item["dt"]);
            var currentTemp = Math.Round((double)Item["temp"]["day"]);
            var minTemp = Math.Round((double)Item["temp"]["min"]);
            var maxTemp = Math.Round((double)Item["temp"]["max"]);
            var nightTemp = Math.Round((double)Item["temp"]["night"]);

            return new Tuple<DateTime, double, double, double, double>(date, currentTemp, nightTemp, minTemp, maxTemp);
        }
        private static bool GetForecastType(Dictionary<string, string> message)
        {
            try
            {
                if (message.ContainsKey("duration_value"))
                {
                    return true;
                }
                else return false;
            }
            catch
            {
                return false;
            }
        }

    }
   
}