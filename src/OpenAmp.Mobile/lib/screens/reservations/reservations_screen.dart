import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/intl.dart';
import 'package:openamp_mobile/core/theme/app_theme.dart';
import 'package:openamp_mobile/models/models.dart';
import 'package:openamp_mobile/screens/reservations/reservation_details_screen.dart';
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
          title: const Text('Rezervacije'),
          actions: [
            IconButton(
              onPressed: state.busy
                  ? null
                  : ref.read(appControllerProvider.notifier).refreshPrivateData,
              icon: const Icon(Icons.refresh_rounded),
            ),
            const SizedBox(width: 8),
          ],
        ),
        body: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Container(
              color: AppColors.paper,
              child: const TabBar(
                tabs: [
                  Tab(text: 'PREDSTOJEĆE'),
                  Tab(text: 'HISTORIJA'),
                ],
              ),
            ),
            Expanded(
              child: TabBarView(
                children: [
                  _ReservationList(items: upcoming),
                  _ReservationList(items: history),
                ],
              ),
            ),
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
      return const Center(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(Icons.album_outlined, size: 54, color: AppColors.textMuted),
            SizedBox(height: 12),
            Text('Još nema termina u ovoj kategoriji.'),
          ],
        ),
      );
    }
    return ListView.separated(
      padding: const EdgeInsets.all(18),
      itemCount: items.length,
      separatorBuilder: (_, _) => const SizedBox(height: 13),
      itemBuilder: (context, index) => InkWell(
        borderRadius: BorderRadius.circular(AppRadii.large),
        onTap: () => Navigator.of(context).push(
          MaterialPageRoute<void>(
            builder: (_) => ReservationDetailsScreen(reservation: items[index]),
          ),
        ),
        child: _ReservationTicket(item: items[index], index: index),
      ),
    );
  }
}

class _ReservationTicket extends StatelessWidget {
  const _ReservationTicket({required this.item, required this.index});

  final Reservation item;
  final int index;

  @override
  Widget build(BuildContext context) {
    final start = item.startsAt.toLocal();
    final paid = item.status.toLowerCase().contains('pla');
    return Container(
      decoration: BoxDecoration(
        color: AppColors.paper,
        borderRadius: BorderRadius.circular(AppRadii.large),
        border: Border.all(color: AppColors.line),
      ),
      child: Column(
        children: [
          Padding(
            padding: const EdgeInsets.all(11),
            child: Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Container(
                  width: 64,
                  padding: const EdgeInsets.symmetric(vertical: 10),
                  decoration: BoxDecoration(
                    color: AppColors.ink,
                    borderRadius: BorderRadius.circular(11),
                  ),
                  child: Column(
                    children: [
                      Text(
                        DateFormat('MMM', 'bs').format(start).toUpperCase(),
                        style: const TextStyle(
                          color: AppColors.signal,
                          fontSize: 9,
                          fontWeight: FontWeight.w900,
                          letterSpacing: .8,
                        ),
                      ),
                      const SizedBox(height: 3),
                      Text(
                        start.day.toString().padLeft(2, '0'),
                        style: const TextStyle(
                          color: Colors.white,
                          fontSize: 25,
                          fontWeight: FontWeight.w900,
                        ),
                      ),
                      Text(
                        DateFormat('HH:mm').format(start),
                        style: const TextStyle(
                          color: Colors.white60,
                          fontSize: 10,
                        ),
                      ),
                    ],
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Row(
                        children: [
                          Expanded(
                            child: Text(
                              '${item.studio} / ${item.band}'.toUpperCase(),
                              maxLines: 1,
                              overflow: TextOverflow.ellipsis,
                              style: const TextStyle(
                                color: AppColors.primary,
                                fontSize: 9,
                                fontWeight: FontWeight.w900,
                                letterSpacing: .75,
                              ),
                            ),
                          ),
                          Text(
                            "#${(index + 1).toString().padLeft(2, '0')}",
                            style: const TextStyle(
                              color: AppColors.textMuted,
                              fontSize: 9,
                              fontWeight: FontWeight.w900,
                            ),
                          ),
                        ],
                      ),
                      const SizedBox(height: 5),
                      Text(
                        item.hall,
                        style: Theme.of(context).textTheme.titleMedium,
                      ),
                      const SizedBox(height: 5),
                      Text(
                        '${DateFormat('HH:mm').format(start)}–${DateFormat('HH:mm').format(item.endsAt.toLocal())} · ${item.band}',
                        style: const TextStyle(fontSize: 12),
                      ),
                      const SizedBox(height: 9),
                      Row(
                        children: [
                          Expanded(
                            child: Text(
                              money(item.total),
                              style: const TextStyle(
                                fontSize: 17,
                                fontWeight: FontWeight.w900,
                              ),
                            ),
                          ),
                          Container(
                            padding: const EdgeInsets.symmetric(
                              horizontal: 9,
                              vertical: 5,
                            ),
                            decoration: BoxDecoration(
                              color: paid
                                  ? AppColors.successSoft
                                  : AppColors.primarySoft,
                              borderRadius: BorderRadius.circular(6),
                            ),
                            child: Text(
                              item.status.toUpperCase(),
                              style: TextStyle(
                                color: paid
                                    ? AppColors.success
                                    : AppColors.primary,
                                fontSize: 9,
                                fontWeight: FontWeight.w900,
                                letterSpacing: .55,
                              ),
                            ),
                          ),
                        ],
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ),
          Container(
            height: 4,
            color: paid ? AppColors.success : AppColors.primary,
          ),
        ],
      ),
    );
  }
}
