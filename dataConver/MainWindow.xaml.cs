using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace dataConver
{
    enum SleepStates:uint
    {
        wake = 0,
        light = 1,
        deep = 2,
        REM = 3
    }

    class SleepDataBit
    {
        public string datetime { get; set; }
        public string level { get; set; }
        public string seconds { get; set; }

    }

    class SleepTimes
    {
        public string startTime { get; set; }
        public string endTime { get; set; }
        //public string dateOfSleep { get; set; }
        //public string duration { get; set; }
        //public string efficiency { get; set; }
        //public string infoCode { get; set; }
        //public string isMainSleep { get; set; }
        //public string levels { get; set; }
        //public string logId { get; set; }
        //public string minutesAfterWakeup { get; set; }
        //public string minutesAsleep { get; set; }
        //public string minutesAwake { get; set; }
        //public string minutesToFallAsleep { get; set; }
        //public string timeInBed { get; set; }
        //public string type { get; set; }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            dataoutfolder = System.AppDomain.CurrentDomain.BaseDirectory + "\\DataFolder";
            if (!File.Exists(dataoutfolder)) { Directory.CreateDirectory(dataoutfolder); }
        }

        private string dataoutfolder;
        private string filePath = "";
        private void FileSelect_MouseUp(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            var dialog = new OpenFileDialog
            {
                CheckPathExists = true,
                CheckFileExists = true,
                Title = "Fileを選択してください。",
            };

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".txt";
            dlg.Filter = "Text Files (*.txt)|*.txt|JSON Files (*.json)|*.json";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
                filePath = filename;
                textBox1.Text = filename;
                lblConvert.IsEnabled = true;
            }
        }

        public List<FileInfo> queue = new List<FileInfo>();
        private void FolderSelect_MouseUp(object sender, MouseButtonEventArgs e)
        {
            CommonOpenFileDialog dlg = new CommonOpenFileDialog();
            var dialog = new CommonOpenFileDialog
            {
                EnsurePathExists = true,
                EnsureFileExists = true,
                AllowNonFileSystemItems = false,
                Title = "フォルダを選択してください。",
                IsFolderPicker = true
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                DirectoryInfo dInfo = new DirectoryInfo(dialog.FileName);
                List<FileInfo> fileList = new List<FileInfo>(dInfo.GetFiles("*.txt"));

                foreach (var file in fileList)
                {
                    queue.Add(file);
                }
                textBox1_folder.Text = string.Format("I got {0} files", queue.Count);
                lblFolderConvert.IsEnabled = true;
            }
        }

        private void LabelFolder_MouseUp(object sender, MouseButtonEventArgs e)
        {
            int numfiles = 0;
            foreach (var file in queue)
            {
                CreateCSVOutput(file.FullName, dataoutfolder + "\\" + System.IO.Path.GetFileNameWithoutExtension(file.Name) + ".csv");
                numfiles++;
            }
            textBox1_folder.Text = "Finished processing" + numfiles + " c:";
        }

        private void Label_MouseUp(object sender, MouseButtonEventArgs e)
        {
            CreateCSVOutput(filePath, dataoutfolder + "\\" + System.IO.Path.GetFileNameWithoutExtension(filePath) + ".csv");
            textBox1_folder.Text = "Finished c:";
        }

        private void CreateCSVOutput(string inputPath, string OutputPath)
        {
            // Read Data File
            string json = File.ReadAllText(inputPath);
            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            JObject fitBitData = JObject.Parse(json);

            ////Obtain start and end times
            //IList<JToken> dates = fitBitData["sleep"].Children().ToList();
            //SleepTimes sleeptimes = new SleepTimes();
            //foreach (JToken bit in dates)
            //{
            //    // JToken.ToObject is a helper method that uses JsonSerializer internally
            //    sleeptimes = bit.ToObject<SleepTimes>();
            //    //Console.WriteLine("datetime: " + bitfit.datetime + "level: " + bitfit.level + "secs: " + bitfit.seconds);
            //}

            //DateTime sleepStarted = DateTime.Parse(sleeptimes.startTime);
            //DateTime sleepEnded = DateTime.Parse(sleeptimes.endTime);

            //TimeSpan span = (sleepEnded - sleepStarted);
            //string a = sleepStarted.ToString("o");

            // get JSON result objects into a list
            IList<JToken> databits = fitBitData["sleep"][0]["levels"]["data"].Children().ToList();

            // serialize JSON results into .NET objects
            var csv = new StringBuilder();
            csv.AppendLine("datetime,level");

            //IList<SleepDataBit> datalist = new List<SleepDataBit>();
            List<SleepDataBit> sleepData = new List<SleepDataBit>();
            foreach (JToken bit in databits)
            {
                // JToken.ToObject is a helper method that uses JsonSerializer internally
                sleepData.Add(bit.ToObject<SleepDataBit>());
                //Console.WriteLine("datetime: " + bitfit.datetime + "level: " + bitfit.level + "secs: " + bitfit.seconds);

            }
            for (int i = 0; i < sleepData.Count; i++)
            {
                SleepDataBit thisdata = sleepData[i];
                DateTime thisTimeStamp = DateTime.Parse(thisdata.datetime);
                Enum.TryParse(sleepData[i].level, out SleepStates level);
                //TimeSpan spaned = (DateTime.Parse(sleepData[i+1].datetime) - DateTime.Parse(sleepData[i].datetime));
                //int numrows = (int)Math.Ceiling(spaned.TotalSeconds);

                for (int j = 0; j < int.Parse(thisdata.seconds); j++)
                {
                    DateTime ts = thisTimeStamp.AddSeconds(j);
                    var newLine = string.Format("{0},{1}", ts.ToString("s") + ".000", ((uint)level).ToString());
                    csv.AppendLine(newLine);
                }
            }
            File.WriteAllText(OutputPath, csv.ToString());
        }
    }
}
