import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/intl.dart';
import 'package:openamp_mobile/core/theme/app_theme.dart';
import 'package:openamp_mobile/models/models.dart';
import 'package:openamp_mobile/repositories/openamp_repository.dart';
import 'package:openamp_mobile/screens/booking/addons_screen.dart';
import 'package:openamp_mobile/state/app_state.dart';
import 'package:openamp_mobile/widgets/common.dart';

class SlotSelectionScreen extends ConsumerStatefulWidget {
  const SlotSelectionScreen({super.key, required this.hall});
  final HallDetails hall;

  @override
  ConsumerState<SlotSelectionScreen> createState() =>
      _SlotSelectionScreenState();
}

class _SlotSelectionScreenState extends ConsumerState<SlotSelectionScreen> {
  late DateTime _date;
  Band? _band;
  DateTime? _start;
  DateTime? _end;
  late Future<List<DateTimeRangeValue>> _availability;

  @override
  void initState() {
    super.initState();
    final tomorrow = DateTime.now().add(const Duration(days: 1));
    _date = DateTime(tomorrow.year, tomorrow.month, tomorrow.day);
    _availability = _load();
  }

  Future<List<DateTimeRangeValue>> _load() => ref
      .read(repositoryProvider)
      .getAvailability(hallId: widget.hall.id, date: _date);

  void _chooseDate(DateTime value) {
    setState(() {
      _date = value;
      _start = null;
      _end = null;
      _availability = _load();
    });
  }

  void _toggleSlot(DateTimeRangeValue slot) {
    setState(() {
      if (_start == null ||
          (_end != null && slot.start != _end && slot.end != _start)) {
        _start = slot.start;
        _end = slot.end;
      } else if (slot.start == _end) {
        _end = slot.end;
      } else if (slot.end == _start) {
        _start = slot.start;
      } else {
        _start = slot.start;
        _end = slot.end;
      }
    });
  }

  bool _selected(DateTimeRangeValue slot) =>
      _start != null &&
      _end != null &&
      !slot.start.isBefore(_start!) &&
      slot.end.isBefore(_end!.add(const Duration(seconds: 1)));

  @override
  Widget build(BuildContext context) {
    final bands = ref.watch(appControllerProvider).bands;
    _band ??= bands.isEmpty ? null : bands.first;
    final dates = List.generate(
      7,
      (index) => DateTime.now().add(Duration(days: index + 1)),
    );
    final duration = _start == null || _end == null
        ? 0.0
        : _end!.difference(_start!).inMinutes / 60;
    return Scaffold(
      appBar: AppBar(title: const Text('Odabir termina')),
      body: ListView(
        padding: const EdgeInsets.fromLTRB(18, 8, 18, 110),
        children: [
          Card(
            child: Padding(
              padding: const EdgeInsets.all(12),
              child: Row(
                children: [
                  HallImage(
                    url: widget.hall.gallery.firstOrNull,
                    width: 82,
                    height: 68,
                    borderRadius: 12,
                  ),
                  const SizedBox(width: 13),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          widget.hall.name,
                          style: Theme.of(context).textTheme.titleMedium,
                        ),
                        Text(
                          money(widget.hall.hourlyPrice) + '/h',
                          style: const TextStyle(
                            color: AppColors.primary,
                            fontWeight: FontWeight.w800,
                          ),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),
          ),
          const SizedBox(height: 18),
          Text('Bend', style: Theme.of(context).textTheme.titleMedium),
          const SizedBox(height: 8),
          if (bands.isEmpty)
            const Card(
              child: ListTile(
                leading: Icon(Icons.info_outline),
                title: Text('Prvo kreirajte bend u tabu Bendovi.'),
              ),
            )
          else
            DropdownButtonFormField<Band>(
              initialValue: _band,
              decoration: const InputDecoration(
                prefixIcon: Icon(Icons.groups_2_outlined),
              ),
              items: bands
                  .map(
                    (band) => DropdownMenuItem(
                      value: band,
                      child: Text(band.name + ' · ' + band.genre),
                    ),
                  )
                  .toList(),
              onChanged: (value) => setState(() => _band = value),
            ),
          const SizedBox(height: 20),
          Text('Datum', style: Theme.of(context).textTheme.titleMedium),
          const SizedBox(height: 9),
          SizedBox(
            height: 76,
            child: ListView.separated(
              scrollDirection: Axis.horizontal,
              itemCount: dates.length,
              separatorBuilder: (_, _) => const SizedBox(width: 8),
              itemBuilder: (_, index) {
                final value = dates[index];
                final selected =
                    value.year == _date.year &&
                    value.month == _date.month &&
                    value.day == _date.day;
                return InkWell(
                  borderRadius: BorderRadius.circular(14),
                  onTap: () => _chooseDate(value),
                  child: AnimatedContainer(
                    duration: const Duration(milliseconds: 180),
                    width: 58,
                    padding: const EdgeInsets.symmetric(vertical: 8),
                    decoration: BoxDecoration(
                      color: selected ? AppColors.primary : Colors.white,
                      borderRadius: BorderRadius.circular(14),
                      border: Border.all(
                        color: selected
                            ? AppColors.primary
                            : const Color(0xFFE4E3EC),
                      ),
                    ),
                    child: Column(
                      children: [
                        Text(
                          DateFormat('E', 'bs').format(value),
                          style: TextStyle(
                            color: selected
                                ? Colors.white70
                                : AppColors.textMuted,
                            fontSize: 12,
                            fontWeight: FontWeight.w700,
                          ),
                        ),
                        const SizedBox(height: 4),
                        Text(
                          value.day.toString(),
                          style: TextStyle(
                            color: selected ? Colors.white : AppColors.text,
                            fontSize: 20,
                            fontWeight: FontWeight.w900,
                          ),
                        ),
                      ],
                    ),
                  ),
                );
              },
            ),
          ),
          const SizedBox(height: 20),
          Text(
            'Slobodni termini · ' + DateFormat('dd.MM.').format(_date),
            style: Theme.of(context).textTheme.titleMedium,
          ),
          const SizedBox(height: 10),
          FutureBuilder<List<DateTimeRangeValue>>(
            future: _availability,
            builder: (context, snapshot) {
              if (snapshot.connectionState != ConnectionState.done) {
                return const Padding(
                  padding: EdgeInsets.all(30),
                  child: Center(child: CircularProgressIndicator()),
                );
              }
              if (snapshot.hasError) {
                return ErrorBanner(
                  message: snapshot.error.toString(),
                  onRetry: () => setState(() => _availability = _load()),
                );
              }
              final slots = snapshot.data!;
              return GridView.builder(
                shrinkWrap: true,
                physics: const NeverScrollableScrollPhysics(),
                gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                  crossAxisCount: 3,
                  childAspectRatio: 1.9,
                  crossAxisSpacing: 8,
                  mainAxisSpacing: 8,
                ),
                itemCount: slots.length,
                itemBuilder: (_, index) {
                  final slot = slots[index];
                  final selected = _selected(slot);
                  return InkWell(
                    borderRadius: BorderRadius.circular(12),
                    onTap: () => _toggleSlot(slot),
                    child: AnimatedContainer(
                      duration: const Duration(milliseconds: 150),
                      alignment: Alignment.center,
                      decoration: BoxDecoration(
                        color: selected ? AppColors.primary : Colors.white,
                        borderRadius: BorderRadius.circular(12),
                        border: Border.all(
                          color: selected
                              ? AppColors.primary
                              : const Color(0xFFE4E3EC),
                        ),
                      ),
                      child: Text(
                        DateFormat('HH:mm').format(slot.start.toLocal()),
                        style: TextStyle(
                          color: selected ? Colors.white : AppColors.text,
                          fontWeight: FontWeight.w800,
                        ),
                      ),
                    ),
                  );
                },
              );
            },
          ),
          const SizedBox(height: 18),
          Card(
            child: Padding(
              padding: const EdgeInsets.all(16),
              child: duration == 0
                  ? const Text('Odaberite jedan ili više uzastopnih slotova.')
                  : Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        const Text('Trajanje i osnovna cijena'),
                        const SizedBox(height: 4),
                        Text(
                          duration.toStringAsFixed(duration % 1 == 0 ? 0 : 1) +
                              ' h × ' +
                              money(widget.hall.hourlyPrice) +
                              ' = ' +
                              money(duration * widget.hall.hourlyPrice),
                          style: Theme.of(context).textTheme.titleMedium,
                        ),
                      ],
                    ),
            ),
          ),
        ],
      ),
      bottomNavigationBar: SafeArea(
        minimum: const EdgeInsets.fromLTRB(18, 10, 18, 14),
        child: FilledButton(
          onPressed: _band == null || _start == null || _end == null
              ? null
              : () {
                  final draft = BookingDraft(
                    hall: widget.hall,
                    band: _band,
                    startsAt: _start,
                    endsAt: _end,
                  );
                  Navigator.of(context).push(
                    MaterialPageRoute<void>(
                      builder: (_) => AddonsScreen(initialDraft: draft),
                    ),
                  );
                },
          child: const Text('Nastavi na opremu'),
        ),
      ),
    );
  }
}
