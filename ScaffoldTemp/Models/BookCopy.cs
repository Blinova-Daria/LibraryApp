using System;
using System.Collections.Generic;

namespace LibraryApp.Models;

public partial class BookCopy
{
    public int CopyId { get; set; }

    public int BookId { get; set; }

    public string InventoryNumber { get; set; } = null!;

    public DateOnly DateReceived { get; set; }

    public virtual Book Book { get; set; } = null!;

    public virtual Loan? Loan { get; set; }
}
