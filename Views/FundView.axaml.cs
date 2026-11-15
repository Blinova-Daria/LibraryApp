using Avalonia.Controls;
using Avalonia.Interactivity;
using LibraryApp.Models;
using LibraryApp.Services;

namespace LibraryApp.Views;

// ФТ-9: каталог с экземплярами.
public partial class FundView : UserControl
{
    public FundView()
    {
        InitializeComponent();
        Tag = "Фонд";
        Loaded += async (s, e) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            var genres = await StaffService.LoadGenresAsync();
            GenreBox.ItemsSource = genres;
            GenreBox.DisplayMemberBinding =
                new Avalonia.Data.Binding("GenreName");
            if (GenreBox.SelectedIndex < 0 && genres.Count > 0)
                GenreBox.SelectedIndex = 0;
            FundList.ItemsSource = await StaffService.LoadFundAsync();
        }
        catch (Exception ex) { MainWindow.Instance.ShowError(ex); }
    }

    private async void AddBookClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (GenreBox.SelectedItem is not Genre g)
                throw new AppException("Выберите жанр.");
            await StaffService.AddBookAsync(TitleBox.Text ?? "", g.GenreId,
                YearBox.Text ?? "", PagesBox.Text ?? "");
            MainWindow.Instance.ShowMessage(
                "Издание добавлено. Теперь добавьте экземпляр.");
            TitleBox.Text = YearBox.Text = PagesBox.Text = "";
            await LoadAsync();
        }
        catch (Exception ex) { MainWindow.Instance.ShowError(ex); }
    }

    private async void AddCopyClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (FundList.SelectedItem is not FundRow row)
                throw new AppException("Выберите издание в списке.");
            await StaffService.AddCopyAsync(row.BookId, InvBox.Text ?? "");
            MainWindow.Instance.ShowMessage(
                $"Экземпляр добавлен к изданию «{row.Title}».");
            InvBox.Text = "";
            await LoadAsync();
        }
        catch (Exception ex) { MainWindow.Instance.ShowError(ex); }
    }
}
