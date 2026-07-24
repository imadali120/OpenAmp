using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using Microsoft.Win32;
using OpenAmp.Desktop.Infrastructure;
using OpenAmp.Desktop.Models;
using OpenAmp.Desktop.Services;
using OpenAmp.Desktop.Views;

namespace OpenAmp.Desktop.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly OpenAmpApiClient _api;
    private readonly AuthSession _session;
    private List<HallItem> _allHalls = [];
    private List<EquipmentItem> _allEquipment = [];
    private List<ArticleItem> _allArticles = [];
    private List<BandItem> _allBands = [];
    private List<UserItem> _allUsers = [];
    private int _selectedPage;
    private bool _isBusy;
    private string _status = "Povezivanje sa API-jem…";
    private string _hallSearch = "";
    private string _equipmentSearch = "";
    private string _bandSearch = "";
    private DateOnly _weekStart = StartOfWeek(DateOnly.FromDateTime(DateTime.Today));

    public MainViewModel(OpenAmpApiClient api, AuthSession session)
    {
        _api = api;
        _session = session;
        NavigateCommand = new RelayCommand(parameter => SelectedPage = Convert.ToInt32(parameter, CultureInfo.InvariantCulture));
        RefreshCommand = new AsyncRelayCommand(_ => LoadCurrentPageAsync());
        AddHallCommand = new AsyncRelayCommand(_ => EditHallAsync(null));
        EditHallCommand = new AsyncRelayCommand(item => EditHallAsync(item as HallItem));
        DeactivateHallCommand = new AsyncRelayCommand(item => DeactivateHallAsync(item as HallItem));
        UploadHallImageCommand = new AsyncRelayCommand(item => UploadHallImageAsync(item as HallItem));
        AddEquipmentCommand = new AsyncRelayCommand(_ => EditEquipmentAsync(null));
        EditEquipmentCommand = new AsyncRelayCommand(item => EditEquipmentAsync(item as EquipmentItem));
        ReportServiceCommand = new AsyncRelayCommand(item => ReportServiceAsync(item as EquipmentItem));
        CompleteServiceCommand = new AsyncRelayCommand(item => CompleteServiceAsync(item as EquipmentItem));
        AddArticleCommand = new AsyncRelayCommand(_ => EditArticleAsync(null));
        EditArticleCommand = new AsyncRelayCommand(item => EditArticleAsync(item as ArticleItem));
        AddReservationCommand = new AsyncRelayCommand(_ => EditReservationAsync(null));
        EditReservationCommand = new AsyncRelayCommand(item => EditReservationAsync(item as ReservationItem));
        PreviousWeekCommand = new AsyncRelayCommand(_ => MoveWeekAsync(-7));
        NextWeekCommand = new AsyncRelayCommand(_ => MoveWeekAsync(7));
        EditBandCommand = new AsyncRelayCommand(item => EditBandAsync(item as BandItem));
        EditUserCommand = new AsyncRelayCommand(item => EditUserAsync(item as UserItem), _ => IsAdmin);
        ApplyFiltersCommand = new RelayCommand(_ => ApplyFilters());
        _ = InitializeAsync();
    }

    public string CurrentUser => $"{_session.Korisnik.Ime} {_session.Korisnik.Prezime}";
    public string CurrentRole => _session.Korisnik.Uloga;
    public string Initials => string.Concat(
        _session.Korisnik.Ime.FirstOrDefault(),
        _session.Korisnik.Prezime.FirstOrDefault()).ToUpperInvariant();
    public bool IsAdmin => _session.Korisnik.Uloga == "Administrator";
    public DesktopLookups Lookups { get; private set; } = new();
    public DashboardData Dashboard { get; private set; } = new();
    public ObservableCollection<HallItem> Halls { get; } = [];
    public ObservableCollection<EquipmentItem> Equipment { get; } = [];
    public ObservableCollection<ArticleItem> Articles { get; } = [];
    public ObservableCollection<BandItem> Bands { get; } = [];
    public ObservableCollection<UserItem> Users { get; } = [];
    public ObservableCollection<WeekDayColumn> WeekDays { get; } = [];

    public int SelectedPage
    {
        get => _selectedPage;
        set
        {
            if (SetProperty(ref _selectedPage, value))
            {
                OnPropertyChanged(nameof(PageTitle));
                _ = LoadCurrentPageAsync();
            }
        }
    }

    public string PageTitle => SelectedPage switch
    {
        0 => "Dashboard",
        1 => "Sale",
        2 => "Oprema",
        3 => "Rezervacije",
        4 => "Bendovi",
        5 => "Artikli",
        6 => "Korisnici",
        _ => "OpenAmp"
    };

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    public string Status
    {
        get => _status;
        private set => SetProperty(ref _status, value);
    }

    public string HallSearch
    {
        get => _hallSearch;
        set => SetProperty(ref _hallSearch, value);
    }

    public string EquipmentSearch
    {
        get => _equipmentSearch;
        set => SetProperty(ref _equipmentSearch, value);
    }

    public string BandSearch
    {
        get => _bandSearch;
        set => SetProperty(ref _bandSearch, value);
    }

    public string WeekLabel => $"{_weekStart:dd.MM.} – {_weekStart.AddDays(6):dd.MM.yyyy}";

    public RelayCommand NavigateCommand { get; }
    public AsyncRelayCommand RefreshCommand { get; }
    public AsyncRelayCommand AddHallCommand { get; }
    public AsyncRelayCommand EditHallCommand { get; }
    public AsyncRelayCommand DeactivateHallCommand { get; }
    public AsyncRelayCommand UploadHallImageCommand { get; }
    public AsyncRelayCommand AddEquipmentCommand { get; }
    public AsyncRelayCommand EditEquipmentCommand { get; }
    public AsyncRelayCommand ReportServiceCommand { get; }
    public AsyncRelayCommand CompleteServiceCommand { get; }
    public AsyncRelayCommand AddArticleCommand { get; }
    public AsyncRelayCommand EditArticleCommand { get; }
    public AsyncRelayCommand AddReservationCommand { get; }
    public AsyncRelayCommand EditReservationCommand { get; }
    public AsyncRelayCommand PreviousWeekCommand { get; }
    public AsyncRelayCommand NextWeekCommand { get; }
    public AsyncRelayCommand EditBandCommand { get; }
    public AsyncRelayCommand EditUserCommand { get; }
    public RelayCommand ApplyFiltersCommand { get; }

    private async Task InitializeAsync()
    {
        await RunAsync(async () =>
        {
            Lookups = await _api.GetLookupsAsync();
            _allHalls = await _api.GetHallsAsync();
            _allBands = await _api.GetBandsAsync();
            Replace(Halls, _allHalls);
            Replace(Bands, _allBands);
            await LoadDashboardAsync();
        }, "Podaci su učitani.");
    }

    private Task LoadCurrentPageAsync() => RunAsync(async () =>
    {
        switch (SelectedPage)
        {
            case 0:
                await LoadDashboardAsync();
                break;
            case 1:
                _allHalls = await _api.GetHallsAsync();
                ApplyFilters();
                break;
            case 2:
                _allEquipment = await _api.GetEquipmentAsync();
                ApplyFilters();
                break;
            case 3:
                await LoadReservationsAsync();
                break;
            case 4:
                _allBands = await _api.GetBandsAsync();
                ApplyFilters();
                break;
            case 5:
                _allArticles = await _api.GetArticlesAsync();
                Replace(Articles, _allArticles);
                break;
            case 6:
                _allUsers = await _api.GetUsersAsync();
                Replace(Users, _allUsers);
                break;
        }
    }, $"{PageTitle} je osvježen.");

    private async Task LoadDashboardAsync()
    {
        Dashboard = await _api.GetDashboardAsync();
        OnPropertyChanged(nameof(Dashboard));
    }

    private async Task LoadReservationsAsync()
    {
        var fromLocal = _weekStart.ToDateTime(new TimeOnly(0, 0), DateTimeKind.Local);
        var toLocal = _weekStart.AddDays(7).ToDateTime(new TimeOnly(0, 0), DateTimeKind.Local);
        var reservations = await _api.GetReservationsAsync(fromLocal.ToUniversalTime(), toLocal.ToUniversalTime());
        WeekDays.Clear();
        var culture = CultureInfo.GetCultureInfo("bs-Latn-BA");
        for (var offset = 0; offset < 7; offset++)
        {
            var date = _weekStart.AddDays(offset);
            WeekDays.Add(new WeekDayColumn
            {
                Datum = date,
                Dan = culture.DateTimeFormat.GetAbbreviatedDayName(date.DayOfWeek).TrimEnd('.').ToUpperInvariant(),
                DatumTekst = date.ToString("dd.MM.", CultureInfo.InvariantCulture),
                Rezervacije = reservations
                    .Where(x => DateOnly.FromDateTime(x.TerminOdUtc.ToLocalTime()) == date)
                    .OrderBy(x => x.TerminOdUtc)
                    .ToList()
            });
        }
        OnPropertyChanged(nameof(WeekLabel));
    }

    private async Task EditHallAsync(HallItem? item)
    {
        var owner = Owner();
        var data = EditorDialogs.Hall(owner, Lookups, item);
        if (data is null)
        {
            return;
        }
        await RunAsync(async () =>
        {
            await _api.SaveHallAsync(item?.Id, new
            {
                data.StudioId,
                data.Naziv,
                data.Kapacitet,
                data.CijenaPoSatu,
                data.StatusId,
                data.Opis,
                data.Akustika
            });
            _allHalls = await _api.GetHallsAsync();
            ApplyFilters();
        }, "Sala je sačuvana.");
    }

    private async Task DeactivateHallAsync(HallItem? item)
    {
        if (item is null || MessageBox.Show(
            Owner(), $"Deaktivirati salu „{item.Naziv}”?", "OpenAmp",
            MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
        {
            return;
        }
        await RunAsync(async () =>
        {
            await _api.DeactivateHallAsync(item.Id);
            _allHalls = await _api.GetHallsAsync();
            ApplyFilters();
        }, "Sala je deaktivirana.");
    }

    private async Task UploadHallImageAsync(HallItem? item)
    {
        if (item is null)
        {
            return;
        }
        var dialog = new OpenFileDialog
        {
            Filter = "Slike|*.jpg;*.jpeg;*.png;*.webp",
            Multiselect = false
        };
        if (dialog.ShowDialog(Owner()) != true)
        {
            return;
        }
        await RunAsync(async () =>
        {
            await _api.UploadHallImageAsync(item.Id, dialog.FileName);
            _allHalls = await _api.GetHallsAsync();
            ApplyFilters();
        }, "Fotografija sale je dodana.");
    }

    private async Task EditEquipmentAsync(EquipmentItem? item)
    {
        var data = EditorDialogs.Equipment(Owner(), Lookups, _allHalls, item);
        if (data is null)
        {
            return;
        }
        await RunAsync(async () =>
        {
            await _api.SaveEquipmentAsync(item?.Id, new
            {
                data.InventarskiBroj,
                data.Naziv,
                data.Opis,
                data.SerijskiBroj,
                data.CijenaNajmaPoSatu,
                data.Stanje,
                data.DatumNabavke,
                data.Napomena,
                data.KategorijaId,
                data.StatusId,
                data.SalaId
            });
            _allEquipment = await _api.GetEquipmentAsync();
            ApplyFilters();
        }, "Oprema je sačuvana.");
    }

    private async Task ReportServiceAsync(EquipmentItem? item)
    {
        if (item is null)
        {
            return;
        }
        var description = EditorDialogs.ReportService(Owner(), item);
        if (description is null)
        {
            return;
        }
        await RunAsync(async () =>
        {
            await _api.ReportServiceAsync(item.Id, description);
            _allEquipment = await _api.GetEquipmentAsync();
            ApplyFilters();
        }, "Kvar je prijavljen i oprema je označena za servis.");
    }

    private async Task CompleteServiceAsync(EquipmentItem? item)
    {
        if (item is null)
        {
            return;
        }
        var open = item.ServisnaHistorija.FirstOrDefault(x => !x.ZavrsenUtc.HasValue);
        var data = EditorDialogs.CompleteService(Owner(), Lookups, item);
        if (open is null || data is null)
        {
            return;
        }
        await RunAsync(async () =>
        {
            await _api.CompleteServiceAsync(item.Id, open.Id, new
            {
                data.IzvrseniRadovi,
                data.Trosak,
                data.Stanje,
                data.StatusId
            });
            _allEquipment = await _api.GetEquipmentAsync();
            ApplyFilters();
        }, "Servis je završen.");
    }

    private async Task EditArticleAsync(ArticleItem? item)
    {
        var data = EditorDialogs.Article(Owner(), Lookups, item);
        if (data is null)
        {
            return;
        }
        await RunAsync(async () =>
        {
            await _api.SaveArticleAsync(item?.Id, new
            {
                data.InventarskiBroj,
                data.Naziv,
                data.Opis,
                data.KolicinaNaStanju,
                data.MinimalnaZaliha,
                data.Cijena,
                data.KategorijaId,
                data.StatusId,
                data.StudioId
            });
            _allArticles = await _api.GetArticlesAsync();
            Replace(Articles, _allArticles);
        }, "Artikal je sačuvan.");
    }

    private async Task EditReservationAsync(ReservationItem? item)
    {
        var data = EditorDialogs.Reservation(Owner(), Lookups, _allHalls, _allBands, item);
        if (data is null)
        {
            return;
        }
        await RunAsync(async () =>
        {
            object request = item is null
                ? new
                {
                    data.SalaId,
                    data.BendId,
                    data.TerminOdUtc,
                    data.TerminDoUtc,
                    data.Napomena
                }
                : new
                {
                    data.SalaId,
                    data.TerminOdUtc,
                    data.TerminDoUtc,
                    StatusId = data.StatusId!.Value,
                    data.Napomena,
                    item.RowVersion
                };
            await _api.SaveReservationAsync(item?.Id, request);
            await LoadReservationsAsync();
        }, "Rezervacija je sačuvana.");
    }

    private async Task MoveWeekAsync(int days)
    {
        _weekStart = _weekStart.AddDays(days);
        await RunAsync(LoadReservationsAsync, "Sedmični raspored je učitan.");
    }

    private async Task EditBandAsync(BandItem? item)
    {
        if (item is null)
        {
            return;
        }
        var data = EditorDialogs.Band(Owner(), Lookups, item);
        if (data is null)
        {
            return;
        }
        await RunAsync(async () =>
        {
            await _api.SaveBandAsync(item.Id, new { data.Naziv, data.ZanrId, data.Opis });
            _allBands = await _api.GetBandsAsync();
            ApplyFilters();
        }, "Podaci benda su sačuvani.");
    }

    private async Task EditUserAsync(UserItem? item)
    {
        if (item is null || !IsAdmin)
        {
            return;
        }
        var data = EditorDialogs.User(Owner(), Lookups, item);
        if (data is null)
        {
            return;
        }
        await RunAsync(async () =>
        {
            await _api.SaveUserAsync(item.Id, new { data.UlogaId, data.Aktivan });
            _allUsers = await _api.GetUsersAsync();
            Replace(Users, _allUsers);
        }, "Korisnički račun je ažuriran.");
    }

    private void ApplyFilters()
    {
        Replace(Halls, _allHalls.Where(x => Contains(x.Naziv, HallSearch) || Contains(x.Studio, HallSearch)));
        Replace(Equipment, _allEquipment.Where(x =>
            Contains(x.Naziv, EquipmentSearch) || Contains(x.InventarskiBroj, EquipmentSearch)
            || Contains(x.Kategorija, EquipmentSearch) || Contains(x.Sala, EquipmentSearch)));
        Replace(Bands, _allBands.Where(x =>
            Contains(x.Naziv, BandSearch) || Contains(x.Zanr, BandSearch)
            || x.Clanovi.Any(c => Contains(c.Username, BandSearch) || Contains(c.ImePrezime, BandSearch))));
    }

    private async Task RunAsync(Func<Task> action, string success)
    {
        IsBusy = true;
        try
        {
            await action();
            Status = success;
        }
        catch (Exception exception)
        {
            Status = exception.Message;
            MessageBox.Show(Owner(), exception.Message, "OpenAmp", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static Window Owner() => Application.Current.MainWindow;

    private static bool Contains(string? value, string search) =>
        string.IsNullOrWhiteSpace(search)
        || value?.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase) == true;

    private static void Replace<T>(ObservableCollection<T> target, IEnumerable<T> values)
    {
        target.Clear();
        foreach (var value in values)
        {
            target.Add(value);
        }
    }

    private static DateOnly StartOfWeek(DateOnly date)
    {
        var offset = ((int)date.DayOfWeek + 6) % 7;
        return date.AddDays(-offset);
    }
}
