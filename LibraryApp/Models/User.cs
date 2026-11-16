using System;
using System.Collections.Generic;

namespace LibraryApp.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Login { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int RoleId { get; set; }

    public string LastName { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string MiddleName { get; set; } = null!;

    public string TicketNumber { get; set; } = null!;

    public int GroupId { get; set; }

    public string Phone { get; set; } = null!;

    public string Email { get; set; } = null!;

    public DateOnly RegisteredDate { get; set; }

    public bool IsActive { get; set; }

    public virtual StudentGroup Group { get; set; } = null!;

    public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();

    public virtual Role Role { get; set; } = null!;
}
