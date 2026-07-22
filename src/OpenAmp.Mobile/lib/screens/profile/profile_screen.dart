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
      appBar: AppBar(
        title: const Text('Moj profil'),
        actions: [
          IconButton(
            tooltip: 'Osvježi',
            onPressed: state.busy
                ? null
                : ref.read(appControllerProvider.notifier).refreshPrivateData,
            icon: const Icon(Icons.refresh_rounded),
          ),
          const SizedBox(width: 8),
        ],
      ),
      body: RefreshIndicator(
        color: AppColors.signal,
        onRefresh: ref.read(appControllerProvider.notifier).refreshPrivateData,
        child: ListView(
          padding: const EdgeInsets.fromLTRB(18, 7, 18, 28),
          children: [
            Container(
              padding: const EdgeInsets.all(18),
              decoration: BoxDecoration(
                color: AppColors.ink,
                borderRadius: BorderRadius.circular(AppRadii.large),
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const Row(
                    children: [
                      SectionEyebrow('OpenAmp member', color: AppColors.signal),
                      Spacer(),
                      Text(
                        'ACTIVE',
                        style: TextStyle(
                          color: AppColors.success,
                          fontSize: 9,
                          fontWeight: FontWeight.w900,
                          letterSpacing: 1,
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 23),
                  Row(
                    children: [
                      Container(
                        width: 70,
                        height: 70,
                        alignment: Alignment.center,
                        decoration: BoxDecoration(
                          color: AppColors.signal,
                          borderRadius: BorderRadius.circular(18),
                        ),
                        child: Text(
                          initials(profile.fullName),
                          style: const TextStyle(
                            color: AppColors.ink,
                            fontSize: 24,
                            fontWeight: FontWeight.w900,
                          ),
                        ),
                      ),
                      const SizedBox(width: 14),
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              profile.fullName,
                              maxLines: 2,
                              overflow: TextOverflow.ellipsis,
                              style: const TextStyle(
                                color: Colors.white,
                                fontSize: 23,
                                height: 1,
                                fontWeight: FontWeight.w900,
                                letterSpacing: -.6,
                              ),
                            ),
                            const SizedBox(height: 7),
                            Text(
                              profile.email,
                              maxLines: 1,
                              overflow: TextOverflow.ellipsis,
                              style: const TextStyle(color: Colors.white60),
                            ),
                          ],
                        ),
                      ),
                    ],
                  ),
                  if (profile.instruments.isNotEmpty) ...[
                    const SizedBox(height: 18),
                    Wrap(
                      spacing: 7,
                      runSpacing: 7,
                      children: profile.instruments
                          .map(
                            (item) => Container(
                              padding: const EdgeInsets.symmetric(
                                horizontal: 10,
                                vertical: 6,
                              ),
                              decoration: BoxDecoration(
                                color: Colors.white10,
                                borderRadius: BorderRadius.circular(6),
                                border: Border.all(color: Colors.white24),
                              ),
                              child: Text(
                                item.toUpperCase(),
                                style: const TextStyle(
                                  color: Colors.white,
                                  fontSize: 9,
                                  fontWeight: FontWeight.w900,
                                  letterSpacing: .7,
                                ),
                              ),
                            ),
                          )
                          .toList(),
                    ),
                  ],
                ],
              ),
            ),
            const SizedBox(height: 16),
            Container(
              decoration: const BoxDecoration(
                border: Border(
                  top: BorderSide(color: AppColors.ink),
                  bottom: BorderSide(color: AppColors.ink),
                ),
              ),
              child: IntrinsicHeight(
                child: Row(
                  children: [
                    _Stat(
                      value: profile.bandCount.toString(),
                      label: 'Bendovi',
                    ),
                    const VerticalDivider(width: 1, color: AppColors.line),
                    _Stat(
                      value: profile.reservationCount.toString(),
                      label: 'Probe',
                    ),
                    const VerticalDivider(width: 1, color: AppColors.line),
                    _Stat(
                      value: profile.totalHours.toStringAsFixed(0),
                      label: 'Sati',
                    ),
                    const VerticalDivider(width: 1, color: AppColors.line),
                    _Stat(
                      value: profile.reviewCount.toString(),
                      label: 'Recenzije',
                    ),
                  ],
                ),
              ),
            ),
            if (profile.favoriteHall != null || profile.topGenre != null) ...[
              const SizedBox(height: 27),
              const SectionEyebrow('Tvoj sound'),
              const SizedBox(height: 9),
              Text(
                'Studio statistika',
                style: Theme.of(context).textTheme.titleLarge,
              ),
              const SizedBox(height: 11),
              Row(
                children: [
                  if (profile.favoriteHall != null)
                    Expanded(
                      child: _Highlight(
                        icon: Icons.favorite_outline_rounded,
                        label: 'OMILJENA SALA',
                        value: profile.favoriteHall!,
                      ),
                    ),
                  if (profile.favoriteHall != null && profile.topGenre != null)
                    const SizedBox(width: 10),
                  if (profile.topGenre != null)
                    Expanded(
                      child: _Highlight(
                        icon: Icons.graphic_eq_rounded,
                        label: 'TOP ŽANR',
                        value: profile.topGenre!,
                      ),
                    ),
                ],
              ),
            ],
            const SizedBox(height: 27),
            const SectionEyebrow('Postavke'),
            const SizedBox(height: 8),
            const _ProfileAction(
              icon: Icons.person_outline,
              label: 'Lični podaci',
            ),
            const _ProfileAction(
              icon: Icons.credit_card_outlined,
              label: 'Načini plaćanja',
            ),
            const _ProfileAction(
              icon: Icons.notifications_none_rounded,
              label: 'Notifikacije',
            ),
            const _ProfileAction(
              icon: Icons.lock_outline_rounded,
              label: 'Sigurnost i lozinka',
            ),
            const SizedBox(height: 18),
            OutlinedButton.icon(
              onPressed: ref.read(appControllerProvider.notifier).logout,
              icon: const Icon(Icons.logout_rounded),
              label: const Text('Odjavi se'),
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
    child: Padding(
      padding: const EdgeInsets.symmetric(vertical: 13),
      child: Column(
        children: [
          Text(
            value,
            style: const TextStyle(
              color: AppColors.ink,
              fontSize: 21,
              fontWeight: FontWeight.w900,
            ),
          ),
          const SizedBox(height: 2),
          Text(
            label.toUpperCase(),
            style: const TextStyle(
              color: AppColors.textMuted,
              fontSize: 8,
              fontWeight: FontWeight.w900,
              letterSpacing: .7,
            ),
          ),
        ],
      ),
    ),
  );
}

class _Highlight extends StatelessWidget {
  const _Highlight({
    required this.icon,
    required this.label,
    required this.value,
  });

  final IconData icon;
  final String label;
  final String value;

  @override
  Widget build(BuildContext context) => Container(
    height: 114,
    padding: const EdgeInsets.all(14),
    decoration: BoxDecoration(
      color: AppColors.paper,
      borderRadius: BorderRadius.circular(AppRadii.medium),
      border: Border.all(color: AppColors.line),
    ),
    child: Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Icon(icon, color: AppColors.signal, size: 20),
        const Spacer(),
        Text(
          label,
          style: const TextStyle(
            color: AppColors.textMuted,
            fontSize: 8,
            fontWeight: FontWeight.w900,
            letterSpacing: .8,
          ),
        ),
        const SizedBox(height: 3),
        Text(
          value,
          maxLines: 1,
          overflow: TextOverflow.ellipsis,
          style: const TextStyle(fontWeight: FontWeight.w900),
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
  Widget build(BuildContext context) => InkWell(
    onTap: () => ScaffoldMessenger.of(
      context,
    ).showSnackBar(SnackBar(content: Text('$label će biti dostupno uskoro.'))),
    child: Container(
      padding: const EdgeInsets.symmetric(vertical: 14),
      decoration: const BoxDecoration(
        border: Border(bottom: BorderSide(color: AppColors.line)),
      ),
      child: Row(
        children: [
          Icon(icon, color: AppColors.primary, size: 21),
          const SizedBox(width: 12),
          Expanded(
            child: Text(
              label,
              style: const TextStyle(fontWeight: FontWeight.w800),
            ),
          ),
          const Icon(Icons.arrow_forward_rounded, size: 19),
        ],
      ),
    ),
  );
}
