using Avalonia.Controls;
using Avalonia.Interactivity;
using LibraryApp.Services;

namespace LibraryApp.Views;

// ФТ-11: справочники (жанры, группы, издательства), только админ.
public partial class DictionariesView : UserControl
{
    public DictionariesView()
    {
        InitializeComponent();
        Tag = "Справочники";
        KindBox.ItemsSource = new[] { "Жанры", "Учебные группы", "Издательства" };
        KindBox.SelectedIndex = 0;
    }

    private string Kind => (string)(KindBox.SelectedItem ?? "Жанры");

    private async Task LoadAsync()
    {
        try { DictList.ItemsSource = await StaffService.LoadDictAsync(Kind); }
        catch (Exception ex) { MainWindow.Instance.ShowError(ex); }
    }

    private async void KindChanged(object? sender, SelectionChangedEventArgs e)
    {
        ExtraBox.IsVisible = Kind == "Издательства";
        await LoadAsync();
    }

    private async void AddClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            await StaffService.AddDictAsync(Kind, NameBox.Text ?? "",
                ExtraBox.Text ?? "");
            MainWindow.Instance.ShowMessage("Запись добавлена.");
            NameBox.Text = ExtraBox.Text = "";
            await LoadAsync();
        }
        catch (Exception ex) { MainWindow.Instance.ShowError(ex); }
    }

    private async void DeleteClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not Button { DataContext: DictRow row }) return;
            await StaffService.DeleteDictAsync(Kind, row.Id);
            MainWindow.Instance.ShowMessage($"Запись «{row.Name}» удалена.");
            await LoadAsync();
        }
        catch (Exception ex) { MainWindow.Instance.ShowError(ex); }
    }
}
