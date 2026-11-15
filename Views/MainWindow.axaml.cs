using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using LibraryApp.Services;

namespace LibraryApp.Views;

public partial class MainWindow : Window
{
    // Единственное главное окно: MainWindow.Instance.OpenPage(...)
    public static MainWindow Instance { get; private set; } = null!;

    // История страниц для кнопки «Назад».
    private readonly Stack<UserControl> history = new();

    public MainWindow()
    {
        InitializeComponent();
        Instance = this;
        OpenPage(new LoginView(), clearHistory: true);
    }

    // Открыть страницу; clearHistory: true - начать историю заново.
    public void OpenPage(UserControl page, bool clearHistory = false)
    {
        if (clearHistory)
        {
            history.Clear();
        }
        else if (PageHost.Content is UserControl current)
        {
            history.Push(current);
        }
        ShowPage(page);
    }

    // Показать страницу и обновить шапку, кнопки и роль.
    private void ShowPage(UserControl page)
    {
        PageHost.Content = page;
        Title = "Библиотека - " + page.Tag;
        MessageText.Text = "";
        UserText.Text = Session.Caption;
        BackButton.IsEnabled = history.Count > 0;
        UpdateNav();
    }

    // Показать только те пункты меню, которые доступны роли.
    private void UpdateNav()
    {
        LoginButton.IsVisible = Session.IsGuest;      // гостю - вход
        MyLoansButton.IsVisible = !Session.IsGuest;   // все вошедшие
        AllLoansButton.IsVisible = Session.IsStaff;   // библиотекарь, админ
        UsersButton.IsVisible = Session.IsAdmin;      // только админ
        ReadersButton.IsVisible = Session.IsStaff;
        FundButton.IsVisible = Session.IsStaff;
        ReportsButton.IsVisible = Session.IsStaff;
        DictButton.IsVisible = Session.IsAdmin;
        LogoutButton.IsVisible = !Session.IsGuest;
    }

    public void ShowMessage(string text)
    {
        MessageText.Foreground = Brushes.Black;
        MessageText.Text = text;
    }

    public void ShowError(Exception error)
    {
        System.Diagnostics.Trace.WriteLine(error);
        MessageText.Foreground = Brushes.DarkRed;
        MessageText.Text = UiSupport.ErrorText(error);
    }

    private void BackClick(object? sender, RoutedEventArgs e)
    {
        if (history.Count > 0)
            ShowPage(history.Pop());
    }

    private void CatalogClick(object? sender, RoutedEventArgs e)
    {
        OpenPage(new CatalogView(), clearHistory: true);
    }

    private void LoginClick(object? sender, RoutedEventArgs e)
    {
        OpenPage(new LoginView(), clearHistory: true);
    }

    private void MyLoansClick(object? sender, RoutedEventArgs e)
    {
        OpenPage(new LoansView(allLoans: false), clearHistory: true);
    }

    private void AllLoansClick(object? sender, RoutedEventArgs e)
    {
        OpenPage(new LoansView(allLoans: true), clearHistory: true);
    }

    private void UsersClick(object? sender, RoutedEventArgs e)
    {
        OpenPage(new UsersView(), clearHistory: true);
    }

    private void ReadersClick(object? sender, RoutedEventArgs e)
    {
        OpenPage(new ReadersView(), clearHistory: true);
    }

    private void FundClick(object? sender, RoutedEventArgs e)
    {
        OpenPage(new FundView(), clearHistory: true);
    }

    private void ReportsClick(object? sender, RoutedEventArgs e)
    {
        OpenPage(new ReportsView(), clearHistory: true);
    }

    private void DictClick(object? sender, RoutedEventArgs e)
    {
        OpenPage(new DictionariesView(), clearHistory: true);
    }

    private void LogoutClick(object? sender, RoutedEventArgs e)
    {
        Session.Logout();
        OpenPage(new LoginView(), clearHistory: true);
    }
}
