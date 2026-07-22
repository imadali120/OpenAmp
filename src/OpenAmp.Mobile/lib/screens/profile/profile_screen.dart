import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:openamp_mobile/core/theme/app_theme.dart';
import 'package:openamp_mobile/state/app_state.dart';
import 'package:openamp_mobile/widgets/common.dart';

class ProfileScreen extends ConsumerWidget {
  const ProfileScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final state = ref.watch(appControllerProvider);
    final profile = state.profile;
    if (profile == null) {
      return Scaffold(
        appBar: AppBar(title: const Text('Profil')),
        body: const Center(child: CircularProgressIndicator()),
      );
    }
    return Scaffold(
      appBar: AppBar(title: const Text('Profil')),
      body: RefreshIndicator(
        onRefresh: ref.read(appControllerProvider.notifier).refreshPrivateData,
        child: ListView(
          children: [
            Container(
              padding: const EdgeInsets.fromLTRB(20, 28, 20, 32),
              color: AppColors.primaryDark,
              child: Column(
                children: [
                  CircleAvatar(
                    radius: 48,
                    backgroundColor: AppColors.primary,
                    child: Text(
                      initials(profile.fullName),
                      style: const TextStyle(
                        color: Colors.white,
                        fontSize: 30,
                        fontWeight: FontWeight.w900,
                      ),
                    ),
                  ),
                  const SizedBox(height: 15),
                  Text(
                    profile.fullName,
                    style: const TextStyle(
                      color: Colors.white,
                      fontSize: 23,
                      fontWeight: FontWeight.w900,
                    ),
                  ),
                  Text(
                    profile.email,
                    style: const TextStyle(color: Colors.white70),
                  ),
                  if (profile.instruments.isNotEmpty) ...[
                    const SizedBox(height: 12),
                    Wrap(
                      spacing: 7,
                      children: profile.instruments
                          .map(
                            (item) => Chip(
                              avatar: const Icon(Icons.music_note, size: 17),
                              label: Text(item),
                              backgroundColor: Colors.white,
                            ),
                          )
                          .toList(),
                    ),
                  ],
                ],
              ),
            ),
            Padding(
              padding: const EdgeInsets.all(18),
              child: Column(
                children: [
                  Card(
                    child: Padding(
                      padding: const EdgeInsets.symmetric(
                        horizontal: 10,
                        vertical: 18,
                      ),
                      child: Row(
                        children: [
                          _Stat(
                            value: profile.bandCount.toString(),
                            label: 'Bendovi',
                          ),
                          _Stat(
                            value: profile.reservationCount.toString(),
                            label: 'Probe',
                          ),
                          _Stat(
                            value: profile.totalHours.toStringAsFixed(0),
                            label: 'Sati',
                          ),
                          _Stat(
                            value: profile.reviewCount.toString(),
                            label: 'Recenzije',
                          ),
                        ],
                      ),
                    ),
                  ),
                  const SizedBox(height: 14),
                  if (profile.favoriteHall != null || profile.topGenre != null)
                    Card(
                      child: Column(
                        children: [
                          if (profile.favoriteHall != null)
                            ListTile(
                              leading: const Icon(
                                Icons.favorite_outline,
                                color: AppColors.primary,
                              ),
                              title: const Text('Omiljena sala'),
                              subtitle: Text(profile.favoriteHall!),
                            ),
                          if (profile.topGenre != null)
                            ListTile(
                              leading: const Icon(
                                Icons.graphic_eq,
                                color: AppColors.primary,
                              ),
                              title: const Text('Najčešći žanr'),
                              subtitle: Text(profile.topGenre!),
                            ),
                        ],
                      ),
                    ),
                  const SizedBox(height: 14),
                  const _ProfileAction(
                    icon: Icons.person_outline,
                    label: 'Lični podaci',
                  ),
                  const _ProfileAction(
                    icon: Icons.credit_card_outlined,
                    label: 'Načini plaćanja',
                  ),
                  const _ProfileAction(
                    icon: Icons.notifications_none,
                    label: 'Notifikacije',
                  ),
                  const _ProfileAction(
                    icon: Icons.lock_outline,
                    label: 'Sigurnost i lozinka',
                  ),
                  const SizedBox(height: 14),
                  SizedBox(
                    width: double.infinity,
                    child: OutlinedButton.icon(
                      onPressed: ref
                          .read(appControllerProvider.notifier)
                          .logout,
                      icon: const Icon(Icons.logout),
                      label: const Text('Odjavi se'),
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _Stat extends StatelessWidget {
  const _Stat({required this.value, required this.label});
  final String value;
  final String label;

  @override
  Widget build(BuildContext context) => Expanded(
    child: Column(
      children: [
        Text(
          value,
          style: const TextStyle(
            color: AppColors.primary,
            fontSize: 22,
            fontWeight: FontWeight.w900,
          ),
        ),
        Text(
          label,
          style: const TextStyle(fontSize: 12, fontWeight: FontWeight.w700),
        ),
      ],
    ),
  );
}

class _ProfileAction extends StatelessWidget {
  const _ProfileAction({required this.icon, required this.label});
  final IconData icon;
  final String label;

  @override
  Widget build(BuildContext context) => Padding(
    padding: const EdgeInsets.only(bottom: 9),
    child: Card(
      child: ListTile(
        leading: Icon(icon, color: AppColors.primary),
        title: Text(label),
        trailing: const Icon(Icons.chevron_right),
        onTap: () => ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(label + ' će biti dostupno uskoro.')),
        ),
      ),
    ),
  );
}
