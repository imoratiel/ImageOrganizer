using ImageOrganizer.Classes;
using ImageOrganizer.Data;
using Microsoft.Extensions.Configuration;
using NLog;
using Notification.Wpf;
using Notification.Wpf.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Cache;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ImageOrganizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private string CurrentFolder = "";
        private string CurrentFile = null;
        private List<FolderItem> folderItems = new List<FolderItem>();
        private int imgCount = 0;
        private FileActionDbContext dbContext;
        private NotificationManager notificationManager;

        public IConfiguration Configuration { get; private set; }

        public enum RotateFlip { left = 0, right = 1 }

        public MainWindow(IConfiguration configuration, FileActionDbContext dbContext)
        {
            InitializeComponent();

            Initialize(configuration, dbContext);

            var actions = dbContext.FileAction.ToList();
            logger.Debug("blah");
        }

        public void Initialize(IConfiguration configuration, FileActionDbContext dbContext)
        {
            this.notificationManager = new NotificationManager();
            this.Configuration = configuration;
            this.dbContext = dbContext;

            if (bool.Parse(Configuration["Startup:Enabled"]))
            {
                string initialFolder = Configuration["Startup:Folder"];
                AddFolderToCombobox(initialFolder);
                ScanFolder(initialFolder);
            }
        }

        #region EVENTS
        private void ButtonScanFolder_Click(object sender, RoutedEventArgs e)
        {
            ScanFolder(CurrentFolder);
        }

        private void ButtonAddFolder_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = fbd.ShowDialog();

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
            }
        }

        private void ButtonOpen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process p = new Process();
                p.StartInfo = new ProcessStartInfo(CurrentFile)
                {
                    UseShellExecute = true
                };
                p.Start();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(String.Format("Error openning file {0}: {1}", CurrentFile, ex.Message), "Error");
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
                SubFoldersButtonPanel.Children.Clear();

                //Folder Scanning
                string[] directories = Directory.GetDirectories(folder, "*", SearchOption.TopDirectoryOnly);
                foreach (string item in directories)
                {
                    Button btn = new Button()
                    {
                        Content = System.IO.Path.GetFileName(item),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(0, 5, 0, 0),
                        Width = 110
                    };
                    btn.Click += BtnMoveImageToFolder_Click;
                    SubFoldersButtonPanel.Children.Add(btn);
                }

                LoadNextImage(folder);
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }

        private void LoadNextImage(string folder)
        {
            try
            {
                // Images
                string[] fileExtension = new string[] { ".jpg", ".png", ".jpeg", ".bmp", ".gif" };
                List<string> ImagesInDirectory = Directory.GetFiles(folder, "*.*", SearchOption.TopDirectoryOnly)
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
                ShowErrorMessage(ex.Message);
            }
        }

        private void BtnMoveImageToFolder_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button obj = sender as System.Windows.Controls.Button;

            try
            {
                string selectedFolder = obj.Content.ToString();
                File.Move(CurrentFile, Path.Combine(CurrentFolder, selectedFolder, System.IO.Path.GetFileName(CurrentFile)));
                LoadNextImage(CurrentFolder);
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
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

                imgMain.Source = bitmap;
                return true;
            }
            catch (Exception ex)
            {
                ShowErrorMessage(String.Format("Error openning file {0}: {1}", imgPath, ex.Message));
                return false;
            }
        }

        private bool RotateImage(string path, RotateFlip rotation)
        {
            try
            {
                Bitmap bitmap = (Bitmap)Bitmap.FromFile(path);
                bitmap.RotateFlip(rotation == RotateFlip.left ? RotateFlipType.Rotate90FlipXY : RotateFlipType.Rotate270FlipXY);
                SaveImage(bitmap, path);
                return true;
            }
            catch (Exception ex)
            {
                ShowErrorMessage(String.Format("Error rotating file {0}: {1}", path, ex.Message));
                return false;
            }
        }

        private void SaveImage(Bitmap bitmap, string filename)
        {
            string fileExtension = System.IO.Path.GetExtension(filename);

            switch (fileExtension)
            {
                case ".jpg":
                case ".jpeg":
                    SaveImageJpeg(bitmap, filename);
                    break;
                case ".png":
                    bitmap.Save(filename);
                    break;
                case ".bmp":
                    break;
                case ".gif":
                    break;
                default:
                    break;
            }
        }
        private void SaveImageJpeg(Bitmap bitmap, string filename)
        {
            ImageCodecInfo myImageCodecInfo;
            System.Drawing.Imaging.Encoder myEncoder;
            EncoderParameter myEncoderParameter;
            EncoderParameters myEncoderParameters;

            myImageCodecInfo = GetEncoderInfo("image/jpeg");
            myEncoder = System.Drawing.Imaging.Encoder.Quality;

            myEncoderParameters = new EncoderParameters(1);
            myEncoderParameter = new EncoderParameter(myEncoder, 100L);
            myEncoderParameters.Param[0] = myEncoderParameter;
            bitmap.Save(filename, myImageCodecInfo, myEncoderParameters);
        }

        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }

        #endregion

        #region ERRORS
        private void ShowErrorMessage(string message, Action method = null)
        {
            ShowMessage("Error", message, System.Windows.Media.Colors.Red, System.Windows.Media.Colors.White, 5, method);
        }
        private void ShowMessage(string title, string message, System.Windows.Media.Color foreground, System.Windows.Media.Color background, int seconds, Action method)
        {
            NotificationContent content = new NotificationContent
            {
                Title = title,
                Message = message,
                Type = NotificationType.Warning,                
                RowsCount = 3, //Will show 3 rows and trim after
                LeftButtonAction = method, 
                //RightButtonAction = () => SomeAction(), //Action on right button click,  button will not show if it null                
                CloseOnClick = true, // Set true if u want close message when left mouse button click on message (base = true)
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red),
                Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White)
            };
            notificationManager.Show(content, null, new TimeSpan(0,0,seconds));
        }
        #endregion
    }
}
