using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DBMS.database;

namespace DBMS;

public partial class RowWindow : Window
{
    private Table _table;
    public Row Row { get; private set; }

    public RowWindow(Table table, Row existingRow = null)
    {
        InitializeComponent();
        _table = table;
        Row = existingRow ?? new Row();
        CreateControls();
    }

    private void CreateControls()
    {
        PanelInputs.Children.Clear();

        foreach (var field in _table.Fields)
        {
            // Create Label
            var label = new TextBlock
            {
                Text = $"{field.Name} ({field.Type})",
                Margin = new Thickness(10, 5, 0, 0) // Adjust margin for better spacing
            };
            PanelInputs.Children.Add(label);

            FrameworkElement inputControl = null; 

            // Create appropriate input control
            switch (field.Type)
            {
                case DataType.Integer:
                case DataType.Real:
                case DataType.String:
                    inputControl = new TextBox
                    {
                        Name = $"input_{field.Name}",
                        Width = 200,
                        Margin = new Thickness(10, 0, 0, 5) 
                    };
                    break;
                case DataType.Char:
                    inputControl = new TextBox
                    {
                        Name = $"input_{field.Name}",
                        Width = 200,
                        MaxLength = 1,
                        Margin = new Thickness(10, 0, 0, 5)
                    };
                    break;
                case DataType.Date:
                    inputControl = new DatePicker
                    {
                        Name = $"input_{field.Name}",
                        Width = 200,
                        Margin = new Thickness(10, 0, 0, 5)
                    };
                    break;
                case DataType.DateInterval:
                    var startDateLabel = new TextBlock
                    {
                        Text = $"{field.Name} Початок:",
                        Margin = new Thickness(10, 0, 0, 5)
                    };
                    PanelInputs.Children.Add(startDateLabel);

                    var startDatePicker = new DatePicker
                    {
                        Name = $"input_{field.Name}_Start",
                        Width = 200,
                        Margin = new Thickness(180, 0, 0, 0)
                    };
                    PanelInputs.Children.Add(startDatePicker);

                    var endDateLabel = new TextBlock
                    {
                        Text = $"{field.Name} Кінець:",
                        Margin = new Thickness(10, 0, 0, 0) 
                    };
                    PanelInputs.Children.Add(endDateLabel);

                    var endDatePicker = new DatePicker
                    {
                        Name = $"input_{field.Name}_End",
                        Width = 200,
                        Margin = new Thickness(180, 0, 0, 0)
                    };
                    PanelInputs.Children.Add(endDatePicker);

                    // If there is an existing value
                    if (Row.Values.ContainsKey(field.Name))
                    {
                        var interval = Row.Values[field.Name].ToString()
                            .Split(new[] { " - " }, StringSplitOptions.None);
                        if (interval.Length == 2)
                        {
                            if (DateTime.TryParse(interval[0], out DateTime startDate))
                                startDatePicker.SelectedDate = startDate; 
                            if (DateTime.TryParse(interval[1], out DateTime endDate))
                                endDatePicker.SelectedDate = endDate; 
                        }
                    }

                    break;
                default:
                    inputControl = new TextBox
                    {
                        Name = $"input_{field.Name}",
                        Width = 200,
                        Margin = new Thickness(10, 0, 0, 5)
                    };
                    break;
            }

            // Set the value of the inputControl if it exists
            if (inputControl != null && Row.Values.ContainsKey(field.Name))
            {
                if (inputControl is DatePicker datePicker)
                {
                    datePicker.SelectedDate = DateTime.Parse(Row.Values[field.Name].ToString());
                }
                else
                {
                    (inputControl as TextBox).Text = Row.Values[field.Name].ToString();
                }
            }

            // Add the control to the panel only if it's not null
            if (inputControl != null)
            {
                PanelInputs.Children.Add(inputControl);
            }
        }
    }

    private void btnSave_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            foreach (var field in _table.Fields)
            {
                object value = null;

                switch (field.Type)
                {
                    case DataType.Integer:
                        var intControl = FindControl<TextBox>($"input_{field.Name}");
                        if (intControl == null)
                            throw new Exception($"Field '{field.Name}' wasn't find");
                        value = int.Parse(intControl.Text);
                        break;

                    case DataType.Real:
                        var realControl = FindControl<TextBox>($"input_{field.Name}");
                        if (realControl == null)
                            throw new Exception($"Field '{field.Name}' wasn't find");
                        value = double.Parse(realControl.Text);
                        break;

                    case DataType.Char:
                        var charControl = FindControl<TextBox>($"input_{field.Name}");
                        if (charControl == null)
                            throw new Exception($"Field '{field.Name}' wasn't find");
                        if (charControl.Text.Length != 1)
                            throw new Exception($"Field '{field.Name}' has to contain one character");
                        value = charControl.Text;
                        break;

                    case DataType.String:
                        var stringControl = FindControl<TextBox>($"input_{field.Name}");
                        if (stringControl == null)
                            throw new Exception($"Field '{field.Name}' wasn't find");
                        value = stringControl.Text;
                        break;

                    case DataType.Date:
                        var dateControl = FindControl<DatePicker>($"input_{field.Name}");
                        if (dateControl == null)
                            throw new Exception($"Field '{field.Name}' wasn't find");
                        value = dateControl.SelectedDate?.ToString("yyyy-MM-dd");
                        break;

                    case DataType.DateInterval:
                        var startControl = FindControl<DatePicker>($"input_{field.Name}_Start");
                        var endControl = FindControl<DatePicker>($"input_{field.Name}_End");

                        if (startControl == null || endControl == null)
                            throw new Exception($"Field for '{field.Name}' wasn't find");

                        if (!startControl.SelectedDate.HasValue || !endControl.SelectedDate.HasValue)
                            throw new Exception($"Enter date for field '{field.Name}'.");

                        DateTime startDate = startControl.SelectedDate.Value;
                        DateTime endDate = endControl.SelectedDate.Value;

                        if (startDate > endDate)
                            throw new Exception(
                                $"Start date cannot be greater than end for '{field.Name}'.");

                        value = $"{startDate:yyyy-MM-dd} - {endDate:yyyy-MM-dd}";
                        break;
                }

                Row.Values[field.Name] = value;
            }

            _table.ValidateRow(Row);
            this.DialogResult = true;
            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}");
        }
    }

    private T FindControl<T>(string name) where T : FrameworkElement
    {
        return PanelInputs.Children.OfType<T>().FirstOrDefault(c => c.Name == name);
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}