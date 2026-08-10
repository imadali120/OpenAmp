import 'dart:io';

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:image_picker/image_picker.dart';
import 'package:openamp_mobile/core/theme/app_theme.dart';
import 'package:openamp_mobile/models/models.dart';
import 'package:openamp_mobile/screens/profile/notifications_screen.dart';
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
                color: AppColors.paper,
                borderRadius: BorderRadius.circular(AppRadii.large),
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    children: [
                      Container(
                        width: 70,
                        height: 70,
                        clipBehavior: Clip.antiAlias,
                        alignment: Alignment.center,
                        decoration: BoxDecoration(
                          color: AppColors.signal,
                          borderRadius: BorderRadius.circular(18),
                        ),
                        child: profile.imageUrl == null
                            ? _ProfileInitials(name: profile.fullName)
                            : Image.network(
                                profile.imageUrl!,
                                width: 70,
                                height: 70,
                                fit: BoxFit.cover,
                                errorBuilder: (_, _, _) =>
                                    _ProfileInitials(name: profile.fullName),
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
                                color: AppColors.text,
                                fontSize: 23,
                                height: 1,
                                fontWeight: FontWeight.w900,
                                letterSpacing: -.6,
                              ),
                            ),
                            const SizedBox(height: 7),
                            Text(
                              '@${profile.username}',
                              maxLines: 1,
                              overflow: TextOverflow.ellipsis,
                              style: const TextStyle(
                                color: AppColors.signal,
                                fontWeight: FontWeight.w800,
                              ),
                            ),
                            const SizedBox(height: 3),
                            Text(
                              profile.email,
                              maxLines: 1,
                              overflow: TextOverflow.ellipsis,
                              style: const TextStyle(
                                color: AppColors.textMuted,
                                fontSize: 12,
                              ),
                            ),
                          ],
                        ),
                      ),
                    ],
                  ),
                  if (profile.instruments.isNotEmpty) ...[
                    const SizedBox(height: 15),
                    Text(
                      profile.instruments.join('  ·  '),
                      style: const TextStyle(
                        color: AppColors.textMuted,
                        fontSize: 12,
                        fontWeight: FontWeight.w700,
                      ),
                    ),
                  ],
                ],
              ),
            ),
            const SizedBox(height: 16),
            Container(
              decoration: BoxDecoration(
                color: AppColors.paper,
                borderRadius: BorderRadius.circular(AppRadii.medium),
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
                  ],
                ),
              ),
            ),
            if (profile.favoriteHall != null || profile.topGenre != null) ...[
              const SizedBox(height: 14),
              Container(
                padding: const EdgeInsets.symmetric(
                  horizontal: 16,
                  vertical: 14,
                ),
                decoration: BoxDecoration(
                  color: AppColors.paper,
                  borderRadius: BorderRadius.circular(AppRadii.medium),
                ),
                child: Row(
                  children: [
                    if (profile.favoriteHall != null)
                      Expanded(
                        child: _ProfileInsight(
                          label: 'Omiljena sala',
                          value: profile.favoriteHall!,
                        ),
                      ),
                    if (profile.favoriteHall != null &&
                        profile.topGenre != null)
                      const SizedBox(
                        height: 38,
                        child: VerticalDivider(color: AppColors.line),
                      ),
                    if (profile.topGenre != null)
                      Expanded(
                        child: _ProfileInsight(
                          label: 'Najčešći žanr',
                          value: profile.topGenre!,
                        ),
                      ),
                  ],
                ),
              ),
            ],
            const SizedBox(height: 18),
            ClipRRect(
              borderRadius: BorderRadius.circular(AppRadii.medium),
              child: ColoredBox(
                color: AppColors.paper,
                child: Column(
                  children: [
                    _ProfileAction(
                      icon: Icons.person_outline,
                      label: 'Uredi profil',
                      onTap: () => _editProfile(context, ref),
                    ),
                    _ProfileAction(
                      icon: Icons.notifications_none_rounded,
                      label: 'Notifikacije',
                      onTap: () => Navigator.of(context).push(
                        MaterialPageRoute<void>(
                          builder: (_) => const NotificationsScreen(),
                        ),
                      ),
                    ),
                    _ProfileAction(
                      icon: Icons.tune_rounded,
                      label: 'Jezik i privatnost',
                      onTap: () => _editSettings(context, ref),
                    ),
                    _ProfileAction(
                      icon: Icons.lock_outline_rounded,
                      label: 'Sigurnost i lozinka',
                      showDivider: false,
                      onTap: () => _changePassword(context, ref),
                    ),
                  ],
                ),
              ),
            ),
            const SizedBox(height: 12),
            TextButton.icon(
              onPressed: ref.read(appControllerProvider.notifier).logout,
              icon: const Icon(Icons.logout_rounded),
              label: const Text('Odjavi se'),
            ),
          ],
        ),
      ),
    );
  }

  Future<void> _editProfile(BuildContext context, WidgetRef ref) async {
    final state = ref.read(appControllerProvider);
    final profile = state.profile!;
    final username = TextEditingController(text: profile.username);
    final firstName = TextEditingController(text: profile.firstName);
    final lastName = TextEditingController(text: profile.lastName);
    final phone = TextEditingController(text: profile.phone);
    final formKey = GlobalKey<FormState>();
    String? selectedPhotoPath;
    final selected =
        state.lookups?.instruments
            .where((item) => profile.instruments.contains(item.name))
            .map((item) => item.id)
            .toSet() ??
        <int>{};
    final accepted = await showDialog<bool>(
      context: context,
      builder: (context) => StatefulBuilder(
        builder: (context, setDialogState) => AlertDialog(
          title: const Text('Lični podaci'),
          content: SingleChildScrollView(
            child: Form(
              key: formKey,
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  StatefulBuilder(
                    builder: (context, setPhotoState) => Column(
                      children: [
                        Container(
                          width: 86,
                          height: 86,
                          clipBehavior: Clip.antiAlias,
                          decoration: BoxDecoration(
                            color: AppColors.signal,
                            borderRadius: BorderRadius.circular(22),
                          ),
                          child: selectedPhotoPath != null
                              ? Image.file(
                                  File(selectedPhotoPath!),
                                  fit: BoxFit.cover,
                                )
                              : profile.imageUrl != null
                              ? Image.network(
                                  profile.imageUrl!,
                                  fit: BoxFit.cover,
                                )
                              : Center(
                                  child: _ProfileInitials(
                                    name: profile.fullName,
                                  ),
                                ),
                        ),
                        TextButton.icon(
                          onPressed: () async {
                            final photo = await ImagePicker().pickImage(
                              source: ImageSource.gallery,
                              maxWidth: 1600,
                              maxHeight: 1600,
                              imageQuality: 88,
                            );
                            if (photo != null) {
                              setPhotoState(
                                () => selectedPhotoPath = photo.path,
                              );
                            }
                          },
                          icon: const Icon(Icons.photo_library_outlined),
                          label: const Text('Odaberi fotografiju'),
                        ),
                      ],
                    ),
                  ),
                  const SizedBox(height: 6),
                  TextFormField(
                    controller: username,
                    autocorrect: false,
                    validator: (value) => _validUsername(value ?? '')
                        ? null
                        : '3–30 znakova: mala slova, brojevi, tačka ili _.',
                    decoration: const InputDecoration(
                      labelText: 'Username',
                      prefixText: '@',
                    ),
                  ),
                  const SizedBox(height: 10),
                  TextFormField(
                    controller: firstName,
                    validator: (value) => (value?.trim().length ?? 0) >= 2
                        ? null
                        : 'Ime mora imati najmanje 2 znaka.',
                    decoration: const InputDecoration(labelText: 'Ime'),
                  ),
                  const SizedBox(height: 10),
                  TextFormField(
                    controller: lastName,
                    validator: (value) => (value?.trim().length ?? 0) >= 2
                        ? null
                        : 'Prezime mora imati najmanje 2 znaka.',
                    decoration: const InputDecoration(labelText: 'Prezime'),
                  ),
                  const SizedBox(height: 10),
                  TextFormField(
                    controller: phone,
                    keyboardType: TextInputType.phone,
                    validator: (value) {
                      final text = value?.trim() ?? '';
                      if (text.isEmpty) return null;
                      return RegExp(r'^\+?[0-9][0-9 ()-]{6,18}$').hasMatch(text)
                          ? null
                          : 'Unesite ispravan broj telefona.';
                    },
                    decoration: const InputDecoration(labelText: 'Telefon'),
                  ),
                  const SizedBox(height: 10),
                  const SizedBox(height: 14),
                  Align(
                    alignment: Alignment.centerLeft,
                    child: Text(
                      'Instrumenti',
                      style: Theme.of(context).textTheme.titleMedium,
                    ),
                  ),
                  const SizedBox(height: 7),
                  Wrap(
                    spacing: 7,
                    runSpacing: 7,
                    children: (state.lookups?.instruments ?? []).map((item) {
                      final isSelected = selected.contains(item.id);
                      return FilterChip(
                        label: Text(item.name),
                        selected: isSelected,
                        selectedColor: AppColors.primary,
                        checkmarkColor: AppColors.ink,
                        labelStyle: TextStyle(
                          color: isSelected ? AppColors.ink : AppColors.text,
                          fontWeight: FontWeight.w700,
                        ),
                        onSelected: (value) => setDialogState(
                          () => value
                              ? selected.add(item.id)
                              : selected.remove(item.id),
                        ),
                      );
                    }).toList(),
                  ),
                ],
              ),
            ),
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.pop(context, false),
              child: const Text('Odustani'),
            ),
            FilledButton(
              onPressed: () {
                if (formKey.currentState!.validate()) {
                  Navigator.pop(context, true);
                }
              },
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
          .updateProfile(
            username: username.text.trim().toLowerCase(),
            firstName: firstName.text.trim(),
            lastName: lastName.text.trim(),
            phone: phone.text.trim().isEmpty ? null : phone.text.trim(),
            imageUrl: null,
            instrumentIds: selected.toList(),
          );
      if (selectedPhotoPath != null) {
        await ref
            .read(appControllerProvider.notifier)
            .uploadProfilePhoto(selectedPhotoPath!);
      }
    } catch (_) {}
  }

  bool _validUsername(String value) => RegExp(
    r'^[a-z0-9](?:[a-z0-9._]{1,28}[a-z0-9])?$',
  ).hasMatch(value.trim().toLowerCase());

  Future<void> _editSettings(BuildContext context, WidgetRef ref) async {
    final current = ref.read(appControllerProvider).settings;
    if (current == null) return;
    var push = current.pushNotifications;
    var email = current.emailNotifications;
    var publicProfile = current.publicProfile;
    var language = current.language;
    final accepted = await showDialog<bool>(
      context: context,
      builder: (context) => StatefulBuilder(
        builder: (context, setDialogState) => AlertDialog(
          title: const Text('Postavke profila'),
          content: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              SwitchListTile(
                value: push,
                onChanged: (value) => setDialogState(() => push = value),
                title: const Text('Push notifikacije'),
                subtitle: const Text('Podsjetnici za probe i pozivnice'),
              ),
              SwitchListTile(
                value: email,
                onChanged: (value) => setDialogState(() => email = value),
                title: const Text('Email notifikacije'),
              ),
              SwitchListTile(
                value: publicProfile,
                onChanged: (value) =>
                    setDialogState(() => publicProfile = value),
                title: const Text('Javan profil'),
              ),
              DropdownButtonFormField<String>(
                initialValue: language,
                decoration: const InputDecoration(labelText: 'Jezik'),
                items: const [
                  DropdownMenuItem(value: 'bs', child: Text('Bosanski')),
                  DropdownMenuItem(value: 'en', child: Text('English')),
                ],
                onChanged: (value) {
                  if (value != null) setDialogState(() => language = value);
                },
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
    if (accepted == true) {
      try {
        await ref
            .read(appControllerProvider.notifier)
            .updateSettings(
              UserSettings(
                pushNotifications: push,
                emailNotifications: email,
                language: language,
                publicProfile: publicProfile,
              ),
            );
      } catch (_) {}
    }
  }

  Future<void> _changePassword(BuildContext context, WidgetRef ref) async {
    final current = TextEditingController();
    final next = TextEditingController();
    final confirm = TextEditingController();
    final passwordFormKey = GlobalKey<FormState>();
    final accepted = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Promijeni lozinku'),
        content: Form(
          key: passwordFormKey,
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              TextFormField(
                controller: current,
                obscureText: true,
                validator: (value) => (value?.isEmpty ?? true)
                    ? 'Unesite trenutnu lozinku.'
                    : null,
                decoration: const InputDecoration(
                  labelText: 'Trenutna lozinka',
                ),
              ),
              const SizedBox(height: 10),
              TextFormField(
                controller: next,
                obscureText: true,
                validator: (value) {
                  final password = value ?? '';
                  final valid =
                      password.length >= 10 &&
                      password.length <= 128 &&
                      password.contains(RegExp('[A-Z]')) &&
                      password.contains(RegExp('[a-z]')) &&
                      password.contains(RegExp('[0-9]')) &&
                      password.contains(RegExp(r'[^A-Za-z0-9]'));
                  return valid
                      ? null
                      : 'Potrebno je veliko i malo slovo, broj i poseban znak.';
                },
                decoration: const InputDecoration(
                  labelText: 'Nova lozinka',
                  helperText: '10+ znakova, A–Z, a–z, broj i poseban znak',
                ),
              ),
              const SizedBox(height: 10),
              TextFormField(
                controller: confirm,
                obscureText: true,
                validator: (value) =>
                    value == next.text ? null : 'Lozinke se ne podudaraju.',
                decoration: const InputDecoration(
                  labelText: 'Ponovi novu lozinku',
                ),
              ),
            ],
          ),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: const Text('Odustani'),
          ),
          FilledButton(
            onPressed: () {
              if (passwordFormKey.currentState!.validate()) {
                Navigator.pop(context, true);
              }
            },
            child: const Text('Promijeni'),
          ),
        ],
      ),
    );
    if (accepted != true) return;
    final validPassword =
        next.text.length >= 10 &&
        next.text.length <= 128 &&
        next.text.contains(RegExp('[A-Z]')) &&
        next.text.contains(RegExp('[a-z]')) &&
        next.text.contains(RegExp('[0-9]')) &&
        next.text.contains(RegExp(r'[^A-Za-z0-9]'));
    if (next.text != confirm.text || !validPassword) {
      if (context.mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text(
              'Lozinke se moraju podudarati i sadržavati veliko i malo slovo, broj i poseban znak.',
            ),
          ),
        );
      }
      return;
    }
    try {
      await ref
          .read(appControllerProvider.notifier)
          .changePassword(current.text, next.text);
      if (context.mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Lozinka je promijenjena.')),
        );
      }
    } catch (_) {}
  }
}

class _ProfileInitials extends StatelessWidget {
  const _ProfileInitials({required this.name});
  final String name;

  @override
  Widget build(BuildContext context) => Text(
    initials(name),
    style: const TextStyle(
      color: AppColors.ink,
      fontSize: 24,
      fontWeight: FontWeight.w900,
    ),
  );
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
              color: AppColors.text,
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

class _ProfileInsight extends StatelessWidget {
  const _ProfileInsight({required this.label, required this.value});

  final String label;
  final String value;

  @override
  Widget build(BuildContext context) => Column(
    crossAxisAlignment: CrossAxisAlignment.start,
    children: [
      Text(
        label,
        style: const TextStyle(color: AppColors.textMuted, fontSize: 11),
      ),
      const SizedBox(height: 4),
      Text(
        value,
        maxLines: 1,
        overflow: TextOverflow.ellipsis,
        style: const TextStyle(fontWeight: FontWeight.w800),
      ),
    ],
  );
}

class _ProfileAction extends StatelessWidget {
  const _ProfileAction({
    required this.icon,
    required this.label,
    required this.onTap,
    this.showDivider = true,
  });

  final IconData icon;
  final String label;
  final VoidCallback onTap;
  final bool showDivider;

  @override
  Widget build(BuildContext context) => InkWell(
    onTap: onTap,
    child: Container(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 15),
      decoration: BoxDecoration(
        border: showDivider
            ? const Border(bottom: BorderSide(color: AppColors.line))
            : null,
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
          const Icon(
            Icons.arrow_forward_rounded,
            size: 18,
            color: AppColors.textMuted,
          ),
        ],
      ),
    ),
  );
}
