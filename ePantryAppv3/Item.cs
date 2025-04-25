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
using System.Drawing;
using System.Net;
using Android.Graphics;

namespace ePantryAppv3
{
    public class Item
    {
        public string ItemID;
        public string ItemName;
        public double ItemWeight;
        public double Quantity;
        public int UserID;
        public string Picture;
        public DateTime ExpirationDate;
        public string Info;

        public Item(string id, string label, double defaultWeight, double userQuantity, int uID, string picString, DateTime expDate, string info)
        {
            ItemID = id;
            ItemName = label;
            ItemWeight = defaultWeight;
            Quantity = userQuantity;
            UserID = uID;
            Picture = picString;
            ExpirationDate = expDate;
            Info = info;
        }

        public Android.Graphics.Bitmap PicBitmap
        {
            get
            {
                var imageBitmap = GetImageBitmapFromUrl(Picture);
                return imageBitmap;
            }
        }


        public string Name { get { return ItemName; } set { this.ItemName = value; } }
        public string QuantPercentage { get {

                //return blank if quantity is 0
                if (Quantity == 0)
                    return "-";
                return $"{Quantity / ItemWeight:P}"; 
        } }        

        private Android.Graphics.Bitmap GetImageBitmapFromUrl(string url)
        {
            Android.Graphics.Bitmap imageBitmap = null;

            if (url == "")
                return imageBitmap;

            using (var webClient = new WebClient())
            {
                var imageBytes = webClient.DownloadData(url);
                if (imageBytes != null && imageBytes.Length > 0)
                {
                    imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                }
            }

            return imageBitmap;
        }
    }

    public class FoodDBItem
    {
        public class Nutrients
        {
            public double ENERC_KCAL { get; set; }
            public double FAT { get; set; }
            public double FASAT { get; set; }
            public double FATRN { get; set; }
            public double FAMS { get; set; }
            public double FAPU { get; set; }
            public double CHOCDF { get; set; }
            public double FIBTG { get; set; }
            public double SUGAR { get; set; }
            public double PROCNT { get; set; }
            public double CHOLE { get; set; }
            public double NA { get; set; }
            public double CA { get; set; }
            public double MG { get; set; }
            public double K { get; set; }
            public double FE { get; set; }
            public double ZN { get; set; }
            public double P { get; set; }
            public double VITC { get; set; }
            public double THIA { get; set; }
            public double RIBF { get; set; }
            public double VITB6A { get; set; }
            public double VITB12 { get; set; }
        }

        public class ServingSize
        {
            public string uri { get; set; }
            public string label { get; set; }
            public double quantity { get; set; }
        }

        public class Food
        {
            public string foodId { get; set; }
            public string label { get; set; }
            public Nutrients nutrients { get; set; }
            public string brand { get; set; }
            public string category { get; set; }
            public string categoryLabel { get; set; }
            public string foodContentsLabel { get; set; }
            public string image { get; set; }
            public List<ServingSize> servingSizes { get; set; }
            public double servingsPerContainer { get; set; }
        }

        public class Measure
        {
            public string uri { get; set; }
            public string label { get; set; }
            public double weight { get; set; }
        }

        public class Hint
        {
            public Food food { get; set; }
            public List<Measure> measures { get; set; }
        }

        public class Root
        {
            public string text { get; set; }
            public List<object> parsed { get; set; }
            public List<Hint> hints { get; set; }
        }
    }
}