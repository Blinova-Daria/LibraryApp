using System;
using System.Collections.Generic;

namespace LibraryApp.Models;

public partial class Author
{
    public int AuthorId { get; set; }

    public string LastName { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string MiddleName { get; set; } = null!;

    public virtual ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
}
