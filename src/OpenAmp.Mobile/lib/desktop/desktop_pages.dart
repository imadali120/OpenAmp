import 'dart:math' as math;

import 'package:file_selector/file_selector.dart';
import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:openamp_mobile/core/config/app_config.dart';
import 'package:openamp_mobile/core/theme/app_theme.dart';
import 'package:openamp_mobile/desktop/admin_repository.dart';
import 'package:printing/printing.dart';

final _money = NumberFormat.currency(locale: 'bs_BA', symbol: 'KM');
final _dateTime = DateFormat('dd.MM.yyyy. HH:mm');

class DesktopPageFrame extends StatelessWidget {
  const DesktopPageFrame({
    super.key,
    required this.title,
    required this.child,
    this.subtitle,
    this.actions = const [],
  });

  final String title;
  final String? subtitle;
  final Widget child;
  final List<Widget> actions;

  @override
  Widget build(BuildContext context) => Column(
    crossAxisAlignment: CrossAxisAlignment.stretch,
    children: [
      Container(
        height: 92,
        padding: const EdgeInsets.symmetric(horizontal: 30),
        decoration: const BoxDecoration(
          border: Border(bottom: BorderSide(color: AppColors.line)),
        ),
        child: Row(
          children: [
            Expanded(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    title,
                    style: Theme.of(context).textTheme.headlineMedium,
                  ),
                  if (subtitle != null) ...[
                    const SizedBox(height: 5),
                    Text(
                      subtitle!,
                      style: Theme.of(context).textTheme.bodyMedium,
                    ),
                  ],
                ],
              ),
            ),
            ...actions,
          ],
        ),
      ),
      Expanded(
        child: Padding(padding: const EdgeInsets.all(28), child: child),
      ),
    ],
  );
}

class DashboardPage extends StatefulWidget {
  const DashboardPage({super.key, required this.repository});
  final AdminRepository repository;

  @override
  State<DashboardPage> createState() => _DashboardPageState();
}

class _DashboardPageState extends State<DashboardPage> {
  late Future<Map<String, dynamic>> future = widget.repository.dashboard();

  void reload() => setState(() => future = widget.repository.dashboard());

  @override
  Widget build(BuildContext context) => DesktopPageFrame(
    title: 'Pregled',
    subtitle: DateFormat('EEEE, d. MMMM', 'bs').format(DateTime.now()),
    actions: [_RefreshButton(onPressed: reload)],
    child: FutureBuilder(
      future: future,
      builder: (context, snapshot) {
        if (snapshot.connectionState != ConnectionState.done) {
          return const Center(child: CircularProgressIndicator());
        }
        if (snapshot.hasError) return _ErrorState(snapshot.error, reload);
        final data = snapshot.data!;
        final reservations =
            (data['rasporedDanas'] as List<dynamic>? ?? const [])
                .cast<Map<String, dynamic>>();
        final occupancy =
            (data['zauzetostSedmica'] as List<dynamic>? ?? const [])
                .cast<Map<String, dynamic>>();
        return ListView(
          children: [
            Wrap(
              spacing: 14,
              runSpacing: 14,
              children: [
                _Metric('Probe danas', data['danasnjeProbe'] ?? 0),
                _Metric('Aktivne sale', data['aktivneSale'] ?? 0),
                _Metric('Oprema na najmu', data['opremaNaNajmu'] ?? 0),
                _Metric(
                  'Niske zalihe',
                  data['niskeZalihe'] ?? 0,
                  alert: (data['niskeZalihe'] ?? 0) > 0,
                ),
              ],
            ),
            const SizedBox(height: 24),
            Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Expanded(
                  flex: 6,
                  child: _Panel(
                    title: 'Današnji raspored',
                    child: reservations.isEmpty
                        ? const _EmptyState('Nema rezervacija za danas.')
                        : Column(
                            children: reservations
                                .map(
                                  (item) => _ScheduleRow(
                                    time: item['vrijeme']?.toString() ?? '',
                                    band: item['bend']?.toString() ?? '',
                                    hall: item['sala']?.toString() ?? '',
                                    status: item['status']?.toString() ?? '',
                                  ),
                                )
                                .toList(),
                          ),
                  ),
                ),
                const SizedBox(width: 18),
                Expanded(
                  flex: 4,
                  child: _Panel(
                    title: 'Zauzetost ove sedmice',
                    child: occupancy.isEmpty
                        ? const _EmptyState('Još nema podataka.')
                        : Column(
                            children: occupancy.take(8).map((item) {
                              final value =
                                  (item['postotak'] as num?)?.toDouble() ?? 0;
                              return Padding(
                                padding: const EdgeInsets.symmetric(
                                  vertical: 8,
                                ),
                                child: Column(
                                  children: [
                                    Row(
                                      children: [
                                        Expanded(
                                          child: Text(
                                            _occupancyLabel(item),
                                            style: const TextStyle(
                                              fontWeight: FontWeight.w700,
                                            ),
                                          ),
                                        ),
                                        Text('${value.round()}%'),
                                      ],
                                    ),
                                    const SizedBox(height: 7),
                                    LinearProgressIndicator(
                                      value: value / 100,
                                      minHeight: 6,
                                      borderRadius: BorderRadius.circular(4),
                                    ),
                                  ],
                                ),
                              );
                            }).toList(),
                          ),
                  ),
                ),
              ],
            ),
          ],
        );
      },
    ),
  );

  String _occupancyLabel(Map<String, dynamic> item) {
    final date = DateTime.tryParse(item['datum']?.toString() ?? '');
    final suffix = date == null
        ? ''
        : ' · ${DateFormat('EEE dd.MM.', 'bs').format(date)}';
    return '${item['sala'] ?? ''}$suffix';
  }
}

class HallsPage extends StatelessWidget {
  const HallsPage({super.key, required this.repository});
  final AdminRepository repository;

  @override
  Widget build(BuildContext context) => _CollectionPage(
    title: 'Sale',
    subtitle: 'Prostor, cijene i raspoloživost',
    searchHint: 'Pretraži sale ili studio',
    load: (query) => repository.halls(search: query),
    columns: const [
      _Column('', 'slikaUrl', image: true),
      _Column('Naziv', 'naziv'),
      _Column('Studio', 'studio'),
      _Column('Kapacitet', 'kapacitet'),
      _Column('Cijena / sat', 'cijenaPoSatu', money: true),
      _Column('Status', 'status', badge: true),
    ],
    onEdit: (context, item) async {
      final lookups = await repository.lookups();
      if (!context.mounted) return false;
      final result = await _showEntityDialog(
        context,
        title: item == null ? 'Nova sala' : 'Uredi salu',
        initial: item,
        fields: [
          _Field.text('naziv', 'Naziv', required: true),
          _Field.dropdown('studioId', 'Studio', lookups['studiji']),
          _Field.integer('kapacitet', 'Kapacitet', minimum: 1),
          _Field.decimal('cijenaPoSatu', 'Cijena po satu', minimum: 0),
          _Field.dropdown('statusId', 'Status', lookups['statusiSala']),
          _Field.text('akustika', 'Akustika'),
          _Field.text('opis', 'Opis', lines: 3),
        ],
      );
      if (result == null) return false;
      await repository.saveHall(result, id: item?['id'] as int?);
      return true;
    },
    onDelete: (item) => repository.deleteHall(item['id'] as int),
  );
}

class EquipmentPage extends StatelessWidget {
  const EquipmentPage({super.key, required this.repository});
  final AdminRepository repository;

  @override
  Widget build(BuildContext context) => _CollectionPage(
    title: 'Oprema',
    subtitle: 'Inventar i servisno stanje',
    load: (_) => repository.equipment(),
    columns: const [
      _Column('Inventarski broj', 'inventarskiBroj'),
      _Column('Naziv', 'naziv'),
      _Column('Kategorija', 'kategorija'),
      _Column('Sala', 'sala'),
      _Column('Stanje', 'stanje'),
      _Column('Status', 'status', badge: true),
    ],
    onEdit: (context, item) async {
      final values = await Future.wait([
        repository.lookups(),
        repository.halls(),
      ]);
      if (!context.mounted) return false;
      final lookups = values[0] as Map<String, dynamic>;
      final halls = values[1] as List<Map<String, dynamic>>;
      final result = await _showEntityDialog(
        context,
        title: item == null ? 'Nova oprema' : 'Uredi opremu',
        initial: item,
        fields: [
          _Field.text('inventarskiBroj', 'Inventarski broj', required: true),
          _Field.text('naziv', 'Naziv', required: true),
          _Field.text('serijskiBroj', 'Serijski broj'),
          _Field.dropdown(
            'kategorijaId',
            'Kategorija',
            lookups['kategorijeOpreme'],
          ),
          _Field.dropdown('statusId', 'Status', lookups['statusiOpreme']),
          _Field.dropdown('salaId', 'Sala', halls, optional: true),
          _Field.integer('stanje', 'Stanje', minimum: 0),
          _Field.decimal(
            'cijenaNajmaPoSatu',
            'Cijena najma po satu',
            minimum: 0,
          ),
          _Field.text('datumNabavke', 'Datum nabavke (YYYY-MM-DD)'),
          _Field.text('opis', 'Opis', lines: 2),
          _Field.text('napomena', 'Napomena', lines: 2),
        ],
      );
      if (result == null) return false;
      await repository.saveEquipment(result, id: item?['id'] as int?);
      return true;
    },
  );
}

class ArticlesPage extends StatelessWidget {
  const ArticlesPage({super.key, required this.repository});
  final AdminRepository repository;

  @override
  Widget build(BuildContext context) => _CollectionPage(
    title: 'Artikli',
    subtitle: 'Prodajni artikli i stanje zaliha',
    searchHint: 'Pretraži naziv, broj ili studio',
    load: (query) => repository.articles(search: query),
    columns: const [
      _Column('Inventarski broj', 'inventarskiBroj'),
      _Column('Naziv', 'naziv'),
      _Column('Studio', 'studio'),
      _Column('Na stanju', 'kolicinaNaStanju'),
      _Column('Minimum', 'minimalnaZaliha'),
      _Column('Cijena', 'cijena', money: true),
      _Column('Status', 'status', badge: true),
    ],
    onEdit: (context, item) async {
      final lookups = await repository.lookups();
      if (!context.mounted) return false;
      final result = await _showEntityDialog(
        context,
        title: item == null ? 'Novi artikal' : 'Uredi artikal',
        initial: item,
        fields: [
          _Field.text('inventarskiBroj', 'Inventarski broj', required: true),
          _Field.text('naziv', 'Naziv', required: true),
          _Field.dropdown(
            'kategorijaId',
            'Kategorija',
            lookups['kategorijeArtikala'],
          ),
          _Field.dropdown('statusId', 'Status', lookups['statusiArtikala']),
          _Field.dropdown('studioId', 'Studio', lookups['studiji']),
          _Field.integer('kolicinaNaStanju', 'Količina', minimum: 0),
          _Field.integer('minimalnaZaliha', 'Minimalna zaliha', minimum: 0),
          _Field.decimal('cijena', 'Cijena', minimum: 0),
          _Field.text('opis', 'Opis', lines: 2),
        ],
      );
      if (result == null) return false;
      await repository.saveArticle(result, id: item?['id'] as int?);
      return true;
    },
  );
}

class ReservationsPage extends StatelessWidget {
  const ReservationsPage({super.key, required this.repository});
  final AdminRepository repository;

  @override
  Widget build(BuildContext context) => _CollectionPage(
    title: 'Rezervacije',
    subtitle: 'Termini u narednih 90 dana',
    load: (_) => repository.reservations(
      from: DateTime.now().subtract(const Duration(days: 30)),
      to: DateTime.now().add(const Duration(days: 90)),
    ),
    columns: const [
      _Column('Termin', 'terminOdUtc', dateTime: true),
      _Column('Bend', 'bend'),
      _Column('Sala', 'sala'),
      _Column('Žanr', 'zanr'),
      _Column('Ukupno', 'ukupnaCijena', money: true),
      _Column('Status', 'status', badge: true),
    ],
    canAdd: false,
    onEdit: (context, item) async {
      final values = await Future.wait([
        repository.lookups(),
        repository.halls(),
      ]);
      if (!context.mounted) return false;
      final lookups = values[0] as Map<String, dynamic>;
      final halls = values[1] as List<Map<String, dynamic>>;
      final result = await _showEntityDialog(
        context,
        title: 'Uredi rezervaciju',
        initial: item,
        fields: [
          _Field.dropdown('salaId', 'Sala', halls),
          _Field.dateTime('terminOdUtc', 'Početak'),
          _Field.dateTime('terminDoUtc', 'Kraj'),
          _Field.dropdown('statusId', 'Status', lookups['statusiRezervacija']),
          _Field.text('napomena', 'Napomena', lines: 2),
        ],
      );
      if (result == null) return false;
      result['rowVersion'] = item!['rowVersion'];
      await repository.updateReservation(item['id'] as int, result);
      return true;
    },
  );
}

class BandsPage extends StatelessWidget {
  const BandsPage({super.key, required this.repository});
  final AdminRepository repository;

  @override
  Widget build(BuildContext context) => _CollectionPage(
    title: 'Bendovi',
    subtitle: 'Profili bendova i članstvo',
    searchHint: 'Pretraži bend',
    load: (query) => repository.bands(search: query),
    columns: const [
      _Column('', 'slikaUrl', image: true),
      _Column('Naziv', 'naziv'),
      _Column('Žanr', 'zanr'),
      _Column('Članovi', 'clanovi', listLength: true),
      _Column('Rezervacije', 'brojRezervacija'),
      _Column('Opis', 'opis'),
    ],
    canAdd: false,
    onEdit: (context, item) async {
      final lookups = await repository.lookups();
      if (!context.mounted) return false;
      final result = await _showEntityDialog(
        context,
        title: 'Uredi bend',
        initial: item,
        fields: [
          _Field.text('naziv', 'Naziv', required: true),
          _Field.dropdown('zanrId', 'Žanr', lookups['zanrovi']),
          _Field.text('opis', 'Opis', lines: 3),
        ],
      );
      if (result == null) return false;
      await repository.updateBand(item!['id'] as int, result);
      return true;
    },
  );
}

class UsersPage extends StatelessWidget {
  const UsersPage({super.key, required this.repository});
  final AdminRepository repository;

  @override
  Widget build(BuildContext context) => _CollectionPage(
    title: 'Korisnici',
    subtitle: 'Uloge i pristup sistemu',
    searchHint: 'Pretraži ime, username ili email',
    load: (query) => repository.users(search: query),
    columns: const [
      _Column('', 'slikaUrl', image: true),
      _Column('Username', 'username'),
      _Column('Ime', 'ime'),
      _Column('Prezime', 'prezime'),
      _Column('Email', 'email'),
      _Column('Uloga', 'uloga', badge: true),
      _Column('Aktivan', 'aktivan', boolean: true),
    ],
    canAdd: false,
    onEdit: (context, item) async {
      final lookups = await repository.lookups();
      if (!context.mounted) return false;
      final result = await _showEntityDialog(
        context,
        title: 'Pristup korisnika',
        initial: item,
        fields: [
          _Field.dropdown('ulogaId', 'Uloga', lookups['uloge']),
          _Field.boolean('aktivan', 'Aktivan korisnik'),
        ],
      );
      if (result == null) return false;
      await repository.updateUser(item!['id'] as int, result);
      return true;
    },
  );
}

class ReportsPage extends StatefulWidget {
  const ReportsPage({super.key, required this.repository});
  final AdminRepository repository;

  @override
  State<ReportsPage> createState() => _ReportsPageState();
}

class _ReportsPageState extends State<ReportsPage> {
  DateTime from = DateTime(DateTime.now().year, DateTime.now().month, 1);
  DateTime to = DateTime.now().add(const Duration(days: 1));
  Map<String, dynamic>? report;
  bool busy = false;
  Object? error;

  @override
  void initState() {
    super.initState();
    load();
  }

  Future<void> load() async {
    setState(() {
      busy = true;
      error = null;
    });
    try {
      report = await widget.repository.report(from: from, to: to);
    } catch (e) {
      error = e;
    } finally {
      if (mounted) setState(() => busy = false);
    }
  }

  Future<void> _pdf(bool print) async {
    final bytes = await widget.repository.reportPdf(from: from, to: to);
    if (print) {
      await Printing.layoutPdf(onLayout: (_) async => bytes);
      return;
    }
    final location = await getSaveLocation(
      suggestedName:
          'OpenAmp-izvjestaj-${DateFormat('yyyyMMdd').format(from)}.pdf',
    );
    if (location == null) return;
    final file = XFile.fromData(
      bytes,
      mimeType: 'application/pdf',
      name: location.path.split(r'\').last,
    );
    await file.saveTo(location.path);
  }

  @override
  Widget build(BuildContext context) => DesktopPageFrame(
    title: 'Izvještaji',
    subtitle: 'Prihod, rezervacije i iskorištenost',
    actions: [
      OutlinedButton.icon(
        onPressed: busy ? null : () => _pdf(false),
        icon: const Icon(Icons.download_outlined),
        label: const Text('PDF'),
      ),
      const SizedBox(width: 10),
      FilledButton.icon(
        onPressed: busy ? null : () => _pdf(true),
        icon: const Icon(Icons.print_outlined),
        label: const Text('Štampaj'),
      ),
    ],
    child: Column(
      children: [
        Row(
          children: [
            _DateButton(
              label: 'Od',
              value: from,
              onChanged: (value) => setState(() => from = value),
            ),
            const SizedBox(width: 10),
            _DateButton(
              label: 'Do',
              value: to.subtract(const Duration(days: 1)),
              onChanged: (value) =>
                  setState(() => to = value.add(const Duration(days: 1))),
            ),
            const SizedBox(width: 10),
            SizedBox(
              width: 130,
              child: FilledButton(
                onPressed: load,
                child: const Text('Primijeni'),
              ),
            ),
          ],
        ),
        const SizedBox(height: 22),
        Expanded(
          child: busy
              ? const Center(child: CircularProgressIndicator())
              : error != null
              ? _ErrorState(error, load)
              : _reportBody(),
        ),
      ],
    ),
  );

  Widget _reportBody() {
    final data = report!;
    final halls = (data['prihodPoSalama'] as List<dynamic>)
        .cast<Map<String, dynamic>>();
    final genres = (data['rezervacijePoZanrovima'] as List<dynamic>)
        .cast<Map<String, dynamic>>();
    return ListView(
      children: [
        Wrap(
          spacing: 14,
          runSpacing: 14,
          children: [
            _Metric('Ukupan prihod', _money.format(data['ukupanPrihod'])),
            _Metric('Rezervacije', data['ukupnoRezervacija']),
            _Metric(
              'Prosječna vrijednost',
              _money.format(data['prosjecnaVrijednostRezervacije']),
            ),
            _Metric('Ukupno sati', data['ukupnoSati']),
          ],
        ),
        const SizedBox(height: 22),
        Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Expanded(
              child: _Panel(
                title: 'Prihod po salama',
                child: _BarList(
                  values: halls,
                  labelKey: 'sala',
                  valueKey: 'postotak',
                  suffix: (item) => _money.format(item['prihod']),
                ),
              ),
            ),
            const SizedBox(width: 18),
            Expanded(
              child: _Panel(
                title: 'Rezervacije po žanru',
                child: _BarList(
                  values: genres,
                  labelKey: 'zanr',
                  valueKey: 'postotak',
                  suffix: (item) => '${item['brojRezervacija']}',
                ),
              ),
            ),
          ],
        ),
      ],
    );
  }
}

class ReferenceDataPage extends StatefulWidget {
  const ReferenceDataPage({super.key, required this.repository});
  final AdminRepository repository;

  @override
  State<ReferenceDataPage> createState() => _ReferenceDataPageState();
}

class _ReferenceDataPageState extends State<ReferenceDataPage> {
  static const labels = {
    'studios': 'Studiji',
    'genres': 'Žanrovi',
    'instruments': 'Instrumenti',
    'hall-statuses': 'Statusi sala',
    'equipment-categories': 'Kategorije opreme',
    'equipment-statuses': 'Statusi opreme',
    'article-categories': 'Kategorije artikala',
    'article-statuses': 'Statusi artikala',
    'reservation-statuses': 'Statusi rezervacija',
    'invitation-statuses': 'Statusi pozivnica',
    'roles': 'Uloge',
  };
  String selected = 'studios';

  @override
  Widget build(BuildContext context) => DesktopPageFrame(
    title: 'Šifarnici',
    subtitle: 'Centralne vrijednosti sistema',
    child: Row(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        SizedBox(
          width: 230,
          child: ListView(
            children: labels.entries.map((entry) {
              final active = selected == entry.key;
              return ListTile(
                selected: active,
                selectedTileColor: AppColors.primarySoft,
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(10),
                ),
                title: Text(entry.value),
                onTap: () => setState(() => selected = entry.key),
              );
            }).toList(),
          ),
        ),
        const VerticalDivider(width: 30),
        Expanded(
          key: ValueKey(selected),
          child: selected == 'studios'
              ? _StudiosCollection(repository: widget.repository)
              : _ReferenceCollection(
                  repository: widget.repository,
                  type: selected,
                  title: labels[selected]!,
                ),
        ),
      ],
    ),
  );
}

class _StudiosCollection extends StatelessWidget {
  const _StudiosCollection({required this.repository});
  final AdminRepository repository;

  @override
  Widget build(BuildContext context) => _CollectionBody(
    searchHint: 'Pretraži studio',
    load: (query) => repository.studios(search: query),
    columns: const [
      _Column('Naziv', 'naziv'),
      _Column('Email', 'email'),
      _Column('Telefon', 'telefon'),
      _Column('Adresa', 'adresa'),
      _Column('Aktivan', 'aktivan', boolean: true),
    ],
    onEdit: (context, item) async {
      final result = await _showEntityDialog(
        context,
        title: item == null ? 'Novi studio' : 'Uredi studio',
        initial: item,
        fields: [
          _Field.text('naziv', 'Naziv', required: true),
          _Field.text('adresa', 'Adresa', required: true),
          _Field.text('grad', 'Grad', required: true),
          _Field.text('telefon', 'Telefon'),
          _Field.email('email', 'Email'),
          _Field.text('opis', 'Opis', lines: 2),
          _Field.text('vremenskaZona', 'Vremenska zona', required: true),
          _Field.text('radnoVrijemeOd', 'Otvara (HH:mm)', required: true),
          _Field.text('radnoVrijemeDo', 'Zatvara (HH:mm)', required: true),
          _Field.integer('puniPovratDoSati', 'Puni povrat do sati', minimum: 0),
          _Field.integer(
            'djelimicniPovratDoSati',
            'Djelimični povrat do sati',
            minimum: 0,
          ),
          _Field.integer(
            'djelimicniPovratPostotak',
            'Djelimični povrat %',
            minimum: 0,
          ),
          _Field.boolean('aktivan', 'Aktivan'),
        ],
      );
      if (result == null) return false;
      await repository.saveStudio(result, id: item?['id'] as int?);
      return true;
    },
    onDelete: (item) => repository.deleteStudio(item['id'] as int),
  );
}

class _ReferenceCollection extends StatelessWidget {
  const _ReferenceCollection({
    required this.repository,
    required this.type,
    required this.title,
  });
  final AdminRepository repository;
  final String type;
  final String title;

  @override
  Widget build(BuildContext context) => _CollectionBody(
    load: (_) => repository.references(type),
    columns: const [_Column('Kod', 'kod'), _Column('Naziv', 'naziv')],
    onEdit: (context, item) async {
      final result = await _showEntityDialog(
        context,
        title: item == null ? 'Nova vrijednost' : 'Uredi vrijednost',
        initial: item,
        fields: [
          _Field.text('kod', 'Kod', required: true),
          _Field.text('naziv', 'Naziv', required: true),
        ],
      );
      if (result == null) return false;
      await repository.saveReference(type, result, id: item?['id'] as int?);
      return true;
    },
    onDelete: (item) => repository.deleteReference(type, item['id'] as int),
  );
}

typedef _LoadRows = Future<List<Map<String, dynamic>>> Function(String? query);
typedef _EditRow =
    Future<bool> Function(BuildContext context, Map<String, dynamic>? item);

class _CollectionPage extends StatelessWidget {
  const _CollectionPage({
    required this.title,
    required this.subtitle,
    required this.load,
    required this.columns,
    required this.onEdit,
    this.searchHint,
    this.onDelete,
    this.canAdd = true,
  });
  final String title;
  final String subtitle;
  final String? searchHint;
  final _LoadRows load;
  final List<_Column> columns;
  final _EditRow onEdit;
  final Future<void> Function(Map<String, dynamic>)? onDelete;
  final bool canAdd;

  @override
  Widget build(BuildContext context) => DesktopPageFrame(
    title: title,
    subtitle: subtitle,
    child: _CollectionBody(
      load: load,
      columns: columns,
      onEdit: onEdit,
      onDelete: onDelete,
      searchHint: searchHint,
      canAdd: canAdd,
    ),
  );
}

class _CollectionBody extends StatefulWidget {
  const _CollectionBody({
    required this.load,
    required this.columns,
    required this.onEdit,
    this.onDelete,
    this.searchHint,
    this.canAdd = true,
  });
  final _LoadRows load;
  final List<_Column> columns;
  final _EditRow onEdit;
  final Future<void> Function(Map<String, dynamic>)? onDelete;
  final String? searchHint;
  final bool canAdd;

  @override
  State<_CollectionBody> createState() => _CollectionBodyState();
}

class _CollectionBodyState extends State<_CollectionBody> {
  final search = TextEditingController();
  List<Map<String, dynamic>> rows = const [];
  bool busy = true;
  Object? error;

  @override
  void initState() {
    super.initState();
    load();
  }

  @override
  void dispose() {
    search.dispose();
    super.dispose();
  }

  Future<void> load() async {
    setState(() {
      busy = true;
      error = null;
    });
    try {
      rows = await widget.load(search.text);
    } catch (e) {
      error = e;
    } finally {
      if (mounted) setState(() => busy = false);
    }
  }

  Future<void> edit(Map<String, dynamic>? row) async {
    try {
      if (await widget.onEdit(context, row)) await load();
    } catch (error) {
      if (mounted) _showError(context, error);
    }
  }

  Future<void> delete(Map<String, dynamic> row) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Potvrda brisanja'),
        content: const Text('Ovu radnju nije moguće poništiti.'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: const Text('Odustani'),
          ),
          FilledButton(
            onPressed: () => Navigator.pop(context, true),
            child: const Text('Obriši'),
          ),
        ],
      ),
    );
    if (confirmed != true) return;
    try {
      await widget.onDelete!(row);
      await load();
    } catch (error) {
      if (mounted) _showError(context, error);
    }
  }

  @override
  Widget build(BuildContext context) => Column(
    children: [
      Row(
        children: [
          if (widget.searchHint != null)
            SizedBox(
              width: 360,
              child: TextField(
                controller: search,
                onSubmitted: (_) => load(),
                decoration: InputDecoration(
                  hintText: widget.searchHint,
                  prefixIcon: const Icon(Icons.search),
                  suffixIcon: search.text.isEmpty
                      ? null
                      : IconButton(
                          onPressed: () {
                            search.clear();
                            load();
                          },
                          icon: const Icon(Icons.close),
                        ),
                ),
              ),
            ),
          if (widget.searchHint != null) const SizedBox(width: 10),
          if (widget.searchHint != null)
            SizedBox(
              width: 110,
              child: OutlinedButton(
                onPressed: load,
                child: const Text('Traži'),
              ),
            ),
          const Spacer(),
          _RefreshButton(onPressed: load),
          if (widget.canAdd) ...[
            const SizedBox(width: 10),
            SizedBox(
              width: 140,
              child: FilledButton.icon(
                onPressed: () => edit(null),
                icon: const Icon(Icons.add),
                label: const Text('Dodaj'),
              ),
            ),
          ],
        ],
      ),
      const SizedBox(height: 18),
      Expanded(
        child: busy
            ? const Center(child: CircularProgressIndicator())
            : error != null
            ? _ErrorState(error, load)
            : rows.isEmpty
            ? const _EmptyState('Nema rezultata za odabrane filtere.')
            : _DataGrid(
                rows: rows,
                columns: widget.columns,
                onEdit: edit,
                onDelete: widget.onDelete == null ? null : delete,
              ),
      ),
    ],
  );
}

class _DataGrid extends StatelessWidget {
  const _DataGrid({
    required this.rows,
    required this.columns,
    required this.onEdit,
    this.onDelete,
  });
  final List<Map<String, dynamic>> rows;
  final List<_Column> columns;
  final ValueChanged<Map<String, dynamic>> onEdit;
  final ValueChanged<Map<String, dynamic>>? onDelete;

  @override
  Widget build(BuildContext context) => Card(
    clipBehavior: Clip.antiAlias,
    child: LayoutBuilder(
      builder: (context, constraints) => Scrollbar(
        child: SingleChildScrollView(
          scrollDirection: Axis.horizontal,
          child: SizedBox(
            width: math.max(constraints.maxWidth, columns.length * 150 + 100),
            child: SingleChildScrollView(
              child: DataTable(
                headingRowColor: WidgetStateProperty.all(AppColors.paperMuted),
                horizontalMargin: 20,
                columnSpacing: 24,
                columns: [
                  ...columns.map((x) => DataColumn(label: Text(x.label))),
                  const DataColumn(label: Text('')),
                ],
                rows: rows.map((row) {
                  return DataRow(
                    cells: [
                      ...columns.map(
                        (column) => DataCell(
                          _Cell(column: column, value: row[column.key]),
                        ),
                      ),
                      DataCell(
                        Row(
                          mainAxisAlignment: MainAxisAlignment.end,
                          children: [
                            IconButton(
                              tooltip: 'Uredi',
                              onPressed: () => onEdit(row),
                              icon: const Icon(Icons.edit_outlined, size: 19),
                            ),
                            if (onDelete != null)
                              IconButton(
                                tooltip: 'Obriši',
                                onPressed: () => onDelete!(row),
                                icon: const Icon(
                                  Icons.delete_outline,
                                  size: 19,
                                  color: AppColors.danger,
                                ),
                              ),
                          ],
                        ),
                      ),
                    ],
                  );
                }).toList(),
              ),
            ),
          ),
        ),
      ),
    ),
  );
}

class _Column {
  const _Column(
    this.label,
    this.key, {
    this.money = false,
    this.badge = false,
    this.boolean = false,
    this.dateTime = false,
    this.listLength = false,
    this.image = false,
  });
  final String label;
  final String key;
  final bool money;
  final bool badge;
  final bool boolean;
  final bool dateTime;
  final bool listLength;
  final bool image;
}

class _Cell extends StatelessWidget {
  const _Cell({required this.column, required this.value});
  final _Column column;
  final dynamic value;

  @override
  Widget build(BuildContext context) {
    if (column.image) {
      final url = AppConfig.resolveMediaUrl(value?.toString());
      return ClipRRect(
        borderRadius: BorderRadius.circular(AppRadii.small),
        child: SizedBox(
          width: 52,
          height: 38,
          child: url == null
              ? const _ImagePlaceholder()
              : Image.network(
                  url,
                  fit: BoxFit.cover,
                  errorBuilder: (_, _, _) => const _ImagePlaceholder(),
                ),
        ),
      );
    }
    if (column.badge) return _Badge(value?.toString() ?? '—');
    if (column.boolean) {
      return Icon(
        value == true ? Icons.check_circle : Icons.cancel_outlined,
        size: 19,
        color: value == true ? AppColors.success : AppColors.textMuted,
      );
    }
    if (column.listLength) return Text('${(value as List?)?.length ?? 0}');
    if (column.money) return Text(_money.format(value ?? 0));
    if (column.dateTime && value != null) {
      return Text(_dateTime.format(DateTime.parse(value.toString()).toLocal()));
    }
    return Text(
      value?.toString() ?? '—',
      maxLines: 1,
      overflow: TextOverflow.ellipsis,
    );
  }
}

class _ImagePlaceholder extends StatelessWidget {
  const _ImagePlaceholder();

  @override
  Widget build(BuildContext context) => const ColoredBox(
    color: AppColors.paperMuted,
    child: Icon(Icons.image_outlined, size: 18, color: AppColors.textMuted),
  );
}

enum _FieldType { text, email, integer, decimal, dropdown, boolean, dateTime }

class _Field {
  const _Field._(
    this.key,
    this.label,
    this.type, {
    this.required = false,
    this.lines = 1,
    this.minimum,
    this.options,
    this.optional = false,
  });
  factory _Field.text(
    String key,
    String label, {
    bool required = false,
    int lines = 1,
  }) => _Field._(key, label, _FieldType.text, required: required, lines: lines);
  factory _Field.email(String key, String label) =>
      _Field._(key, label, _FieldType.email, required: true);
  factory _Field.integer(String key, String label, {num? minimum}) => _Field._(
    key,
    label,
    _FieldType.integer,
    required: true,
    minimum: minimum,
  );
  factory _Field.decimal(String key, String label, {num? minimum}) => _Field._(
    key,
    label,
    _FieldType.decimal,
    required: true,
    minimum: minimum,
  );
  factory _Field.dropdown(
    String key,
    String label,
    dynamic options, {
    bool optional = false,
  }) => _Field._(
    key,
    label,
    _FieldType.dropdown,
    required: !optional,
    options: (options as List<dynamic>? ?? const [])
        .cast<Map<String, dynamic>>(),
    optional: optional,
  );
  factory _Field.boolean(String key, String label) =>
      _Field._(key, label, _FieldType.boolean);
  factory _Field.dateTime(String key, String label) =>
      _Field._(key, label, _FieldType.dateTime, required: true);

  final String key;
  final String label;
  final _FieldType type;
  final bool required;
  final int lines;
  final num? minimum;
  final List<Map<String, dynamic>>? options;
  final bool optional;
}

Future<Map<String, dynamic>?> _showEntityDialog(
  BuildContext context, {
  required String title,
  required List<_Field> fields,
  Map<String, dynamic>? initial,
}) {
  return showDialog<Map<String, dynamic>>(
    context: context,
    barrierDismissible: false,
    builder: (_) =>
        _EntityDialog(title: title, fields: fields, initial: initial),
  );
}

class _EntityDialog extends StatefulWidget {
  const _EntityDialog({
    required this.title,
    required this.fields,
    this.initial,
  });
  final String title;
  final List<_Field> fields;
  final Map<String, dynamic>? initial;

  @override
  State<_EntityDialog> createState() => _EntityDialogState();
}

class _EntityDialogState extends State<_EntityDialog> {
  final formKey = GlobalKey<FormState>();
  final values = <String, dynamic>{};
  final controllers = <String, TextEditingController>{};

  @override
  void initState() {
    super.initState();
    for (final field in widget.fields) {
      final value = widget.initial?[field.key];
      if (field.type == _FieldType.boolean ||
          field.type == _FieldType.dropdown) {
        values[field.key] = value;
      } else {
        controllers[field.key] = TextEditingController(
          text: field.type == _FieldType.dateTime && value != null
              ? DateFormat(
                  'yyyy-MM-dd HH:mm',
                ).format(DateTime.parse(value.toString()).toLocal())
              : value?.toString() ?? '',
        );
      }
    }
  }

  @override
  void dispose() {
    for (final controller in controllers.values) {
      controller.dispose();
    }
    super.dispose();
  }

  void submit() {
    if (!formKey.currentState!.validate()) return;
    formKey.currentState!.save();
    for (final field in widget.fields) {
      if (field.type == _FieldType.boolean ||
          field.type == _FieldType.dropdown) {
        continue;
      }
      final text = controllers[field.key]!.text.trim();
      values[field.key] = switch (field.type) {
        _FieldType.integer => int.parse(text),
        _FieldType.decimal => double.parse(text.replaceAll(',', '.')),
        _FieldType.dateTime => DateFormat(
          'yyyy-MM-dd HH:mm',
        ).parseStrict(text).toUtc().toIso8601String(),
        _ => text.isEmpty ? null : text,
      };
    }
    Navigator.pop(context, values);
  }

  @override
  Widget build(BuildContext context) => AlertDialog(
    title: Text(widget.title),
    content: SizedBox(
      width: 560,
      child: Form(
        key: formKey,
        child: SingleChildScrollView(
          child: Wrap(
            spacing: 12,
            runSpacing: 14,
            children: widget.fields
                .map(
                  (field) => SizedBox(
                    width: field.lines > 1 ? 548 : 268,
                    child: _buildField(field),
                  ),
                )
                .toList(),
          ),
        ),
      ),
    ),
    actions: [
      TextButton(
        onPressed: () => Navigator.pop(context),
        child: const Text('Odustani'),
      ),
      FilledButton(onPressed: submit, child: const Text('Sačuvaj')),
    ],
  );

  Widget _buildField(_Field field) {
    if (field.type == _FieldType.boolean) {
      return SwitchListTile(
        contentPadding: const EdgeInsets.symmetric(horizontal: 8),
        title: Text(field.label),
        value: values[field.key] as bool? ?? true,
        onChanged: (value) => setState(() => values[field.key] = value),
      );
    }
    if (field.type == _FieldType.dropdown) {
      return DropdownButtonFormField<int>(
        initialValue: values[field.key] as int?,
        decoration: InputDecoration(labelText: field.label),
        items: [
          if (field.optional)
            const DropdownMenuItem(value: null, child: Text('Nije odabrano')),
          ...field.options!.map(
            (item) => DropdownMenuItem<int>(
              value: item['id'] as int,
              child: Text(
                (item['naziv'] ?? item['studio'] ?? '').toString(),
                overflow: TextOverflow.ellipsis,
              ),
            ),
          ),
        ],
        validator: (value) =>
            field.required && value == null ? 'Odaberite vrijednost.' : null,
        onChanged: (value) => values[field.key] = value,
      );
    }
    return TextFormField(
      controller: controllers[field.key],
      maxLines: field.lines,
      decoration: InputDecoration(
        labelText: field.label,
        hintText: field.type == _FieldType.dateTime ? '2026-07-27 18:00' : null,
      ),
      validator: (value) {
        final text = value?.trim() ?? '';
        if (field.required && text.isEmpty) return 'Polje je obavezno.';
        if (text.isEmpty) return null;
        if (field.type == _FieldType.email &&
            !RegExp(r'^[^@\s]+@[^@\s]+\.[^@\s]+$').hasMatch(text)) {
          return 'Unesite ispravnu email adresu.';
        }
        if (field.type == _FieldType.integer) {
          final number = int.tryParse(text);
          if (number == null) return 'Unesite cijeli broj.';
          if (field.minimum != null && number < field.minimum!) {
            return 'Minimalna vrijednost je ${field.minimum}.';
          }
        }
        if (field.type == _FieldType.decimal) {
          final number = double.tryParse(text.replaceAll(',', '.'));
          if (number == null) return 'Unesite broj.';
          if (field.minimum != null && number < field.minimum!) {
            return 'Minimalna vrijednost je ${field.minimum}.';
          }
        }
        if (field.type == _FieldType.dateTime) {
          try {
            DateFormat('yyyy-MM-dd HH:mm').parseStrict(text);
          } on FormatException {
            return 'Format mora biti YYYY-MM-DD HH:mm.';
          }
        }
        return null;
      },
    );
  }
}

class _Metric extends StatelessWidget {
  const _Metric(this.label, this.value, {this.alert = false});
  final String label;
  final Object value;
  final bool alert;

  @override
  Widget build(BuildContext context) => SizedBox(
    width: 220,
    child: Card(
      child: Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(label, style: Theme.of(context).textTheme.bodyMedium),
            const SizedBox(height: 10),
            Text(
              '$value',
              style: Theme.of(context).textTheme.headlineMedium!.copyWith(
                color: alert ? AppColors.warning : AppColors.text,
              ),
            ),
          ],
        ),
      ),
    ),
  );
}

class _Panel extends StatelessWidget {
  const _Panel({required this.title, required this.child});
  final String title;
  final Widget child;

  @override
  Widget build(BuildContext context) => Card(
    child: Padding(
      padding: const EdgeInsets.all(20),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(title, style: Theme.of(context).textTheme.titleMedium),
          const SizedBox(height: 16),
          child,
        ],
      ),
    ),
  );
}

class _ScheduleRow extends StatelessWidget {
  const _ScheduleRow({
    required this.time,
    required this.band,
    required this.hall,
    required this.status,
  });
  final String time;
  final String band;
  final String hall;
  final String status;

  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.symmetric(vertical: 13),
    decoration: const BoxDecoration(
      border: Border(bottom: BorderSide(color: AppColors.line)),
    ),
    child: Row(
      children: [
        SizedBox(
          width: 100,
          child: Text(
            time,
            style: const TextStyle(fontWeight: FontWeight.w800),
          ),
        ),
        Expanded(child: Text(band)),
        Expanded(child: Text(hall)),
        _Badge(status),
      ],
    ),
  );
}

class _Badge extends StatelessWidget {
  const _Badge(this.label);
  final String label;

  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.symmetric(horizontal: 9, vertical: 5),
    decoration: BoxDecoration(
      color: AppColors.paperMuted,
      borderRadius: BorderRadius.circular(7),
      border: Border.all(color: AppColors.line),
    ),
    child: Text(
      label,
      style: const TextStyle(fontSize: 12, fontWeight: FontWeight.w700),
    ),
  );
}

class _BarList extends StatelessWidget {
  const _BarList({
    required this.values,
    required this.labelKey,
    required this.valueKey,
    required this.suffix,
  });
  final List<Map<String, dynamic>> values;
  final String labelKey;
  final String valueKey;
  final String Function(Map<String, dynamic>) suffix;

  @override
  Widget build(BuildContext context) {
    if (values.isEmpty) return const _EmptyState('Nema podataka za period.');
    return Column(
      children: values.map((item) {
        final percent = (item[valueKey] as num?)?.toDouble() ?? 0;
        return Padding(
          padding: const EdgeInsets.only(bottom: 15),
          child: Column(
            children: [
              Row(
                children: [
                  Expanded(child: Text(item[labelKey]?.toString() ?? '')),
                  Text(
                    suffix(item),
                    style: const TextStyle(fontWeight: FontWeight.w800),
                  ),
                ],
              ),
              const SizedBox(height: 7),
              LinearProgressIndicator(
                value: percent / 100,
                minHeight: 7,
                borderRadius: BorderRadius.circular(4),
              ),
            ],
          ),
        );
      }).toList(),
    );
  }
}

class _DateButton extends StatelessWidget {
  const _DateButton({
    required this.label,
    required this.value,
    required this.onChanged,
  });
  final String label;
  final DateTime value;
  final ValueChanged<DateTime> onChanged;

  @override
  Widget build(BuildContext context) => OutlinedButton.icon(
    onPressed: () async {
      final result = await showDatePicker(
        context: context,
        firstDate: DateTime(2020),
        lastDate: DateTime(2035),
        initialDate: value,
      );
      if (result != null) onChanged(result);
    },
    icon: const Icon(Icons.calendar_today_outlined, size: 18),
    label: Text('$label: ${DateFormat('dd.MM.yyyy.').format(value)}'),
  );
}

class _RefreshButton extends StatelessWidget {
  const _RefreshButton({required this.onPressed});
  final VoidCallback onPressed;

  @override
  Widget build(BuildContext context) => IconButton.outlined(
    tooltip: 'Osvježi',
    onPressed: onPressed,
    icon: const Icon(Icons.refresh),
  );
}

class _EmptyState extends StatelessWidget {
  const _EmptyState(this.message);
  final String message;

  @override
  Widget build(BuildContext context) => Center(
    child: Padding(
      padding: const EdgeInsets.all(36),
      child: Text(message, style: Theme.of(context).textTheme.bodyMedium),
    ),
  );
}

class _ErrorState extends StatelessWidget {
  const _ErrorState(this.error, this.retry);
  final Object? error;
  final VoidCallback retry;

  @override
  Widget build(BuildContext context) => Center(
    child: Column(
      mainAxisSize: MainAxisSize.min,
      children: [
        const Icon(Icons.error_outline, color: AppColors.danger, size: 36),
        const SizedBox(height: 12),
        Text('$error', textAlign: TextAlign.center),
        const SizedBox(height: 14),
        OutlinedButton(onPressed: retry, child: const Text('Pokušaj ponovo')),
      ],
    ),
  );
}

void _showError(BuildContext context, Object error) {
  ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('$error')));
}
