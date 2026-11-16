using Avalonia.Controls;
using Avalonia.Interactivity;
using LibraryApp.Services;

namespace LibraryApp.Views;

// ФТ-10: отчёты о выдачах и должниках.
public partial class ReportsView : UserControl
{
    private List<ReportLine> current = new();
    private string currentName = "report";

    public ReportsView()
    {
        InitializeComponent();
        Tag = "Отчёты";
        ToPicker.SelectedDate = DateTime.Today;
        FromPicker.SelectedDate = DateTime.Today.AddMonths(-1);
    }

    private void Show(string name, List<ReportLine> lines)
    {
        current = lines;
        currentName = name;
        ReportList.ItemsSource = lines;
        CountText.Text = $"Записей: {lines.Count}";
    }

    private async void LoansClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (FromPicker.SelectedDate is not DateTime f ||
                ToPicker.SelectedDate is not DateTime t)
                throw new AppException("Укажите даты периода.");
            Show("loans", await StaffService.LoansReportAsync(
                DateOnly.FromDateTime(f), DateOnly.FromDateTime(t)));
        }
        catch (Exception ex) { MainWindow.Instance.ShowError(ex); }
    }

    private async void DebtorsClick(object? sender, RoutedEventArgs e)
    {
        try { Show("debtors", await StaffService.DebtorsReportAsync()); }
        catch (Exception ex) { MainWindow.Instance.ShowError(ex); }
    }

    private void SaveClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (current.Count == 0)
                throw new AppException("Сначала сформируйте отчёт.");
            string path = StaffService.SaveCsv(currentName, current);
            MainWindow.Instance.ShowMessage("Отчёт сохранён: " + path);
        }
        catch (Exception ex) { MainWindow.Instance.ShowError(ex); }
    }
}
