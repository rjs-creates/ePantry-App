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
using System.Collections;
using Newtonsoft.Json;
using System.Net.Http;
using Syncfusion.SfDataGrid;

namespace ePantryAppv3
{
    public class FragmentPantry : Fragment
    {
        SfDataGrid sfGrid;
        DateTime date = default(DateTime);
        private Button datepick;
        public List<Item> _inventory = new List<Item>();    //current user's inventory
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
             return inflater.Inflate(Resource.Layout.Pantry, container, false);
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);
            sfGrid = View.FindViewById<SfDataGrid>(Resource.Id.sfDataGrid1);
            Button btnCalibrate = View.FindViewById<Button>(Resource.Id.PantryCalibrate);
            btnCalibrate.Click += BtnCalibrate_Click;
            RetrievePantry();
        }

        //Occurs when the user taps the calibrate button, updates the pantry with latest info
        private async void BtnCalibrate_Click(object sender, EventArgs e)
        {
            await User.RetrieveUser(User.userData.Username);
            if (User.userData.PantryID != null)
            {
                await User.RetrievePantry();
                View.FindViewById<TextView>(Resource.Id.PantryWeight).Text = User.userPantry.Weight.ToString();
                View.FindViewById<TextView>(Resource.Id.PantryTemperature).Text = User.userPantry.Temperature.ToString();
                View.FindViewById<TextView>(Resource.Id.PantryIdText).Text = User.userPantry.PantryID.ToString();
            }
            else
            {
                Toast.MakeText(Context, "Please Link Pantry", ToastLength.Short).Show();
            }
        }

        /// <summary>
        /// Retrieves the current pantry weight and temperature, as well as other status indicators
        /// </summary>
        /// <param name="filter"></param>
        async private void RetrievePantry(string filter = "")
        {
            //sends the post data and awaits the response
            using (HttpClient httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(WebService.DBUrl + $"?action=QueryItems&UserID={User.userData.UserID}&filter={filter.Replace(" ", "%20")}");

                //if the response isn't null then decode the json string
                if (response.Content != null)
                {
                    var responseString = await response.Content.ReadAsStringAsync();

                    var responseTemplate = new { data = new List<Item>(), status = "" };

                    //deserializes the string into inventory
                    _inventory = JsonConvert.DeserializeAnonymousType(responseString, responseTemplate).data;

                    //creates table for items
                    sfGrid.ItemsSource = _inventory;
                    sfGrid.GridTapped += SfGrid_GridTapped;
                    sfGrid.ColumnSizer = ColumnSizer.SizeToHeader;
                    sfGrid.Columns[0].Width = 100;
                    sfGrid.Columns[1].Width = 160;
                    sfGrid.Columns[2].Width = 100;
                    sfGrid.Columns[0].HeaderText = "";
                    sfGrid.Columns[2].HeaderText = "Quantity";
                    sfGrid.RowHeight = 70;
                }
            }
        }

        /// <summary>
        /// Occurs when the user taps an item row, allows them to delete, use, or edit an item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SfGrid_GridTapped(object sender, GridTappedEventArgs e)
        {
            Item rowData = e.RowData as Item;
            LayoutInflater layoutInflater = LayoutInflater.From(Context);
            View view = layoutInflater.Inflate(Resource.Layout.RowOption, null);
            Android.Support.V7.App.AlertDialog.Builder alertbuilder = new Android.Support.V7.App.AlertDialog.Builder(Context);

            view.FindViewById<EditText>(Resource.Id.EditInfo).Text = rowData.Info;
            view.FindViewById<EditText>(Resource.Id.EditQuantity).Text = rowData.Quantity.ToString();
            view.FindViewById<Button>(Resource.Id.buttonPickDatePantry).Text =  rowData.ExpirationDate.ToString("yyyy-MM-dd");

            datepick = view.FindViewById<Button>(Resource.Id.buttonPickDatePantry);
            datepick.Click += (c, ev) =>
            {
                DateTime currently = DateTime.Now;
                DatePickerDialog dp = new DatePickerDialog(Context);
                dp.UpdateDate(currently);
                dp.Show();
                dp.DateSet += this.Dp_DateSet1;
            };
            alertbuilder.SetView(view);
            
            //var userdata = view.FindViewById<EditText>(Resource.Id.editText);
            alertbuilder.SetCancelable(true)
                //deletes item from inventory
            .SetNeutralButton("Delete", delegate {

                DeleteFromInventory(rowData.ItemID);
                RetrievePantry();
                Toast.MakeText(Context, $"Deleted", ToastLength.Short).Show();
            })
                //closes alert
            .SetPositiveButton("Cancel", delegate
            {
                alertbuilder.Dispose();
            })
                //sends the current update item info to the database
            .SetNegativeButton("Edit", delegate
            {
                UpdateItem(rowData.ItemID, view.FindViewById<EditText>(Resource.Id.EditInfo).Text, view.FindViewById<EditText>(Resource.Id.EditQuantity).Text, date);
                
                //updates the pantry page
                
                Toast.MakeText(Context, "Item edited", ToastLength.Short).Show();
                alertbuilder.Dispose();
            });
            Android.Support.V7.App.AlertDialog dialog = alertbuilder.Create();
            dialog.SetTitle("Edit or Delete item");
            dialog.Show();

            Button useItem = view.FindViewById<Button>(Resource.Id.PantryUseItem);

            //Occurs when the user clicks the use button, initiates a prompt for the user to use the item and place it back in their pantry
            useItem.Click += (sender, e) =>
            {
                if (User.userData.PantryID != null)
                {
                    AlertDialog alert = new AlertDialog.Builder(Context).Create();
                    alert.SetCancelable(false);
                    alert.SetTitle("Use Item");
                    alert.SetMessage($"Do you really want to use" + rowData.ItemName + " ?");
                    alert.SetButton("OK", (c, ev) =>
                    {
                        //take weight of pantry before item usage
                        double pantryWeightBefore = User.userPantry.Weight;

                        //create dialog to use item
                        AlertDialog alert2 = new AlertDialog.Builder(Context).Create();
                        alert2.SetCancelable(false);
                        alert2.SetTitle("Use Item");
                        alert2.SetMessage($"Tap OK when you have finished using the item\nWARNING: Placing/Removing anything else from the pantry will require a full recalibration");
                        alert2.SetButton("OK", async delegate
                        {
                            //wait for pantry data to be recieved
                            await User.RetrievePantry();

                            //calculate current item weight by taking the item weight and subtracting the weight difference from before and after the item has been used
                            double currentItemWeight = rowData.Quantity - (pantryWeightBefore - User.userPantry.Weight);

                            //updates the item with current quantity and updates the pantry page
                            UpdateItem(rowData.ItemID, view.FindViewById<EditText>(Resource.Id.EditInfo).Text, currentItemWeight.ToString(), date);
                            RetrievePantry();
                        });
                        alert2.Show();
                    });
                    alert.SetButton2("Cancel", (c, ev) => { });
                    alert.Show();
                    Toast.MakeText(Context, "Item Being Used", ToastLength.Short).Show();
                    alertbuilder.Dispose();
                    dialog.Dismiss();
                }
                //if the user has no currently linked pantry, display an error message alert
                else
                {
                    AlertDialog alert = new AlertDialog.Builder(Context).Create();
                    alert.SetCancelable(false);
                    alert.SetTitle("No Pantry");
                    alert.SetMessage($"There is no pantry currently linked to your account");
                    alert.SetButton("OK", (c, ev) => { });
                    alert.Show();
                }
                
            };

        }

        private void Dp_DateSet1(object sender, DatePickerDialog.DateSetEventArgs e)
        {
            date = new DateTime(e.Year, e.Month, e.DayOfMonth);
            datepick.Text = date.ToString("yyyy-MM-dd");
            Toast.MakeText(Context, "You have selected :" + date.Date.ToShortDateString(), ToastLength.Long);
        }

        /// <summary>
        /// Deletes an item from user's inventory
        /// </summary>
        /// <param name="newItem">Item to be deleted from inventory.</param>
        async private void DeleteFromInventory(string itemID)
        {
            //data being sent to the url through POST
            var postData = new Dictionary<string, string>()
                {
                    { "action", "DeleteItem" },
                    { "ItemID", itemID },
                    { "UserID", User.userData.UserID.ToString() }
                };

            //send post request
            await WebService.POSTRequest(postData);

            //get reply from post
            WebService.Reply reply = WebService.LastReply;

            if ((bool)reply.Data)
            {
                Toast.MakeText(Context, reply.Status, ToastLength.Long).Show();
            }
            else
            {
                Toast.MakeText(Context, reply.Status, ToastLength.Long).Show();
                return;
            }
            
        }

        /// <summary>
        /// Updates an item from user's inventory with new Quantity
        /// </summary>
        /// <param name="newItem">Item to be updated from inventory.</param>
        async private void UpdateItem(string itemID, string info, string quantity, DateTime expDate)
        {

                //data being sent to the url through POST
                var postData = new Dictionary<string, string>()
                {
                    { "action", "UpdateItem" },
                    { "ItemID", itemID },
                    { "UserID", User.userData.UserID.ToString() },
                    { "Info", info },
                    { "ExpirationDate", expDate.ToString("yyyy-MM-dd")},
                    { "Quantity", quantity }
                };

            //send post request
            await WebService.POSTRequest(postData);

            //get reply from post
            WebService.Reply reply = WebService.LastReply;

            //TESTING
            Console.WriteLine(reply.Status);

            RetrievePantry();

        }
    }
}