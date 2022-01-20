using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImageOrganizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            /*
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(@"Resources\load.jpg", UriKind.Relative); 
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();

            imgMain.Stretch = Stretch.Uniform;
            
            imgMain.Source = bitmap;
            */
        }

        #region EVENTS
        private void ButtonScanFolder_Click(object sender, RoutedEventArgs e)
        {

           
        }

        private void ButtonAddFolder_Click(object sender, RoutedEventArgs e)
        {

        }

        #endregion
    }
}
