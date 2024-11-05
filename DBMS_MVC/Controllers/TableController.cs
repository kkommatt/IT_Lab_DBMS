using DBMS_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DBMS_MVC.Controllers;

public class TableController : Controller
{
    private readonly DbProcessor _dbProcessor;

        public TableController(DbProcessor dbProcessor)
        {
            _dbProcessor = dbProcessor;
        }

        public IActionResult Index()
        {
            var tables = _dbProcessor.GetTables();
            return View(tables);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Table table)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _dbProcessor.CreateTable(table.Name, table.Fields);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(table);
        }

        public IActionResult Details(string name)
        {
            var table = _dbProcessor.GetTableByName(name);
            if (table == null)
                return NotFound();

            return View(table);
        }

        public IActionResult AddRow(string name)
        {
            var table = _dbProcessor.GetTableByName(name);
            if (table == null)
                return NotFound();

            ViewBag.TableName = name;
            return View(table);
        }

        [HttpPost]
        public IActionResult AddRow(string name, IFormCollection form)
        {
            var table = _dbProcessor.GetTableByName(name);
            if (table == null)
                return NotFound();

            try
            {
                var values = new Dictionary<string, string>();

                foreach (var field in table.Fields)
                {
                    if (field.Type == DataType.DateInterval)
                    {
                        // Retrieve start and end dates
                        var startKey = $"values[{field.Name}]_Start";
                        var endKey = $"values[{field.Name}]_End";

                        var startValue = form[startKey];
                        var endValue = form[endKey];

                        if (string.IsNullOrWhiteSpace(startValue) || string.IsNullOrWhiteSpace(endValue))
                        {
                            throw new Exception($"Значення для поля '{field.Name}' не задано.");
                        }

                        values[field.Name] = $"{startValue} - {endValue}";
                    }
                    else
                    {
                        var key = $"values[{field.Name}]";
                        var value = form[key];

                        if (string.IsNullOrWhiteSpace(value))
                        {
                            throw new Exception($"Значення для поля '{field.Name}' не задано.");
                        }

                        values[field.Name] = value;
                    }
                }

                _dbProcessor.AddRowToTable(name, values);
                return RedirectToAction("Details", new { name = name });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.TableName = name;
                return View(table);
            }
        }


        public IActionResult RenameField(string name)
        {
            var table = _dbProcessor.GetTableByName(name);
            if (table == null)
            {
                return NotFound("Table not found");
            }
            ViewBag.Fields = table.Fields.Select(f => new SelectListItem
            {
                Text = f.Name,
                Value = f.Name
            }).ToList();
            
            return View(table);
        }

        [HttpPost]
        public IActionResult RenameField(string name, string oldFieldName, string newFieldName)
        {
            try
            {
                var table = _dbProcessor.GetTableByName(name);
                if (table == null)
                {
                    return NotFound("Table not found");
                }

                if (table.Fields.Any(f => f.Name.Equals(newFieldName, StringComparison.OrdinalIgnoreCase)))
                {
                    ModelState.AddModelError("", $"The field name '{newFieldName}' already exists.");
                    ViewBag.Fields = table.Fields.Select(f => new SelectListItem
                    {
                        Text = f.Name,
                        Value = f.Name
                    }).ToList();
                    return View(table);
                }
                _dbProcessor.RenameField(name, oldFieldName, newFieldName);
                return RedirectToAction("Details", new { name = name });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var table = _dbProcessor.GetTableByName(name);
                if (table == null)
                {
                    return NotFound("Table not found");
                }
                return View(table);
            }
        }


        public IActionResult EditRow(string name, int index)
        {
            var table = _dbProcessor.GetTableByName(name);
            if (table == null || index < 0 || index >= table.Rows.Count)
                return NotFound();

            var row = table.Rows[index];
            ViewBag.RowIndex = index;
            return View(new Tuple<Table, Row>(table, row));
        }

        [HttpPost]
        public IActionResult EditRow(string name, int index, IFormCollection form)
        {
            var table = _dbProcessor.GetTableByName(name);
            if (table == null || index < 0 || index >= table.Rows.Count)
                return NotFound();

            try
            {
                var values = new Dictionary<string, string>();

                foreach (var field in table.Fields)
                {
                    if (field.Type == DataType.DateInterval)
                    {
                        var startKey = $"values[{field.Name}]_Start";
                        var endKey = $"values[{field.Name}]_End";

                        var startValue = form[startKey];
                        var endValue = form[endKey];

                        if (string.IsNullOrWhiteSpace(startValue) || string.IsNullOrWhiteSpace(endValue))
                        {
                            throw new Exception($"Value for '{field.Name}' wasn't provided");
                        }

                        values[field.Name] = $"{startValue} - {endValue}";
                    }
                    else
                    {
                        var key = $"values[{field.Name}]";
                        var value = form[key];

                        if (string.IsNullOrWhiteSpace(value))
                        {
                            throw new Exception($"Value for field '{field.Name}' wasn't provided");
                        }

                        values[field.Name] = value;
                    }
                }

                var row = new Row { Values = new Dictionary<string, object>() };
                foreach (var field in table.Fields)
                {
                    var value = _dbProcessor.ConvertValue(values[field.Name], field.Type);
                    row.Values[field.Name] = value;
                }

                table.Rows[index] = row;
                return RedirectToAction("Details", new { name = name });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.RowIndex = index;
                return View(new Tuple<Table, Row>(table, table.Rows[index]));
            }
        }


        public IActionResult DeleteRow(string name, int index)
        {
            var table = _dbProcessor.GetTableByName(name);
            if (table == null || index < 0 || index >= table.Rows.Count)
                return NotFound();

            return View(new Tuple<Table, int>(table, index));
        }

        [HttpPost, ActionName("DeleteRow")]
        public IActionResult DeleteRowConfirmed(string name, int index)
        {
            var table = _dbProcessor.GetTableByName(name);
            if (table == null || index < 0 || index >= table.Rows.Count)
                return NotFound();

            table.Rows.RemoveAt(index);
            return RedirectToAction("Details", new { name = name });
        }

        public IActionResult Delete(string name)
        {
            var table = _dbProcessor.GetTableByName(name);
            if (table == null)
                return NotFound();

            return View(table);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(string name)
        {
            try
            {
                _dbProcessor.DeleteTable(name);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var table = _dbProcessor.GetTableByName(name);
                return View(table);
            }
        }
}