using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ColorAttribute : Attribute
{
    public string Color { get; set; }
    public ColorAttribute(string Color)
    {
        this.Color = Color;
    }
}