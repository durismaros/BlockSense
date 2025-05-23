using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using BlockSense.Models.User;
using BlockSense.Views;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.Utilities
{
    public class ProfilePictureHandler
    {
        private readonly int _userId;
        private readonly string _userPfpPath;
        private readonly Bitmap _defaultPfpBitmap;
        private static readonly string _profilePicturesDirPath;

        static ProfilePictureHandler()
        {
            _profilePicturesDirPath = @"C:\Users\d3str\Desktop\School\BlockSense\BlockSenseAPI\ProfilePictures";
        }

        public ProfilePictureHandler(UserInfoModel userInfo)
        {
            _userId = userInfo.UserId;
            _userPfpPath = Path.Combine(_profilePicturesDirPath, _userId + ".jpeg");
            _defaultPfpBitmap = new Bitmap(@"C:\Users\d3str\Desktop\School\BlockSense\BlockSense\BlockSense\Assets\defaultPfp.png");
        }

        public async Task UploadFile(Window parentWindow)
        {
            var topLevel = TopLevel.GetTopLevel(parentWindow);

            // Start async operation to open the dialog.
            var importedFile = await topLevel!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Upload a Profile Picture",
                AllowMultiple = false,
                FileTypeFilter = new List<FilePickerFileType>
            {
                new FilePickerFileType("Image Files")
                {
                    Patterns = new[] { "*.jpg", "*.jpeg", "*.png", "*.gif" }
                }
            }
            });

            if (importedFile != null && importedFile.Count > 0)
            {
                string filePath = importedFile.Single().Path.ToString().Remove(0, 8);
                File.Copy(filePath, _userPfpPath, true);
            }
        }


        public Bitmap SetDefaultPfp()
        {
            if (File.Exists(_userPfpPath))
            {
                File.Delete(_userPfpPath);
            }
            return _defaultPfpBitmap;
        }
        public Bitmap GetExistingPicture()
        {
            if (File.Exists(_userPfpPath))
            {
                return new Bitmap(_userPfpPath);
            }
            return _defaultPfpBitmap;
        }
    }
}
