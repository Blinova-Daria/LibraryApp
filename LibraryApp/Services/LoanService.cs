using LibraryApp.Data;
using LibraryApp.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.Services;

// Бронирование издания по правилам БП-1, БП-3 - БП-5.
public static class LoanService
{
    private const int MaxLoans = 5;          // БП-1
    private const int HoldDays = 3;          // БП-3
    private const int StatusPending = 1;     // Ожидает подтверждения
    private const int StatusIssued = 3;      // Выдана
    private const int LastActiveStatus = 4;  // статусы 1-4 активные

    // Возвращает номер созданной брони.
    public static async Task<int> ReserveAsync(int bookId)
    {
        Session.RequireLogin();
        User user = Session.CurrentUser!;

        await LoanManager.CancelExpiredAsync();   // БП-3
        using var db = Db.Create();
        DateOnly today = DateOnly.FromDateTime(DateTime.Now);

        // БП-5: просроченная выдача блокирует бронь.
        bool overdue = await db.Loans.AnyAsync(l =>
            l.UserId == user.UserId &&
            l.StatusId == StatusIssued &&
            l.DueDate < today);
        if (overdue)
            throw new AppException(
                "Есть просроченное издание. Верните его.");

        // БП-1: не более пяти активных записей.
        int active = await db.Loans.CountAsync(l =>
            l.UserId == user.UserId &&
            l.StatusId <= LastActiveStatus);
        if (active >= MaxLoans)
            throw new AppException(
                "Лимит: не более пяти изданий одновременно.");

        // БП-4: экземпляр без активной записи.
        BookCopy? copy = await db.BookCopies
            .Where(c => c.BookId == bookId &&
                !db.Loans.Any(l => l.CopyId == c.CopyId &&
                                   l.StatusId <= LastActiveStatus))
            .OrderBy(c => c.CopyId)
            .FirstOrDefaultAsync();
        if (copy == null)
            throw new AppException("Свободных экземпляров нет.");

        var loan = new Loan
        {
            UserId = user.UserId,
            CopyId = copy.CopyId,
            StatusId = StatusPending,
            BookingDate = today,
            DueDate = today.AddDays(HoldDays)
        };
        db.Loans.Add(loan);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Уникальный индекс ux_loans_active_copy
            // не пустил второго читателя на тот же экземпляр.
            throw new AppException(
                "Экземпляр уже занят. Обновите каталог.");
        }
        return loan.LoanId;
    }
}
