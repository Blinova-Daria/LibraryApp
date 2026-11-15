using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace LibraryApp.Services;

// Всё, что показывается на экране для одного издания.
public class BookCard
{
    public int Id { get; set; }                     // book_id
    public string Title { get; set; } = "";         // название
    public string Authors { get; set; } = "";       // авторы
    public string Genre { get; set; } = "";         // жанр
    public string Publisher { get; set; } = "";     // издательства
    public int Year { get; set; }                   // год издания
    public string Description { get; set; } = "";   // описание
    public int Free { get; set; }                   // свободных
    public string FreeText { get; set; } = "";      // «Свободно: 2»
    public bool CanReserve { get; set; }            // кнопка брони видна
    public IBrush? Background { get; set; }         // фон карточки
    public Bitmap? Image { get; set; }              // обложка
}
