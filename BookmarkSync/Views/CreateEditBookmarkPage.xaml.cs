using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Nullfactory.PhoneClient.Helpers;
using Nullfactory.BookmarkSync.ViewModel;
using System.Windows.Data;

namespace Nullfactory.BookmarkSync.Views
{
    public partial class CreateEditBookmarkPage : PhoneApplicationPage
    {
        public CreateEditBookmarkPage()
        {
            InitializeComponent();

            DataContext = App.ViewModelLocator.CreateEditBookmarkViewModel;
        }
        
        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            // Update the binding source
            BindingExpression bindingExpr = textBox.GetBindingExpression(TextBox.TextProperty);
            bindingExpr.UpdateSource();
        }
    }
}