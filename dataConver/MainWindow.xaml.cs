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
    enum SleepStates : uint
    {
        wake = 0,
        light = 1,
        deep = 2,
        rem = 3
    }

    class SleepDataBit
    {
        public string datetime { get; set; }
        public string level { get; set; }
        public string seconds { get; set; }

    }

    class HRDayInfo
    {
        public string dateTime { get; set; }
    }

    class HearthDataBit
    {
        public string time { get; set; }
        public string value { get; set; }
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
                lblConvertHR.IsEnabled = true;
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
                lblConvertHR_Copy.IsEnabled = true;
            }
        }

        private void SleepFoder_MouseUp(object sender, MouseButtonEventArgs e)
        {
            int numfiles = 0;
            foreach (var file in queue)
            {
                try
                {
                    CreateSleepOutput(file.FullName, dataoutfolder + "\\" + System.IO.Path.GetFileNameWithoutExtension(file.Name) + ".csv");
                    numfiles++;
                }
                catch (NullReferenceException)
                {
                    // just ignore the file if its not in the correct format
                } 
            }
            textBox1_folder.Text = "Finished processing" + numfiles + " c:";
        }

        private void Sleep_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                CreateSleepOutput(filePath, dataoutfolder + "\\" + System.IO.Path.GetFileNameWithoutExtension(filePath) + ".csv");
                textBox1.Text = "Finished c:";
            }
            catch (NullReferenceException)
            {
                textBox1.Text = "ERROR >:c";
                MessageBox.Show("This does not seem to be a valid Fitbit Sleep data file. Do you think you are very funny? >:(", "Invalid Input FIle", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HeartOutput_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                CreateHeartOutput(filePath, dataoutfolder + "\\" + System.IO.Path.GetFileNameWithoutExtension(filePath) + ".csv");
                textBox1.Text = "Finished c:";
            }
            catch (NullReferenceException)
            {
                textBox1.Text = "ERROR >:c";
                MessageBox.Show("This does not seem to be a valid Fitbit Heartrate data file. Do you think you are very funny? >:(", "Invalid Input FIle", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HeartOutputFolder_MouseUp(object sender, MouseButtonEventArgs e)
        {
            int numfiles = 0;
            foreach (var file in queue)
            {
                try
                {
                    CreateHeartOutput(file.FullName, dataoutfolder + "\\" + System.IO.Path.GetFileNameWithoutExtension(filePath) + ".csv");
                    numfiles++;
                }
                catch (NullReferenceException)
                {
                    // just ignore the file if its not in the correct format
                }
            }
            textBox1_folder.Text = "Finished processing" + numfiles + " c:";
        }

        private void CreateSleepOutput(string inputPath, string OutputPath)
        {
            // Read Data File
            string json = File.ReadAllText(inputPath);
            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            JObject fitBitSleepData = JObject.Parse(json);

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
            IList<JToken> databits = fitBitSleepData["sleep"][0]["levels"]["data"].Children().ToList();

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

        private void CreateHeartOutput(string inputPath, string OutputPath)
        {
            // Read Data File
            string json = File.ReadAllText(inputPath);
            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            JObject fitBitHRData = JObject.Parse(json);

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
            HRDayInfo datetoken = fitBitHRData["activities-heart"].Children().ToList().First().ToObject<HRDayInfo>();
            IList<JToken> HRdatabits = fitBitHRData["activities-heart-intraday"]["dataset"].Children().ToList();
            // serialize JSON results into .NET objects
            var csv = new StringBuilder();
            csv.AppendLine("datetime,bpm");
            string YearMonthDay = datetoken.dateTime;

            //IList<SleepDataBit> datalist = new List<SleepDataBit>();
            List<HearthDataBit> HRData = new List<HearthDataBit>();

            foreach (JToken bit in HRdatabits)
            {
                HRData.Add(bit.ToObject<HearthDataBit>());
            }

            for (int i = 0; i < HRData.Count; i++)
            {
                HearthDataBit thisdata = HRData[i];
                int numrows = 1;
                double step = 0;
                if (i == HRData.Count - 1)
                {
                    // if its last entry
                    DateTime thisTimeStamp = DateTime.Parse(YearMonthDay + "T" + thisdata.time + ".000");
                    var newLine = string.Format("{0},{1}", thisTimeStamp.ToString("s") + ".000", int.Parse(thisdata.value).ToString());
                    csv.AppendLine(newLine);

                }
                else
                {
                    HearthDataBit nextData = HRData[i + 1];
                    // calculate num of seconds and step size
                    TimeSpan spaned = (DateTime.Parse(nextData.time) - DateTime.Parse(thisdata.time));
                    numrows = (int)Math.Ceiling(spaned.TotalSeconds);
                    // calculate step size
                    step = (double)(int.Parse(nextData.value) - int.Parse(thisdata.value)) / (double)numrows;

                    //prepare to output rows
                    DateTime thisTimeStamp = DateTime.Parse(YearMonthDay + "T" + thisdata.time + ".000");
                    for (int j = 0; j < numrows; j++)
                    {
                        DateTime ts = thisTimeStamp.AddSeconds(j);
                        var newLine = string.Format("{0},{1}", ts.ToString("s") + ".000", Math.Round(((double.Parse(thisdata.value) + (step * (double)j)))).ToString());
                        csv.AppendLine(newLine);
                    }
                }

            }
            File.WriteAllText(OutputPath, csv.ToString());
        }
    }
}
