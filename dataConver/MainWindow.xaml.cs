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
    class SleepDataBit
    {
        public string datetime { get; set; }
        public string level { get; set; }
        public string seconds { get; set; }

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
            foreach(var file in queue)
            {
                CreateCSVOutput(file.FullName, dataoutfolder + "\\" + System.IO.Path.GetFileNameWithoutExtension(file.Name) + ".csv");
            }
        }

        private void Label_MouseUp(object sender, MouseButtonEventArgs e)
        {
            CreateCSVOutput(filePath, dataoutfolder + "\\" + System.IO.Path.GetFileNameWithoutExtension(filePath) + ".csv");
        }

        private void CreateCSVOutput(string inputPath, string OutputPath)
        {
            // Read Data File
            string json = File.ReadAllText(inputPath);
            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            JObject fitBitData = JObject.Parse(json);

            // get JSON result objects into a list
            IList<JToken> databits = fitBitData["sleep"][0]["levels"]["data"].Children().ToList();

            // serialize JSON results into .NET objects
            var csv = new StringBuilder();
            csv.AppendLine("datetime,level,seconds");

            IList<SleepDataBit> datalist = new List<SleepDataBit>();
            foreach (JToken bit in databits)
            {
                // JToken.ToObject is a helper method that uses JsonSerializer internally
                SleepDataBit bitfit = bit.ToObject<SleepDataBit>();
                //Console.WriteLine("datetime: " + bitfit.datetime + "level: " + bitfit.level + "secs: " + bitfit.seconds);

                var newLine = string.Format("{0},{1},{2}", bitfit.datetime, bitfit.level, bitfit.seconds);
                csv.AppendLine(newLine);
            }   
            File.WriteAllText(OutputPath, csv.ToString());
        }

    }
}
