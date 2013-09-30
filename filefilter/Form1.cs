using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DataModel;

namespace filefilter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            RefreshDataGrid();
        }

        private void RefreshDataGrid()
        {
            var CurrentContext = new FishTaggingDataContext();
            FilesDataGridView.DataSource =
                from file in CurrentContext.RawDataFiles
                where file.FileName.EndsWith(".csv")
                select new { file.FileName, types = types(file), line = secondline(file) };
        }

        private string secondline(RawDataFile file)
        {
            var contents = file.Contents.ToArray();
            var lines = Encoding.ASCII.GetString(contents).Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            return lines[1];
        }

        private int types(RawDataFile file)
        {
            var contents = file.Contents.ToArray();
            var lines = Encoding.ASCII.GetString(contents).Split(new [] {Environment.NewLine}, StringSplitOptions.None);
            var dataLines =  lines.Skip(7).TakeWhile(s => s.Length > 5);
            var d1 = new Dictionary<int, int>();
            foreach (var dataLine in dataLines)
            {
                var l = dataLine.Split(',').Length;
                if (d1.ContainsKey(l))
                    d1[l]++;
                else d1[l] = 1;
            }
            return d1.Keys.Count;
        }
    }
}
/*
Aerial Format with Lat/Long: YY-Year,DDD-Day,HH-Hour,MM-Minute,SS-Second,(FF)FFFF-Frequency,nnm-Tag number and mortality,TTT-Signal Strength,(-)LL.LLLLLLLL-Latitude,(-)lll.llllllll-Longitude,         AA-GPS Age, 
Aerial Format with UTM:      YY-Year,DDD-Day,HH-Hour,MM-Minute,SS-Second,(FF)FFFF-Frequency,nnm-Tag number and mortality,TTT-Signal Strength,XXXXXXXX.X-X coordinate,  YYYYYYYY.Y-Y coordinate,ZZZ=Zone,AA-GPS Age, 
Stationary Target Format:    YY-Year,DDD-Day,HH-Hour,MM-Minute,A-Antenna,(FF)FFFF-Frequency,nnm-Tag number and mortality,TTT-Signal Strength,ddd-Duplicate Count, 
*/