using System;
using System.Collections.Generic;

namespace ApiProject.Models;

public partial class Employee
{
    public int Eid { get; set; }

    public string? Name { get; set; }

    public string? Position { get; set; }

    public decimal? Salary { get; set; }

    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
}
