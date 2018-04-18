using CoreTweet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using Weather;
using System.IO;

namespace Weather
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            var param = new Parameter();
            var pro = new Program();

            try
            {
                var twitterTokens = CoreTweet.Tokens.Create("Consumer Key", "Consumer Secret",
                    "Access Token", "Access Token Secret");
                var profile = twitterTokens.Account.VerifyCredentials();
                param.Bio = profile.Description;
                param.Location = profile.Location;
                param.Url = profile.Url;
                param.Name = "name";

                var weatherApiKey = "API_KEY";            
                Uri uri = new Uri(string.Format("http://api.openweathermap.org/data/2.5/forecast?q=Morioka,jp&units=metric&APPID={0}", weatherApiKey));
                Uri now = new Uri(string.Format("http://api.openweathermap.org/data/2.5/weather?q=Morioka,jp&units=metric&APPID={0}", weatherApiKey));

                while (true)
                {
                    Task<string> httpUri = GetApiAsync(uri);
                    Task<string> httpNow = GetApiAsync(now);
                    string json = httpUri.Result;
                    string jsonNow = httpNow.Result;
                    JObject root = JObject.Parse(json);
                    JObject rootNow = JObject.Parse(jsonNow);
                    string id = root["list"].ElementAt(3)["weather"].First()["id"].ToString();
                    string idNow = rootNow["weather"].First()["id"].ToString();
                    var tweMoji = pro.WeatherSelect(id.ToString());
                    var tweMojiNow = pro.WeatherSelect(idNow.ToString());
                    Console.WriteLine(DateTime.Now + " | " + id + ":" + tweMoji + " | " + idNow + ":" + tweMojiNow);

                    twitterTokens.Account.UpdateProfile(tweMojiNow + param.Name + tweMoji, param.Url, param.Location, param.Bio);

                    Thread.Sleep(1000 * 60 * 10);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error : " + ex.Message);
            }
        }

        public static async Task<string> GetApiAsync(Uri uri)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string result = await client.GetStringAsync(uri);
                    return result;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                return null;
            }
        }


        public string WeatherSelect(string id)
        {
            string res = "";
            if (id.StartsWith("2"))
            {
                res = "⛈";
            }
            else if (id.StartsWith("3"))
            {
                res = "🌧";
            }
            else if (id.IsAny("500", "501", "502", "503", "504"))
            {
                res = "🌦";
            }
            else if (id.IsAny("511", "600", "601", "602", "611", "612", "615", "616", "620", "621", "622"))
            {
                res = "🌨";
            }
            else if (id.IsAny("520", "521", "522", "531"))
            {
                res = "☔";
            }
            else if (id.StartsWith("7"))
            {
                res = "☁";
            }
            else if (id == "800")
            {
                res = "☀";
            }
            else if (id == "801")
            {
                res = "🌤";
            }
            else if (id == "802")
            {
                res = "🌥";
            }
            else if (id.IsAny("803", "804"))
            {
                res = "☁";
            }
            else if (id.StartsWith("9"))
            {
                res = "🌪";
            }
            return res;
        }
    }

    public static partial class StringExtensions
    {
        /// <summary>
        /// 文字列が指定されたいずれかの文字列と等しいかどうかを返します
        /// </summary>
        public static bool IsAny(this string self, params string[] values)
        {
            return values.Any(c => c == self);
        }
    }
}
