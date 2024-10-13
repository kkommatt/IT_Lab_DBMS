using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBMS.database;

public class Table
    {
        public string Name { get; set; }
        public List<Field> Fields { get; set; }
        public List<Row> Rows { get; set; }

        public Table(string name)
        {
            Name = name;
            Fields = new List<Field>();
            Rows = new List<Row>();
        }

        public void AddField(Field field)
        {
            if (Fields.Exists(f => f.Name == field.Name))
                throw new Exception($"Field with name '{field.Name}' already exists");
            Fields.Add(field);
        }

        public void AddRow(Row row)
        {
            ValidateRow(row);
            Rows.Add(row);
        }

        public void UpdateRow(int index, Row row)
        {
            ValidateRow(row);
            Rows[index] = row;
        }

        public void DeleteRow(int index)
        {
            Rows.RemoveAt(index);
        }

        public void ValidateRow(Row row)
        {
            foreach (var field in Fields)
            {
                if (!row.Values.ContainsKey(field.Name))
                    throw new Exception($"Field '{field.Name}' is not present in table");

                var value = row.Values[field.Name];
                if (!IsValidType(value, field.Type))
                    throw new Exception($"Value of field '{field.Name}' has invalid type");
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
                    case DataType.Time:
                        DateTime.ParseExact(value.ToString(), "HH:mm", null, System.Globalization.DateTimeStyles.None);
                        return true;
                    case DataType.TimeInterval:
                        // Split on " - " to separate start and end dates
                        var dates = value.ToString().Split(new string[] { " - " }, StringSplitOptions.None);
                        if (dates.Length != 2)
                            return false;

                        // Parse start date
                        if (!DateTime.TryParseExact(dates[0].Trim(), "HH:mm", null, System.Globalization.DateTimeStyles.None, out DateTime startDate))
                            return false;
                        // Parse end date
                        if (!DateTime.TryParseExact(dates[1].Trim(), "HH:mm", null, System.Globalization.DateTimeStyles.None, out DateTime endDate))
                            return false;

                        // Ensure start date is earlier than or equal to end date
                        return startDate <= endDate;
                    default:
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }
        
        //todo: check this carefully Method to rename a column
        public void RenameColumn(string oldName, string newName)
        {
            // Check if the old column exists
            var field = Fields.Find(f => f.Name == oldName);
            if (field == null)
                throw new Exception($"Column '{oldName}' does not exist.");

            // Check if the new column name already exists
            if (Fields.Exists(f => f.Name == newName))
                throw new Exception($"Column '{newName}' already exists.");

            // Rename the field
            field.Name = newName;

            // Update all rows to reflect the change in the column name
            foreach (var row in Rows)
            {
                if (row.Values.ContainsKey(oldName))
                {
                    var value = row.Values[oldName];
                    row.Values.Remove(oldName);
                    row.Values[newName] = value;
                }
            }
        }

    }