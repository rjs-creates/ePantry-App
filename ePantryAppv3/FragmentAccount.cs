using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Newtonsoft.Json;
//using Android.Support.V4.App;

namespace ePantryAppv3
{
    public class FragmentAccount : Fragment
    {
        bool linked = false;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            return inflater.Inflate(Resource.Layout.Account, container, false);  
        } 

        /// <summary>
        /// Retrieves user information to display on account page
        /// </summary>
        private void CallUser()
        {
            View.FindViewById<TextView>(Resource.Id.UserFullName).Text = User.userData.FirstName + " " + User.userData.LastName;
            View.FindViewById<TextView>(Resource.Id.UseruserID).Text = "User No: " + User.userData.UserID.ToString();
            View.FindViewById<TextView>(Resource.Id.UserEmail).Text = User.userData.Username;
            string address = User.userData.City + ", " + User.userData.Country + ", " + User.userData.PostalCode;
            if (address == ", , ")
                View.FindViewById<TextView>(Resource.Id.UserAddress).Text = "Address not entered";
            else
                View.FindViewById<TextView>(Resource.Id.UserAddress).Text = address;

            if (User.userData.PhoneNum == "")
                View.FindViewById<TextView>(Resource.Id.UserPhoneNo).Text = "Phone number not entered";
            else
                View.FindViewById<TextView>(Resource.Id.UserPhoneNo).Text = User.userData.PhoneNum;

            //if there is a pantry linked then change the button
            if (User.userData.PantryID != null)
            {
                View.FindViewById<TextView>(Resource.Id.UserLinkPantry).Text = "Unlink";
            }
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);

            CallUser();

            Button EditUser = View.FindViewById<Button>(Resource.Id.UserEdit);
            Button Delete = View.FindViewById<Button>(Resource.Id.UserDelete);
            Button ChangePass = View.FindViewById<Button>(Resource.Id.UserChangePass);
            Button logout = View.FindViewById<Button>(Resource.Id.UserLogOut);
            Button LinkPantry = View.FindViewById<Button>(Resource.Id.UserLinkPantry);

            EditUser.Click += EditUser_Click;
            Delete.Click += Delete_Click;
            ChangePass.Click += ChangePass_Click;
            logout.Click += Logout_Click;
            LinkPantry.Click += LinkPantry_Click;
        }

        /// <summary>
        /// Occurs when user taps link pantry button, either unlinks or links user's pantry
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void  LinkPantry_Click(object sender, EventArgs e)
        {
            await User.RetrieveUser(User.userData.Username);
            if (User.userData.PantryID == null)
            {
                LayoutInflater layoutInflater = LayoutInflater.From(Context);
                View view = layoutInflater.Inflate(Resource.Layout.LinkPantry, null);
                Android.Support.V7.App.AlertDialog.Builder alertbuilder = new Android.Support.V7.App.AlertDialog.Builder(Context);
                alertbuilder.SetView(view);
                alertbuilder.SetTitle("Link Pantry");
                alertbuilder.SetMessage("Please enter Pantry ID to link");
                alertbuilder.SetCancelable(false)
                .SetPositiveButton("Submit", async delegate
                {
                //data being sent to the url through POST
                var postData = new Dictionary<string, string>()
                        {
                            { "action", "LinkPantry" },
                            { "UserID", User.userData.UserID.ToString() },
                            { "PantryID", view.FindViewById<EditText>(Resource.Id.LinkPantryID).Text }
                        };

                //send post request
                await WebService.POSTRequest(postData);

                //get reply from post
                WebService.Reply reply = WebService.LastReply;

                //if password change worked continue, else return
                if ((bool)reply.Data)
                    {
                        Toast.MakeText(Context, reply.Status, ToastLength.Long).Show();

                    }
                    else
                    {
                        Toast.MakeText(Context, reply.Status, ToastLength.Long).Show();
                        return;
                    }
                    linked = true;
                    View.FindViewById<Button>(Resource.Id.UserLinkPantry).Text = "Unlink";
                })
                .SetNegativeButton("Cancel", delegate
                {
                    alertbuilder.Dispose();
                });
                Android.Support.V7.App.AlertDialog dialog = alertbuilder.Create();
                dialog.SetTitle("Link Pantry");
                dialog.Show();
            }
            else
            {

                //Do unlinking here
                //create alert
                AlertDialog alert = new AlertDialog.Builder(Context).Create();
                alert.SetCancelable(false);
                alert.SetTitle("Unlink Pantry");
                alert.SetMessage($"Do you want to Unlink Pantry?");
                alert.SetButton("OK", async (c, ev) =>
                {
                    var postData = new Dictionary<string, string>()
                    {
                            { "action", "UnlinkPantry" },
                            { "UserID", User.userData.UserID.ToString() },
                    };

                    //send post request
                    await WebService.POSTRequest(postData);

                    //get reply from post
                    WebService.Reply reply = WebService.LastReply;

                    //if password change worked continue, else return
                    if ((bool)reply.Data)
                    {
                        Toast.MakeText(Context, reply.Status, ToastLength.Long).Show();
                    }
                    else
                    {
                        Toast.MakeText(Context, reply.Status, ToastLength.Long).Show();
                        return;
                    }
                    linked = false;
                    View.FindViewById<Button>(Resource.Id.UserLinkPantry).Text = "link";
                });
                alert.SetButton2("Cancel", (c, ev) => { });
                alert.Show();
            }
        }

        //logs out the current user and returns to login page
        private void Logout_Click(object sender, EventArgs e)
        {
            StartActivity(new Intent(Context, typeof(LoginActivity)));
        }

        /// <summary>
        /// Occurs whjen user taps change pass button, allows user to change current password
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangePass_Click(object sender, EventArgs e)
        {
            //controls
            string suPass;
            string suRePass;

            LayoutInflater layoutInflater = LayoutInflater.From(Context);
            View view = layoutInflater.Inflate(Resource.Layout.ChangeUserPass, null);
            Android.Support.V7.App.AlertDialog.Builder alertbuilder = new Android.Support.V7.App.AlertDialog.Builder(Context);
            alertbuilder.SetView(view);
            alertbuilder.SetCancelable(false)
                //submits password change
            .SetPositiveButton("Submit", async delegate
            {
                suPass = view.FindViewById<EditText>(Resource.Id.ChangesuPass).Text;
                suRePass = view.FindViewById<EditText>(Resource.Id.ChangesuRePass).Text;

                //if passwords dont match, return
                if (suPass != suRePass)
                {
                    Toast.MakeText(Context, "Passwords do not match", ToastLength.Long).Show();
                    return;
                }

                //if password is empty, return
                if (suPass.Trim() == "")
                {
                    Toast.MakeText(Context, "Please provide a password", ToastLength.Long).Show();
                    return;
                }

                //data being sent to the url through POST
                var postData = new Dictionary<string, string>()
                    {
                        { "action", "UpdatePass" },
                        { "UserID", User.userData.UserID.ToString() },
                        { "Password", suPass }
                    };

                //send post request
                await WebService.POSTRequest(postData);

                //get reply from post
                WebService.Reply reply = WebService.LastReply;

                //if password change worked continue, else return
                if ((bool)reply.Data)
                {
                    Toast.MakeText(Context, reply.Status, ToastLength.Long).Show();
                }
                else
                {
                    Toast.MakeText(Context, reply.Status, ToastLength.Long).Show();
                    return;
                }

                Toast.MakeText(Context, $"Password Changed", ToastLength.Short).Show();
            })
            .SetNegativeButton("Cancel", delegate
            {
                alertbuilder.Dispose();
            });
            Android.Support.V7.App.AlertDialog dialog = alertbuilder.Create();
            dialog.SetTitle("Change Password");
            dialog.Show();
            
        }

        /// <summary>
        /// Occurs when user taps delete button, deletes currently logged in user
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Delete_Click(object sender, EventArgs e)
        {
            LayoutInflater layoutInflater = LayoutInflater.From(Context);
            View view = layoutInflater.Inflate(Resource.Layout.UserDelete, null);
            view.FindViewById<TextView>(Resource.Id.deleteText).Text = "Do you really want to delete " + User.userData.FirstName + "?";
            Android.Support.V7.App.AlertDialog.Builder alertbuilder = new Android.Support.V7.App.AlertDialog.Builder(Context);
            alertbuilder.SetView(view);
            
            alertbuilder.SetCancelable(false)
                //sumbits user deletion which deletes user from DB and returns them to the login page
            .SetPositiveButton("Submit", async delegate
            {
                //data being sent to the url through POST
                var postData = new Dictionary<string, string>()
                    {
                        { "action", "DeleteUser" },
                        { "UserID", User.userData.UserID.ToString() }
                    };

                //send post request
                await WebService.POSTRequest(postData);

                //get reply from post
                WebService.Reply reply = WebService.LastReply;

                //if deletion worked continue, else return
                if ((bool)reply.Data)
                {
                    Toast.MakeText(Context, reply.Status, ToastLength.Long).Show();
                }
                else
                {
                    Toast.MakeText(Context, reply.Status, ToastLength.Long).Show();
                    return;
                }

                Toast.MakeText(Context, "User Deleted", ToastLength.Short).Show();
                StartActivity(new Intent(Context, typeof(LoginActivity)));
            })
            .SetNegativeButton("Cancel", delegate
            {
                alertbuilder.Dispose();
            });
            Android.Support.V7.App.AlertDialog dialog = alertbuilder.Create();
            dialog.SetTitle("Delete Account");
            dialog.Show();
            
        }

        /// <summary>
        /// Occurs when user taps edit button, allows the user edit their current information
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditUser_Click(object sender, EventArgs e)
        {
            LayoutInflater layoutInflater = LayoutInflater.From(Context);
            View view = layoutInflater.Inflate(Resource.Layout.EditUser, null);

            //changes values of the textboxes to current user property values
            view.FindViewById<EditText>(Resource.Id.ChangesuEmail).Text = User.userData.Username;
            view.FindViewById<EditText>(Resource.Id.ChangesuFirst).Text = User.userData.FirstName;
            view.FindViewById<EditText>(Resource.Id.ChangesuLast).Text = User.userData.LastName;
            view.FindViewById<EditText>(Resource.Id.ChangesuCity).Text = User.userData.City;
            view.FindViewById<EditText>(Resource.Id.ChangesuCountry).Text = User.userData.Country;
            view.FindViewById<EditText>(Resource.Id.ChangesuPhone).Text = User.userData.PhoneNum;
            view.FindViewById<EditText>(Resource.Id.ChangesuPostal).Text = User.userData.PostalCode;

            Android.Support.V7.App.AlertDialog.Builder alertbuilder = new Android.Support.V7.App.AlertDialog.Builder(Context);
            alertbuilder.SetView(view);
            alertbuilder.SetCancelable(false)
            //submits user edit when pressing submit
            .SetPositiveButton("Submit", async delegate
            {
            ////data being sent to the url through POST
            //var postData = new Dictionary<string, string>()
            //    {
            //        { "action", "UpdateUser" },
            //        { "UserID", User.userData.UserID.ToString() },
            //        { "Username", view.FindViewById<EditText>(Resource.Id.ChangesuEmail).Text },
            //        { "FirstName", view.FindViewById<EditText>(Resource.Id.ChangesuFirst).Text },
            //        { "LastName", view.FindViewById<EditText>(Resource.Id.ChangesuLast).Text },
            //        { "City", view.FindViewById<EditText>(Resource.Id.ChangesuCity).Text },
            //        { "Country", view.FindViewById<EditText>(Resource.Id.ChangesuCountry).Text },
            //        { "PostalCode", view.FindViewById<EditText>(Resource.Id.ChangesuPostal).Text },
            //        { "PhoneNum", view.FindViewById<EditText>(Resource.Id.ChangesuPhone).Text },
            //        { "ManualItemsAdded", User.userData.ManualItemsAdded.ToString() }
            //    };

            //// send post request
            //await WebService.POSTRequest(postData);

            ////get reply from post
            //WebService.Reply reply = WebService.LastReply;

            //if edit worked continue, else return
            if (await User.UpdateUser(view.FindViewById<EditText>(Resource.Id.ChangesuEmail).Text, view.FindViewById<EditText>(Resource.Id.ChangesuFirst).Text, view.FindViewById<EditText>(Resource.Id.ChangesuLast).Text, view.FindViewById<EditText>(Resource.Id.ChangesuCity).Text, view.FindViewById<EditText>(Resource.Id.ChangesuCountry).Text, view.FindViewById<EditText>(Resource.Id.ChangesuPostal).Text, view.FindViewById<EditText>(Resource.Id.ChangesuPhone).Text, User.userData.ManualItemsAdded))
            {
                Toast.MakeText(Context, "Edit successful", ToastLength.Long).Show();
            }
            else
            {
                Toast.MakeText(Context, "Edit failed", ToastLength.Long).Show();
                return;
            }
            })
            .SetNegativeButton("Cancel", delegate
            {
                alertbuilder.Dispose();
            });
            Android.Support.V7.App.AlertDialog dialog = alertbuilder.Create();
            dialog.SetTitle("Edit Account");
            dialog.Show();
        }
    }
}