using LibraryApp.Data;
using LibraryApp.Models;
using LibraryApp.Views;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.Services;

public static class CatalogService
{
    // Активные записи журнала: статусы 1-4 (ожидает, подтверждена,
    // выдана, продлена); совпадает с индексом ux_loans_active_copy.
    private const int LastActiveStatus = 4;

    public static async Task<List<BookCard>> LoadCardsAsync()
    {
        await LoanManager.CancelExpiredAsync();   // БП-3
        using var db = Db.Create();

        List<Book> books = await db.Books
            .Include(b => b.Genre)
            .Include(b => b.BookAuthors).ThenInclude(x => x.Author)
            .Include(b => b.BookPublishers)
                .ThenInclude(x => x.Publisher)
            .Include(b => b.BookCopies)
            .AsNoTracking()
            .OrderBy(b => b.Title)
            .ToListAsync();

        // Номера экземпляров, которые сейчас заняты активной записью.
        HashSet<int> busy = (await db.Loans
            .Where(l => l.StatusId <= LastActiveStatus)
            .Select(l => l.CopyId)
            .ToListAsync())
            .ToHashSet();

        var cards = new List<BookCard>();
        foreach (Book b in books)
        {
            int total = b.BookCopies.Count;
            int free = b.BookCopies.Count(c => !busy.Contains(c.CopyId));

            cards.Add(new BookCard
            {
                Id = b.BookId,
                Title = b.Title,
                Authors = string.Join(", ",
                    b.BookAuthors.Select(x => x.Author.ShortName)),
                Genre = b.Genre.GenreName,
                Publisher = string.Join(", ",
                    b.BookPublishers.Select(
                        x => x.Publisher.PublisherName)),
                Year = b.PublishYear,
                Description = b.Description,
                Free = free,
                FreeText = $"Свободно: {free} из {total}",
                CanReserve = !Session.IsGuest,
                Background = UiSupport.Brush(
                    free == 0 ? "NoCopiesBrush" : "NormalBrush"),
                Image = UiSupport.Picture(b.CoverImage)
            });
        }
        return cards;
    }
}
