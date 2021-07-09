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
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace ePantryAppv3
{
    public static class User
    {
        public static UserData userData;        //data on the currently logged in user
        public static PantryData userPantry;    //data on the current user's pantry

        static User()
        {
            userData = new UserData();
            userPantry = new PantryData();
        }

        /// <summary>
        /// Retrieves current information of the logged in user
        /// </summary>
        /// <param name="username">Username of current user</param>
        /// <returns></returns>
        async public static Task RetrieveUser(string username)
        {
            MultipartFormDataContent form = new MultipartFormDataContent();

            //data being sent to the url through POST
            var postData = new Dictionary<string, string>()
            {
                { "action", "GetUserData" },
                { "Username", username },
            };

            //adds parameters to form 
            foreach (var item in postData)
            {
                form.Add(new StringContent(item.Value), item.Key);
            }

            //sends the post data and awaits the response
            using (HttpClient httpClient = new HttpClient())
            {
                var response = await httpClient.PostAsync("https://thor.net.nait.ca/~estrickl/capstone/webservices.php", form);


                //if the response isn't null then decode the json string
                if (response.Content != null)
                {
                    var responseString = await response.Content.ReadAsStringAsync();

                    var responseTemplate = new { data = new[] { new { UserID = 0, Username = "", FirstName = "", LastName = "", City = "", Country = "", PostalCode = "", PhoneNum = "", PantryID = "", ManualItemsAdded = 0 } }, status = "" };

                    //deserializes the string into user
                    var responseData = JsonConvert.DeserializeAnonymousType(responseString, responseTemplate).data;

                    //gets user info
                    userData.UserID = responseData[0].UserID;
                    userData.Username = responseData[0].Username;
                    userData.FirstName = responseData[0].FirstName;
                    userData.LastName = responseData[0].LastName;
                    userData.City = responseData[0].City;
                    userData.PostalCode = responseData[0].PostalCode;
                    userData.PhoneNum = responseData[0].PhoneNum;
                    userData.Country = responseData[0].Country;
                    userData.PantryID = responseData[0].PantryID;
                    userData.ManualItemsAdded = responseData[0].ManualItemsAdded;
                }
            }
        }

        async public static Task UpdateManualItemCount(int num)
        {
            await UpdateUser(userData.Username, userData.FirstName, userData.LastName, userData.City, userData.Country, userData.PostalCode, userData.PhoneNum, num);
        }

        /// <summary>
        /// Updates currently logged in user
        /// </summary>
        /// <param name="Username"></param>
        /// <param name="FirstName"></param>
        /// <param name="LastName"></param>
        /// <param name="City"></param>
        /// <param name="Country"></param>
        /// <param name="PostalCode"></param>
        /// <param name="PhoneNum"></param>
        /// <param name="ManualItemsAdded"></param>
        /// <returns></returns>
        async public static Task<bool> UpdateUser(string Username, string FirstName, string LastName, string City, string Country, string PostalCode, string PhoneNum, int ManualItemsAdded)
        {
            //data being sent to the url through POST
            var postData = new Dictionary<string, string>()
                    {
                        { "action", "UpdateUser" },
                        { "UserID", userData.UserID.ToString() },
                        { "Username", Username },
                        { "FirstName", FirstName },
                        { "LastName", LastName },
                        { "City", City },
                        { "Country", Country },
                        { "PostalCode", PostalCode },
                        { "PhoneNum", PhoneNum },
                        { "ManualItemsAdded", ManualItemsAdded.ToString() }
                    };

            // send post request
            await WebService.POSTRequest(postData);

            //get reply from post
            WebService.Reply reply = WebService.LastReply;

            return (bool)reply.Data;
        }

        /// <summary>
        /// Retrieves current pantry information of logged in user
        /// </summary>
        /// <returns></returns>
        async public static Task RetrievePantry()
        {
            MultipartFormDataContent form = new MultipartFormDataContent();

            //data being sent to the url through POST
            var postData = new Dictionary<string, string>()
            {
                { "action", "GetPantryData" },
                { "PantryID", userData.PantryID },
            };

            //adds parameters to form 
            foreach (var item in postData)
            {
                form.Add(new StringContent(item.Value), item.Key);
            }

            //sends the post data and awaits the response
            using (HttpClient httpClient = new HttpClient())
            {
                var response = await httpClient.PostAsync("https://thor.net.nait.ca/~estrickl/capstone/webservices.php", form);

                //if the response isn't null then decode the json string
                if (response.Content != null)
                {
                    var responseString = await response.Content.ReadAsStringAsync();

                    var responseTemplate = new { data = new[] { new { PantryID = "", Weight = 0.0, Temperature = 0.0, StatusMessage = "", Timestamp = default(DateTime) } }, status = "" };

                    //deserializes the string into user
                    var responseData = JsonConvert.DeserializeAnonymousType(responseString, responseTemplate).data;

                    //gets pantry info
                    userPantry.PantryID = responseData[0].PantryID;
                    userPantry.Weight = responseData[0].Weight;
                    userPantry.Temperature = responseData[0].Temperature;
                    userPantry.StatusMessage = responseData[0].StatusMessage;
                    userPantry.Timestamp = responseData[0].Timestamp;
                }
            }
        }
    }

    public class UserData
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string PhoneNum { get; set; }
        public string PantryID { get; set; }
        public int ManualItemsAdded { get; set; }
    }

    public class PantryData
    {
        public string PantryID { get; set; }
        public double Weight { get; set; }
        public double Temperature { get; set; }
        public string StatusMessage { get; set; }
        public DateTime Timestamp { get; set; }
    }
}