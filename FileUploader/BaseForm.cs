using System.Drawing;
using System.Windows.Forms;

namespace FileUploader
{
    internal class BaseForm : Form
    {

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);

            Properties.Settings.Default[Name + "Location"] = Location;
            Properties.Settings.Default[Name + "Size"] = Size;
            Properties.Settings.Default.Save();
        }

        protected void RestoreWindow()
        {
            var location = (Point)Properties.Settings.Default[Name + "Location"];
            if (location.X == 0 && location.Y == 0)
                StartPosition = FormStartPosition.WindowsDefaultLocation;
            else
            {
                StartPosition = FormStartPosition.Manual;
                Location = location;
            }

            var size = (Size)Properties.Settings.Default[Name + "Size"];
            if (Size.Height == 0 || Size.Width == 0)
                return;
            Size = size;
        }
    }
}
