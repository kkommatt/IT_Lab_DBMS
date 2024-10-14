using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using DBMS_MVC.Models;

namespace DBMS_MVC.Controllers;

public class HomeController : Controller
{
    private readonly DbProcessor _dbProcessor;

    public HomeController(DbProcessor dbProcessor)
    {
        _dbProcessor = dbProcessor;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public IActionResult LoadDatabase(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            ModelState.AddModelError("", "Please select a valid JSON file.");
            return View("Index");
        }

        var filePath = Path.Combine(Path.GetTempPath(), file.FileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            file.CopyTo(stream);
        }

        try
        {
            _dbProcessor.LoadFromFile(filePath);
            return RedirectToAction("Index", "Table");
            
        }
        catch (Exception ex)
        {
            return Content($"Error while loading database: {ex.Message}");
        }
    }

    [HttpPost]
    public IActionResult CreateDatabase(string fileName, IFormFile file)
    {
        if (string.IsNullOrEmpty(fileName) || file == null || file.Length == 0)
        {
            ModelState.AddModelError("", "File name and a valid file must be provided.");
            return View("Index");
        }

        var folderPath = Path.GetDirectoryName(file.FileName);
        var fullFilePath = Path.Combine(folderPath, $"{fileName}.json");
    
        try
        {
            _dbProcessor.SaveToFile(fullFilePath);
            _dbProcessor.LoadFromFile(fullFilePath);
            return RedirectToAction("Index", "Table");
        }
        catch (Exception ex)
        {
            return Content($"Error while creating database: {ex.Message}");
        }
    }

    public IActionResult SaveDatabase(string filePath)
    {
        try
        {
            _dbProcessor.SaveToFile(filePath);
            return RedirectToAction("Index", "Table");
        }
        catch (Exception ex)
        {
            return Content($"Error while saving db: {ex.Message}");
        }
    }
}