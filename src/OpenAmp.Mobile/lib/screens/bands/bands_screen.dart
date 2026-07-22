import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:openamp_mobile/core/theme/app_theme.dart';
import 'package:openamp_mobile/models/models.dart';
import 'package:openamp_mobile/state/app_state.dart';
import 'package:openamp_mobile/widgets/common.dart';

class BandsScreen extends ConsumerWidget {
  const BandsScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final state = ref.watch(appControllerProvider);
    return Scaffold(
      appBar: AppBar(
        title: const Text('Moji bendovi'),
        actions: [
          IconButton(
            tooltip: 'Osvježi',
            onPressed: state.busy
                ? null
                : ref.read(appControllerProvider.notifier).refreshPrivateData,
            icon: const Icon(Icons.refresh),
          ),
        ],
      ),
      body: RefreshIndicator(
        onRefresh: ref.read(appControllerProvider.notifier).refreshPrivateData,
        child: ListView(
          padding: const EdgeInsets.fromLTRB(18, 10, 18, 24),
          children: [
            Align(
              alignment: Alignment.centerRight,
              child: FilledButton.icon(
                onPressed: state.busy
                    ? null
                    : () => _showCreateBand(context, ref),
                icon: const Icon(Icons.add),
                label: const Text('Novi bend'),
                style: FilledButton.styleFrom(
                  minimumSize: const Size(0, 48),
                  padding: const EdgeInsets.symmetric(horizontal: 20),
                ),
              ),
            ),
            const SizedBox(height: 14),
            if (state.error != null) ErrorBanner(message: state.error!),
            if (state.bands.isEmpty)
              const Padding(
                padding: EdgeInsets.only(top: 80),
                child: Column(
                  children: [
                    Icon(
                      Icons.groups_2_outlined,
                      size: 66,
                      color: AppColors.textMuted,
                    ),
                    SizedBox(height: 14),
                    Text('Još nemaš bend. Kreiraj prvi!'),
                  ],
                ),
              )
            else
              ...state.bands.map(
                (band) => Padding(
                  padding: const EdgeInsets.only(bottom: 12),
                  child: _BandCard(
                    band: band,
                    onInvite: band.isFounder
                        ? () => _showInvite(context, ref, band)
                        : null,
                  ),
                ),
              ),
          ],
        ),
      ),
    );
  }

  Future<void> _showCreateBand(BuildContext context, WidgetRef ref) async {
    final lookups = ref.read(appControllerProvider).lookups;
    if (lookups == null || lookups.genres.isEmpty) return;
    final name = TextEditingController();
    final description = TextEditingController();
    var genre = lookups.genres.first;
    final accepted = await showDialog<bool>(
      context: context,
      builder: (context) => StatefulBuilder(
        builder: (context, setDialogState) => AlertDialog(
          title: const Text('Novi bend'),
          content: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              TextField(
                controller: name,
                autofocus: true,
                decoration: const InputDecoration(labelText: 'Naziv benda'),
              ),
              const SizedBox(height: 12),
              DropdownButtonFormField<LookupItem>(
                initialValue: genre,
                decoration: const InputDecoration(labelText: 'Žanr'),
                items: lookups.genres
                    .map(
                      (item) =>
                          DropdownMenuItem(value: item, child: Text(item.name)),
                    )
                    .toList(),
                onChanged: (value) {
                  if (value != null) setDialogState(() => genre = value);
                },
              ),
              const SizedBox(height: 12),
              TextField(
                controller: description,
                maxLines: 3,
                decoration: const InputDecoration(
                  labelText: 'Opis (opcionalno)',
                ),
              ),
            ],
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.pop(context, false),
              child: const Text('Odustani'),
            ),
            FilledButton(
              onPressed: () => Navigator.pop(context, true),
              child: const Text('Kreiraj'),
            ),
          ],
        ),
      ),
    );
    if (accepted != true || name.text.trim().length < 2) return;
    try {
      await ref
          .read(appControllerProvider.notifier)
          .createBand(name.text, genre.id, description.text);
    } catch (_) {}
  }

  Future<void> _showInvite(
    BuildContext context,
    WidgetRef ref,
    Band band,
  ) async {
    final email = TextEditingController();
    final accepted = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: Text('Pozovi u ' + band.name),
        content: TextField(
          controller: email,
          autofocus: true,
          keyboardType: TextInputType.emailAddress,
          decoration: const InputDecoration(
            labelText: 'Email muzičara',
            prefixIcon: Icon(Icons.alternate_email),
          ),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: const Text('Odustani'),
          ),
          FilledButton(
            onPressed: () => Navigator.pop(context, true),
            child: const Text('Pošalji pozivnicu'),
          ),
        ],
      ),
    );
    if (accepted != true || !email.text.contains('@')) return;
    try {
      await ref
          .read(appControllerProvider.notifier)
          .inviteMember(band.id, email.text);
      if (context.mounted) {
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(const SnackBar(content: Text('Pozivnica je kreirana.')));
      }
    } catch (_) {}
  }
}

class _BandCard extends StatelessWidget {
  const _BandCard({required this.band, this.onInvite});
  final Band band;
  final VoidCallback? onInvite;

  Color get accent {
    final genre = band.genre.toLowerCase();
    if (genre.contains('metal')) return AppColors.primaryDark;
    if (genre.contains('jazz')) return const Color(0xFF258BCA);
    if (genre.contains('funk')) return const Color(0xFFE29B20);
    return AppColors.primary;
  }

  @override
  Widget build(BuildContext context) => Card(
    child: ExpansionTile(
      shape: const Border(),
      leading: Container(
        width: 6,
        height: 55,
        decoration: BoxDecoration(
          color: accent,
          borderRadius: BorderRadius.circular(8),
        ),
      ),
      title: Row(
        children: [
          Expanded(
            child: Text(
              band.name,
              style: Theme.of(context).textTheme.titleLarge,
            ),
          ),
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
            decoration: BoxDecoration(
              color: accent,
              borderRadius: BorderRadius.circular(999),
            ),
            child: Text(
              band.genre,
              style: const TextStyle(
                color: Colors.white,
                fontSize: 12,
                fontWeight: FontWeight.w800,
              ),
            ),
          ),
        ],
      ),
      subtitle: Text(
        band.reservationCount.toString() +
            ' rezervacija · ' +
            band.members.length.toString() +
            ' članova',
      ),
      childrenPadding: const EdgeInsets.fromLTRB(18, 0, 18, 18),
      children: [
        Align(
          alignment: Alignment.centerLeft,
          child: Wrap(
            spacing: 8,
            runSpacing: 8,
            children: band.members
                .map(
                  (member) => Tooltip(
                    message:
                        member.fullName +
                        (member.instrument == null
                            ? ''
                            : ' · ' + member.instrument!),
                    child: CircleAvatar(
                      backgroundColor: member.isFounder
                          ? accent
                          : const Color(0xFF2BA875),
                      foregroundColor: Colors.white,
                      child: Text(
                        initials(member.fullName),
                        style: const TextStyle(fontWeight: FontWeight.w800),
                      ),
                    ),
                  ),
                )
                .toList(),
          ),
        ),
        if (band.invitations.isNotEmpty) ...[
          const SizedBox(height: 14),
          Align(
            alignment: Alignment.centerLeft,
            child: Text(
              'Aktivne pozivnice',
              style: Theme.of(context).textTheme.titleMedium,
            ),
          ),
          ...band.invitations
              .take(3)
              .map(
                (invite) => ListTile(
                  dense: true,
                  contentPadding: EdgeInsets.zero,
                  leading: const Icon(Icons.mail_outline),
                  title: Text(invite.email),
                  subtitle: Text(invite.status + ' · kod ' + invite.code),
                ),
              ),
        ],
        if (onInvite != null) ...[
          const SizedBox(height: 8),
          SizedBox(
            width: double.infinity,
            child: FilledButton.tonalIcon(
              onPressed: onInvite,
              icon: const Icon(Icons.person_add_alt),
              label: const Text('Pozovi člana'),
            ),
          ),
        ],
      ],
    ),
  );
}
