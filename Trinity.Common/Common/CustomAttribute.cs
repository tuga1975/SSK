using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CustomAttribute : Attribute
{
    public CustomAttribute() { }
    public CustomAttribute(string Color, string Name)
    {
        this.Color = Color;
        this.Name = Name;
    }
    public string Color { get; set; }
    public string Name { get; set; }
    public string IgnoreParameter { get; set; }
    public string AlowParameter { get; set; }
}