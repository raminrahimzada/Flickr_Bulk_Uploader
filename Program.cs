using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FlickrNet;

namespace Flickr_Bulk_Uploader
{
    public class Program
    {
        private static bool TestFileDictionary()
        {
            var fd = new FileDictionary<string, string>("test.dbr");
            fd.Set("x1", "y1");
            fd.Set("x2", "y2");
            fd.Set("x3", "y3");
            fd.Close();
            var f = new FileDictionary<string, string>("test.dbr");
            var y1 = f.Get("x1");
            var y2 = f.Get("x2");
            var y3 = f.Get("x3");
            var ok = true;
            ok &= (y1 == "y1");
            ok &= (y2 == "y2");
            ok &= (y3 == "y3");
            f.Dispose();
            return ok;
        }
        private static List<string> _filter = new List<string>();
        public static void Main(string[] args)
        {
            Console.WriteLine("Loading Config...");
            Dictionary<string, string> config;
            try
            {
                config = File.ReadAllLines("config.txt")
                    .ToDictionary(line => line.Split('=')[0].Trim(), line => line.Split('=')[1].Trim());
            }
            catch (Exception e)
            {
                Console.WriteLine("config.txt file cannot be loaded:" + e.Message);
                return;
            }
            //getting config values
            string apiKey, apiSecret, callback, directory;
            config.TryGetValue("ApiKey", out apiKey);
            config.TryGetValue("ApiSecret", out apiSecret);
            config.TryGetValue("callback", out callback);
            config.TryGetValue("directory", out directory);
            
            //getting Extensions Filter List
            string filterString;
            config.TryGetValue("Filter", out filterString);
            if (filterString != null) _filter = filterString.Split('|').ToList();
            
            //validating config parameters
            if (_filter == null || _filter.Count == 0)
            {
                Console.WriteLine("filter must not be empty");
                return;
            }
            if (string.IsNullOrEmpty(directory))
            {
                Console.WriteLine("directory must not be empty");
                return;
            }
            if (!Directory.Exists(directory))
            {
                Console.WriteLine("directory must be existed");
                return;
            }
            if (!TestFileDictionary())
            {
                throw new Exception("File Dictionary Is Not Working Properly");
            }
            Console.WriteLine("Loading API...");
            //Loading Api
            var flickr = new Flickr
            {
                ApiKey = apiKey,
                ApiSecret = apiSecret
            };
            Console.WriteLine("Requesting O-Auth Request Token...");
            //get request token just like that
            var requestToken = flickr.OAuthGetRequestToken(callback);
            //get verifier just from here https://www.flickr.com/services/oauth/authorize?oauth_token=<REQUEST_TOKEN_HERE>&perms=write
            var verifierUrl = $"https://www.flickr.com/services/oauth/authorize?oauth_token={requestToken.Token}&perms=write";
            Process.Start(verifierUrl);
            Console.WriteLine("Authorize from just opened url and give me verifier code:");
            var verifier = Console.ReadLine();
            Console.WriteLine("Requesting O-Auth Access Token...");
            var accessToken = flickr.OAuthGetAccessToken(requestToken.Token, requestToken.TokenSecret, verifier);
            flickr.OAuthAccessToken = accessToken.Token;
            flickr.OAuthAccessTokenSecret = accessToken.TokenSecret;

            const string dbFile = "baza.dbr";//Do not change this during multiple sessions

            var db = new FileDictionary<string, string>(dbFile);
            Console.WriteLine("Enumerating Files...");
            var filenames = Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories);
            foreach (var filename in filenames)
            {
                if (db.Get(filename) == "ok") continue;
                var ext = Path.GetExtension(filename);
                if (!_filter.Contains(ext)) continue;
                Console.Write(Path.GetFileNameWithoutExtension(filename) + "\t--->");
                var url = flickr.UploadPicture(filename);
                Console.WriteLine(url + "\t");
                db.Set(filename, "ok");
            }
            Console.ReadLine();
        }
    }
}
