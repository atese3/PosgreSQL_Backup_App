using Npgsql;
using Postgre_Backup_Restore.ParamaterManager;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using WindowsInput;
using WindowsInput.Native;
using CheckBox = System.Windows.Controls.CheckBox;
using MessageBox = System.Windows.MessageBox;

namespace Postgre_Backup_Restore
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public NpgsqlConnection MNpgsqlConnection;
        private string _strConnection;
        static List<string> _list = new List<string>();
        private readonly char[] _ch =
        {
            (char) 92, (char) 34
        }; /// (") and (\) ASCII characters 

        private void search(string sDir)
        {
            /// Search Loop for Directories
            foreach (var d in Directory.GetDirectories(sDir))
            {
                try
                {
                    if (_list.Count == 0)
                    {
                        Parameters.Params.PgDump = d;
                        foreach (var f in Directory.GetFiles(d, "pg_dump*"))
                        {
                            _list.Add(f);
                        }
                        if (_list.Count == 0)
                        {
                            search(d);
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
        }
        public MainWindow()
        {
            /// Initialize the Steps
            InitializeComponent();
            Init();
        }
        private void Init()
        {
            /// Read Parameters From File
            var parameters = Parameters.Params;
            UiParameter.TxtHost.Text = Parameters.Params.Host;
            UiParameter.TxtDbName.Text = Parameters.Params.Base;
            UiParameter.TxtPassword.Password = Parameters.Params.Password;
            UiParameter.TxtPort.Text = Parameters.Params.Port;
            UiParameter.TxtUsername.Text = Parameters.Params.User;
            UiParameter.TxtPgDump.Text = Parameters.Params.PgDump;

            /// Check Backup Directory Selected
            if (string.IsNullOrEmpty(UiParameter.TxtBackup.Text))
            {
                UiParameter.BtnBackup.Background = Brushes.Red;
            }

            /// Find pg_dump.exe File in Logical Drivers
            if (string.IsNullOrEmpty(Parameters.Params.PgDump))
            {
                _list.Clear();
                UiParameter.BtnDump.Background = Brushes.Red;
                foreach (var s in Directory.GetLogicalDrives())
                {
                    if (_list.Count == 0)
                    {
                        foreach (var f in Directory.GetFiles(s, "pg_dump*"))
                        {
                            _list.Add(f);
                        }
                        if (_list.Count == 0)
                        {
                            search(s);
                        }
                    }
                }
            }

            /// Check pg_dmp.exe Found
            if (!string.IsNullOrEmpty(Parameters.Params.PgDump))
            {
                UiParameter.BtnDump.Background = Brushes.LawnGreen;
            }

            var checkBoxs = new List<CheckBox>();
            _strConnection = "Host=" + parameters.Host + ";Port=" + parameters.Port + ";Database=" + parameters.Base + ";Username=" + parameters.User + ";Password=" + parameters.Password + ";";

            var dsDb = GetData("SELECT * FROM pg_catalog.pg_tables WHERE schemaname != 'pg_catalog' AND schemaname != 'information_schema';");
            if (dsDb == null)
                return;

            if (dsDb.Tables[0].Rows.Count > 0)
            {
                for (var i = 0; i < dsDb.Tables[0].Rows.Count; i++)
                {
                    /// Schema Name is Controlled for Uppercase Letter
                    /// If it has upper case letter arranged as "\"name\"
                    /// Otherwise use it without change
                    var schemaName = "" + _ch[1] + dsDb.Tables[0].Rows[i][0];
                    if (HasUpperCase(schemaName))
                    {
                        schemaName = "" + _ch[1] + _ch[0] + _ch[0] + schemaName + _ch[0] + _ch[0] + _ch[1];
                    }

                    var uid = Regex.Unescape(schemaName + "." + dsDb.Tables[0].Rows[i][1] + _ch[1]);
                    var content = dsDb.Tables[0].Rows[i][0] + " => " + dsDb.Tables[0].Rows[i][1];
                    content = content.Replace("_", "__");
                    /// Build CheckBoxes with Table, Schema and Row Names
                    var cb = new CheckBox
                    {
                        Height = 20,
                        Content = content,
                        Uid = uid, /// Arranged table name
                        IsChecked = true
                    };

                    /// Control Click Events
                    cb.Click += cb_Click;

                    /// Add CheckBoxes to List
                    checkBoxs.Add(cb);
                }

                /// ORder Check Boxes with Content Names
                checkBoxs = checkBoxs.OrderBy(x => x.Content).ToList();
                /// Bind the List to ListBox
                Combo.ItemsSource = checkBoxs;
            }
            else
            {
                MessageBox.Show("No Database is existing");
            }
        }
        public static string ReplaceFirstOccurrence(string source, string find, string replace)
        {
            var place = source.IndexOf(find, StringComparison.Ordinal);
            if (place > 0)
            {
                var result = source.Remove(place, find.Length).Insert(place, replace);
                return result;
            }
            return source;
        }

        public static string ReplaceLastOccurrence(string source, string find, string replace)
        {
            var place = source.LastIndexOf(find, StringComparison.Ordinal);
            if (place > 0)
            {
             
                var result = source.Remove(place, find.Length).Insert(place, replace);
                return result;   
            }
            return source;
        }

        /// <summary>
        /// Check the Input has Upper Case Letter,
        /// If it has returns true
        /// otherwise returns false
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private bool HasUpperCase(string input)
        {
            foreach (var character in input)
            {
                if (char.IsUpper(character))
                    return true;
            }

            return false;
        }

        /// Control Mechanism to Check Selected Row Parameter
        private void cb_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show(((CheckBox)sender).Uid);
        }


        public DataSet GetData(string strQuery)
        {
            /// Get All Data from Postgresql with Selected Parameters
            var objDataSet = new DataSet();
            try
            {
                MNpgsqlConnection = new NpgsqlConnection(_strConnection);
                
                var objSqlAdapter = new NpgsqlDataAdapter(strQuery, MNpgsqlConnection);
                objSqlAdapter.Fill(objDataSet);
                return objDataSet;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            /// build pg_dump parameter
            var dir = UiParameter.TxtBackup.Text + "\\" + UiParameter.TxtDbName.Text + "_Backup" + "_" + DateTime.Now.ToString("yyMMdd_HHmm");
            var pgDump = "--host " + _ch[1] + Parameters.Params.Host + _ch[1] + " --port " + _ch[1] + Parameters.Params.Port + _ch[1] + " --username " + _ch[1] + Parameters.Params.User + _ch[1]; 
            var exclude = string.Empty;
            foreach(var item in Combo.ItemsSource)
            {
                if (item.GetType() == typeof(CheckBox))
                {
                    /// build omitted table parameter
                    var isChecked = ((CheckBox)item).IsChecked;
                    if (isChecked != null && !(bool)isChecked)
                    {
                        exclude += " -T " + ((CheckBox)item).Uid;
                    }
                }
            }
            pgDump += exclude;
            pgDump += " --format=c --blobs --verbose --file \"" + dir + "\" \"" + Parameters.Params.Base + "\"";
            
            /// pg_dump process settings
            var myProcess = new Process
            {
                StartInfo =
                {
                    FileName = Parameters.Params.PgDump + "\\pg_dump.exe",
                    Arguments = pgDump,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    ErrorDialog = false
                    //CreateNoWindow = true
                }
            };

            myProcess.Start();

            /// Used for password step
            var bw = new BackgroundWorker();
            bw.DoWork += bw_DoWork;
            bw.RunWorkerAsync();

            /// End of process and handle exit
            myProcess.WaitForExit();
            myProcess.Dispose();
        }

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            /// Wait for the process start
            Thread.Sleep(1000);
            /// Input simulator used for password input to simulate key presses
            var sim = new InputSimulator();
            foreach (var character in Parameters.Params.Password)
            {
                /// Input keys arranged as Enum with name starting as "VK_" and  continues with Upper Cases
                sim.Keyboard.KeyDown((VirtualKeyCode)Enum.Parse(typeof(VirtualKeyCode), "VK_" + character.ToString().ToUpper()));
            }

            /// Finally Pressed Enter
            sim.Keyboard.KeyDown(VirtualKeyCode.RETURN);
        }

        private void UiParameter_OnOnSaveClick(object sender, RoutedEventArgs e)
        {
            /// Parameter Check
            Init();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            /// Assign Select All Check Value to All Rows
            var cb = (CheckBox) sender;
            foreach (var item in Combo.ItemsSource)
            {
                if (item.GetType() == typeof(CheckBox))
                {
                    ((CheckBox) item).IsChecked = cb.IsChecked;
                }
            }
        }

        private void UiParameter_OnOnBackupClick(object sender, RoutedEventArgs e)
        {
            /// Backup Directory Selection Changed
            if (!string.IsNullOrEmpty(UiParameter.TxtBackup.Text))
            {
                UiParameter.BtnBackup.Background = Brushes.LawnGreen;
            }
        }
    }
}
