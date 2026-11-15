using LibraryApp.Data;
using LibraryApp.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.Services;

// Строка журнала выдач для экрана.
public class LoanRow
{
    public int LoanId { get; set; }
    public string BookTitle { get; set; } = "";
    public string Details { get; set; } = "";      // инв. номер, даты
    public string ReaderText { get; set; } = "";   // читатель (для сотрудников)
    public string StatusName { get; set; } = "";
    public string ActionText { get; set; } = "";   // текст кнопки
    public bool HasAction { get; set; }            // кнопка видна
}

// Строка списка пользователей для администратора.
public class UserRow
{
    public int UserId { get; set; }
    public string Login { get; set; } = "";
    public string FullName { get; set; } = "";
    public string RoleName { get; set; } = "";
}

// Работа с выдачами и пользователями по правилам ролей.
public static class LoanManager
{
    private const int StatusPending = 1;     // ожидает подтверждения
    private const int StatusConfirmed = 2;   // подтверждена
    private const int StatusIssued = 3;      // выдана
    private const int StatusRenewed = 4;     // продлена
    private const int StatusReturned = 5;    // возвращена
    private const int StatusCancelled = 6;   // отменена
    private const int HoldDays = 3;          // БП-3: бронь хранится 3 дня
    private const int LoanDays = 14;         // БП-2: срок пользования
    private const int RenewDays = 7;         // БП-2: продление на 7 дней

    private static readonly Dictionary<int, string> StatusNames = new()
    {
        [1] = "Ожидает подтверждения",
        [2] = "Подтверждена",
        [3] = "Выдана",
        [4] = "Продлена",
        [5] = "Возвращена",
        [6] = "Отменена"
    };

    // Журнал: только свои записи (onlyUserId) или все (null).
    private static async Task<List<LoanRow>> QueryAsync(int? onlyUserId)
    {
        await CancelExpiredAsync();
        using var db = Db.Create();
        bool own = onlyUserId != null;

        var raw = await (
            from l in db.Loans
            join c in db.BookCopies on l.CopyId equals c.CopyId
            join b in db.Books on c.BookId equals b.BookId
            join u in db.Users on l.UserId equals u.UserId
            where onlyUserId == null || l.UserId == onlyUserId
            orderby l.LoanId descending
            select new
            {
                l.LoanId, l.StatusId, l.BookingDate, l.DueDate,
                b.Title, c.InventoryNumber,
                u.LastName, u.FirstName, u.Login
            }).AsNoTracking().ToListAsync();

        return raw.Select(r =>
        {
            var row = new LoanRow
            {
                LoanId = r.LoanId,
                BookTitle = r.Title,
                StatusName = StatusNames.GetValueOrDefault(
                    r.StatusId, "Неизвестно"),
                Details = $"Инв. № {r.InventoryNumber} · бронь " +
                          $"{r.BookingDate:dd.MM.yyyy} · срок до " +
                          $"{r.DueDate:dd.MM.yyyy}",
                ReaderText = own ? "" :
                    $"Читатель: {r.LastName} {r.FirstName} ({r.Login})"
            };

            (row.ActionText, row.HasAction) = (own, r.StatusId) switch
            {
                (true, StatusPending) => ("Отменить бронь", true),
                (true, StatusIssued) => ("Продлить на 7 дней", true),
                (false, StatusPending) => ("Подтвердить бронь", true),
                (false, StatusConfirmed) => ("Выдать книгу", true),
                (false, StatusIssued) => ("Принять возврат", true),
                (false, StatusRenewed) => ("Принять возврат", true),
                _ => ("", false)
            };
            return row;
        }).ToList();
    }

    // Мои выдачи - для любого вошедшего пользователя.
    public static Task<List<LoanRow>> LoadOwnAsync()
    {
        Session.RequireLogin();
        User user = Session.CurrentUser!;
        return QueryAsync(user.UserId);
    }

    // Все выдачи - только для библиотекаря и администратора.
    public static Task<List<LoanRow>> LoadAllAsync()
    {
        Session.RequireStaff();
        return QueryAsync(null);
    }

    // БП-3: подтверждённая бронь хранится 3 дня, затем отменяется.
    // Возвращает число отменённых броней.
    public static async Task<int> CancelExpiredAsync()
    {
        using var db = Db.Create();
        DateOnly today = DateOnly.FromDateTime(DateTime.Now);
        List<Loan> expired = await db.Loans
            .Where(l => l.StatusId == StatusConfirmed && l.DueDate < today)
            .ToListAsync();
        foreach (Loan l in expired)
            l.StatusId = StatusCancelled;
        if (expired.Count > 0)
            await db.SaveChangesAsync();
        return expired.Count;
    }

    // БП-2: одно продление на 7 дней, если на издание нет брони
    // другого читателя.
    public static async Task RenewAsync(int loanId)
    {
        Session.RequireLogin();
        User user = Session.CurrentUser!;

        using var db = Db.Create();
        Loan loan = await db.Loans.FindAsync(loanId)
            ?? throw new AppException("Запись не найдена.");
        if (loan.UserId != user.UserId)
            throw new AppException("Можно продлить только свою выдачу.");
        if (loan.StatusId == StatusRenewed)
            throw new AppException("Продлить можно только один раз.");
        if (loan.StatusId != StatusIssued)
            throw new AppException("Продлить можно только выданное издание.");

        DateOnly today = DateOnly.FromDateTime(DateTime.Now);
        if (loan.DueDate < today)
            throw new AppException("Срок истёк. Верните издание.");

        int bookId = await db.BookCopies
            .Where(c => c.CopyId == loan.CopyId)
            .Select(c => c.BookId).SingleAsync();

        // Есть ли у другого читателя бронь на любой экземпляр издания.
        bool booked = await (
            from l in db.Loans
            join c in db.BookCopies on l.CopyId equals c.CopyId
            where c.BookId == bookId && l.UserId != user.UserId &&
                  (l.StatusId == StatusPending ||
                   l.StatusId == StatusConfirmed)
            select l.LoanId).AnyAsync();
        if (booked)
            throw new AppException(
                "Продлить нельзя: на издание есть бронь другого читателя.");

        loan.StatusId = StatusRenewed;
        loan.DueDate = loan.DueDate.AddDays(RenewDays);
        await db.SaveChangesAsync();
    }

    // Читатель отменяет свою бронь, пока её не подтвердили.
    public static async Task CancelAsync(int loanId)
    {
        Session.RequireLogin();
        User user = Session.CurrentUser!;

        using var db = Db.Create();
        Loan loan = await db.Loans.FindAsync(loanId)
            ?? throw new AppException("Запись не найдена.");
        if (loan.UserId != user.UserId)
            throw new AppException("Можно отменить только свою бронь.");
        if (loan.StatusId != StatusPending)
            throw new AppException(
                "Отменить можно только бронь, которую ещё не подтвердили.");

        loan.StatusId = StatusCancelled;
        await db.SaveChangesAsync();
    }

    // Библиотекарь переводит запись на следующий шаг процесса.
    public static async Task<string> AdvanceAsync(int loanId)
    {
        Session.RequireStaff();

        using var db = Db.Create();
        Loan loan = await db.Loans.FindAsync(loanId)
            ?? throw new AppException("Запись не найдена.");
        DateOnly today = DateOnly.FromDateTime(DateTime.Now);

        switch (loan.StatusId)
        {
            case StatusPending:
                loan.StatusId = StatusConfirmed;
                loan.DueDate = today.AddDays(HoldDays);
                break;
            case StatusConfirmed:
                loan.StatusId = StatusIssued;
                loan.DueDate = today.AddDays(LoanDays);
                break;
            case StatusIssued:
            case StatusRenewed:
                loan.StatusId = StatusReturned;
                break;
            default:
                throw new AppException(
                    "Для этого статуса действие не предусмотрено.");
        }

        await db.SaveChangesAsync();
        return StatusNames[loan.StatusId];
    }

    // Список пользователей - только для администратора.
    public static async Task<List<UserRow>> LoadUsersAsync()
    {
        Session.RequireAdmin();

        using var db = Db.Create();
        var users = await db.Users
            .Include(u => u.Role)
            .AsNoTracking()
            .OrderBy(u => u.LastName)
            .ToListAsync();

        return users.Select(u => new UserRow
        {
            UserId = u.UserId,
            Login = u.Login,
            FullName = u.FullName,
            RoleName = u.Role.RoleName
        }).ToList();
    }

    // Назначить роль пользователю - только администратор.
    public static async Task SetRoleAsync(int userId, string roleName)
    {
        Session.RequireAdmin();
        if (Session.CurrentUser!.UserId == userId &&
            roleName != "Администратор")
        {
            throw new AppException(
                "Нельзя снять роль администратора с самого себя.");
        }

        using var db = Db.Create();
        Role role = await db.Roles
            .SingleOrDefaultAsync(r => r.RoleName == roleName)
            ?? throw new AppException(
                $"Роль «{roleName}» не найдена в базе.");
        User user = await db.Users.FindAsync(userId)
            ?? throw new AppException("Пользователь не найден.");

        user.RoleId = role.RoleId;
        await db.SaveChangesAsync();
    }
}
