using DBMS_MVC.Models;
using Newtonsoft.Json;

namespace DBMS_MVC;

public class DbProcessor : IDisposable
{
    private readonly ILogger<DbProcessor> _logger;
    private Database _database;

    public DbProcessor(ILogger<DbProcessor> logger)
    {
        _logger = logger;
        _database = new Database();

        try
        {
            LoadFromFile("database.json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while loading database");
        }
    }

    public List<Table> GetTables()
    {
        return _database.Tables;
    }

    public Table GetTableByName(string name)
    {
        return _database.Tables.FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public void CreateTable(string name, List<Field> fields)
    {
        if (_database.Tables.Any(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            throw new Exception("Table with such name already exists");

        var table = new Table { Name = name, Fields = fields };
        _database.Tables.Add(table);
    }

    public void AddRowToTable(string tableName, Dictionary<string, string> values)
    {
        var table = GetTableByName(tableName);
        if (table == null)
            throw new Exception("Table was not found");

        var row = new Row { Values = new Dictionary<string, object>() };

        foreach (var field in table.Fields)
        {
            if (!values.TryGetValue(field.Name, out string valueStr))
                throw new Exception($"Value for field '{field.Name}' wasn't set");

            var value = ConvertValue(valueStr, field.Type);
            row.Values[field.Name] = value;
        }

        table.Rows.Add(row);
    }


    public object ConvertValue(string valueStr, DataType type)
    {
        try
        {
            switch (type)
            {
                case DataType.Integer:
                    return int.Parse(valueStr);
                case DataType.Real:
                    return double.Parse(valueStr);
                case DataType.Char:
                    if (valueStr.Length != 1)
                        throw new Exception("Char value has an invalid length");
                    return valueStr[0];
                case DataType.String:
                    return valueStr;
                case DataType.Date:
                    return DateTime.ParseExact(valueStr, "yyyy-MM-dd", null).Date;
                case DataType.DateInterval:
                    var dates = valueStr.Split(new string[] { " - " }, StringSplitOptions.None);
                    if (dates.Length != 2)
                        throw new Exception(
                            $"Invalid data format, use 'yyyy-MM-dd - yyyy-MM-dd'");

                    var startDate = DateTime.ParseExact(dates[0].Trim(), "yyyy-MM-dd", null).Date;
                    var endDate = DateTime.ParseExact(dates[1].Trim(), "yyyy-MM-dd", null).Date;
                    if (startDate > endDate)
                        throw new Exception("Start date cannot be greater than end date");

                    return $"{startDate:yyyy-MM-dd} - {endDate:yyyy-MM-dd}";
                default:
                    throw new Exception($"Wrong");
            }
        }
        catch (FormatException fe)
        {
            throw new Exception($"Invalid value format: {fe.Message}");
        }
        catch (Exception ex)
        {
            throw new Exception($"Wrong: {ex.Message}");
        }
    }
    public void DeleteTable(string name)
    {
        var table = GetTableByName(name);
        if (table == null)
            throw new Exception("Table was not found");

        _database.Tables.Remove(table);
    }

    public void SaveToFile(string filePath)
    {
        var jsonSettings = new JsonSerializerSettings
        {
            DateFormatString = "yyyy-MM-dd" 
        };
        var json = JsonConvert.SerializeObject(_database, Formatting.Indented, jsonSettings);
        File.WriteAllText(filePath, json);
    }

    public void LoadFromFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            var json = File.ReadAllText(filePath);
            _database = JsonConvert.DeserializeObject<Database>(json);
        }
        else
        {
            throw new Exception("The specified file does not exist.");
        }
    }
    
    public void RenameField(string tableName, string oldFieldName, string newFieldName) 
    {
        var table = GetTableByName(tableName);
        if (table == null)
        {
            throw new Exception("Table was not found");
        }

        var field = table.Fields.FirstOrDefault(f => f.Name.Equals(oldFieldName, StringComparison.OrdinalIgnoreCase));
        if (field == null)
        {
            throw new Exception($"Field '{oldFieldName}' was not found in table '{tableName}'");
        }

        if (table.Fields.Any(f => f.Name.Equals(newFieldName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new Exception($"Field with name '{newFieldName}' already exists in table '{tableName}'");
        }

        field.Name = newFieldName;

        foreach (var row in table.Rows)
        {
            if (row.Values.TryGetValue(oldFieldName, out var value))
            {
                row.Values.Remove(oldFieldName);
                row.Values[newFieldName] = value;
            }
        }
    }


    public void Dispose(string filePath)
    {
        try
        {
            SaveToFile(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while saving database");
        }
    }

    public void Dispose()
    {
        
    }
}
    
