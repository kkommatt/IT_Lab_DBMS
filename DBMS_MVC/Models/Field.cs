using System.ComponentModel.DataAnnotations;

namespace DBMS_MVC.Models;

public enum DataType
{
    Integer,
    Real,
    Char,
    String,
    Date,
    DateInterval,
}

public class Field
{
    [Required] 
    public string Name { get; set; }
    [Required]
    public DataType Type { get; set; }
}