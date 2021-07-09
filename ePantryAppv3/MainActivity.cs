using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
//using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;
using Android.Bluetooth;
using Android.Graphics;
using ZXing.Net.Mobile;
using ZXing;
using ZXing.Mobile;
using System.Collections.Generic;
using System.IO;
using Android.Support.Design.Internal;
using ePantryAppv3.Resources.Adapter;

namespace ePantryAppv3
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar")]
    public class MainActivity : AppCompatActivity, BottomNavigationView.IOnNavigationItemSelectedListener
    {
        //ViewPager viewPager;
        //Fragment[] fragments;
        TextView _upcText;
        //BottomNavigationView Navigation;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Window.SetStatusBarColor(Color.ParseColor("#447604"));
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            ZXing.Net.Mobile.Forms.Android.Platform.Init();

            var Navigation = FindViewById<BottomNavigationView>(Resource.Id.bottomNavigationView1);
            Navigation.SetOnNavigationItemSelectedListener(this);         
            //Navigation.NavigationItemSelected += Navigation_NavigationItemSelected;

            MobileBarcodeScanner.Initialize(Application);
            Button barcodeButton = FindViewById<Button>(Resource.Id.barcode);
            //barcodeButton.Click += BarcodeButton_Click;

            _upcText = FindViewById<TextView>(Resource.Id.BarcodeOutput);

            Fragment myfrag = new FragmentScan();
            FragmentTransaction Ft = this.FragmentManager.BeginTransaction();
            Ft.Replace(Resource.Id.fragment_content, myfrag).Commit();


        }

        async private void BarcodeButton_Click(object sender, EventArgs e)
        {
            //Console.WriteLine(barcodeReader.Decode(bitmapData).Text);
            #if __ANDROID__
                MobileBarcodeScanner.Initialize(Application);
            #endif

            var scanner = new MobileBarcodeScanner();

            var result = await scanner.Scan();

            if (result != null)
            {
                _upcText.Text = result.Text;
                Console.WriteLine("Scanned Barcode: " + result.Text);
            }        
        }

        public bool OnNavigationItemSelected(IMenuItem item)
        {
            Fragment selectedFragment = null;
            switch (item.ItemId)
            {
                case Resource.Id.action_account:
                    Console.WriteLine("Account");
                    selectedFragment = new FragmentAccount();
                    break;
                case Resource.Id.action_scan:
                    Console.WriteLine("Scan");
                    selectedFragment = new FragmentScan();
                    break;
                case Resource.Id.action_pantry:
                    Console.WriteLine("Pantry");
                    selectedFragment = new FragmentPantry();
                    break;
            }
            FragmentTransaction FTX = this.FragmentManager.BeginTransaction();
            FTX.Replace(Resource.Id.fragment_content, selectedFragment).Commit();
            return true;

        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View) sender;
            Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
                .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            global::ZXing.Net.Mobile.Android.PermissionsHandler.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
	}
}
