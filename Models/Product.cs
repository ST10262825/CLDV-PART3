﻿using System;
using System.Collections.Generic;

namespace JamesCrafts.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public decimal? Price { get; set; }

    public string? Category { get; set; }

    public string? Availability { get; set; }

    public string? ImageUrl { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
