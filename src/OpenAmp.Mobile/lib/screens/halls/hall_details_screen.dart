import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:openamp_mobile/core/theme/app_theme.dart';
import 'package:openamp_mobile/models/models.dart';
import 'package:openamp_mobile/screens/booking/slot_selection_screen.dart';
import 'package:openamp_mobile/state/app_state.dart';
import 'package:openamp_mobile/widgets/common.dart';
import 'package:url_launcher/url_launcher.dart';

class HallDetailsScreen extends ConsumerWidget {
  const HallDetailsScreen({super.key, required this.hallId});

  final int hallId;

  @override
  Widget build(BuildContext context, WidgetRef ref) =>
      FutureBuilder<HallDetails>(
        future: ref.read(repositoryProvider).getHall(hallId),
        builder: (context, snapshot) {
          if (snapshot.connectionState != ConnectionState.done) {
            return Scaffold(
              appBar: AppBar(title: const Text('Detalji sale')),
              body: const Center(child: CircularProgressIndicator()),
            );
          }
          if (snapshot.hasError) {
            return Scaffold(
              appBar: AppBar(title: const Text('Detalji sale')),
              body: Padding(
                padding: const EdgeInsets.all(20),
                child: ErrorBanner(message: snapshot.error.toString()),
              ),
            );
          }
          return _HallDetailsView(hall: snapshot.data!);
        },
      );
}

class _HallDetailsView extends ConsumerWidget {
  const _HallDetailsView({required this.hall});

  final HallDetails hall;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final saved = ref
        .watch(appControllerProvider)
        .favoriteHallIds
        .contains(hall.id);
    return Scaffold(
      body: CustomScrollView(
        slivers: [
          SliverAppBar(
            pinned: true,
            expandedHeight: 296,
            backgroundColor: AppColors.ink,
            foregroundColor: Colors.white,
            surfaceTintColor: Colors.transparent,
            title: Text(
              "SALA / ${hall.id.toString().padLeft(2, '0')}",
              style: const TextStyle(
                color: Colors.white,
                fontSize: 11,
                fontWeight: FontWeight.w900,
                letterSpacing: 1.1,
              ),
            ),
            actions: [
              IconButton(
                tooltip: saved ? 'Ukloni iz omiljenih' : 'Sačuvaj',
                onPressed: () async {
                  try {
                    await ref
                        .read(appControllerProvider.notifier)
                        .setFavoriteHall(hall.id, !saved);
                    if (context.mounted) {
                      ScaffoldMessenger.of(context).showSnackBar(
                        SnackBar(
                          content: Text(
                            saved
                                ? 'Sala je uklonjena iz omiljenih.'
                                : 'Sala je sačuvana u omiljene.',
                          ),
                        ),
                      );
                    }
                  } catch (_) {}
                },
                icon: Icon(
                  saved
                      ? Icons.favorite_rounded
                      : Icons.favorite_border_rounded,
                  color: saved ? AppColors.signal : Colors.white,
                ),
              ),
              const SizedBox(width: 6),
            ],
            flexibleSpace: FlexibleSpaceBar(
              background: Stack(
                fit: StackFit.expand,
                children: [
                  PageView.builder(
                    itemCount: hall.gallery.isEmpty ? 1 : hall.gallery.length,
                    itemBuilder: (_, index) => HallImage(
                      url: hall.gallery.isEmpty ? null : hall.gallery[index],
                      height: 296,
                      borderRadius: 0,
                    ),
                  ),
                  const DecoratedBox(
                    decoration: BoxDecoration(
                      gradient: LinearGradient(
                        begin: Alignment.topCenter,
                        end: Alignment.bottomCenter,
                        colors: [
                          Colors.black26,
                          Colors.transparent,
                          Colors.black87,
                        ],
                        stops: [0, .5, 1],
                      ),
                    ),
                  ),
                  Positioned(
                    left: 20,
                    right: 20,
                    bottom: 22,
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          '${hall.studio} / ${hall.city}'.toUpperCase(),
                          style: const TextStyle(
                            color: AppColors.signal,
                            fontSize: 10,
                            fontWeight: FontWeight.w900,
                            letterSpacing: 1.1,
                          ),
                        ),
                        const SizedBox(height: 6),
                        Text(
                          hall.name,
                          maxLines: 2,
                          overflow: TextOverflow.ellipsis,
                          style: const TextStyle(
                            color: Colors.white,
                            fontSize: 29,
                            height: 1,
                            fontWeight: FontWeight.w900,
                            letterSpacing: -1,
                          ),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),
          ),
          SliverPadding(
            padding: const EdgeInsets.fromLTRB(18, 20, 18, 126),
            sliver: SliverList.list(
              children: [
                _SpecStrip(hall: hall),
                const SizedBox(height: 14),
                OutlinedButton.icon(
                  onPressed: () => _openMap(hall),
                  icon: const Icon(Icons.directions_outlined),
                  label: Text('Navigacija do ${hall.studio}'),
                ),
                if (hall.description != null) ...[
                  const SizedBox(height: 28),
                  const SectionEyebrow('Opis'),
                  const SizedBox(height: 9),
                  Text(
                    'O prostoru',
                    style: Theme.of(context).textTheme.titleLarge,
                  ),
                  const SizedBox(height: 9),
                  Text(
                    hall.description!,
                    style: const TextStyle(
                      color: AppColors.inkSoft,
                      fontSize: 15,
                      height: 1.55,
                    ),
                  ),
                ],
                if (hall.acoustics != null) ...[
                  const SizedBox(height: 17),
                  Container(
                    padding: const EdgeInsets.all(16),
                    decoration: BoxDecoration(
                      color: AppColors.ink,
                      borderRadius: BorderRadius.circular(AppRadii.medium),
                    ),
                    child: Row(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        const Icon(
                          Icons.graphic_eq_rounded,
                          color: AppColors.signal,
                        ),
                        const SizedBox(width: 12),
                        Expanded(
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              const Text(
                                'AKUSTIKA',
                                style: TextStyle(
                                  color: AppColors.signal,
                                  fontSize: 9,
                                  fontWeight: FontWeight.w900,
                                  letterSpacing: 1.2,
                                ),
                              ),
                              const SizedBox(height: 6),
                              Text(
                                hall.acoustics!,
                                style: const TextStyle(
                                  color: Colors.white,
                                  height: 1.4,
                                ),
                              ),
                            ],
                          ),
                        ),
                      ],
                    ),
                  ),
                ],
                const SizedBox(height: 29),
                Row(
                  crossAxisAlignment: CrossAxisAlignment.end,
                  children: [
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          const SectionEyebrow('Oprema'),
                          const SizedBox(height: 8),
                          Text(
                            'Oprema u prostoru',
                            style: Theme.of(context).textTheme.titleLarge,
                          ),
                        ],
                      ),
                    ),
                    Text(
                      '${hall.equipment.length} stavke',
                      style: const TextStyle(
                        color: AppColors.primary,
                        fontSize: 10,
                        fontWeight: FontWeight.w900,
                        letterSpacing: 1,
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 11),
                ...hall.equipment.indexed.map(
                  (entry) => _EquipmentRow(index: entry.$1, item: entry.$2),
                ),
                if (hall.reviews.isNotEmpty) ...[
                  const SizedBox(height: 28),
                  const SectionEyebrow('Recenzije'),
                  const SizedBox(height: 8),
                  Text(
                    'Šta kažu muzičari',
                    style: Theme.of(context).textTheme.titleLarge,
                  ),
                  const SizedBox(height: 11),
                  ...hall.reviews
                      .take(3)
                      .map((review) => _ReviewRow(review: review)),
                ],
              ],
            ),
          ),
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
            label: 'Rezerviši · ${money(hall.hourlyPrice)}/h',
            onPressed: () => Navigator.of(context).push(
              MaterialPageRoute<void>(
                builder: (_) => SlotSelectionScreen(hall: hall),
              ),
            ),
          ),
        ),
      ),
    );
  }

  Future<void> _openMap(HallDetails hall) async {
    final query = hall.latitude != null && hall.longitude != null
        ? '${hall.latitude},${hall.longitude}'
        : '${hall.address}, ${hall.city}';
    final uri = Uri.https('www.google.com', '/maps/search/', {
      'api': '1',
      'query': query,
    });
    await launchUrl(uri, mode: LaunchMode.externalApplication);
  }
}

class _SpecStrip extends StatelessWidget {
  const _SpecStrip({required this.hall});

  final HallDetails hall;

  @override
  Widget build(BuildContext context) => Container(
    decoration: const BoxDecoration(
      border: Border(
        top: BorderSide(color: AppColors.line),
        bottom: BorderSide(color: AppColors.line),
      ),
    ),
    child: IntrinsicHeight(
      child: Row(
        children: [
          _Spec(
            icon: Icons.star_rounded,
            value: hall.reviewCount == 0
                ? 'Novo'
                : hall.rating.toStringAsFixed(1),
            label: '${hall.reviewCount} recenzija',
          ),
          const VerticalDivider(width: 1, color: AppColors.line),
          _Spec(
            icon: Icons.groups_2_outlined,
            value: hall.capacity.toString(),
            label: 'članova max',
          ),
          const VerticalDivider(width: 1, color: AppColors.line),
          _Spec(
            icon: Icons.location_on_outlined,
            value: hall.city,
            label: hall.address,
          ),
        ],
      ),
    ),
  );
}

class _Spec extends StatelessWidget {
  const _Spec({required this.icon, required this.value, required this.label});

  final IconData icon;
  final String value;
  final String label;

  @override
  Widget build(BuildContext context) => Expanded(
    child: Padding(
      padding: const EdgeInsets.symmetric(horizontal: 9, vertical: 13),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Icon(icon, size: 17, color: AppColors.signal),
          const SizedBox(height: 7),
          Text(
            value,
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
            style: const TextStyle(fontWeight: FontWeight.w900),
          ),
          const SizedBox(height: 2),
          Text(
            label,
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
            style: const TextStyle(color: AppColors.textMuted, fontSize: 9),
          ),
        ],
      ),
    ),
  );
}

class _EquipmentRow extends StatelessWidget {
  const _EquipmentRow({required this.index, required this.item});

  final int index;
  final EquipmentItem item;

  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.symmetric(vertical: 12),
    decoration: const BoxDecoration(
      border: Border(bottom: BorderSide(color: AppColors.line)),
    ),
    child: Row(
      children: [
        SizedBox(
          width: 32,
          child: Text(
            (index + 1).toString().padLeft(2, '0'),
            style: const TextStyle(
              color: AppColors.signal,
              fontSize: 10,
              fontWeight: FontWeight.w900,
            ),
          ),
        ),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                item.name,
                style: const TextStyle(fontWeight: FontWeight.w800),
              ),
              Text(
                item.category.toUpperCase(),
                style: const TextStyle(
                  color: AppColors.textMuted,
                  fontSize: 9,
                  fontWeight: FontWeight.w700,
                  letterSpacing: .7,
                ),
              ),
            ],
          ),
        ),
        Text(
          item.hourlyPrice > 0 ? '${money(item.hourlyPrice)}/h' : 'UKLJUČENO',
          style: TextStyle(
            color: item.hourlyPrice > 0 ? AppColors.text : AppColors.success,
            fontSize: 10,
            fontWeight: FontWeight.w900,
          ),
        ),
      ],
    ),
  );
}

class _ReviewRow extends StatelessWidget {
  const _ReviewRow({required this.review});

  final HallReview review;

  @override
  Widget build(BuildContext context) => Padding(
    padding: const EdgeInsets.only(bottom: 10),
    child: Container(
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: AppColors.paper,
        borderRadius: BorderRadius.circular(AppRadii.medium),
        border: Border.all(color: AppColors.line),
      ),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          CircleAvatar(
            radius: 18,
            backgroundColor: AppColors.ink,
            foregroundColor: Colors.white,
            child: Text(
              initials(review.user),
              style: const TextStyle(fontSize: 11, fontWeight: FontWeight.w900),
            ),
          ),
          const SizedBox(width: 11),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  children: [
                    Expanded(
                      child: Text(
                        review.user,
                        style: const TextStyle(fontWeight: FontWeight.w900),
                      ),
                    ),
                    Text(
                      '★ ${review.rating}',
                      style: const TextStyle(
                        color: AppColors.warning,
                        fontWeight: FontWeight.w900,
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 4),
                Text(review.comment ?? 'Bez komentara'),
              ],
            ),
          ),
        ],
      ),
    ),
  );
}
