import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/intl.dart';
import 'package:openamp_mobile/core/theme/app_theme.dart';
import 'package:openamp_mobile/models/models.dart';
import 'package:openamp_mobile/state/app_state.dart';
import 'package:openamp_mobile/widgets/common.dart';

class ReservationsScreen extends ConsumerWidget {
  const ReservationsScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final state = ref.watch(appControllerProvider);
    final now = DateTime.now().toUtc();
    final upcoming = state.reservations
        .where(
          (x) =>
              x.endsAt.isAfter(now) &&
              !x.status.toLowerCase().contains('otkaz'),
        )
        .toList();
    final history = state.reservations
        .where((x) => !upcoming.contains(x))
        .toList();
    return DefaultTabController(
      length: 2,
      child: Scaffold(
        appBar: AppBar(
          title: const Text('Moje rezervacije'),
          bottom: const TabBar(
            tabs: [
              Tab(text: 'Predstojeće'),
              Tab(text: 'Historija'),
            ],
          ),
          actions: [
            IconButton(
              onPressed: state.busy
                  ? null
                  : ref.read(appControllerProvider.notifier).refreshPrivateData,
              icon: const Icon(Icons.refresh),
            ),
          ],
        ),
        body: TabBarView(
          children: [
            _ReservationList(items: upcoming),
            _ReservationList(items: history),
          ],
        ),
      ),
    );
  }
}

class _ReservationList extends StatelessWidget {
  const _ReservationList({required this.items});
  final List<Reservation> items;

  @override
  Widget build(BuildContext context) {
    if (items.isEmpty) {
      return const Center(child: Text('Nema rezervacija u ovoj kategoriji.'));
    }
    return ListView.separated(
      padding: const EdgeInsets.all(18),
      itemCount: items.length,
      separatorBuilder: (_, _) => const SizedBox(height: 12),
      itemBuilder: (_, index) {
        final item = items[index];
        return Card(
          clipBehavior: Clip.antiAlias,
          child: Column(
            children: [
              HallImage(url: item.imageUrl, height: 115, borderRadius: 0),
              ListTile(
                title: Text(
                  item.hall,
                  style: Theme.of(context).textTheme.titleMedium,
                ),
                subtitle: Text(
                  item.studio +
                      '\n' +
                      DateFormat(
                        'dd.MM.yyyy. · HH:mm',
                      ).format(item.startsAt.toLocal()) +
                      '–' +
                      DateFormat('HH:mm').format(item.endsAt.toLocal()) +
                      '\n' +
                      item.band,
                ),
                isThreeLine: true,
                trailing: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  crossAxisAlignment: CrossAxisAlignment.end,
                  children: [
                    Text(
                      money(item.total),
                      style: const TextStyle(fontWeight: FontWeight.w900),
                    ),
                    const SizedBox(height: 5),
                    Text(
                      item.status,
                      style: TextStyle(
                        color: item.status.toLowerCase().contains('pla')
                            ? AppColors.success
                            : AppColors.primary,
                        fontWeight: FontWeight.w700,
                        fontSize: 12,
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
        );
      },
    );
  }
}
