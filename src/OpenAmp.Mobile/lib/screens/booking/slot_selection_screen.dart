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
      appBar: AppBar(
        title: const Text('Odabir termina'),
        actions: const [
          Padding(
            padding: EdgeInsets.only(right: 18),
            child: Center(
              child: Text(
                'STEP 01 / 03',
                style: TextStyle(
                  color: AppColors.primary,
                  fontSize: 9,
                  fontWeight: FontWeight.w900,
                  letterSpacing: 1,
                ),
              ),
            ),
          ),
        ],
      ),
      body: ListView(
        keyboardDismissBehavior: ScrollViewKeyboardDismissBehavior.onDrag,
        padding: const EdgeInsets.fromLTRB(18, 8, 18, 116),
        children: [
          _RoomTicket(hall: widget.hall),
          const SizedBox(height: 25),
          const SectionEyebrow('Ko svira'),
          const SizedBox(height: 8),
          Text(
            'Odaberi postavu',
            style: Theme.of(context).textTheme.titleLarge,
          ),
          const SizedBox(height: 10),
          if (bands.isEmpty)
            Container(
              padding: const EdgeInsets.all(15),
              decoration: BoxDecoration(
                color: AppColors.signalSoft,
                borderRadius: BorderRadius.circular(AppRadii.medium),
                border: Border.all(color: AppColors.signal),
              ),
              child: const Row(
                children: [
                  Icon(Icons.info_outline, color: AppColors.signal),
                  SizedBox(width: 10),
                  Expanded(child: Text('Prvo kreiraj bend u tabu Bendovi.')),
                ],
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
                      child: Text('${band.name} · ${band.genre}'),
                    ),
                  )
                  .toList(),
              onChanged: (value) => setState(() => _band = value),
            ),
          const SizedBox(height: 26),
          const SectionEyebrow('Kalendar'),
          const SizedBox(height: 8),
          Text('Dan probe', style: Theme.of(context).textTheme.titleLarge),
          const SizedBox(height: 11),
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
                  borderRadius: BorderRadius.circular(AppRadii.small),
                  onTap: () => _chooseDate(value),
                  child: AnimatedContainer(
                    duration: const Duration(milliseconds: 160),
                    width: 57,
                    padding: const EdgeInsets.symmetric(vertical: 8),
                    decoration: BoxDecoration(
                      color: selected ? AppColors.signal : AppColors.paper,
                      borderRadius: BorderRadius.circular(AppRadii.small),
                      border: Border.all(
                        color: selected ? AppColors.ink : AppColors.line,
                        width: selected ? 1.4 : 1,
                      ),
                    ),
                    child: Column(
                      children: [
                        Text(
                          DateFormat('E', 'bs').format(value).toUpperCase(),
                          style: TextStyle(
                            color: selected
                                ? AppColors.ink
                                : AppColors.textMuted,
                            fontSize: 9,
                            fontWeight: FontWeight.w900,
                            letterSpacing: .7,
                          ),
                        ),
                        const SizedBox(height: 5),
                        Text(
                          value.day.toString(),
                          style: TextStyle(
                            color: AppColors.ink,
                            fontSize: 22,
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
          const SizedBox(height: 26),
          Row(
            crossAxisAlignment: CrossAxisAlignment.end,
            children: [
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const SectionEyebrow('Grid po satima'),
                    const SizedBox(height: 8),
                    Text(
                      'Slobodni slotovi',
                      style: Theme.of(context).textTheme.titleLarge,
                    ),
                  ],
                ),
              ),
              Text(
                DateFormat('dd.MM.').format(_date),
                style: const TextStyle(
                  color: AppColors.primary,
                  fontWeight: FontWeight.w900,
                ),
              ),
            ],
          ),
          const SizedBox(height: 11),
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
                    borderRadius: BorderRadius.circular(AppRadii.small),
                    onTap: () => _toggleSlot(slot),
                    child: AnimatedContainer(
                      duration: const Duration(milliseconds: 140),
                      alignment: Alignment.center,
                      decoration: BoxDecoration(
                        color: selected ? AppColors.ink : AppColors.paper,
                        borderRadius: BorderRadius.circular(AppRadii.small),
                        border: Border.all(
                          color: selected ? AppColors.signal : AppColors.line,
                          width: selected ? 2 : 1,
                        ),
                      ),
                      child: Text(
                        DateFormat('HH:mm').format(slot.start.toLocal()),
                        style: TextStyle(
                          color: selected ? Colors.white : AppColors.ink,
                          fontWeight: FontWeight.w900,
                          letterSpacing: .2,
                        ),
                      ),
                    ),
                  );
                },
              );
            },
          ),
          const SizedBox(height: 17),
          _PricePreview(duration: duration, hall: widget.hall),
        ],
      ),
      bottomNavigationBar: Container(
        decoration: const BoxDecoration(
          color: AppColors.canvas,
          border: Border(top: BorderSide(color: AppColors.line)),
        ),
        child: SafeArea(
          minimum: const EdgeInsets.fromLTRB(18, 10, 18, 12),
          child: SignalButton(
            label: duration == 0
                ? 'Odaberi vrijeme'
                : 'Oprema · ${duration.toStringAsFixed(duration % 1 == 0 ? 0 : 1)} h',
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
          ),
        ),
      ),
    );
  }
}

class _RoomTicket extends StatelessWidget {
  const _RoomTicket({required this.hall});

  final HallDetails hall;

  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.all(11),
    decoration: BoxDecoration(
      color: AppColors.ink,
      borderRadius: BorderRadius.circular(AppRadii.large),
    ),
    child: Row(
      children: [
        HallImage(
          url: hall.gallery.firstOrNull,
          width: 88,
          height: 78,
          borderRadius: 11,
        ),
        const SizedBox(width: 13),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                hall.studio.toUpperCase(),
                style: const TextStyle(
                  color: AppColors.signal,
                  fontSize: 9,
                  fontWeight: FontWeight.w900,
                  letterSpacing: 1,
                ),
              ),
              const SizedBox(height: 4),
              Text(
                hall.name,
                maxLines: 2,
                overflow: TextOverflow.ellipsis,
                style: const TextStyle(
                  color: Colors.white,
                  fontSize: 18,
                  fontWeight: FontWeight.w900,
                ),
              ),
              const SizedBox(height: 5),
              Text(
                '${money(hall.hourlyPrice)} / sat',
                style: const TextStyle(color: Colors.white60, fontSize: 12),
              ),
            ],
          ),
        ),
      ],
    ),
  );
}

class _PricePreview extends StatelessWidget {
  const _PricePreview({required this.duration, required this.hall});

  final double duration;
  final HallDetails hall;

  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.all(15),
    decoration: BoxDecoration(
      color: duration == 0 ? AppColors.paper : AppColors.primarySoft,
      borderRadius: BorderRadius.circular(AppRadii.medium),
      border: Border.all(
        color: duration == 0 ? AppColors.line : AppColors.primary,
      ),
    ),
    child: duration == 0
        ? const Row(
            children: [
              Icon(Icons.drag_indicator_rounded, color: AppColors.textMuted),
              SizedBox(width: 9),
              Expanded(
                child: Text('Odaberi jedan ili više uzastopnih slotova.'),
              ),
            ],
          )
        : Row(
            children: [
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Text(
                      'CIJENA SALE',
                      style: TextStyle(
                        color: AppColors.primary,
                        fontSize: 9,
                        fontWeight: FontWeight.w900,
                        letterSpacing: 1,
                      ),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      '${duration.toStringAsFixed(duration % 1 == 0 ? 0 : 1)} h × ${money(hall.hourlyPrice)}',
                      style: const TextStyle(fontWeight: FontWeight.w800),
                    ),
                  ],
                ),
              ),
              Text(
                money(duration * hall.hourlyPrice),
                style: const TextStyle(
                  color: AppColors.ink,
                  fontSize: 20,
                  fontWeight: FontWeight.w900,
                ),
              ),
            ],
          ),
  );
}
