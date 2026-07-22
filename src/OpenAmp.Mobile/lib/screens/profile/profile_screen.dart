import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
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
                color: AppColors.ink,
                borderRadius: BorderRadius.circular(AppRadii.large),
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const Row(
                    children: [
                      SectionEyebrow('Profil', color: AppColors.signal),
                      Spacer(),
                      Text(
                        'AKTIVAN',
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
              const SectionEyebrow('Statistika'),
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
                        label: 'NAJČEŠĆI ŽANR',
                        value: profile.topGenre!,
                      ),
                    ),
                ],
              ),
            ],
            const SizedBox(height: 27),
            const SectionEyebrow('Postavke'),
            const SizedBox(height: 8),
            _ProfileAction(
              icon: Icons.person_outline,
              label: 'Lični profil',
              onTap: () => _editProfile(context, ref),
            ),
            _ProfileAction(
              icon: Icons.credit_card_outlined,
              label: 'Načini plaćanja',
              onTap: () => _paymentMethodsInfo(context),
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
              onTap: () => _changePassword(context, ref),
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

  Future<void> _editProfile(BuildContext context, WidgetRef ref) async {
    final state = ref.read(appControllerProvider);
    final profile = state.profile!;
    final firstName = TextEditingController(text: profile.firstName);
    final lastName = TextEditingController(text: profile.lastName);
    final phone = TextEditingController(text: profile.phone);
    final imageUrl = TextEditingController(text: profile.imageUrl);
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
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                TextField(
                  controller: firstName,
                  decoration: const InputDecoration(labelText: 'Ime'),
                ),
                const SizedBox(height: 10),
                TextField(
                  controller: lastName,
                  decoration: const InputDecoration(labelText: 'Prezime'),
                ),
                const SizedBox(height: 10),
                TextField(
                  controller: phone,
                  keyboardType: TextInputType.phone,
                  decoration: const InputDecoration(labelText: 'Telefon'),
                ),
                const SizedBox(height: 10),
                TextField(
                  controller: imageUrl,
                  keyboardType: TextInputType.url,
                  decoration: const InputDecoration(
                    labelText: 'URL profilne fotografije',
                  ),
                ),
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
                      selectedColor: AppColors.ink,
                      checkmarkColor: AppColors.signal,
                      labelStyle: TextStyle(
                        color: isSelected ? Colors.white : AppColors.ink,
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
    if (accepted != true ||
        firstName.text.trim().length < 2 ||
        lastName.text.trim().length < 2) {
      return;
    }
    try {
      await ref
          .read(appControllerProvider.notifier)
          .updateProfile(
            firstName: firstName.text.trim(),
            lastName: lastName.text.trim(),
            phone: phone.text.trim().isEmpty ? null : phone.text.trim(),
            imageUrl: imageUrl.text.trim().isEmpty
                ? null
                : imageUrl.text.trim(),
            instrumentIds: selected.toList(),
          );
    } catch (_) {}
  }

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
    final accepted = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Promijeni lozinku'),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            TextField(
              controller: current,
              obscureText: true,
              decoration: const InputDecoration(labelText: 'Trenutna lozinka'),
            ),
            const SizedBox(height: 10),
            TextField(
              controller: next,
              obscureText: true,
              decoration: const InputDecoration(labelText: 'Nova lozinka'),
            ),
            const SizedBox(height: 10),
            TextField(
              controller: confirm,
              obscureText: true,
              decoration: const InputDecoration(
                labelText: 'Ponovi novu lozinku',
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
            child: const Text('Promijeni'),
          ),
        ],
      ),
    );
    if (accepted != true) return;
    if (next.text != confirm.text || next.text.length < 10) {
      if (context.mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Nove lozinke se ne podudaraju ili su prekratke.'),
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

  Future<void> _paymentMethodsInfo(BuildContext context) => showDialog<void>(
    context: context,
    builder: (context) => AlertDialog(
      icon: const Icon(Icons.credit_card_rounded, size: 46),
      title: const Text('Sačuvane kartice'),
      content: const Text(
        'Kartice se sigurno čuvaju kod Stripea, ne u OpenAmp bazi. Tokom sljedećeg plaćanja možeš sačuvati novu ili ukloniti postojeću karticu direktno u Stripe PaymentSheetu.',
        textAlign: TextAlign.center,
      ),
      actions: [
        FilledButton(
          onPressed: () => Navigator.pop(context),
          child: const Text('U redu'),
        ),
      ],
    ),
  );
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
  const _ProfileAction({
    required this.icon,
    required this.label,
    required this.onTap,
  });

  final IconData icon;
  final String label;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) => InkWell(
    onTap: onTap,
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
