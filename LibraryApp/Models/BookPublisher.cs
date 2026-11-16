using System;
using System.Collections.Generic;

namespace LibraryApp.Models;

public partial class BookPublisher
{
    public int BookPublisherId { get; set; }

    public int BookId { get; set; }

    public int PublisherId { get; set; }

    public virtual Book Book { get; set; } = null!;

    public virtual Publisher Publisher { get; set; } = null!;
}
