using System;
using System.Collections.Generic;
using System.Windows;
using DBMS.database;

namespace DBMS;

public partial class CreateTableWindow : Window
{
    private Database _database;
    private List<Field> _fields = new List<Field>();

    public CreateTableWindow(Database database)
    {
        InitializeComponent();
        _database = database;

        CmbDataType.ItemsSource = Enum.GetValues(typeof(DataType));

        LblFieldNameHint.Content = "Enter name and press add field";
    }

    private void btnAddField_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtFieldName.Text))
        {
            MessageBox.Show("Enter field name");
            return;
        }

        var fieldName = TxtFieldName.Text;
        var dataType = (DataType)CmbDataType.SelectedItem;
        var field = new Field(fieldName, dataType);
        _fields.Add(field);

        ListBoxFields.Items.Add($"{field.Name} ({field.Type})");
        LblFieldsCount.Content = $"Total count of fields: {_fields.Count}";

        TxtFieldName.Clear();
        CmbDataType.SelectedIndex = 0;
    }

    private void btnRemoveField_Click(object sender, RoutedEventArgs e)
    {
        if (ListBoxFields.SelectedIndex >= 0)
        {
            _fields.RemoveAt(ListBoxFields.SelectedIndex);
            ListBoxFields.Items.RemoveAt(ListBoxFields.SelectedIndex);
            LblFieldsCount.Content = $"Total count of fields: {_fields.Count}";
        }
        else
        {
            MessageBox.Show("Choose field to remove");
        }
    }

    private void btnEditField_Click(object sender, RoutedEventArgs e)
    {
        if (ListBoxFields.SelectedIndex >= 0)
        {
            var selectedField = _fields[ListBoxFields.SelectedIndex];
            TxtFieldName.Text = selectedField.Name;
            CmbDataType.SelectedItem = selectedField.Type;
        }
        else
        {
            MessageBox.Show("Select field to edit");
        }
    }

    private void btnCreateTable_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtTableName.Text))
        {
            MessageBox.Show("Enter table name");
            return;
        }

        if (_fields.Count == 0)
        {
            MessageBox.Show("Add more fields");
            return;
        }

        var table = new Table(TxtTableName.Text);
        foreach (var field in _fields)
        {
            table.AddField(field);
        }

        try
        {
            _database.AddTable(table);
            this.DialogResult = true;
            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }
}