using System.Security.Cryptography;
using System.Text;
using LibraryApp.Data;
using LibraryApp.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.Services;

// Session помнит, кто сейчас работает с программой.
public static class Session
{
    // null означает «гость».
    public static User? CurrentUser { get; private set; }

    public static string RoleName =>
        CurrentUser?.Role.RoleName ?? "";

    // Названия ролей - как в таблице roles.
    public static bool IsGuest => CurrentUser == null;
    public static bool IsStaff =>
        RoleName == "Библиотекарь" || RoleName == "Администратор";
    public static bool IsAdmin => RoleName == "Администратор";

    // Подпись в шапке окна.
    public static string Caption =>
        CurrentUser == null
            ? "Гость"
            : $"{CurrentUser.FullName} · {RoleName}";

    // SHA-256 в нижнем регистре, как в скрипте базы данных.
    private static string Hash(string password) =>
        Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(password)))
        .ToLowerInvariant();

    // Вход по логину и паролю.
    public static async Task LoginAsync(string login, string password)
    {
        login = login.Trim();
        if (login == "" || password == "")
            throw new AppException("Введите логин и пароль.");

        using var db = Db.Create();
        User? user = await db.Users
            .Include(u => u.Role)
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Login == login);

        if (user == null || user.PasswordHash != Hash(password))
            throw new AppException("Неверный логин или пароль.");
        if (!user.IsActive)
            throw new AppException("Учётная запись отключена.");

        CurrentUser = user;
    }

    public static void Logout()
    {
        CurrentUser = null;
    }

    // Проверки прав: вызываются в службах ПЕРЕД изменением данных.
    // Проверка входа: гость не может выполнять действие.
    public static void RequireLogin()
    {
        if (IsGuest)
            throw new AppException("Войдите в систему.");
    }

    // Проверка роли: только сотрудники библиотеки.
    public static void RequireStaff()
    {
        RequireLogin();
        if (!IsStaff)
            throw new AppException("Действие доступно сотрудникам.");
    }

    public static void RequireAdmin()
    {
        if (!IsAdmin)
            throw new AppException("Нужна роль администратора.");
    }
}
