using System;
using System.Collections.Generic;

namespace LibraryApp.Models;

public partial class Publisher
{
    public int PublisherId { get; set; }

    public string PublisherName { get; set; } = null!;

    public string City { get; set; } = null!;

    public virtual ICollection<BookPublisher> BookPublishers { get; set; } = new List<BookPublisher>();
}
