using Avalonia.Controls;
using Avalonia.Interactivity;
using LibraryApp.Services;

namespace LibraryApp.Views;

// Журнал выдач: для читателя - свои записи, для сотрудников - все.
public partial class LoansView : UserControl
{
    private readonly bool allLoans;

    public LoansView(bool allLoans)
    {
        InitializeComponent();
        this.allLoans = allLoans;
        Tag = allLoans ? "Все выдачи" : "Мои выдачи";
        TitleText.Text = allLoans ? "Все выдачи" : "Мои выдачи";
        Loaded += async (s, e) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            List<LoanRow> rows = allLoans
                ? await LoanManager.LoadAllAsync()
                : await LoanManager.LoadOwnAsync();
            LoansList.ItemsSource = rows;
            CountText.Text = $"Записей: {rows.Count}";
        }
        catch (Exception ex)
        {
            MainWindow.Instance.ShowError(ex);
        }
    }

    // Кнопка действия в строке: отмена (читатель) или шаг процесса.
    private async void ActionClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not Button { DataContext: LoanRow row })
                return;

            if (allLoans)
            {
                string status = await LoanManager.AdvanceAsync(row.LoanId);
                MainWindow.Instance.ShowMessage(
                    $"Запись № {row.LoanId}: новый статус «{status}».");
            }
            else if (row.ActionText.StartsWith("Продлить"))
            {
                await LoanManager.RenewAsync(row.LoanId);
                MainWindow.Instance.ShowMessage(
                    $"Выдача № {row.LoanId} продлена на 7 дней.");
            }
            else
            {
                await LoanManager.CancelAsync(row.LoanId);
                MainWindow.Instance.ShowMessage(
                    $"Бронь № {row.LoanId} отменена.");
            }
            await LoadAsync();
        }
        catch (Exception ex)
        {
            MainWindow.Instance.ShowError(ex);
        }
    }
}
