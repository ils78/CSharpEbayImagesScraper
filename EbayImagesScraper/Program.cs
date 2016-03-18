using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Security.Cryptography;

namespace EbayImagesScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            string startUrl = "http://www.ebay.com/sch/Accessory-Bundles-/176971/i.html";
            HtmlAgilityPack.HtmlWeb htmlWeb = new HtmlAgilityPack.HtmlWeb();
            var doc = htmlWeb.Load(startUrl);
            var urls = doc.DocumentNode.SelectNodes("//h3[@class=\"lvtitle\"]//a").Select((linkNode) => linkNode.GetAttributeValue("href", "/"));
            foreach (var url in urls)
            {
                var subDoc = htmlWeb.Load(url);
                Regex rgx = new Regex(@".+""imgArr"" : (.+\]), ""islarge"".+", RegexOptions.Multiline);
                var jsonTexts = subDoc.DocumentNode.SelectNodes("//script[contains(.,\"imgArr\")]").Select(scriptNode => scriptNode.InnerText).Select(text => rgx.Match(text).Groups[1].Value).SingleOrDefault();
                var images = JArray.Parse(jsonTexts).Children().Select(img => img.Value<string>("maxImageUrl"));
                using (WebClient webClient = new WebClient())
                {
                    HashAlgorithm algorithm = MD5.Create();
                    foreach (string image_url in images)
                    {
                        Uri uri = new Uri(image_url);
                        string extension = System.IO.Path.GetExtension(uri.LocalPath);
                        string hash = string.Join("", algorithm.ComputeHash(Encoding.UTF8.GetBytes(image_url)).Select(b => b.ToString("X2")));
                        Console.WriteLine("Downloading {0} file", image_url);
                        webClient.DownloadFile(image_url, string.Format("{0}{1}", hash, extension));
                    }
                }
            }
        }
    }
}
