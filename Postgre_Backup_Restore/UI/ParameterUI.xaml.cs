using Postgre_Backup_Restore.ParamaterManager;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace Postgre_Backup_Restore.UI
{
    /// <summary>
    /// Interaction logic for ParameterUI.xaml
    /// </summary>
    public partial class ParameterUi : UserControl
    {
        public ParameterUi()
        {
            InitializeComponent();
        }
        //----------------------
        /// <summary>
        /// Onsaveclick event to listen process from outside of the component
        /// </summary>
        public static readonly RoutedEvent OnSaveClickEvent = EventManager.RegisterRoutedEvent(
            "OnSaveClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ParameterUi));
        // Provide CLR accessors for the event
        public event RoutedEventHandler OnSaveClick
        {
            add
            {
                AddHandler(OnSaveClickEvent, value);
            }
            remove
            {
                RemoveHandler(OnSaveClickEvent, value);
            }
        }
        // This method raises the Save event
        private void RaiseOnSaveClickEvent()
        {
            var newEventArgs = new RoutedEventArgs(OnSaveClickEvent);
            RaiseEvent(newEventArgs);
        }
        //--------------------

        //----------------------
        /// <summary>
        /// Onsaveclick event to listen process from outside of the component
        /// </summary>
        public static readonly RoutedEvent OnBackupClickEvent = EventManager.RegisterRoutedEvent(
            "OnBackupClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ParameterUi));
        // Provide CLR accessors for the event
        public event RoutedEventHandler OnBackupClick
        {
            add
            {
                AddHandler(OnBackupClickEvent, value);
            }
            remove
            {
                RemoveHandler(OnBackupClickEvent, value);
            }
        }
        // This method raises the Save event
        private void RaiseOnBackupClickEvent()
        {
            var newEventArgs = new RoutedEventArgs(OnBackupClickEvent);
            RaiseEvent(newEventArgs);
        }
        //--------------------
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Parameters.Params.Host = TxtHost.Text;
            Parameters.Params.Base = TxtDbName.Text;
            Parameters.Params.Password = TxtPassword.Password;
            Parameters.Params.Port = TxtPort.Text;
            Parameters.Params.User = TxtUsername.Text;
            Parameters.Params.PgDump = TxtPgDump.Text;

            var serializer = new XmlSerializer(typeof(Parameters));
            using (TextWriter writer = new StreamWriter(@"Parameters.xml"))
            {
                serializer.Serialize(writer, Parameters.Params);
            }

            RaiseOnSaveClickEvent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
            {
                var result = fbd.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    //
                    // The user selected a folder and pressed the OK button.
                    // We print the number of files found.
                    //
                    var files = Directory.GetFiles(fbd.SelectedPath);
                    var temp = false;
                    if (files.Any(item => item.Contains("pg_dump.exe")))
                    {
                        TxtPgDump.Text = fbd.SelectedPath;
                        temp = true;
                    }
                    if (!temp)
                    {
                        MessageBox.Show("Seçili Klasörde pgdump.exe bulunamadı");
                    }
                }
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
            {
                var result = fbd.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    TxtBackup.Text = fbd.SelectedPath;
                    RaiseOnBackupClickEvent();
                }
            }
        }
    }
}
