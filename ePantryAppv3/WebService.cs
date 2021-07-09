using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ePantryAppv3
{
    static class WebService
    {
        public static string DBUrl = "https://thor.net.nait.ca/~estrickl/capstone/webservices.php";  //url for user/inventory database

        public static Reply LastReply = new Reply();

        public async static Task POSTRequest(Dictionary<string, string> parms)
        {
            MultipartFormDataContent form = new MultipartFormDataContent();

            //adds parameters to form 
            foreach (var item in parms)
            {
                form.Add(new StringContent(item.Value), item.Key);
            }

            //sends the post data and awaits the response
            using (HttpClient httpClient = new HttpClient())
            {
                var response = await httpClient.PostAsync(DBUrl, form);

                //if the response isn't null then decode the json string
                if (response.Content != null)
                {
                    var responseString = await response.Content.ReadAsStringAsync();

                    //deserializes the string into an array of objects
                    LastReply = JsonConvert.DeserializeAnonymousType(responseString, new Reply());
                }
            }
        }

        public class Reply
        {
            private object _data;
            private string _status;

            public object Data
            {
                get { return _data; }
                set { _data = value; }
            }

            public string Status
            {
                get { return _status; }
                set { _status = value; }
            }

            public Reply()
            {
                Data = new object();
                _status = "";
            }
        }
    }
}