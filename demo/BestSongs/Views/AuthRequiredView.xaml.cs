﻿using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace BestSongs
{
    public partial class AuthRequiredView : ContentView
    {
        public AuthRequiredView(UserInfo info)
        {
            InitializeComponent();

            BindingContext = info;
        }
    }
}
