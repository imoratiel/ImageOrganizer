using ImageOrganizer.Classes;
using ImageOrganizer.Model;
using ImageOrganizer.Repositories;
using Microsoft.Extensions.Configuration;
using NLog;
using Notification.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Cache;
using System.Security.Cryptography;
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
        private NotificationManager notificationManager;
        private FileActionRepository fileActionRepository;
        private string LastFileHash =null;

        public IConfiguration Configuration { get; private set; }

        public enum RotateFlip { left = 0, right = 1 }

        public MainWindow(IConfiguration configuration, FileActionDbContext dbContext)
        {
            InitializeComponent();            

            Initialize(configuration, dbContext);

            logger.Debug(string.Format("Current Action Queue"));
            fileActionRepository.Print();
        }

        public void Initialize(IConfiguration configuration, FileActionDbContext dbContext)
        {
            this.notificationManager = new NotificationManager();
            this.Configuration = configuration;

            fileActionRepository = new FileActionRepository(dbContext);

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
                ShowErrorMessage(String.Format("Error openning file {0}: {1}", CurrentFile, ex.Message));
            }
        }
        private void BtnMoveImageToFolder_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button obj = sender as System.Windows.Controls.Button;

            try
            {
                string selectedFolder = obj.Content.ToString();
                bool overwriteFileIfExists = toggleFileExists.IsChecked.GetValueOrDefault();
                MoveImageToFolder(selectedFolder, overwriteFileIfExists);
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }
        private void ButtonSkip_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SkipFile(CurrentFile);
                LoadNextImage(CurrentFolder);
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }
        private void ButtonUndo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FileAction lastAction = fileActionRepository.GetLastItem();
                if(lastAction != null)
                {
                    switch (lastAction.actionType)
                    {
                        case ActionType.Move:
                            File.Move(lastAction.destination, lastAction.source);
                            fileActionRepository.RemoveLastItem();
                            LoadNextImage(CurrentFolder);
                            break;
                        case ActionType.Delete:
                            fileActionRepository.RemoveLastItem();
                            break;
                        case ActionType.RoteteRight:
                            if (File.Exists(lastAction.destination))
                            {
                                RotateImage(lastAction.destination, RotateFlip.left);
                            }
                            fileActionRepository.RemoveLastItem();
                            break;
                        case ActionType.RotateLeft:
                            if (File.Exists(lastAction.destination))
                            {
                                RotateImage(CurrentFile, RotateFlip.left);
                            }                                
                            fileActionRepository.RemoveLastItem();
                            break;
                        case ActionType.Skip:
                            fileActionRepository.RemoveLastItem();
                            break;
                        default:
                            ShowErrorMessage("Something smells rotten in Denmark");
                            break;
                    }

                    fileActionRepository.Print();
                }
                else
                {
                    ShowWarningMessage("Action Queue Empty");
                }                
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }

        private void ButtonResetIgnored_Click(object sender, RoutedEventArgs e)
        {
            fileActionRepository.ResetSkippedFiles(CurrentFolder);
            LoadNextImage(CurrentFolder);
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
                ShowErrorMessage("Folder already exists");
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
                    .OrderBy(file => System.IO.Path.GetFileName(file))
                    .ToList();

                int numberOfFiles = ImagesInDirectory.Count();

                List<string> skippedFiles = fileActionRepository.GetSkippedItems(folder);
                ImagesInDirectory.RemoveAll(x => skippedFiles.Contains(x));

                lblBottomRight.Content = String.Format("{0} of {1}  {2}%", skippedFiles.Count + 1, numberOfFiles, (skippedFiles.Count + 1) * 100 / numberOfFiles);

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
        /// <summary>
        /// Returns true if image is loaded
        /// </summary>
        /// <param name="imgPath"></param>
        /// <returns></returns>
        private bool LoadImage(string imgPath)
        {
            try
            {
                CheckDuplicatedFile(imgPath);
                LastFileHash = (new FileHash()).GetHashSha256(imgPath);                

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
            string fileExtension = System.IO.Path.GetExtension(filename).ToLower();

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
        private void MoveImageToFolder(string selectedFolder, bool overwriteFileIfExists)
        {   
            string destination = Path.Combine(CurrentFolder, selectedFolder, System.IO.Path.GetFileName(CurrentFile));
            if (overwriteFileIfExists || File.Exists(destination) == false)
            {
                File.Move(CurrentFile, destination,true);
                fileActionRepository.AddAction(ActionType.Move, CurrentFile, destination);                
            }
            else
            {
                ShowInfoMessage("File already exists in destination folder");
            }
            LoadNextImage(CurrentFolder);
            logger.Debug(string.Format("Moving file {0} to {1}", CurrentFile, destination));
        }
        private void SkipFile(string file)
        {
            fileActionRepository.AddAction(ActionType.Skip, CurrentFile, null);
        }

        private void CheckDuplicatedFile(string file)
        {
            string hash = (new FileHash()).GetHashSha256(file);
            if(LastFileHash != null && hash == LastFileHash)
            {
                ShowInfoMessage(string.Format("Duplicated file {0}, same hash as last loaded image", Path.GetFileNameWithoutExtension(file)), null, 20);
            } 
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
        private void ShowWarningMessage(string message, Action method = null)
        {
            ShowMessage("Warning", message, System.Windows.Media.Colors.Orange, System.Windows.Media.Colors.Black, 5, method);
        }
        private void ShowInfoMessage(string message, Action method = null, int seconds = 5)
        {
            ShowMessage("Info", message, System.Windows.Media.Colors.ForestGreen, System.Windows.Media.Colors.White, seconds, method);
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
                Background = new System.Windows.Media.SolidColorBrush(foreground),
                Foreground = new System.Windows.Media.SolidColorBrush(background)
            };
            notificationManager.Show(content, null, new TimeSpan(0,0,seconds));
        }
        #endregion
    }
}
