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
using System.IO;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text.RegularExpressions;
using Android.Graphics;

namespace ePantryAppv3
{
    [Activity(Label = "ePantry",MainLauncher = true,Icon ="@drawable/ePantry")]
    public class LoginActivity : Activity
    {
        EditText email;
        EditText password;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Window.SetStatusBarColor(Color.ParseColor("#447604"));
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Login);
            email = FindViewById<EditText>(Resource.Id.username);
            password = FindViewById<EditText>(Resource.Id.password);

            var button = FindViewById<Button>(Resource.Id.btnLogin);
            button.Click += ButtonLogin_Click;

            var SignUpButton = FindViewById<Button>(Resource.Id.buttonSignUp);
            SignUpButton.Click += SignUpButton_Click;
        }

        /// <summary>
        /// Occurs when the user clicks the sign up button, allows them to create a new user using their credentials
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SignUpButton_Click(object sender, EventArgs e)
        {
            //sign up controls
            string suEmail;
            string suPass;
            string suRePass;
            string suFirst;
            string suLast;
            string suCity;
            string suCountry;
            string suPostal;
            string suPhone;

            LayoutInflater layoutInflater = LayoutInflater.From(this);
            View view = layoutInflater.Inflate(Resource.Layout.SignUp, null);
            Android.Support.V7.App.AlertDialog.Builder alertbuilder = new Android.Support.V7.App.AlertDialog.Builder(this);
            alertbuilder.SetView(view);
            alertbuilder.SetCancelable(false)
                //Occurs when the submit button is tapped, attempts to create user with given information
            .SetPositiveButton("Submit", async delegate
            {
                //sign up controls
                suEmail = view.FindViewById<EditText>(Resource.Id.suEmail).Text;
                suPass = view.FindViewById<EditText>(Resource.Id.suPass).Text;
                suRePass = view.FindViewById<EditText>(Resource.Id.suRePass).Text;
                suFirst = view.FindViewById<EditText>(Resource.Id.suFirst).Text;
                suLast = view.FindViewById<EditText>(Resource.Id.suLast).Text;
                suCity = view.FindViewById<EditText>(Resource.Id.suCity).Text;
                suCountry = view.FindViewById<EditText>(Resource.Id.suCountry).Text;
                suPostal = view.FindViewById<EditText>(Resource.Id.suPostal).Text;
                suPhone = view.FindViewById<EditText>(Resource.Id.suPhone).Text;

                //if passwords dont match, return
                if (suPass != suRePass)
                {
                    Toast.MakeText(this, "Passwords do not match", ToastLength.Long).Show();
                    return;
                }

                //if password is empty, return
                if (suPass.Trim() == "")
                {
                    Toast.MakeText(this, "Please provide a password", ToastLength.Long).Show();
                    return;
                }

                //if first name is empty, return
                if (suFirst.Trim() == "")
                {
                    Toast.MakeText(this, "Please provide a first name", ToastLength.Long).Show();
                    return;
                }

                //if last name is empty, return
                if (suLast.Trim() == "")
                {
                    Toast.MakeText(this, "Please provide a last name", ToastLength.Long).Show();
                    return;
                }

                //submission, if email address isnt in proper format then return
                if (Regex.IsMatch(suEmail, @"[[:alnum:]]*@[[:alnum:]]*.[[:alnum:]]*"))
                {
                    //data being sent to the url through POST
                    var postData = new Dictionary<string, string>()
                    {
                        { "action", "AddUser" },
                        { "Username", suEmail },
                        { "Password", suPass },
                        { "FirstName", suFirst },
                        { "LastName", suLast },
                        { "City", suCity },
                        { "Country", suCountry },
                        { "PostalCode", suPostal },
                        { "PhoneNum", suPhone }
                    };

                    //send post request
                    await WebService.POSTRequest(postData);

                    //get reply from post
                    WebService.Reply reply = WebService.LastReply;

                    if ((bool)reply.Data)
                    {
                        Toast.MakeText(this, reply.Status, ToastLength.Long).Show();
                    }
                    else
                    {
                        Toast.MakeText(this, reply.Status, ToastLength.Long).Show();
                    }
                }
                else
                {
                    Toast.MakeText(this, "Please provide valid email address", ToastLength.Long).Show();
                    return;
                }
            })
            .SetNegativeButton("Cancel", delegate
            {
                
            });
            Android.Support.V7.App.AlertDialog dialog = alertbuilder.Create();
            dialog.SetTitle("Create a new User");
            dialog.Show();
        }

        /// <summary>
        /// Occurs when user taps the login button, attempts to validate credentials and successfully logs in if they are validated
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ButtonLogin_Click(object sender, EventArgs e)
        {
            if (Regex.IsMatch(email.Text, @"[[:alnum:]]*@[[:alnum:]]*.[[:alnum:]]*"))
            {
                //data being sent to the url through POST
                var postData = new Dictionary<string, string>()
                {
                    { "action", "ValidateUser" },
                    { "Username", email.Text },
                    { "Password", password.Text }
                };

                //send post request
                await WebService.POSTRequest(postData);

                //get reply from post
                WebService.Reply reply = WebService.LastReply;

                //if user successfully logs in, move to main page and retrieve all user information
                if ((bool)reply.Data)
                {
                    Toast.MakeText(this, "Login successfully done!", ToastLength.Long).Show();
                    await User.RetrieveUser(email.Text);
                    StartActivity(typeof(MainActivity));
                }
                else
                {
                    Toast.MakeText(this, reply.Status, ToastLength.Long).Show();
                }
            }

            else
            {
                Toast.MakeText(this, "Please provide valid email address", ToastLength.Long).Show();
            }
        }



        
    }
}