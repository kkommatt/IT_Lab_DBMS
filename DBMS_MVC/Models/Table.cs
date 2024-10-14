using System.ComponentModel.DataAnnotations;

namespace DBMS_MVC.Models;

public class Table
{
    [Required]
    public string Name { get; set; }
    public List<Field> Fields { get; set; } = new List<Field>();
    public List<Row> Rows { get; set; } = new List<Row>();
}