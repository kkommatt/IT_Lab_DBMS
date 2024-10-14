namespace DBMS.database;
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public class Row
{
    public Dictionary<string, object> Values { get; set; }

    public Row()
    {
        Values = new Dictionary<string, object>();
    }
    public Row(Dictionary<string, object> values)
    {
        Values = values;
    }
    public override bool Equals(object obj)
    {
        if (obj is Row other)
        {
            if (Values.Count != other.Values.Count)
                return false;

            return Values.All(kvp => 
                other.Values.TryGetValue(kvp.Key, out var value) && 
                Equals(kvp.Value, value)); 
        }
        return false;
    }
}