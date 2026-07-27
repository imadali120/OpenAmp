using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using OpenAmp.Desktop.Infrastructure;
using OpenAmp.Desktop.Models;

namespace OpenAmp.Desktop.Views;

public sealed record HallEditData(
    int StudioId, string Naziv, int Kapacitet, decimal CijenaPoSatu,
    int StatusId, string? Opis, string? Akustika);

public sealed record EquipmentEditData(
    string InventarskiBroj, string Naziv, string? Opis, string? SerijskiBroj,
    decimal CijenaNajmaPoSatu, int Stanje, DateOnly? DatumNabavke,
    string? Napomena, int KategorijaId, int StatusId, int? SalaId);

public sealed record ArticleEditData(
    string InventarskiBroj, string Naziv, string? Opis, int KolicinaNaStanju,
    int MinimalnaZaliha, decimal Cijena, int KategorijaId, int StatusId, int StudioId);

public sealed record ReservationEditData(
    int SalaId, int BendId, DateTime TerminOdUtc, DateTime TerminDoUtc,
    int? StatusId, string? Napomena);

public sealed record BandEditData(string Naziv, int ZanrId, string? Opis);
public sealed record UserEditData(int UlogaId, bool Aktivan);
public sealed record CompleteServiceData(string IzvrseniRadovi, decimal Trosak, int Stanje, int StatusId);

public static class EditorDialogs
{
    public static HallEditData? Hall(
        Window owner,
        DesktopLookups lookups,
        HallItem? item)
    {
        var form = new FormDialog(owner, item is null ? "Nova sala" : "Uredi salu");
        form.Combo("studio", "Studio", lookups.Studiji, item?.StudioId);
        form.Text("name", "Naziv", item?.Naziv);
        form.Text("capacity", "Kapacitet", item?.Kapacitet.ToString(CultureInfo.InvariantCulture) ?? "6");
        form.Text("price", "Cijena po satu (KM)", item?.CijenaPoSatu.ToString(CultureInfo.InvariantCulture) ?? "30");
        form.Combo("status", "Status", lookups.StatusiSala, item?.StatusId);
        form.Text("description", "Opis", item?.Opis, true);
        form.Text("acoustics", "Akustika", item?.Akustika, true);
        if (form.ShowDialog() != true)
        {
            return null;
        }
        if (!int.TryParse(form.Value("capacity"), out var capacity)
            || !decimal.TryParse(form.Value("price"), NumberStyles.Number, CultureInfo.InvariantCulture, out var price))
        {
            Invalid(owner);
            return null;
        }
        return new HallEditData(
            form.SelectedId("studio"), form.Value("name"), capacity, price,
            form.SelectedId("status"), Null(form.Value("description")), Null(form.Value("acoustics")));
    }

    public static EquipmentEditData? Equipment(
        Window owner,
        DesktopLookups lookups,
        IReadOnlyCollection<HallItem> halls,
        EquipmentItem? item)
    {
        var hallOptions = halls.Select(x => new LookupItem { Id = x.Id, Naziv = $"{x.Naziv} · {x.Studio}" }).ToList();
        hallOptions.Insert(0, new LookupItem { Id = 0, Naziv = "Nije dodijeljena" });
        var form = new FormDialog(owner, item is null ? "Nova oprema" : "Uredi opremu");
        form.Text("inventory", "Inventarski broj", item?.InventarskiBroj);
        form.Text("name", "Naziv", item?.Naziv);
        form.Combo("category", "Kategorija", lookups.KategorijeOpreme, item?.KategorijaId);
        form.Combo("hall", "Dodijeljena sala", hallOptions, item?.SalaId ?? 0);
        form.Text("price", "Cijena najma po satu", item?.CijenaNajmaPoSatu.ToString(CultureInfo.InvariantCulture) ?? "0");
        form.Text("condition", "Stanje (1–5)", item?.Stanje.ToString(CultureInfo.InvariantCulture) ?? "5");
        form.Combo("status", "Status", lookups.StatusiOpreme, item?.StatusId);
        form.Text("serial", "Serijski broj", item?.SerijskiBroj);
        form.Date("purchase", "Datum nabavke", item?.DatumNabavke);
        form.Text("description", "Opis", item?.Opis, true);
        form.Text("note", "Napomena", item?.Napomena, true);
        if (form.ShowDialog() != true)
        {
            return null;
        }
        if (!decimal.TryParse(form.Value("price"), NumberStyles.Number, CultureInfo.InvariantCulture, out var price)
            || !int.TryParse(form.Value("condition"), out var condition))
        {
            Invalid(owner);
            return null;
        }
        var hallId = form.SelectedId("hall");
        return new EquipmentEditData(
            form.Value("inventory"), form.Value("name"), Null(form.Value("description")),
            Null(form.Value("serial")), price, condition, form.DateValue("purchase"),
            Null(form.Value("note")), form.SelectedId("category"), form.SelectedId("status"),
            hallId == 0 ? null : hallId);
    }

    public static ArticleEditData? Article(
        Window owner,
        DesktopLookups lookups,
        ArticleItem? item)
    {
        var form = new FormDialog(owner, item is null ? "Novi artikal" : "Uredi artikal");
        form.Text("inventory", "Inventarski broj", item?.InventarskiBroj);
        form.Text("name", "Naziv", item?.Naziv);
        form.Combo("studio", "Studio", lookups.Studiji, item?.StudioId);
        form.Combo("category", "Kategorija", lookups.KategorijeArtikala, item?.KategorijaId);
        form.Text("stock", "Količina na stanju", item?.KolicinaNaStanju.ToString(CultureInfo.InvariantCulture) ?? "0");
        form.Text("minimum", "Minimalna zaliha", item?.MinimalnaZaliha.ToString(CultureInfo.InvariantCulture) ?? "5");
        form.Text("price", "Prodajna cijena", item?.Cijena.ToString(CultureInfo.InvariantCulture) ?? "0");
        form.Combo("status", "Status", lookups.StatusiArtikala, item?.StatusId);
        form.Text("description", "Opis", item?.Opis, true);
        if (form.ShowDialog() != true)
        {
            return null;
        }
        if (!int.TryParse(form.Value("stock"), out var stock)
            || !int.TryParse(form.Value("minimum"), out var minimum)
            || !decimal.TryParse(form.Value("price"), NumberStyles.Number, CultureInfo.InvariantCulture, out var price))
        {
            Invalid(owner);
            return null;
        }
        return new ArticleEditData(
            form.Value("inventory"), form.Value("name"), Null(form.Value("description")),
            stock, minimum, price, form.SelectedId("category"), form.SelectedId("status"), form.SelectedId("studio"));
    }

    public static ReservationEditData? Reservation(
        Window owner,
        DesktopLookups lookups,
        IReadOnlyCollection<HallItem> halls,
        IReadOnlyCollection<BandItem> bands,
        ReservationItem? item)
    {
        var hallOptions = halls.Select(x => new LookupItem { Id = x.Id, Naziv = $"{x.Naziv} · {x.Studio}" }).ToList();
        var bandOptions = bands.Select(x => new LookupItem { Id = x.Id, Naziv = $"{x.Naziv} · {x.Zanr}" }).ToList();
        var localStart = item?.TerminOdUtc.ToLocalTime() ?? DateTime.Today.AddDays(1).AddHours(18);
        var localEnd = item?.TerminDoUtc.ToLocalTime() ?? localStart.AddHours(2);
        var form = new FormDialog(owner, item is null ? "Nova rezervacija" : "Uredi rezervaciju");
        form.Combo("hall", "Sala", hallOptions, item?.SalaId);
        form.Combo("band", "Bend", bandOptions, item?.BendId, item is not null);
        form.Date("date", "Datum", DateOnly.FromDateTime(localStart));
        form.Text("from", "Početak (HH:mm)", localStart.ToString("HH:mm", CultureInfo.InvariantCulture));
        form.Text("to", "Kraj (HH:mm)", localEnd.ToString("HH:mm", CultureInfo.InvariantCulture));
        if (item is not null)
        {
            form.Combo("status", "Status", lookups.StatusiRezervacija, item.StatusId);
        }
        form.Text("note", "Napomena", item?.Napomena, true);
        if (form.ShowDialog() != true)
        {
            return null;
        }
        var date = form.DateValue("date");
        if (!date.HasValue
            || !TimeOnly.TryParseExact(form.Value("from"), "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var from)
            || !TimeOnly.TryParseExact(form.Value("to"), "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var to))
        {
            Invalid(owner);
            return null;
        }
        var startLocal = DateTime.SpecifyKind(date.Value.ToDateTime(from), DateTimeKind.Local);
        var endLocal = DateTime.SpecifyKind(date.Value.ToDateTime(to), DateTimeKind.Local);
        return new ReservationEditData(
            form.SelectedId("hall"), form.SelectedId("band"), startLocal.ToUniversalTime(), endLocal.ToUniversalTime(),
            item is null ? null : form.SelectedId("status"), Null(form.Value("note")));
    }

    public static BandEditData? Band(Window owner, DesktopLookups lookups, BandItem item)
    {
        var form = new FormDialog(owner, "Uredi bend");
        form.Text("name", "Naziv", item.Naziv);
        form.Combo("genre", "Žanr", lookups.Zanrovi, item.ZanrId);
        form.Text("description", "Opis", item.Opis, true);
        return form.ShowDialog() == true
            ? new BandEditData(form.Value("name"), form.SelectedId("genre"), Null(form.Value("description")))
            : null;
    }

    public static UserEditData? User(Window owner, DesktopLookups lookups, UserItem item)
    {
        var form = new FormDialog(owner, $"Korisnik @{item.Username}");
        form.Combo("role", "Uloga", lookups.Uloge, item.UlogaId);
        form.Check("active", "Aktivan račun", item.Aktivan);
        return form.ShowDialog() == true
            ? new UserEditData(form.SelectedId("role"), form.Checked("active"))
            : null;
    }

    public static string? ReportService(Window owner, EquipmentItem item)
    {
        var form = new FormDialog(owner, $"Prijavi kvar · {item.Naziv}");
        form.Text("description", "Opis kvara", null, true);
        return form.ShowDialog() == true ? Null(form.Value("description")) : null;
    }

    public static CompleteServiceData? CompleteService(
        Window owner,
        DesktopLookups lookups,
        EquipmentItem item)
    {
        var open = item.ServisnaHistorija.FirstOrDefault(x => !x.ZavrsenUtc.HasValue);
        if (open is null)
        {
            MessageBox.Show(owner, "Oprema nema otvoren servis.", "OpenAmp", MessageBoxButton.OK, MessageBoxImage.Information);
            return null;
        }
        var available = lookups.StatusiOpreme.Where(x => x.Kod != "SERVIS").ToList();
        var form = new FormDialog(owner, $"Završi servis · {item.Naziv}");
        form.Text("work", "Izvršeni radovi", null, true);
        form.Text("cost", "Trošak (KM)", "0");
        form.Text("condition", "Novo stanje (1–5)", item.Stanje.ToString(CultureInfo.InvariantCulture));
        form.Combo("status", "Status nakon servisa", available, available.FirstOrDefault(x => x.Kod == "DOSTUPNA")?.Id);
        if (form.ShowDialog() != true)
        {
            return null;
        }
        if (!decimal.TryParse(form.Value("cost"), NumberStyles.Number, CultureInfo.InvariantCulture, out var cost)
            || !int.TryParse(form.Value("condition"), out var condition))
        {
            Invalid(owner);
            return null;
        }
        return new CompleteServiceData(form.Value("work"), cost, condition, form.SelectedId("status"));
    }

    private static string? Null(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static void Invalid(Window owner) =>
        MessageBox.Show(owner, "Provjeri unesene brojčane vrijednosti i vrijeme.", "Neispravan unos",
            MessageBoxButton.OK, MessageBoxImage.Warning);
}

internal sealed class FormDialog : Window
{
    private readonly StackPanel _fields = new();
    private readonly Dictionary<string, Control> _controls = [];

    public FormDialog(Window owner, string title)
    {
        WindowAppearance.UseOpenAmpChrome(this);
        Owner = owner;
        Title = title;
        Width = 520;
        MaxHeight = 800;
        SizeToContent = SizeToContent.Height;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ResizeMode = ResizeMode.NoResize;
        Background = (System.Windows.Media.Brush)FindResource("BackgroundBrush");
        Foreground = (System.Windows.Media.Brush)FindResource("TextBrush");
        FontFamily = new System.Windows.Media.FontFamily("Segoe UI Variable Text, Segoe UI");
        var root = new Grid
        {
            Margin = new Thickness(28),
            Background = (System.Windows.Media.Brush)FindResource("BackgroundBrush")
        };
        root.RowDefinitions.Add(new RowDefinition());
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        var scroll = new ScrollViewer
        {
            Content = _fields,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            MaxHeight = 650
        };
        root.Children.Add(scroll);
        var actions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 20, 0, 0)
        };
        var cancel = new Button { Content = "Odustani", MinWidth = 100, Margin = new Thickness(0, 0, 10, 0) };
        cancel.Click += (_, _) => DialogResult = false;
        var save = new Button { Content = "Sačuvaj", MinWidth = 110 };
        save.SetResourceReference(StyleProperty, "AccentButton");
        save.Click += (_, _) => DialogResult = true;
        actions.Children.Add(cancel);
        actions.Children.Add(save);
        Grid.SetRow(actions, 1);
        root.Children.Add(actions);
        Content = root;
    }

    public void Text(string key, string label, string? value, bool multiline = false)
    {
        var input = new TextBox
        {
            Text = value ?? "",
            MinHeight = multiline ? 74 : 38,
            AcceptsReturn = multiline,
            TextWrapping = multiline ? TextWrapping.Wrap : TextWrapping.NoWrap,
            VerticalScrollBarVisibility = multiline ? ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled
        };
        Add(key, label, input);
    }

    public void Combo(string key, string label, IReadOnlyCollection<LookupItem> values, int? selectedId, bool disabled = false)
    {
        var combo = new ComboBox
        {
            ItemsSource = values,
            DisplayMemberPath = nameof(LookupItem.Naziv),
            SelectedValuePath = nameof(LookupItem.Id),
            SelectedValue = selectedId ?? values.FirstOrDefault()?.Id,
            Height = 38,
            IsEnabled = !disabled
        };
        Add(key, label, combo);
    }

    public void Date(string key, string label, DateOnly? value)
    {
        var picker = new DatePicker
        {
            SelectedDate = value?.ToDateTime(new TimeOnly(0, 0)),
            Height = 38
        };
        Add(key, label, picker);
    }

    public void Check(string key, string label, bool value)
    {
        var check = new CheckBox { Content = label, IsChecked = value, Margin = new Thickness(0, 8, 0, 8) };
        _fields.Children.Add(check);
        _controls[key] = check;
    }

    public string Value(string key) => ((TextBox)_controls[key]).Text.Trim();
    public int SelectedId(string key) => (int)(((ComboBox)_controls[key]).SelectedValue ?? 0);
    public bool Checked(string key) => ((CheckBox)_controls[key]).IsChecked == true;
    public DateOnly? DateValue(string key) =>
        ((DatePicker)_controls[key]).SelectedDate is DateTime value ? DateOnly.FromDateTime(value) : null;

    private void Add(string key, string label, Control control)
    {
        _fields.Children.Add(new TextBlock
        {
            Text = label,
            Foreground = (System.Windows.Media.Brush)FindResource("MutedBrush"),
            Margin = new Thickness(0, 0, 0, 6)
        });
        control.Margin = new Thickness(0, 0, 0, 15);
        _fields.Children.Add(control);
        _controls[key] = control;
    }
}
