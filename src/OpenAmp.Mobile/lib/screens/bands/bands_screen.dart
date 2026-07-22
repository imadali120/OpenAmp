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
          padding: const EdgeInsets.fromLTRB(18, 5, 18, 24),
          children: [
            Row(
              crossAxisAlignment: CrossAxisAlignment.end,
              children: [
                Expanded(
                  child: Text(
                    'Bendovi',
                    style: Theme.of(context).textTheme.titleLarge,
                  ),
                ),
                IconButton.filled(
                  tooltip: 'Novi bend',
                  onPressed: state.busy
                      ? null
                      : () => _showCreateBand(context, ref),
                  style: IconButton.styleFrom(
                    backgroundColor: AppColors.signal,
                    foregroundColor: AppColors.ink,
                    minimumSize: const Size(52, 52),
                  ),
                  icon: const Icon(Icons.add_rounded),
                ),
              ],
            ),
            const SizedBox(height: 22),
            if (state.error != null) ErrorBanner(message: state.error!),
            if (state.receivedInvitations.any((item) => item.pending)) ...[
              const SectionEyebrow('Pozivnice za tebe'),
              const SizedBox(height: 8),
              ...state.receivedInvitations
                  .where((item) => item.pending)
                  .map(
                    (invitation) => Card(
                      child: ListTile(
                        leading: const CircleAvatar(
                          backgroundColor: AppColors.signal,
                          foregroundColor: AppColors.ink,
                          child: Icon(Icons.mark_email_unread_outlined),
                        ),
                        title: Text(invitation.band),
                        subtitle: Text(
                          '${invitation.genre} · pozvao ${invitation.invitedBy}',
                        ),
                        trailing: Row(
                          mainAxisSize: MainAxisSize.min,
                          children: [
                            IconButton(
                              tooltip: 'Odbij',
                              onPressed: state.busy
                                  ? null
                                  : () => _respondInvitation(
                                      context,
                                      ref,
                                      invitation,
                                      false,
                                    ),
                              icon: const Icon(Icons.close_rounded),
                            ),
                            IconButton.filled(
                              tooltip: 'Prihvati',
                              onPressed: state.busy
                                  ? null
                                  : () => _respondInvitation(
                                      context,
                                      ref,
                                      invitation,
                                      true,
                                    ),
                              icon: const Icon(Icons.check_rounded),
                            ),
                          ],
                        ),
                      ),
                    ),
                  ),
              const SizedBox(height: 18),
            ],
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
                    onEdit: band.isFounder
                        ? () => _showEditBand(context, ref, band)
                        : null,
                    onManageMember: band.isFounder
                        ? (member) =>
                              _showEditMember(context, ref, band, member)
                        : null,
                    onRemoveMember: band.isFounder
                        ? (member) => _removeMember(context, ref, band, member)
                        : null,
                    onLeave: !band.isFounder && state.session != null
                        ? () => _leaveBand(
                            context,
                            ref,
                            band,
                            state.session!.userId,
                          )
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

  Future<void> _respondInvitation(
    BuildContext context,
    WidgetRef ref,
    ReceivedBandInvitation invitation,
    bool accept,
  ) async {
    int? instrumentId;
    if (accept) {
      final instruments =
          ref.read(appControllerProvider).lookups?.instruments ?? [];
      instrumentId = await showDialog<int?>(
        context: context,
        builder: (context) => SimpleDialog(
          title: const Text('Koji instrument sviraš?'),
          children: [
            SimpleDialogOption(
              onPressed: () => Navigator.pop(context),
              child: const Text('Nije navedeno'),
            ),
            ...instruments.map(
              (item) => SimpleDialogOption(
                onPressed: () => Navigator.pop(context, item.id),
                child: Text(item.name),
              ),
            ),
          ],
        ),
      );
      if (!context.mounted) return;
    }
    try {
      await ref
          .read(appControllerProvider.notifier)
          .respondToInvitation(invitation.id, accept, instrumentId);
      if (context.mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(
              accept
                  ? 'Dobrodošao u ${invitation.band}!'
                  : 'Pozivnica je odbijena.',
            ),
          ),
        );
      }
    } catch (_) {}
  }

  Future<void> _showEditBand(
    BuildContext context,
    WidgetRef ref,
    Band band,
  ) async {
    final lookups = ref.read(appControllerProvider).lookups;
    if (lookups == null) return;
    final name = TextEditingController(text: band.name);
    final description = TextEditingController(text: band.description);
    var genre = lookups.genres.firstWhere(
      (item) => item.name == band.genre,
      orElse: () => lookups.genres.first,
    );
    final accepted = await showDialog<bool>(
      context: context,
      builder: (context) => StatefulBuilder(
        builder: (context, setDialogState) => AlertDialog(
          title: const Text('Uredi bend'),
          content: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              TextField(
                controller: name,
                decoration: const InputDecoration(labelText: 'Naziv'),
              ),
              const SizedBox(height: 10),
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
              const SizedBox(height: 10),
              TextField(
                controller: description,
                maxLines: 3,
                decoration: const InputDecoration(labelText: 'Opis'),
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
              child: const Text('Sačuvaj'),
            ),
          ],
        ),
      ),
    );
    if (accepted != true) return;
    try {
      await ref
          .read(appControllerProvider.notifier)
          .updateBand(band.id, name.text, genre.id, description.text);
    } catch (_) {}
  }

  Future<void> _showEditMember(
    BuildContext context,
    WidgetRef ref,
    Band band,
    BandMember member,
  ) async {
    final instruments =
        ref.read(appControllerProvider).lookups?.instruments ?? [];
    LookupItem? instrument;
    for (final item in instruments) {
      if (item.name == member.instrument) instrument = item;
    }
    final role = TextEditingController(text: member.role);
    final accepted = await showDialog<bool>(
      context: context,
      builder: (context) => StatefulBuilder(
        builder: (context, setDialogState) => AlertDialog(
          title: Text(member.fullName),
          content: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              DropdownButtonFormField<LookupItem?>(
                initialValue: instrument,
                decoration: const InputDecoration(labelText: 'Instrument'),
                items: [
                  const DropdownMenuItem(
                    value: null,
                    child: Text('Nije navedeno'),
                  ),
                  ...instruments.map(
                    (item) =>
                        DropdownMenuItem(value: item, child: Text(item.name)),
                  ),
                ],
                onChanged: (value) => setDialogState(() => instrument = value),
              ),
              const SizedBox(height: 10),
              TextField(
                controller: role,
                decoration: const InputDecoration(labelText: 'Uloga u bendu'),
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
              child: const Text('Sačuvaj'),
            ),
          ],
        ),
      ),
    );
    if (accepted != true) return;
    try {
      await ref
          .read(appControllerProvider.notifier)
          .updateBandMember(band.id, member.userId, instrument?.id, role.text);
    } catch (_) {}
  }

  Future<void> _removeMember(
    BuildContext context,
    WidgetRef ref,
    Band band,
    BandMember member,
  ) async {
    if (member.isFounder) return;
    final accepted = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Ukloniti člana?'),
        content: Text('${member.fullName} više neće imati pristup bendu.'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: const Text('Ne'),
          ),
          FilledButton(
            onPressed: () => Navigator.pop(context, true),
            child: const Text('Ukloni'),
          ),
        ],
      ),
    );
    if (accepted == true) {
      try {
        await ref
            .read(appControllerProvider.notifier)
            .removeBandMember(band.id, member.userId);
      } catch (_) {}
    }
  }

  Future<void> _leaveBand(
    BuildContext context,
    WidgetRef ref,
    Band band,
    int userId,
  ) async {
    final accepted = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: Text('Napustiti ${band.name}?'),
        content: const Text('Bend više neće biti prikazan u tvom profilu.'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: const Text('Ne'),
          ),
          FilledButton(
            onPressed: () => Navigator.pop(context, true),
            child: const Text('Napusti'),
          ),
        ],
      ),
    );
    if (accepted == true) {
      try {
        await ref
            .read(appControllerProvider.notifier)
            .removeBandMember(band.id, userId);
      } catch (_) {}
    }
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
  const _BandCard({
    required this.band,
    this.onInvite,
    this.onEdit,
    this.onManageMember,
    this.onRemoveMember,
    this.onLeave,
  });
  final Band band;
  final VoidCallback? onInvite;
  final VoidCallback? onEdit;
  final ValueChanged<BandMember>? onManageMember;
  final ValueChanged<BandMember>? onRemoveMember;
  final VoidCallback? onLeave;

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
        ...band.members.map(
          (member) => ListTile(
            contentPadding: EdgeInsets.zero,
            leading: CircleAvatar(
              backgroundColor: member.isFounder
                  ? accent
                  : const Color(0xFF2BA875),
              foregroundColor: Colors.white,
              child: Text(
                initials(member.fullName),
                style: const TextStyle(fontWeight: FontWeight.w800),
              ),
            ),
            title: Text(member.fullName),
            subtitle: Text(
              [member.instrument, member.role]
                  .whereType<String>()
                  .where((value) => value.isNotEmpty)
                  .join(' · '),
            ),
            trailing: onManageMember == null || member.isFounder
                ? null
                : PopupMenuButton<String>(
                    onSelected: (value) {
                      if (value == 'edit') onManageMember!(member);
                      if (value == 'remove') onRemoveMember!(member);
                    },
                    itemBuilder: (_) => const [
                      PopupMenuItem(value: 'edit', child: Text('Uredi člana')),
                      PopupMenuItem(
                        value: 'remove',
                        child: Text('Ukloni člana'),
                      ),
                    ],
                  ),
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
        if (onEdit != null) ...[
          const SizedBox(height: 8),
          SizedBox(
            width: double.infinity,
            child: OutlinedButton.icon(
              onPressed: onEdit,
              icon: const Icon(Icons.edit_outlined),
              label: const Text('Uredi bend'),
            ),
          ),
        ],
        if (onLeave != null) ...[
          const SizedBox(height: 8),
          SizedBox(
            width: double.infinity,
            child: TextButton.icon(
              onPressed: onLeave,
              icon: const Icon(Icons.logout_rounded),
              label: const Text('Napusti bend'),
            ),
          ),
        ],
      ],
    ),
  );
}
