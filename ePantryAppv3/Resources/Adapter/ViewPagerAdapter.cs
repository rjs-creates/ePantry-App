//using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Support.V4.App;

namespace ePantryAppv3.Resources.Adapter
{
    public class ViewPagerAdapter : FragmentPagerAdapter
    {

        Fragment[] _fragments;

        public ViewPagerAdapter(FragmentManager fm, Fragment[] fragments):base(fm)
        {
            _fragments = fragments;
        }

        public override int Count => _fragments.Length;

        public override Fragment GetItem(int position) 
        {
            return _fragments[position];
        }

    }
}