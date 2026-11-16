using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using LibraryApp.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.Views;

// Цвета, картинки и тексты ошибок для форм.
public static class UiSupport
{
    private const string AssetsRoot = "avares://LibraryApp/Assets/";

    // Уже загруженные картинки: ключ - адрес файла.
    private static readonly Dictionary<string, Bitmap> cache = new();

    // Цвет из App.axaml по имени, например Brush("NoCopiesBrush").
    public static IBrush Brush(string key)
    {
        return (IBrush)Application.Current!.Resources[key]!;
    }

    // Обложка из Assets/Images по имени из столбца cover_image.
    // Если имени или файла нет - заглушка picture.png.
    public static Bitmap Picture(string? fileName)
    {
        string uri = AssetsRoot + "Images/" + fileName;
        if (string.IsNullOrWhiteSpace(fileName) ||
            !AssetLoader.Exists(new Uri(uri)))
        {
            uri = AssetsRoot + "picture.png";
        }

        if (!cache.TryGetValue(uri, out Bitmap? bitmap))
        {
            using var stream = AssetLoader.Open(new Uri(uri));
            bitmap = new Bitmap(stream);
            cache[uri] = bitmap;
        }
        return bitmap;
    }

    // Превращает техническую ошибку в понятный текст.
    public static string ErrorText(Exception error)
    {
        if (error is AppException)
            return error.Message;

        if (error is SqlException || error is DbUpdateException)
            return "Ошибка базы данных. Проверьте, что SQL Server " +
                   "запущен и строка в connection.txt верна.";

        if (error is IOException)
            return "Не найден файл connection.txt рядом с программой.";

        return "Операция не выполнена: " + error.Message;
    }
}
