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
using ZXing.Mobile;
using ZXing.Net.Mobile;
using ZXing;
using System.IO;
using Newtonsoft.Json;
using System.Net.Http;
using static Android.App.DatePickerDialog;


namespace ePantryAppv3
{
    public class FragmentScan : Fragment
    {
        TextView _upcText;
        private TextView _statusText; //status label
        DateTime date = default(DateTime) ;
        private Button datePick = null;
        private string _foodAPIUrl = "https://api.edamam.com/api/food-database/v2/parser";      //url for food database
        private int _currentUser = 5;   //hard coded user id for testing purposes
        public List<Item> _inventory = new List<Item>();    //current user's inventory
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            //Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            ZXing.Net.Mobile.Forms.Android.Platform.Init();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);
            View view = inflater.Inflate(Resource.Layout.scan, container, false);
            return view;
            //AlertDialog ad = new AlertDialog.Builder(LocalActivityManager.Get)
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);
            _upcText = View.FindViewById<TextView>(Resource.Id.BarcodeOutput);
            Button barcodeButton = View.FindViewById<Button>(Resource.Id.barcode);
            barcodeButton.Click += BarcodeButton_Click;
            _statusText = View.FindViewById<TextView>(Resource.Id.StatusLabel);

            Button AddManual = View.FindViewById<Button>(Resource.Id.buttonManualAdd);
            AddManual.Click += AddManual_Click;
        }

        /// <summary>
        /// Occurs when user clicks the add manually button, allows user to create a new item to be added to pantry
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddManual_Click(object sender, EventArgs e)
        {
            Item newItem;
            LayoutInflater layoutInflater = LayoutInflater.From(Context);
            View view = layoutInflater.Inflate(Resource.Layout.ManualAdd, null);
            datePick = view.FindViewById<Button>(Resource.Id.buttonPickDate);
            datePick.Click += (c, ev) =>
            {
                 DateTime currently = DateTime.Now;
                 DatePickerDialog dp = new DatePickerDialog(Context);
                 dp.UpdateDate(currently);
                 dp.Show();
                 dp.DateSet += Dp_DateSet;
             };

            //create alert
            Android.Support.V7.App.AlertDialog.Builder alertbuilder = new Android.Support.V7.App.AlertDialog.Builder(Context);
            alertbuilder.SetView(view);
            //create buttons for the alert
            alertbuilder.SetCancelable(false)
                //submit button will add the item to pantry
            .SetPositiveButton("Submit", async delegate
            {
                double weight;  //stock weight of current item

                //name of current item
                string name = view.FindViewById<EditText>(Resource.Id.EnterItemName).Text;

                //if there is no weight described then set to 0
                if (!double.TryParse(view.FindViewById<EditText>(Resource.Id.EnterItemWeight).Text, out weight))
                    weight = 0;

                //set picture of item
                string pic = view.FindViewById<EditText>(Resource.Id.EnterPicture).Text;

                //if the user has a linked pantry, recieve the quantity of the item
                if (User.userData.PantryID != null)
                {
                    await User.RetrievePantry();

                    //take weight of pantry before item usage
                    double pantryWeightBefore = User.userPantry.Weight;

                    //create dialog to use item
                    AlertDialog alert2 = new AlertDialog.Builder(Context).Create();
                    alert2.SetCancelable(false);
                    alert2.SetTitle("Use Item");
                    alert2.SetMessage($"Tap OK when you have placed the item in the pantry");
                    alert2.SetButton("OK", async delegate
                    {
                        //wait for pantry data to be recieved
                        await User.RetrievePantry();

                        //calculate current item weight by taking the item weight and subtracting the weight difference from before and after the item has been used
                        double currentItemWeight = pantryWeightBefore - User.userPantry.Weight;

                        //adds new item to inventory
                        newItem = new Item((User.userData.ManualItemsAdded += 1).ToString(), name, weight, currentItemWeight, User.userData.UserID, pic, default(DateTime), null);
                        AddToInventory(newItem);
                    });
                    alert2.Show();
                }
                //if the user has no linked pantry then set quantity to 0
                else
                {                 
                    newItem = new Item((User.userData.ManualItemsAdded += 1).ToString(), name, weight, 0, User.userData.UserID, pic, date, null);
                    AddToInventory(newItem);
                }

                await User.UpdateManualItemCount(User.userData.ManualItemsAdded);
            })
            //closes the alert
            .SetNegativeButton("Cancel", delegate
            {
                alertbuilder.Dispose();
            });
            Android.Support.V7.App.AlertDialog dialog = alertbuilder.Create();
            dialog.SetTitle("Add Item Manually");
            dialog.Show();
        }

        private void DatePick_Click(object sender, EventArgs e)
        {
            DateTime currently = DateTime.Now;
            DatePickerDialog dp = new DatePickerDialog(Context);
            dp.UpdateDate(currently);
            dp.Show();
            dp.DateSet += Dp_DateSet;
            
        }

        private void Dp_DateSet(object sender, DateSetEventArgs e)
        {
            date = new DateTime(e.Year, e.Month, e.DayOfMonth);
            datePick.Text = date.ToString("yyyy-MM-dd");
            Toast.MakeText(Context, "You have selected :" + date.Date.ToShortDateString(), ToastLength.Long);
        }

        /// <summary>
        /// Occurs when the user opens the barcode scanner, awaits a barcode to be scanned and retrieves the food item if it exists
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async private void BarcodeButton_Click(object sender, EventArgs e)
        {
            try
            {
                //opens scanner and waits for barcode
                var scanner = new MobileBarcodeScanner();

                var result = await scanner.Scan();

                if (result != null)
                {
                    //retrieves the upc code and searches for the food using the API
                    _upcText.Text = result.Text;
                    Console.WriteLine("Scanned Barcode: " + result.Text);

                    RetrieveItemFoodAPI(result.Text);
                }
            }
            catch (Exception err)
            {
                _statusText.Text = err.Message;
            }
            
        }

        /// <summary>
        /// Retrieves food item from edamam API
        /// </summary>
        /// <param name="upcCode">UPC code of item scanned by user</param>
        private async void RetrieveItemFoodAPI(string upcCode)
        {
            {
                //sends the post data and awaits the response
                using (HttpClient httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(_foodAPIUrl + $"?upc={upcCode}&app_id=a683a155&app_key=0c5c56fcfc75a00569411d9e07dc6253");

                    //if the response isn't null then decode the json string
                    if (response.Content != null)
                    {
                        var responseString = await response.Content.ReadAsStringAsync();

                        //deserializes the string into an item
                        var item = JsonConvert.DeserializeObject<FoodDBItem.Root>(responseString);

                        if(item.hints == null)
                        {
                            Toast.MakeText(Context, "Item not in database, ask user for manual insertion",ToastLength.Long).Show();
                            return;
                        }

                        //DO SOMETHING WITH ITEM

                        Item newItem = new Item(upcCode, item.hints[0].food.label, Math.Round(item.hints[0].measures[1].weight, 2), Math.Round(item.hints[0].measures[1].weight, 2), User.userData.UserID, item.hints[0].food.image, default(DateTime), null);

                        //create alert
                        AlertDialog alert = new AlertDialog.Builder(Context).Create();
                        alert.SetCancelable(false);
                        alert.SetTitle("Add Item");
                        alert.SetMessage($"Do you want to Add {newItem.ItemName} of weight {newItem.ItemWeight}g?");
                        alert.SetButton("OK", (c, ev) =>
                        {
                            //ADDING TO INVENTORY
                            AddToInventory(newItem);
                        });
                        alert.SetButton2("Cancel", (c, ev) => { });
                        alert.Show();

                        //testing
                        Console.WriteLine($"Label: {item.hints[0].food.label}\nWeight (Grams): {Math.Round(item.hints[0].measures[1].weight, 2)}");

                        //RetrievePantry(" ");
                    }
                }
            }
        }

        /// <summary>
        /// Retireves filtered/unfiltered inventory of the current user
        /// </summary>
        /// <param name="filter">Search filter for the inventory search provided by user.</param>
        async private void RetrievePantry(string filter)
        {
            //sends the post data and awaits the response
            using (HttpClient httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(WebService.DBUrl + $"?action=QueryItems&UserID={_currentUser}&filter={filter.Replace(" ", "%20")}");

                //if the response isn't null then decode the json string
                if (response.Content != null)
                {
                    var responseString = await response.Content.ReadAsStringAsync();

                    //var responseTemplate = new {​​​​ data = new List<Item>()​​​​, status = "" }​​​​;

                    var responseTemplate = new { data = new List<Item>(), status = "" };

                    //deserializes the string into inventory
                    _inventory = JsonConvert.DeserializeAnonymousType(responseString, responseTemplate).data;

                    Console.Write(_inventory.ToString());
                }
            }
        }

        /// <summary>
        /// Adds an item to user's inventory
        /// </summary>
        /// <param name="newItem">Item to be added to inventory.</param>
        async private void AddToInventory(Item newItem)
        {
            //data being sent to the url through POST
            var postData = new Dictionary<string, string>()
            {
                { "action", "AddItem" },
                { "UserID", User.userData.UserID.ToString() },
                { "ItemID", newItem.ItemID.ToString() },
                { "ItemName", newItem.ItemName },
                { "ItemWeight", newItem.ItemWeight.ToString() },
                { "Picture", newItem.Picture },
                { "Quantity", newItem.Quantity.ToString() },
                {"ExpirationDate", newItem.ExpirationDate.ToString("yyyy-MM-dd")},
                { "Info", "" }

            };

            //send post request
            await WebService.POSTRequest(postData);

            //get reply from post
            WebService.Reply reply = WebService.LastReply;

            //set status text to the reply status
            _statusText.Text = reply.Status;
        }
    }
}