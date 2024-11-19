using System;
using System.Collections.Generic;

namespace ApiProject.Models;

public partial class Address
{
    public int Aid { get; set; }

    public int? Eid { get; set; }

    public string? Street { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? PinCode { get; set; }

    public virtual Employee? EidNavigation { get; set; }
}
