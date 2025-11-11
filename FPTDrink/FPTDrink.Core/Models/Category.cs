using System;
using System.Collections.Generic;

namespace FPTDrink.Core.Models;

public partial class Category
{
    public int Id { get; set; }

    public string Alias { get; set; } = null!;

    public string? Link { get; set; }

    public string? SeoTitle { get; set; }

    public string? SeoDescription { get; set; }

    public string? SeoKeywords { get; set; }

    public bool IsActive { get; set; }

    public int? Position { get; set; }

    public int Status { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? Modifiedby { get; set; }

    public string Title { get; set; } = null!;

    public virtual ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
}
