using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DBMS.database;

public class Database
{
    public List<Table> Tables { get; set; }

    public Database()
    {
        Tables = new List<Table>();
    }

    public void AddTable(Table table)
    {
        if (Tables.Exists(t => t.Name == table.Name))
            throw new Exception($"Table with name '{table.Name}' already exists");
        Tables.Add(table);
    }

    public void DeleteTable(string name)
    {
        var table = GetTable(name);
        if (table == null)
            throw new Exception($"Table '{name}' wasn't found");
        Tables.Remove(table);
    }

    public Table GetTable(string name)
    {
        return Tables.Find(t => t.Name == name);
    }

    public void SaveToFile(string filePath)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(this, options);
        File.WriteAllText(filePath, json);
    }

    public static Database LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            return new Database();

        var json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<Database>(json);
    }
    // todo: replace with Rename column in table method
    


    private bool CanConvertType(DataType type1, DataType type2)
    {
        if ((type1 == DataType.String && type2 == DataType.Time) ||
            (type1 == DataType.Time && type2 == DataType.String))
            return true;

        return false; 
    }

    private bool AreRowsEqual(Row row1, Row row2, List<Field> fields)
    {
        foreach (var field in fields)
        {
            if (!row1.Values.ContainsKey(field.Name) || !row2.Values.ContainsKey(field.Name))
                return false;

            var value1 = row1.Values[field.Name];
            var value2 = row2.Values[field.Name];

            if (value1 == null || value2 == null)
            {
                if (value1 != value2) 
                    return false;
                continue; 
            }

            if (field.Type == DataType.Time)
            {
                if (!(value1 is DateTime dt1) || !(value2 is DateTime dt2))
                    return false;
                if (dt1.Date != dt2.Date)
                    return false;
            }
            else if (field.Type == DataType.TimeInterval)
            {
                var intervalParts1 = value1.ToString().Split(new string[] { " - " }, StringSplitOptions.None);
                var intervalParts2 = value2.ToString().Split(new string[] { " - " }, StringSplitOptions.None);

                if (intervalParts1.Length != 2 || intervalParts2.Length != 2)
                    return false;

                if (!DateTime.TryParse(intervalParts1[0].Trim(), out DateTime start1) ||
                    !DateTime.TryParse(intervalParts1[1].Trim(), out DateTime end1) ||
                    !DateTime.TryParse(intervalParts2[0].Trim(), out DateTime start2) ||
                    !DateTime.TryParse(intervalParts2[1].Trim(), out DateTime end2))
                    return false;

                if (start1.Date != start2.Date || end1.Date != end2.Date)
                    return false;
            }
            else
            {
                if (!value1.Equals(value2))
                    return false;
            }
        }

        return true;
    }


    private bool AreTableStructuresEqual(Table table1, Table table2)
    {
        if (table1.Fields.Count != table2.Fields.Count)
            return false;

        for (int i = 0; i < table1.Fields.Count; i++)
        {
            if (table1.Fields[i].Name != table2.Fields[i].Name ||
                table1.Fields[i].Type != table2.Fields[i].Type)
                return false;
        }

        return true;
    }

    private bool RowsAreEqual(Row row1, Row row2)
    {
        foreach (var field in row1.Values.Keys)
        {
            if (!row2.Values.ContainsKey(field) || !row2.Values[field].Equals(row1.Values[field]))
                return false;
        }

        return true;
    }
}