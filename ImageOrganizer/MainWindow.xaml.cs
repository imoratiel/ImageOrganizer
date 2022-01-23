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
using System.Drawing;
using System.Net.Cache;

namespace ImageOrganizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string CurrentFolder = "";
        private string CurrentFile = null;
        private List<FolderItem> folderItems = new List<FolderItem>();
        private int imgCount = 0;

        public enum RotateFlip { left= 0, right = 1 }        

        public MainWindow()
        {
            InitializeComponent();

            Initialize();
        }

        public void Initialize()
        {
            AddFolderToCombobox(@"C:\Users\Usuario\Desktop\Test");
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
                AddFolderToCombobox(fbd.SelectedPath);
            }
        }

        private void ButtonRotate_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentFile != null)
            {
                System.Windows.Controls.Button obj = sender as System.Windows.Controls.Button;
                if (obj.Name == "btnRotateLeft")
                {
                    RotateImage(CurrentFile, RotateFlip.left);
                } 
                else
                {
                    RotateImage(CurrentFile, RotateFlip.right);
                }
                LoadImage(CurrentFile);
                //LoadImage(@"C:\Users\Usuario\Desktop\Test\_meriialro___CQ35iOcr2-9___.jpg");                
            }            
        }

        #endregion

        #region FUNCTIONS

        private void AddFolderToCombobox(string pathToAdd)
        {
            if (folderItems.Where(i => i.Path == pathToAdd).Count() == 0)
            {
                CurrentFolder = pathToAdd;

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

        private void ScanFolder(string folder)
        {
            try
            {
                string[] fileExtension = new string[] { ".jpg", ".png", ".jpeg", ".bmp",".gif" };
                List<string> ImagesInDirectory = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories)
                    .Where(file => fileExtension.Contains(System.IO.Path.GetExtension(file).ToLower()))
                    .ToList();

                int numberOfFiles = ImagesInDirectory.Count();
                lblBottomRight.Content = String.Format("{0} of {1}", 1, numberOfFiles);
                imgCount = 0;

                string image = ImagesInDirectory.First();
                if (LoadImage(image))
                {
                    CurrentFile = image;
                    lblBottomLeft.Content = string.Format("{0}", System.IO.Path.GetFileName(image));
                }
                else
                {
                    CurrentFile = null;
                    lblBottomLeft.Content = string.Format("Not Loaded {0}", System.IO.Path.GetFileName(image));
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
                imgMain.Source = null;

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imgPath);
                bitmap.CacheOption = BitmapCacheOption.None;
                bitmap.UriCachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bitmap.EndInit();
                //imgMain.Stretch = Stretch.Uniform;
                imgMain.Source = bitmap;
                return true;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(String.Format("Error openning file {0}: {1}", imgPath, ex.Message), "Error");
                return false; 
            }            
        }

        private bool RotateImage(string path, RotateFlip rotation)
        {
            try
            {
                Bitmap bitmap = (Bitmap)Bitmap.FromFile(path);
                bitmap.RotateFlip(rotation == RotateFlip.left ? RotateFlipType.Rotate90FlipXY : RotateFlipType.Rotate270FlipXY);
                bitmap.Save(path);
                return true;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(String.Format("Error rotating file {0}: {1}", path, ex.Message), "Error");
                return false;
            }
        }

        #endregion
    }
}
