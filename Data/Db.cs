using Microsoft.EntityFrameworkCore;

namespace LibraryApp.Data;

// В любом месте программы: using var db = Db.Create();
public static class Db
{
    public static LibraryContext Create()
    {
        // connection.txt лежит рядом с .exe (настройка в .csproj).
        string path = Path.Combine(AppContext.BaseDirectory, "connection.txt");
        string connection = File.ReadAllText(path).Trim();

        var options = new DbContextOptionsBuilder<LibraryContext>()
            .UseSqlServer(connection)
            .Options;
        return new LibraryContext(options);
    }
}
