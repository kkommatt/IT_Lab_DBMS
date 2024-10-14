using System;
using System.Linq;
using System.Windows;
using DBMS.database;

namespace DBMS;

public partial class RenameFieldWindow : Window
{
    private Table _table;
    
    public RenameFieldWindow(Table table)
    {
        InitializeComponent();
        _table = table;
        LoadFields();
    }
    
    private void LoadFields()
    {
        ComboBoxFields.ItemsSource = _table.Fields.Select(f => f.Name).ToList();
    }
    
    private void BtnRename_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var oldName = ComboBoxFields.SelectedItem?.ToString();
            var newName = TxtNewFieldName.Text.Trim();

            if (string.IsNullOrEmpty(oldName) || string.IsNullOrEmpty(newName))
            {
                MessageBox.Show("Please select a field and enter a new name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _table.RenameColumn(oldName, newName);

            MessageBox.Show("Field renamed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            this.DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}