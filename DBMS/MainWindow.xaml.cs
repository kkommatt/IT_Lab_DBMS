using System;
using System.Collections.Generic;
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
using DBMS.database;

namespace DBMS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Database _database;
        private string _databaseFilePath;

        public MainWindow(string databaseFilePath)
        {
            _databaseFilePath = databaseFilePath;
            InitializeComponent();
            LoadDatabase();
            UpdateTableList();
            this.Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var openWindow = new OpenWindow();
            openWindow.Show();
        }


        private void LoadDatabase()
        {
            _database = Database.LoadFromFile(_databaseFilePath);
        }

        private void SaveDatabase()
        {
            _database.SaveToFile(_databaseFilePath);
        }

        private void UpdateTableList()
        {
            ListBoxTables.Items.Clear();
            foreach (var table in _database.Tables)
            {
                ListBoxTables.Items.Add(table.Name);
            }
        }

        private void btnCreateTable_Click(object sender, RoutedEventArgs e)
        {
            var createTableWindow = new CreateTableWindow(_database);
            if (createTableWindow.ShowDialog() == true)
            {
                SaveDatabase();
                UpdateTableList();
            }
        }

        private void btnViewTable_Click(object sender, RoutedEventArgs e)
        {
            if (ListBoxTables.SelectedItem != null)
            {
                string tableName = ListBoxTables.SelectedItem.ToString();
                var table = _database.GetTable(tableName);
                var tableViewWindow = new TableViewWindow(table, _database);
                tableViewWindow.ShowDialog();
                SaveDatabase();
            }
            else
            {
                MessageBox.Show("Please, choose table");
            }
        }

        private void btnDeleteTable_Click(object sender, RoutedEventArgs e)
        {
            if (ListBoxTables.SelectedItem != null)
            {
                string tableName = ListBoxTables.SelectedItem.ToString();
                var result = MessageBox.Show($"Are you sure in deletetion '{tableName}'?", "Approve",
                    MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    _database.DeleteTable(tableName);
                    SaveDatabase();
                    UpdateTableList();
                }
            }
            else
            {
                MessageBox.Show("Please, choose table");
            }
        }

        private void btnSaveDatabase_Click(object sender, RoutedEventArgs e)
        {
            SaveDatabase();
            MessageBox.Show("Database saved");
            this.Close();
        }

        private void btnLoadDatabase_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to load a new database? This will close the current session.",
                "Load Database",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                this.Close();
            }
        }
    }
}