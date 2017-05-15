using System.Collections.Generic;
using System.Text;
using System.Device.Location;
using WitBotTest.Location;
using System;

namespace WitBotTest.Distance
{
    public class WitBotDistance
    {
        public static string GetDistance(Dictionary<string, string> message)
        {
            StringBuilder replyString = new StringBuilder();
            string firstCity;
            string secondCity;
            message.TryGetValue("location_value", out firstCity);
            message.TryGetValue("location_value2", out secondCity);
            var firstLocation = WitBotLocation.GetCityLocation(firstCity);
            var secondLocation = WitBotLocation.GetCityLocation(secondCity);

            GeoCoordinate first = new GeoCoordinate(firstLocation.Item1, firstLocation.Item2);
            GeoCoordinate second = new GeoCoordinate(secondLocation.Item1, secondLocation.Item2);

            var distance = Math.Round(first.GetDistanceTo(second) / 1000);

            replyString.AppendLine($"{distance} km.");

            return replyString.ToString();
        }
    }
}