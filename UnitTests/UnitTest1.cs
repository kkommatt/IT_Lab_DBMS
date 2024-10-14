using DBMS.database;

namespace UnitTests;

using NUnit.Framework;
using System;
using System.Collections.Generic;

[TestFixture]
public class DatabaseTests
{
    [Test]
    public void TestCreateAndDeleteTable()
    {
        var database = new Database();
        var table = new Table("TestTable");
        database.AddTable(table);

        Assert.AreEqual(1, database.Tables.Count);
        Assert.AreEqual("TestTable", database.Tables[0].Name);

        database.DeleteTable("TestTable");
        Assert.AreEqual(0, database.Tables.Count);
    }

    [Test]
    public void TestAddRowWithValidation()
    {
        var table = new Table("TestTable");
        table.AddField(new Field("Id", DataType.Integer));
        table.AddField(new Field("Name", DataType.String));

        var row = new Row();
        row.Values["Id"] = 1;
        row.Values["Name"] = "John Doe";
        table.AddRow(row);

        Assert.AreEqual(1, table.Rows.Count);

        // Testing validation with incorrect data type
        var invalidRow = new Row();
        invalidRow.Values["Id"] = "NotAnInteger";
        invalidRow.Values["Name"] = "Jane Doe";

        var ex = Assert.Throws<Exception>(() => table.AddRow(invalidRow));
        Assert.That(ex.Message, Is.EqualTo("Value of field 'Id' has invalid type"));
    }

    [Test]
    public void TestTimeTypeField()
    {
        var table = new Table("TimeTable");
        table.AddField(new Field("EventTime", DataType.Date)); 

        var row = new Row();
        row.Values["EventTime"] = "2024-10-10"; 
        table.AddRow(row);

        Assert.AreEqual(1, table.Rows.Count);
        Assert.AreEqual("2024-10-10", row.Values["EventTime"].ToString());
    }

    [Test]
    public void TestTimeIntervalTypeField()
    {
        var table = new Table("IntervalTable");
        table.AddField(new Field("TimeRange", DataType.DateInterval)); 

        var row = new Row();
        row.Values["TimeRange"] = "2024-10-10 - 2024-11-11"; 
        table.AddRow(row);

        Assert.AreEqual(1, table.Rows.Count);
        Assert.AreEqual("2024-10-10 - 2024-11-11", row.Values["TimeRange"].ToString());
    }
    
    [Test]
    public void TestRenameColumn_Success()
    {
        // Arrange
        var table = new Table("TestTable");
        table.AddField(new Field("OldName", DataType.String));
        var row = new Row();
        row.Values["OldName"] = "TestValue";
        table.AddRow(row);

        // Act
        table.RenameColumn("OldName", "NewName");

        // Assert
        Assert.AreEqual(1, table.Fields.Count);
        Assert.AreEqual("NewName", table.Fields[0].Name);
        Assert.IsTrue(row.Values.ContainsKey("NewName"));
        Assert.AreEqual("TestValue", row.Values["NewName"]);
    }
}