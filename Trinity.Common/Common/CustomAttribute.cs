using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CustomAttribute : Attribute
{
    public string Color { get; set; }
    public string Name { get; set; }
}