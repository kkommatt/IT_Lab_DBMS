using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DBMS.database;
using System.Windows.Data;


namespace DBMS;

public partial class TableViewWindow : Window
{
    private Table _table;
    private Database _database;

    public TableViewWindow(Table table, Database database)
    {
        InitializeComponent();
        _table = table;
        _database = database;
        Title = $"Table: {_table.Name}";
        LoadData();
    }

    private void LoadData()
    {
        DataGrid.ItemsSource = null;
        var rows = new List<Dictionary<string, object>>();
        var columns = new List<DataGridColumn>();

        foreach (var field in _table.Fields)
        {
            var column = new DataGridTextColumn
            {
                Header = $"{field.Name} ({field.Type})",
                Binding = new Binding($"[{field.Name}]")
            };
            columns.Add(column);
        }

        DataGrid.Columns.Clear();
        foreach (var column in columns)
        {
            DataGrid.Columns.Add(column);
        }

        foreach (var row in _table.Rows)
        {
            var rowData = new Dictionary<string, object>();
            foreach (var field in _table.Fields)
            {
                rowData[field.Name] = row.Values[field.Name];
            }

            rows.Add(rowData);
        }

        DataGrid.ItemsSource = rows;
    }

    private void btnAddRow_Click(object sender, RoutedEventArgs e)
    {
        var rowWindow = new RowWindow(_table);
        if (rowWindow.ShowDialog() == true)
        {
            _table.AddRow(rowWindow.Row);
            LoadData();
        }
    }

    private void btnEditRow_Click(object sender, RoutedEventArgs e)
    {
        if (DataGrid.SelectedItem != null)
        {
            var existingRowData = (Dictionary<string, object>)DataGrid.SelectedItem;
            var existingRowIndex = _table.Rows.FindIndex(r => r.Values.SequenceEqual(existingRowData));

            if (existingRowIndex != -1)
            {
                var existingRow = _table.Rows[existingRowIndex];
                var rowWindow = new RowWindow(_table, existingRow);

                if (rowWindow.ShowDialog() == true)
                {
                    _table.UpdateRow(existingRowIndex, rowWindow.Row);
                    LoadData();
                }
            }
        }
        else
        {
            MessageBox.Show("Please select row");
        }
    }

    private void btnDeleteRow_Click(object sender, RoutedEventArgs e)
    {
        if (DataGrid.SelectedItem != null)
        {
            if (DataGrid.SelectedItem != null)
            {
                int index = DataGrid.SelectedIndex;

                var result = MessageBox.Show("Are you sure?", "Approve", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    _table.DeleteRow(index);
                    LoadData();
                }
            }
            else
            {
                MessageBox.Show("Please, select row");
            }
        }
    }

    private void BtnRenameField_Click(object sender, RoutedEventArgs e)
    {
        var renameFieldWindow = new RenameFieldWindow(_table);
        if (renameFieldWindow.ShowDialog() == true)
        {
            LoadData(); 
        }
    }
}