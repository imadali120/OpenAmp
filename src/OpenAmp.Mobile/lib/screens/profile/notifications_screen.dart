import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/intl.dart';
import 'package:openamp_mobile/core/notifications/local_notification_service.dart';
import 'package:openamp_mobile/models/models.dart';
import 'package:openamp_mobile/state/app_state.dart';

class NotificationsScreen extends ConsumerWidget {
  const NotificationsScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final state = ref.watch(appControllerProvider);
    final settings = state.settings;
    final upcoming =
        state.reservations
            .where(
              (item) =>
                  item.startsAt.isAfter(DateTime.now().toUtc()) &&
                  !item.status.toLowerCase().contains('otkaz'),
            )
            .toList()
          ..sort((a, b) => a.startsAt.compareTo(b.startsAt));
    final invitations = state.receivedInvitations
        .where((item) => item.pending)
        .toList();

    return Scaffold(
      appBar: AppBar(title: const Text('Notifikacije')),
      body: settings == null
          ? const Center(child: CircularProgressIndicator())
          : RefreshIndicator(
              onRefresh: ref
                  .read(appControllerProvider.notifier)
                  .refreshPrivateData,
              child: ListView(
                padding: const EdgeInsets.fromLTRB(18, 8, 18, 28),
                children: [
                  Card(
                    child: Column(
                      children: [
                        SwitchListTile(
                          value: settings.pushNotifications,
                          title: const Text('Podsjetnici na uređaju'),
                          subtitle: const Text(
                            'Obavijest 2 sata prije rezervisanog termina',
                          ),
                          onChanged: state.busy
                              ? null
                              : (value) => _setDeviceNotifications(
                                  context,
                                  ref,
                                  settings,
                                  value,
                                ),
                        ),
                        const Divider(height: 1),
                        SwitchListTile(
                          value: settings.emailNotifications,
                          title: const Text('Email obavijesti'),
                          subtitle: const Text(
                            'Status rezervacije i pozivnice za bend',
                          ),
                          onChanged: state.busy
                              ? null
                              : (value) => _save(
                                  context,
                                  ref,
                                  UserSettings(
                                    pushNotifications:
                                        settings.pushNotifications,
                                    emailNotifications: value,
                                    language: settings.language,
                                    publicProfile: settings.publicProfile,
                                  ),
                                ),
                        ),
                      ],
                    ),
                  ),
                  if (settings.pushNotifications) ...[
                    const SizedBox(height: 10),
                    OutlinedButton.icon(
                      onPressed: () => _sendTest(context),
                      icon: const Icon(Icons.notifications_active_outlined),
                      label: const Text('Pošalji testnu notifikaciju'),
                    ),
                  ],
                  const SizedBox(height: 24),
                  Text(
                    'Aktuelno',
                    style: Theme.of(context).textTheme.titleLarge,
                  ),
                  const SizedBox(height: 10),
                  if (invitations.isEmpty && upcoming.isEmpty)
                    const Card(
                      child: Padding(
                        padding: EdgeInsets.all(18),
                        child: Text('Nema novih obavijesti.'),
                      ),
                    ),
                  ...invitations.map(
                    (item) => Padding(
                      padding: const EdgeInsets.only(bottom: 8),
                      child: Card(
                        child: ListTile(
                          leading: const Icon(Icons.group_add_outlined),
                          title: Text('Pozivnica za ${item.band}'),
                          subtitle: Text('Pozvao/la: ${item.invitedBy}'),
                        ),
                      ),
                    ),
                  ),
                  ...upcoming
                      .take(5)
                      .map(
                        (item) => Padding(
                          padding: const EdgeInsets.only(bottom: 8),
                          child: Card(
                            child: ListTile(
                              leading: const Icon(Icons.event_outlined),
                              title: Text(item.hall),
                              subtitle: Text(
                                '${DateFormat('dd.MM.yyyy. HH:mm', 'bs').format(item.startsAt.toLocal())} · ${item.band}',
                              ),
                            ),
                          ),
                        ),
                      ),
                ],
              ),
            ),
    );
  }

  Future<void> _setDeviceNotifications(
    BuildContext context,
    WidgetRef ref,
    UserSettings current,
    bool enabled,
  ) async {
    if (enabled) {
      final granted = await LocalNotificationService.instance
          .requestPermission();
      if (granted != true) {
        if (context.mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Text(
                granted == null
                    ? 'Notifikacije nisu dostupne na ovom uređaju.'
                    : 'Dozvola za notifikacije nije odobrena.',
              ),
            ),
          );
        }
        return;
      }
    }
    if (!context.mounted) return;
    final next = UserSettings(
      pushNotifications: enabled,
      emailNotifications: current.emailNotifications,
      language: current.language,
      publicProfile: current.publicProfile,
    );
    final saved = await _save(context, ref, next);
    if (!saved) return;
    await LocalNotificationService.instance.syncReservations(
      enabled: enabled,
      reservations: ref.read(appControllerProvider).reservations,
    );
  }

  Future<bool> _save(
    BuildContext context,
    WidgetRef ref,
    UserSettings settings,
  ) async {
    try {
      await ref.read(appControllerProvider.notifier).updateSettings(settings);
      return true;
    } catch (error) {
      if (context.mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Postavke nisu sačuvane: $error')),
        );
      }
      return false;
    }
  }

  Future<void> _sendTest(BuildContext context) async {
    final granted = await LocalNotificationService.instance.requestPermission();
    if (!context.mounted) return;
    if (granted != true) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(
            granted == null
                ? 'Notifikacije nisu dostupne na ovom uređaju.'
                : 'Dozvola za notifikacije nije odobrena.',
          ),
        ),
      );
      return;
    }
    final sent = await LocalNotificationService.instance.showTest();
    if (!context.mounted) return;
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text(
          sent
              ? 'Testna notifikacija je poslana.'
              : 'Testnu notifikaciju nije moguće poslati.',
        ),
      ),
    );
  }
}
