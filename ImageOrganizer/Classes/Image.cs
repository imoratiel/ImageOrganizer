using System;
using System.Linq;
using System.Windows.Media.Imaging;

namespace ImageOrganizer.Classes
{
    public static class ImageUtil
    {
        private const int exifOrientationID = 0x112;
        public static Rotation GetRotateFromExif(string fileName)
        {
            System.Drawing.Image img = System.Drawing.Image.FromFile(fileName);

            if (!img.PropertyIdList.Contains(exifOrientationID))
                return Rotation.Rotate0;

            var prop = img.GetPropertyItem(exifOrientationID);
            int val = BitConverter.ToUInt16(prop.Value, 0);
            img.Dispose();

            if (val == 3 || val == 4)
                //rot = RotateFlipType.Rotate180FlipNone;
                return Rotation.Rotate180;
            else if (val == 5 || val == 6)
                //rot = RotateFlipType.Rotate90FlipNone;
                return Rotation.Rotate90;
            else if (val == 7 || val == 8)
                //rot = RotateFlipType.Rotate270FlipNone;
                return Rotation.Rotate270;
            else if (val == 2 || val == 4 || val == 5 || val == 7)
                return Rotation.Rotate0;

            //if (rot != RotateFlipType.RotateNoneFlipNone)
            //    img.RotateFlip(rot);
            return Rotation.Rotate0;
        }
    }
}
