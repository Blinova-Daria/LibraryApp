using LibraryApp.Data;
using LibraryApp.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.Services;

// Строки экранов сотрудников.
public class ReaderRow
{
    public int UserId { get; set; }
    public string FullName { get; set; } = "";
    public string Details { get; set; } = "";
    public string StatusText { get; set; } = "";
    public string ToggleText { get; set; } = "";
}

public class FundRow
{
    public int BookId { get; set; }
    public string Title { get; set; } = "";
    public string Details { get; set; } = "";
}

public class DictRow
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Extra { get; set; } = "";
}

public class ReportLine
{
    public string Text { get; set; } = "";
}

// ФТ-9 (картотека, фонд), ФТ-10 (отчёты), ФТ-11 (справочники).
public static class StaffService
{
    private const int LastActiveStatus = 4;
    private const int StatusIssued = 3;
    private const int StatusRenewed = 4;
    private const int ReaderRoleId = 1;   // «Читатель» в таблице roles

    // ---------- Картотека читателей ----------
    public static async Task<List<ReaderRow>> LoadReadersAsync()
    {
        Session.RequireStaff();
        using var db = Db.Create();
        var raw = await (
            from u in db.Users
            join g in db.StudentGroups on u.GroupId equals g.GroupId
            where u.RoleId == ReaderRoleId
            orderby u.LastName, u.FirstName
            select new { u, g.GroupName }).AsNoTracking().ToListAsync();

        return raw.Select(r => new ReaderRow
        {
            UserId = r.u.UserId,
            FullName = r.u.FullName,
            Details = $"Билет {r.u.TicketNumber} · группа {r.GroupName} · " +
                      $"{r.u.Phone} · {r.u.Email} · логин {r.u.Login}",
            StatusText = r.u.IsActive ? "Активен" : "Заблокирован",
            ToggleText = r.u.IsActive ? "Заблокировать" : "Разблокировать"
        }).ToList();
    }

    public static async Task<List<StudentGroup>> LoadGroupsAsync()
    {
        Session.RequireStaff();
        using var db = Db.Create();
        return await db.StudentGroups.AsNoTracking()
            .OrderBy(g => g.GroupName).ToListAsync();
    }

    public static async Task AddReaderAsync(string lastName, string firstName,
        string login, string ticket, int groupId, string phone, string email)
    {
        Session.RequireStaff();
        lastName = lastName.Trim(); firstName = firstName.Trim();
        login = login.Trim(); ticket = ticket.Trim();
        phone = phone.Trim(); email = email.Trim();
        if (lastName == "" || firstName == "" || login == "" ||
            ticket == "" || phone == "" || email == "")
            throw new AppException("Заполните все поля формы.");
        if (!email.Contains('@'))
            throw new AppException("Адрес почты указан неверно.");

        using var db = Db.Create();
        if (await db.Users.AnyAsync(u => u.Login == login))
            throw new AppException("Такой логин уже есть.");
        if (await db.Users.AnyAsync(u => u.TicketNumber == ticket))
            throw new AppException("Такой номер билета уже есть.");

        db.Users.Add(new User
        {
            Login = login,
            PasswordHash = PasswordHelper.Hash(login),   // пароль = логин
            RoleId = ReaderRoleId,
            LastName = lastName,
            FirstName = firstName,
            MiddleName = "-",
            TicketNumber = ticket,
            GroupId = groupId,
            Phone = phone,
            Email = email,
            RegisteredDate = DateOnly.FromDateTime(DateTime.Now),
            IsActive = true
        });
        await db.SaveChangesAsync();
    }

    public static async Task ToggleReaderAsync(int userId)
    {
        Session.RequireStaff();
        using var db = Db.Create();
        User u = await db.Users.FindAsync(userId)
            ?? throw new AppException("Читатель не найден.");
        u.IsActive = !u.IsActive;
        await db.SaveChangesAsync();
    }

    // ---------- Фонд: издания и экземпляры ----------
    public static async Task<List<FundRow>> LoadFundAsync()
    {
        Session.RequireStaff();
        using var db = Db.Create();
        var books = await db.Books.AsNoTracking()
            .OrderBy(b => b.Title).ToListAsync();
        var copies = await db.BookCopies.AsNoTracking().ToListAsync();
        var busy = (await db.Loans
            .Where(l => l.StatusId <= LastActiveStatus)
            .Select(l => l.CopyId).ToListAsync()).ToHashSet();

        return books.Select(b =>
        {
            var mine = copies.Where(c => c.BookId == b.BookId).ToList();
            int free = mine.Count(c => !busy.Contains(c.CopyId));
            string inv = mine.Count == 0 ? "нет экземпляров" :
                string.Join(", ", mine.Select(c => c.InventoryNumber));
            return new FundRow
            {
                BookId = b.BookId,
                Title = b.Title,
                Details = $"Экземпляров: {mine.Count}, свободно: {free} · {inv}"
            };
        }).ToList();
    }

    public static async Task<List<Genre>> LoadGenresAsync()
    {
        Session.RequireStaff();
        using var db = Db.Create();
        return await db.Genres.AsNoTracking()
            .OrderBy(g => g.GenreName).ToListAsync();
    }

    // БП-6: инвентарный номер уникален.
    public static async Task AddCopyAsync(int bookId, string inventory)
    {
        Session.RequireStaff();
        inventory = inventory.Trim();
        if (inventory == "")
            throw new AppException("Введите инвентарный номер.");

        using var db = Db.Create();
        if (await db.BookCopies.AnyAsync(c => c.InventoryNumber == inventory))
            throw new AppException("Такой инвентарный номер уже есть.");

        db.BookCopies.Add(new BookCopy
        {
            BookId = bookId,
            InventoryNumber = inventory,
            DateReceived = DateOnly.FromDateTime(DateTime.Now)
        });
        await db.SaveChangesAsync();
    }

    public static async Task AddBookAsync(string title, int genreId,
        string year, string pages)
    {
        Session.RequireStaff();
        title = title.Trim();
        if (title == "")
            throw new AppException("Введите название.");
        if (!short.TryParse(year, out short y) || y < 1800 || y > 2100)
            throw new AppException("Год издания: число от 1800 до 2100.");
        if (!short.TryParse(pages, out short p) || p <= 0)
            throw new AppException("Число страниц: положительное число.");

        using var db = Db.Create();
        db.Books.Add(new Book
        {
            Title = title,
            GenreId = genreId,
            PublishYear = y,
            PageCount = p,
            Description = "Описание отсутствует",
            CoverImage = "no_cover.jpg"
        });
        await db.SaveChangesAsync();
    }

    // ---------- Отчёты ----------
    public static async Task<List<ReportLine>> LoansReportAsync(
        DateOnly dateFrom, DateOnly dateTo)
    {
        Session.RequireStaff();
        if (dateTo < dateFrom)
            throw new AppException("Конец периода раньше начала.");

        using var db = Db.Create();
        var raw = await (
            from l in db.Loans
            join c in db.BookCopies on l.CopyId equals c.CopyId
            join b in db.Books on c.BookId equals b.BookId
            join u in db.Users on l.UserId equals u.UserId
            join s in db.LoanStatuses on l.StatusId equals s.StatusId
            where l.BookingDate >= dateFrom && l.BookingDate <= dateTo
            orderby l.BookingDate, l.LoanId
            select new
            {
                l.LoanId,
                l.BookingDate,
                l.DueDate,
                b.Title,
                c.InventoryNumber,
                u.LastName,
                u.FirstName,
                s.StatusName
            }).AsNoTracking().ToListAsync();

        return raw.Select(r => new ReportLine
        {
            Text = $"№ {r.LoanId} · {r.BookingDate:dd.MM.yyyy} · " +
                   $"{r.Title} ({r.InventoryNumber}) · " +
                   $"{r.LastName} {r.FirstName} · {r.StatusName} · " +
                   $"срок до {r.DueDate:dd.MM.yyyy}"
        }).ToList();
    }

    public static async Task<List<ReportLine>> DebtorsReportAsync()
    {
        Session.RequireStaff();
        DateOnly today = DateOnly.FromDateTime(DateTime.Now);
        using var db = Db.Create();
        var raw = await (
            from l in db.Loans
            join c in db.BookCopies on l.CopyId equals c.CopyId
            join b in db.Books on c.BookId equals b.BookId
            join u in db.Users on l.UserId equals u.UserId
            where (l.StatusId == StatusIssued || l.StatusId == StatusRenewed)
                  && l.DueDate < today
            orderby l.DueDate
            select new
            {
                l.LoanId,
                l.DueDate,
                b.Title,
                c.InventoryNumber,
                u.LastName,
                u.FirstName,
                u.Phone
            })
            .AsNoTracking().ToListAsync();

        return raw.Select(r => new ReportLine
        {
            Text = $"{r.LastName} {r.FirstName} ({r.Phone}) · {r.Title} " +
                   $"({r.InventoryNumber}) · срок был {r.DueDate:dd.MM.yyyy}" +
                   $" · просрочка {today.DayNumber - r.DueDate.DayNumber} дн."
        }).ToList();
    }

    // Сохранение отчёта в CSV рядом с программой (открывается в Excel).
    public static string SaveCsv(string name, IEnumerable<ReportLine> lines)
    {
        string path = Path.Combine(AppContext.BaseDirectory,
            $"{name}_{DateTime.Now:yyyyMMdd_HHmm}.csv");
        File.WriteAllLines(path,
            lines.Select(l => "\"" + l.Text.Replace("\"", "\"\"") + "\""),
            new System.Text.UTF8Encoding(true));
        return path;
    }

    // ---------- Справочники (администратор) ----------
    public static async Task<List<DictRow>> LoadDictAsync(string kind)
    {
        Session.RequireAdmin();
        using var db = Db.Create();
        return kind switch
        {
            "Жанры" => (await db.Genres.AsNoTracking()
                .OrderBy(x => x.GenreName).ToListAsync())
                .Select(x => new DictRow { Id = x.GenreId, Name = x.GenreName })
                .ToList(),
            "Учебные группы" => (await db.StudentGroups.AsNoTracking()
                .OrderBy(x => x.GroupName).ToListAsync())
                .Select(x => new DictRow { Id = x.GroupId, Name = x.GroupName })
                .ToList(),
            _ => (await db.Publishers.AsNoTracking()
                .OrderBy(x => x.PublisherName).ToListAsync())
                .Select(x => new DictRow
                { Id = x.PublisherId, Name = x.PublisherName, Extra = x.City })
                .ToList()
        };
    }

    public static async Task AddDictAsync(string kind, string name, string extra)
    {
        Session.RequireAdmin();
        name = name.Trim(); extra = extra.Trim();
        if (name == "")
            throw new AppException("Введите название.");
        if (kind == "Издательства" && extra == "")
            throw new AppException("Для издательства укажите город.");

        using var db = Db.Create();
        switch (kind)
        {
            case "Жанры":
                db.Genres.Add(new Genre { GenreName = name }); break;
            case "Учебные группы":
                db.StudentGroups.Add(new StudentGroup { GroupName = name });
                break;
            default:
                db.Publishers.Add(new Publisher
                { PublisherName = name, City = extra });
                break;
        }
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException)
        { throw new AppException("Такая запись уже есть."); }
    }

    public static async Task DeleteDictAsync(string kind, int id)
    {
        Session.RequireAdmin();
        using var db = Db.Create();
        switch (kind)
        {
            case "Жанры":
                db.Genres.Remove(await db.Genres.FindAsync(id)
                    ?? throw new AppException("Запись не найдена.")); break;
            case "Учебные группы":
                db.StudentGroups.Remove(await db.StudentGroups.FindAsync(id)
                    ?? throw new AppException("Запись не найдена.")); break;
            default:
                db.Publishers.Remove(await db.Publishers.FindAsync(id)
                    ?? throw new AppException("Запись не найдена.")); break;
        }
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException)
        { throw new AppException("Запись используется, удалить нельзя."); }
    }
}
