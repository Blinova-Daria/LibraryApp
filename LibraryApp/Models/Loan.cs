using System;
using System.Collections.Generic;

namespace LibraryApp.Models;

public partial class Loan
{
    public int LoanId { get; set; }

    public int UserId { get; set; }

    public int CopyId { get; set; }

    public int StatusId { get; set; }

    public DateOnly BookingDate { get; set; }

    public DateOnly DueDate { get; set; }

    public virtual BookCopy Copy { get; set; } = null!;

    public virtual LoanStatus Status { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
