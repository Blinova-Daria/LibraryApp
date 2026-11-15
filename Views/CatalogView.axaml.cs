using Avalonia.Controls;
using Avalonia.Interactivity;
using LibraryApp.Services;

namespace LibraryApp.Views;

public partial class CatalogView : UserControl
{
    // Все карточки из БД. Фильтры всегда начинают с полного списка.
    private List<BookCard> allCards = new();

    public CatalogView()
    {
        InitializeComponent();
        SortBox.SelectedIndex = 0;
        Loaded += async (s, e) => await LoadCardsAsync();
    }

    // Загрузить карточки из БД и заполнить список жанров.
    private async Task LoadCardsAsync()
    {
        try
        {
            string? oldGenre = GenreBox.SelectedItem as string;
            allCards = await CatalogService.LoadCardsAsync();

            var genres = new List<string> { "Все жанры" };
            genres.AddRange(allCards.Select(c => c.Genre)
                                    .Distinct().OrderBy(g => g));
            GenreBox.ItemsSource = genres;

            if (oldGenre != null && genres.Contains(oldGenre))
                GenreBox.SelectedItem = oldGenre;
            else
                GenreBox.SelectedIndex = 0;

            ApplyFilters();
        }
        catch (Exception ex)
        {
            MainWindow.Instance.ShowError(ex);
        }
    }

    // Поиск, жанр и сортировка; вызывается при любом изменении.
    private void ApplyFilters()
    {
        string text = (SearchBox.Text ?? "").Trim();

        // 1) Поиск: название или авторы содержат текст.
        IEnumerable<BookCard> rows = allCards.Where(c =>
            c.Title.Contains(text, StringComparison.OrdinalIgnoreCase) ||
            c.Authors.Contains(text, StringComparison.OrdinalIgnoreCase));

        // 2) Жанр: индекс 0 - «Все жанры», тогда не фильтруем.
        if (GenreBox.SelectedIndex > 0)
        {
            string genre = (string)GenreBox.SelectedItem!;
            rows = rows.Where(c => c.Genre == genre);
        }

        // 3) Сортировка.
        rows = SortBox.SelectedIndex switch
        {
            1 => rows.OrderByDescending(c => c.Year),
            2 => rows.OrderByDescending(c => c.Free)
                     .ThenBy(c => c.Title),
            _ => rows.OrderBy(c => c.Title)
        };

        // 4) Показать результат.
        List<BookCard> result = rows.ToList();
        CardsList.ItemsSource = result;
        CountText.Text = $"Найдено: {result.Count} из {allCards.Count}";
    }

    private void SearchChanged(object? sender, TextChangedEventArgs e)
    {
        ApplyFilters();
    }

    private void FilterChanged(object? sender,
                               SelectionChangedEventArgs e)
    {
        ApplyFilters();
    }

    private async void ReloadClick(object? sender, RoutedEventArgs e)
    {
        await LoadCardsAsync();
    }

    // Кнопка «Забронировать» на карточке издания.
    private async void BookClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not Button { DataContext: BookCard card })
                return;

            int loanId = await LoanService.ReserveAsync(card.Id);
            MainWindow.Instance.ShowMessage(
                $"Издание «{card.Title}» забронировано. Номер брони: " +
                $"{loanId}. Ждите подтверждения библиотекаря.");
            await LoadCardsAsync();
        }
        catch (Exception ex)
        {
            MainWindow.Instance.ShowError(ex);
        }
    }
}
