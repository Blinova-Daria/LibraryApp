using Avalonia.Controls;
using Avalonia.Interactivity;
using LibraryApp.Models;
using LibraryApp.Services;

namespace LibraryApp.Views;

// ФТ-9: картотека читателей.
public partial class ReadersView : UserControl
{
    public ReadersView()
    {
        InitializeComponent();
        Tag = "Читатели";
        Loaded += async (s, e) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            var groups = await StaffService.LoadGroupsAsync();
            GroupBox.ItemsSource = groups;
            GroupBox.DisplayMemberBinding =
                new Avalonia.Data.Binding("GroupName");
            if (GroupBox.SelectedIndex < 0 && groups.Count > 0)
                GroupBox.SelectedIndex = 0;
            ReadersList.ItemsSource = await StaffService.LoadReadersAsync();
        }
        catch (Exception ex) { MainWindow.Instance.ShowError(ex); }
    }

    private async void AddClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (GroupBox.SelectedItem is not StudentGroup g)
                throw new AppException("Выберите группу.");
            await StaffService.AddReaderAsync(LastBox.Text ?? "",
                FirstBox.Text ?? "", LoginBox.Text ?? "",
                TicketBox.Text ?? "", g.GroupId,
                PhoneBox.Text ?? "", EmailBox.Text ?? "");
            MainWindow.Instance.ShowMessage("Читатель добавлен.");
            LastBox.Text = FirstBox.Text = LoginBox.Text =
                TicketBox.Text = PhoneBox.Text = EmailBox.Text = "";
            await LoadAsync();
        }
        catch (Exception ex) { MainWindow.Instance.ShowError(ex); }
    }

    private async void ToggleClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not Button { DataContext: ReaderRow row }) return;
            await StaffService.ToggleReaderAsync(row.UserId);
            MainWindow.Instance.ShowMessage(
                $"Статус читателя «{row.FullName}» изменён.");
            await LoadAsync();
        }
        catch (Exception ex) { MainWindow.Instance.ShowError(ex); }
    }
}
