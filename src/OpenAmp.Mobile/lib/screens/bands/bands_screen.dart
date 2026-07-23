import 'dart:io';

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:image_picker/image_picker.dart';
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
                    '${state.bands.length} ${state.bands.length == 1 ? 'bend' : 'bendova'}',
                    style: Theme.of(context).textTheme.titleMedium,
                  ),
                ),
                IconButton.filled(
                  tooltip: 'Novi bend',
                  onPressed: state.busy
                      ? null
                      : () => _showCreateBand(context, ref),
                  style: IconButton.styleFrom(
                    backgroundColor: AppColors.signal,
                    foregroundColor: AppColors.primaryDark,
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
    String? selectedPhotoPath;
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
              StatefulBuilder(
                builder: (context, setPhotoState) => Column(
                  children: [
                    Container(
                      width: double.infinity,
                      height: 120,
                      clipBehavior: Clip.antiAlias,
                      decoration: BoxDecoration(
                        color: AppColors.ink,
                        borderRadius: BorderRadius.circular(AppRadii.medium),
                      ),
                      child: selectedPhotoPath != null
                          ? Image.file(
                              File(selectedPhotoPath!),
                              fit: BoxFit.cover,
                            )
                          : band.imageUrl != null
                          ? Image.network(band.imageUrl!, fit: BoxFit.cover)
                          : const Icon(
                              Icons.photo_outlined,
                              color: Colors.white54,
                              size: 36,
                            ),
                    ),
                    TextButton.icon(
                      onPressed: () async {
                        final photo = await ImagePicker().pickImage(
                          source: ImageSource.gallery,
                          maxWidth: 1800,
                          maxHeight: 1200,
                          imageQuality: 88,
                        );
                        if (photo != null) {
                          setPhotoState(() => selectedPhotoPath = photo.path);
                        }
                      },
                      icon: const Icon(Icons.photo_library_outlined),
                      label: const Text('Odaberi naslovnu fotografiju'),
                    ),
                  ],
                ),
              ),
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
      if (selectedPhotoPath != null) {
        await ref
            .read(appControllerProvider.notifier)
            .uploadBandPhoto(band.id, selectedPhotoPath!);
      }
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
    final username = TextEditingController();
    final accepted = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: Text('Pozovi u ' + band.name),
        content: TextField(
          controller: username,
          autofocus: true,
          autocorrect: false,
          decoration: const InputDecoration(
            labelText: 'Username muzičara',
            prefixIcon: Icon(Icons.alternate_email),
            helperText: 'Pozvati možeš registrovanog OpenAmp korisnika.',
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
    final normalized = username.text.trim().toLowerCase();
    final valid = RegExp(
      r'^[a-z0-9](?:[a-z0-9._]{1,28}[a-z0-9])?$',
    ).hasMatch(normalized);
    if (accepted != true || !valid) return;
    try {
      await ref
          .read(appControllerProvider.notifier)
          .inviteMember(band.id, normalized);
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

  @override
  Widget build(BuildContext context) => Card(
    clipBehavior: Clip.antiAlias,
    child: Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        SizedBox(
          height: 156,
          child: Stack(
            fit: StackFit.expand,
            children: [
              if (band.imageUrl != null)
                Image.network(
                  band.imageUrl!,
                  fit: BoxFit.cover,
                  errorBuilder: (_, _, _) => const _BandCoverFallback(),
                )
              else
                const _BandCoverFallback(),
              const DecoratedBox(
                decoration: BoxDecoration(
                  gradient: LinearGradient(
                    begin: Alignment.topCenter,
                    end: Alignment.bottomCenter,
                    colors: [Colors.transparent, Color(0xE617131F)],
                    stops: [.25, 1],
                  ),
                ),
              ),
              Positioned(
                left: 17,
                right: 17,
                bottom: 15,
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      band.genre.toUpperCase(),
                      style: const TextStyle(
                        color: AppColors.signal,
                        fontSize: 9,
                        fontWeight: FontWeight.w900,
                        letterSpacing: 1.1,
                      ),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      band.name,
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                      style: const TextStyle(
                        color: Colors.white,
                        fontSize: 25,
                        height: 1,
                        fontWeight: FontWeight.w900,
                        letterSpacing: -.7,
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),
        Padding(
          padding: const EdgeInsets.fromLTRB(17, 15, 17, 17),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              if (band.description?.trim().isNotEmpty == true) ...[
                Text(
                  band.description!,
                  maxLines: 3,
                  overflow: TextOverflow.ellipsis,
                  style: Theme.of(context).textTheme.bodyMedium,
                ),
                const SizedBox(height: 14),
              ],
              Row(
                children: [
                  _BandMetric(
                    icon: Icons.group_outlined,
                    value: '${band.members.length}',
                    label: 'članova',
                  ),
                  const SizedBox(width: 18),
                  _BandMetric(
                    icon: Icons.calendar_month_outlined,
                    value: '${band.reservationCount}',
                    label: 'rezervacija',
                  ),
                ],
              ),
              const SizedBox(height: 18),
              const SectionEyebrow('Postava'),
              const SizedBox(height: 7),
              ...band.members.map(
                (member) => Container(
                  padding: const EdgeInsets.symmetric(vertical: 10),
                  decoration: const BoxDecoration(
                    border: Border(bottom: BorderSide(color: AppColors.line)),
                  ),
                  child: Row(
                    children: [
                      CircleAvatar(
                        radius: 19,
                        backgroundColor: member.isFounder
                            ? AppColors.signal
                            : AppColors.paperMuted,
                        foregroundColor: member.isFounder
                            ? AppColors.primaryDark
                            : AppColors.ink,
                        child: Text(
                          initials(member.fullName),
                          style: const TextStyle(fontWeight: FontWeight.w900),
                        ),
                      ),
                      const SizedBox(width: 11),
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              member.fullName,
                              style: const TextStyle(
                                fontWeight: FontWeight.w800,
                              ),
                            ),
                            Text(
                              '@${member.username}${_memberDetails(member)}',
                              style: Theme.of(context).textTheme.bodySmall,
                            ),
                          ],
                        ),
                      ),
                      if (member.isFounder)
                        const Icon(
                          Icons.star_rounded,
                          size: 18,
                          color: AppColors.signal,
                        )
                      else if (onManageMember != null)
                        PopupMenuButton<String>(
                          tooltip: 'Opcije člana',
                          onSelected: (value) {
                            if (value == 'edit') onManageMember!(member);
                            if (value == 'remove') onRemoveMember!(member);
                          },
                          itemBuilder: (_) => const [
                            PopupMenuItem(
                              value: 'edit',
                              child: Text('Uredi člana'),
                            ),
                            PopupMenuItem(
                              value: 'remove',
                              child: Text('Ukloni člana'),
                            ),
                          ],
                        ),
                    ],
                  ),
                ),
              ),
              if (band.invitations.isNotEmpty) ...[
                const SizedBox(height: 18),
                const SectionEyebrow('Pozivnice'),
                const SizedBox(height: 6),
                ...band.invitations
                    .take(3)
                    .map(
                      (invite) => Padding(
                        padding: const EdgeInsets.symmetric(vertical: 5),
                        child: Row(
                          children: [
                            const Icon(
                              Icons.alternate_email_rounded,
                              size: 17,
                              color: AppColors.textMuted,
                            ),
                            const SizedBox(width: 8),
                            Expanded(
                              child: Text(
                                invite.username,
                                style: const TextStyle(
                                  fontWeight: FontWeight.w700,
                                ),
                              ),
                            ),
                            Text(
                              invite.status,
                              style: Theme.of(context).textTheme.bodySmall,
                            ),
                          ],
                        ),
                      ),
                    ),
              ],
              const SizedBox(height: 17),
              if (onInvite != null || onEdit != null)
                Row(
                  children: [
                    if (onInvite != null)
                      Expanded(
                        child: FilledButton.icon(
                          onPressed: onInvite,
                          icon: const Icon(Icons.person_add_alt_1_rounded),
                          label: const Text('Pozovi'),
                        ),
                      ),
                    if (onInvite != null && onEdit != null)
                      const SizedBox(width: 9),
                    if (onEdit != null)
                      Expanded(
                        child: OutlinedButton.icon(
                          onPressed: onEdit,
                          icon: const Icon(Icons.edit_outlined),
                          label: const Text('Uredi'),
                        ),
                      ),
                  ],
                ),
              if (onLeave != null)
                SizedBox(
                  width: double.infinity,
                  child: TextButton.icon(
                    onPressed: onLeave,
                    icon: const Icon(Icons.logout_rounded),
                    label: const Text('Napusti bend'),
                  ),
                ),
            ],
          ),
        ),
      ],
    ),
  );

  static String _memberDetails(BandMember member) {
    final details = [
      member.instrument,
      member.role,
    ].whereType<String>().where((value) => value.trim().isNotEmpty).join(' · ');
    return details.isEmpty ? '' : '  ·  $details';
  }
}

class _BandMetric extends StatelessWidget {
  const _BandMetric({
    required this.icon,
    required this.value,
    required this.label,
  });

  final IconData icon;
  final String value;
  final String label;

  @override
  Widget build(BuildContext context) => Row(
    mainAxisSize: MainAxisSize.min,
    children: [
      Icon(icon, size: 18, color: AppColors.signal),
      const SizedBox(width: 6),
      Text(
        '$value $label',
        style: const TextStyle(fontWeight: FontWeight.w800, fontSize: 12),
      ),
    ],
  );
}

class _BandCoverFallback extends StatelessWidget {
  const _BandCoverFallback();

  @override
  Widget build(BuildContext context) => Container(
    color: AppColors.primaryDark,
    alignment: Alignment.topRight,
    padding: const EdgeInsets.all(16),
    child: const Icon(
      Icons.graphic_eq_rounded,
      color: Color(0x55FFFFFF),
      size: 58,
    ),
  );
}
