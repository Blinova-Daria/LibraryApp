using System;
using System.Collections.Generic;

namespace LibraryApp.Models;

public partial class Book
{
    public int BookId { get; set; }

    public string Title { get; set; } = null!;

    public int GenreId { get; set; }

    public short PublishYear { get; set; }

    public short PageCount { get; set; }

    public string Description { get; set; } = null!;

    public string CoverImage { get; set; } = null!;

    public virtual ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();

    public virtual ICollection<BookCopy> BookCopies { get; set; } = new List<BookCopy>();

    public virtual ICollection<BookPublisher> BookPublishers { get; set; } = new List<BookPublisher>();

    public virtual Genre Genre { get; set; } = null!;
}
