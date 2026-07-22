import 'package:flutter/foundation.dart';
import 'package:flutter_local_notifications/flutter_local_notifications.dart';
import 'package:intl/intl.dart';
import 'package:openamp_mobile/models/models.dart';
import 'package:timezone/data/latest.dart' as tz_data;
import 'package:timezone/timezone.dart' as tz;

class LocalNotificationService {
  LocalNotificationService._();

  static final instance = LocalNotificationService._();

  final FlutterLocalNotificationsPlugin _plugin =
      FlutterLocalNotificationsPlugin();
  bool _initialized = false;

  static const _details = NotificationDetails(
    android: AndroidNotificationDetails(
      'openamp_rehearsals',
      'Podsjetnici za probe',
      channelDescription: 'Podsjetnici za predstojeće OpenAmp rezervacije',
      importance: Importance.high,
      priority: Priority.high,
    ),
    iOS: DarwinNotificationDetails(),
  );

  Future<void> initialize() async {
    if (_initialized || kIsWeb) return;
    if (defaultTargetPlatform != TargetPlatform.android &&
        defaultTargetPlatform != TargetPlatform.iOS) {
      return;
    }
    tz_data.initializeTimeZones();
    try {
      await _plugin.initialize(
        settings: const InitializationSettings(
          android: AndroidInitializationSettings('app_icon'),
          iOS: DarwinInitializationSettings(
            requestAlertPermission: false,
            requestBadgePermission: false,
            requestSoundPermission: false,
          ),
        ),
      );
      _initialized = true;
    } catch (error) {
      debugPrint('Notification initialization failed: $error');
    }
  }

  Future<bool> requestPermission() async {
    await initialize();
    if (defaultTargetPlatform == TargetPlatform.android) {
      return await _plugin
              .resolvePlatformSpecificImplementation<
                AndroidFlutterLocalNotificationsPlugin
              >()
              ?.requestNotificationsPermission() ??
          false;
    }
    if (defaultTargetPlatform == TargetPlatform.iOS) {
      return await _plugin
              .resolvePlatformSpecificImplementation<
                IOSFlutterLocalNotificationsPlugin
              >()
              ?.requestPermissions(alert: true, badge: true, sound: true) ??
          false;
    }
    return false;
  }

  Future<void> syncReservations({
    required bool enabled,
    required List<Reservation> reservations,
  }) async {
    await initialize();
    if (!_initialized) return;
    await _plugin.cancelAllPendingNotifications();
    if (!enabled) return;

    final now = DateTime.now().toUtc();
    for (final reservation in reservations) {
      final scheduledAt = reservation.startsAt.subtract(
        const Duration(hours: 2),
      );
      if (scheduledAt.isBefore(now) ||
          reservation.status.toLowerCase().contains('otkaz')) {
        continue;
      }
      await _plugin.zonedSchedule(
        id: 100000 + reservation.id,
        title: 'Proba za 2 sata',
        body:
            '${reservation.hall} · ${DateFormat('dd.MM. HH:mm', 'bs').format(reservation.startsAt.toLocal())}',
        scheduledDate: tz.TZDateTime.from(scheduledAt, tz.UTC),
        notificationDetails: _details,
        androidScheduleMode: AndroidScheduleMode.inexactAllowWhileIdle,
        payload: 'reservation:${reservation.id}',
      );
    }
  }

  Future<void> showTest() async {
    await initialize();
    if (!_initialized) return;
    await _plugin.show(
      id: 900001,
      title: 'OpenAmp notifikacije su uključene',
      body: 'Podsjetnik za probu će stići 2 sata prije termina.',
      notificationDetails: _details,
    );
  }
}
