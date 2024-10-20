using DBMS_WEBAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DBMS_WEBAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TablesController : Controller
{
    private readonly DbProcessor _dbProcessor;

    public TablesController(DbProcessor dbProcessor)
    {
        _dbProcessor = dbProcessor;
    }

    // GET: api/Tables
    [HttpGet]
    public ActionResult<List<Table>> GetTables()
    {
        return _dbProcessor.GetAllTables();
    }

    // GET: api/Tables/{tableName}
    [HttpGet("{tableName}")]
    public ActionResult<Table> GetTable(string tableName)
    {
        var table = _dbProcessor.GetTable(tableName);
        if (table == null)
            return NotFound();

        return table;
    }

    // POST: api/Tables
    [HttpPost]
    public ActionResult CreateTable([FromBody] Table table)
    {
        try
        {
            _dbProcessor.CreateTable(table);
            return Ok();
        }
        catch (System.Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // DELETE: api/Tables/{tableName}
    [HttpDelete("{tableName}")]
    public ActionResult DeleteTable(string tableName)
    {
        try
        {
            _dbProcessor.DeleteTable(tableName);
            return Ok();
        }
        catch (System.Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    // GET: api/Tables/{tableName}/Rows
    [HttpGet("{tableName}/Rows")]
    public ActionResult<List<Row>> GetRows(string tableName)
    {
        var table = _dbProcessor.GetTable(tableName);
        if (table == null)
            return NotFound();

        return table.Rows;
    }

    // POST: api/Tables/{tableName}/Rows
    [HttpPost("{tableName}/Rows")]
    public ActionResult AddRow(string tableName, [FromBody] Row row)
    {
        try
        {
            _dbProcessor.AddRow(tableName, row);
            return Ok();
        }
        catch (System.Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PUT: api/Tables/{tableName}/Rows/{index}
    [HttpPut("{tableName}/Rows/{index}")]
    public ActionResult UpdateRow(string tableName, int index, [FromBody] Row row)
    {
        try
        {
            _dbProcessor.UpdateRow(tableName, index, row);
            return Ok();
        }
        catch (System.Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    // DELETE: api/Tables/{tableName}/Rows/{index}
    [HttpDelete("{tableName}/Rows/{index}")]
    public ActionResult DeleteRow(string tableName, int index)
    {
        try
        {
            _dbProcessor.DeleteRow(tableName, index);
            return Ok();
        }
        catch (System.Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    // POST: api/Tables/Rename
    [HttpPost("Rename")]
    public ActionResult<Table> RenameColumns([FromBody] RenameRequest request)
    {
        try
        {
            _dbProcessor.RenameField(request.tableName, request.oldFieldName, request.newFieldName);
            return Ok();
        }
        catch (System.Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    public class RenameRequest
    {
        public string tableName { get; set; }
        public string oldFieldName { get; set; }
        public string newFieldName { get; set; }
    }
}