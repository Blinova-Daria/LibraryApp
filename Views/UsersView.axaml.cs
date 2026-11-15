using Avalonia.Controls;
using Avalonia.Interactivity;
using LibraryApp.Services;

namespace LibraryApp.Views;

// Экран администратора: назначение ролей пользователям.
public partial class UsersView : UserControl
{
    public UsersView()
    {
        InitializeComponent();
        Tag = "Пользователи";
        Loaded += async (s, e) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            UsersList.ItemsSource = await LoanManager.LoadUsersAsync();
        }
        catch (Exception ex)
        {
            MainWindow.Instance.ShowError(ex);
        }
    }

    // Кнопка роли в строке пользователя; название роли - в Tag кнопки.
    private async void RoleClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not Button { DataContext: UserRow row } button)
                return;

            string role = (string)button.Tag!;
            await LoanManager.SetRoleAsync(row.UserId, role);
            MainWindow.Instance.ShowMessage(
                $"Пользователю {row.Login} назначена роль «{role}».");
            await LoadAsync();
        }
        catch (Exception ex)
        {
            MainWindow.Instance.ShowError(ex);
        }
    }
}
