import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:openamp_mobile/core/theme/app_theme.dart';
import 'package:openamp_mobile/models/models.dart';
import 'package:openamp_mobile/screens/booking/slot_selection_screen.dart';
import 'package:openamp_mobile/state/app_state.dart';
import 'package:openamp_mobile/widgets/common.dart';

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

class _HallDetailsView extends StatelessWidget {
  const _HallDetailsView({required this.hall});
  final HallDetails hall;

  @override
  Widget build(BuildContext context) => Scaffold(
    appBar: AppBar(
      title: const Text('Detalji sale'),
      actions: [
        IconButton(
          tooltip: 'Sačuvaj',
          onPressed: () => ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(content: Text('Sala je dodana u omiljene.')),
          ),
          icon: const Icon(Icons.favorite_border),
        ),
      ],
    ),
    body: CustomScrollView(
      slivers: [
        SliverToBoxAdapter(
          child: SizedBox(
            height: 245,
            child: PageView.builder(
              itemCount: hall.gallery.isEmpty ? 1 : hall.gallery.length,
              itemBuilder: (_, index) => HallImage(
                url: hall.gallery.isEmpty ? null : hall.gallery[index],
                height: 245,
                borderRadius: 0,
              ),
            ),
          ),
        ),
        SliverPadding(
          padding: const EdgeInsets.fromLTRB(20, 20, 20, 120),
          sliver: SliverList.list(
            children: [
              Row(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          hall.name,
                          style: Theme.of(context).textTheme.headlineMedium,
                        ),
                        const SizedBox(height: 4),
                        Text(hall.studio + ' · ' + hall.city),
                      ],
                    ),
                  ),
                  Column(
                    crossAxisAlignment: CrossAxisAlignment.end,
                    children: [
                      Text(
                        money(hall.hourlyPrice) + '/h',
                        style: const TextStyle(
                          color: AppColors.primary,
                          fontSize: 22,
                          fontWeight: FontWeight.w900,
                        ),
                      ),
                      Row(
                        children: [
                          const Icon(
                            Icons.star_rounded,
                            color: AppColors.warning,
                            size: 18,
                          ),
                          Text(
                            hall.reviewCount == 0
                                ? ' Nova'
                                : ' ' + hall.rating.toStringAsFixed(1),
                          ),
                        ],
                      ),
                    ],
                  ),
                ],
              ),
              const SizedBox(height: 18),
              _InfoRow(
                icon: Icons.groups_2_outlined,
                text: 'Kapacitet do ' + hall.capacity.toString() + ' osoba',
              ),
              _InfoRow(
                icon: Icons.location_on_outlined,
                text: hall.address + ', ' + hall.city,
              ),
              if (hall.description != null) ...[
                const SizedBox(height: 18),
                Text(
                  'O prostoru',
                  style: Theme.of(context).textTheme.titleLarge,
                ),
                const SizedBox(height: 8),
                Text(hall.description!),
              ],
              if (hall.acoustics != null) ...[
                const SizedBox(height: 14),
                Card(
                  child: Padding(
                    padding: const EdgeInsets.all(14),
                    child: Row(
                      children: [
                        const Icon(Icons.spatial_audio_off_outlined),
                        const SizedBox(width: 12),
                        Expanded(child: Text(hall.acoustics!)),
                      ],
                    ),
                  ),
                ),
              ],
              const SizedBox(height: 22),
              Text(
                'Osnovna i dodatna oprema',
                style: Theme.of(context).textTheme.titleLarge,
              ),
              const SizedBox(height: 10),
              ...hall.equipment.map(
                (item) => Padding(
                  padding: const EdgeInsets.only(bottom: 8),
                  child: Card(
                    child: ListTile(
                      leading: const CircleAvatar(
                        backgroundColor: Color(0xFFECE9FF),
                        child: Icon(
                          Icons.music_note_rounded,
                          color: AppColors.primary,
                        ),
                      ),
                      title: Text(item.name),
                      subtitle: Text(item.category),
                      trailing: item.hourlyPrice > 0
                          ? Text(money(item.hourlyPrice) + '/h')
                          : const Text('Uključeno'),
                    ),
                  ),
                ),
              ),
              if (hall.reviews.isNotEmpty) ...[
                const SizedBox(height: 20),
                Text(
                  'Iskustva muzičara',
                  style: Theme.of(context).textTheme.titleLarge,
                ),
                const SizedBox(height: 10),
                ...hall.reviews
                    .take(3)
                    .map(
                      (review) => ListTile(
                        contentPadding: EdgeInsets.zero,
                        leading: CircleAvatar(
                          child: Text(initials(review.user)),
                        ),
                        title: Text(review.user),
                        subtitle: Text(review.comment ?? 'Bez komentara'),
                        trailing: Text('★ ' + review.rating.toString()),
                      ),
                    ),
              ],
            ],
          ),
        ),
      ],
    ),
    bottomNavigationBar: SafeArea(
      minimum: const EdgeInsets.fromLTRB(20, 10, 20, 14),
      child: FilledButton(
        onPressed: () => Navigator.of(context).push(
          MaterialPageRoute<void>(
            builder: (_) => SlotSelectionScreen(hall: hall),
          ),
        ),
        child: const Text('Odaberi termin za probu'),
      ),
    ),
  );
}

class _InfoRow extends StatelessWidget {
  const _InfoRow({required this.icon, required this.text});
  final IconData icon;
  final String text;

  @override
  Widget build(BuildContext context) => Padding(
    padding: const EdgeInsets.only(bottom: 8),
    child: Row(
      children: [
        Icon(icon, size: 20, color: AppColors.primary),
        const SizedBox(width: 9),
        Expanded(child: Text(text)),
      ],
    ),
  );
}
