using DBMS_WEBAPI.Models;
using Newtonsoft.Json;

namespace DBMS_WEBAPI;

public class DbProcessor
{
    private Database database;
    private string filePath = "database.json";

    public DbProcessor()
    {
        LoadFromFile();
    }

    private void LoadFromFile()
    {
        if (File.Exists(filePath))
        {
            var jsonSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            };
            var json = File.ReadAllText(filePath);
            database = JsonConvert.DeserializeObject<Database>(json, jsonSettings);
        }
        else
        {
            database = new Database();
        }
    }

    public List<Table> GetAllTables()
    {
        return database.Tables;
    }

    public Table GetTable(string name)
    {
        return database.Tables.FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public void CreateTable(Table table)
    {
        if (database.Tables.Any(t => t.Name.Equals(table.Name, StringComparison.OrdinalIgnoreCase)))
            throw new Exception("Table with such name already exists.");

        database.Tables.Add(table);
        SaveToFile();
    }

    private void SaveToFile()
    {
        var jsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented
        };
        var json = JsonConvert.SerializeObject(database, jsonSettings);
        File.WriteAllText(filePath, json);
    }

    public void DeleteTable(string name)
    {
        var table = GetTable(name);
        if (table == null)
            throw new Exception("Table with such name does not exist.");

        database.Tables.Remove(table);
        SaveToFile();
    }

    public void AddRow(string tableName, Row row)
    {
        var table = GetTable(tableName);
        if (table == null)
            throw new Exception("Table with such name does not exist.");

        ValidateRow(table, row);

        table.Rows.Add(row);
        SaveToFile();
    }

    private void ValidateRow(Table table, Row row)
    {
        foreach (var field in table.Fields)
        {
            if (!row.Values.ContainsKey(field.Name))
                throw new Exception($"Field {field.Name} is absent in row");

            var value = row.Values[field.Name];

            if (!IsValidType(value, field.Type))
                throw new Exception($"Field value {field.Name} has invalid type.");
        }
    }

    private bool IsValidType(object value, DataType type)
    {
        try
        {
            switch (type)
            {
                case DataType.Integer:
                    Convert.ToInt32(value);
                    return true;
                case DataType.Real:
                    Convert.ToDouble(value);
                    return true;
                case DataType.Char:
                    return value is string s && s.Length == 1;
                case DataType.String:
                    return value is string;
                case DataType.Date:
                    DateTime.Parse(value.ToString());
                    return true;
                case DataType.DateInterval:
                    var dates = value.ToString().Split('-');
                    if (dates.Length != 2)
                        return false;
                    DateTime.Parse(dates[0].Trim());
                    DateTime.Parse(dates[1].Trim());
                    return true;
                default:
                    return false;
            }
        }
        catch
        {
            return false;
        }
    }

    public void UpdateRow(string tableName, int index, Row row)
    {
        var table = GetTable(tableName);
        if (table == null)
            throw new Exception("Table with such name does not exist.");

        if (index < 0 || index >= table.Rows.Count)
            throw new Exception("Row index is out of range.");

        ValidateRow(table, row);

        table.Rows[index] = row;
        SaveToFile();
    }

    public void DeleteRow(string tableName, int index)
    {
        var table = GetTable(tableName);
        if (table == null)
            throw new Exception("Table with such name does not exist.");

        if (index < 0 || index >= table.Rows.Count)
            throw new Exception("Row index is out of range.");

        table.Rows.RemoveAt(index);
        SaveToFile();
    }

    public void RenameField(string tableName, string oldFieldName, string newFieldName)
    {
        var table = GetTable(tableName);
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
        SaveToFile();
    }

    private bool RowValuesAreEqual(Dictionary<string, object> values1, Dictionary<string, object> values2,
        List<Field> fields)
    {
        foreach (var field in fields)
        {
            var key = field.Name;
            if (values1.ContainsKey(key) && values2.ContainsKey(key))
            {
                if (field.Type == DataType.DateInterval)
                {
                    var val1 = values1[key]?.ToString();
                    var val2 = values2[key]?.ToString();
                    if (!DateIntervalEqual(val1, val2))
                    {
                        return false;
                    }
                }
                else if (!values1[key].Equals(values2[key]))
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    private bool DateIntervalEqual(string interval1, string interval2)
    {
        if (string.IsNullOrEmpty(interval1) || string.IsNullOrEmpty(interval2))
            return false;

        var dates1 = interval1.Split('-').Select(d => d.Trim()).ToArray();
        var dates2 = interval2.Split('-').Select(d => d.Trim()).ToArray();

        if (dates1.Length != 2 || dates2.Length != 2)
            return false;

        return dates1[0] == dates2[0] && dates1[1] == dates2[1];
    }
}