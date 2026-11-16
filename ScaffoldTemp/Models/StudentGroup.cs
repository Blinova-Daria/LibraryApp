using System;
using System.Collections.Generic;

namespace LibraryApp.Models;

public partial class StudentGroup
{
    public int GroupId { get; set; }

    public string GroupName { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
