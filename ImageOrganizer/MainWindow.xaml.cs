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
using System.Windows.Forms;
using System.IO;
using ImageOrganizer.Classes;

namespace ImageOrganizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string CurrentFolder = "";
        private List<FolderItem> folderItems = new List<FolderItem>();
        private int imgCount = 0;

        public MainWindow()
        {
            InitializeComponent();

            Initialize();
        }

        public void Initialize()
        {
           
        }

        #region EVENTS
        private void ButtonScanFolder_Click(object sender, RoutedEventArgs e)
        {


           ScanFolder(CurrentFolder);
        }

        private void ButtonAddFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();            
            DialogResult result = fbd.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                if(folderItems.Where(i => i.Path == fbd.SelectedPath).Count() == 0)
                {
                    CurrentFolder = fbd.SelectedPath;

                    if (folderItems.Count == 0)
                    {
                        CbdFolder.Items.Clear();
                    }

                    FolderItem item = new FolderItem();
                    item.TimestampAdded = DateTime.Now;
                    item.Path = CurrentFolder;
                    item.Name = System.IO.Path.GetFileName(CurrentFolder);
                    folderItems.Insert(0, item);

                    CbdFolder.Items.Insert(0, item.Name);
                    CbdFolder.SelectedIndex = 0;
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Folder already exists", "Message");
                }
            }
        }

        #endregion

        #region FUNCTIONS

        private void ScanFolder(string folder)
        {
            try
            {
                string[] fileExtension = new string[] { ".jpg", ".png" };
                List<string> ImagesInDirectory = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories)
                    .Where(file => new string[] { ".jpg", ".gif", ".png" }
                    .Contains(System.IO.Path.GetExtension(file))).ToList();

                int numberOfFiles = ImagesInDirectory.Count();
                lblBottomRight.Content = String.Format("{0} of {1}", 1, numberOfFiles);
                imgCount = 0;

                foreach (string item in ImagesInDirectory)
                {
                    LoadImage(item);
                    lblBottomLeft.Content = string.Format("{0}",System.IO.Path.GetFileName(item));
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, "Error");
            }
        }

        /// <summary>
        /// Returns rtue if image is loaded
        /// </summary>
        /// <param name="imgPath"></param>
        /// <returns></returns>
        private bool LoadImage(string imgPath)
        {
            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imgPath, UriKind.Relative);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                imgMain.Stretch = Stretch.Uniform;
                imgMain.Source = bitmap;
                return true;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(String.Format("Error openning file {0}: {1}", imgPath, ex.Message), "Error");
                return false; 
            }            
        }

        #endregion
    }
}
