namespace LibraryApp.Models;

// Созданные scaffold файлы не редактируются: добавочные
// свойства вынесены в продолжения (partial) тех же классов.
public partial class User
{
    // ФИО одной строкой; "-" в отчестве означает, что его нет.
    public string FullName =>
        $"{LastName} {FirstName} " +
        (MiddleName == "-" ? "" : MiddleName).Trim();

    public override string ToString() => $"{Login} - {FullName}";
}

public partial class Author
{
    // «Фамилия И.» для подписи в карточке издания.
    public string ShortName => $"{LastName} {FirstName[0]}.";
}

public partial class BookCopy
{
    public override string ToString() => InventoryNumber;
}

public partial class Loan
{
    public override string ToString() =>
        $"Запись № {LoanId} · {BookingDate:dd.MM.yyyy}";
}
