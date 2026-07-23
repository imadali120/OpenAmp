import 'dart:async';

import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:openamp_mobile/core/network/api_client.dart';
import 'package:openamp_mobile/core/notifications/local_notification_service.dart';
import 'package:openamp_mobile/core/storage/session_store.dart';
import 'package:openamp_mobile/models/models.dart';
import 'package:openamp_mobile/repositories/openamp_repository.dart';

final sessionStoreProvider = Provider<SessionStore>((ref) => SessionStore());
final apiClientProvider = Provider<ApiClient>(
  (ref) => ApiClient(ref.read(sessionStoreProvider)),
);
final repositoryProvider = Provider<OpenAmpRepository>(
  (ref) => OpenAmpRepository(ref.read(apiClientProvider)),
);
final appControllerProvider = NotifierProvider<AppController, AppState>(
  AppController.new,
);

class AppState {
  const AppState({
    this.initialized = false,
    this.busy = false,
    this.session,
    this.lookups,
    this.halls = const [],
    this.bands = const [],
    this.receivedInvitations = const [],
    this.reservations = const [],
    this.favoriteHallIds = const {},
    this.profile,
    this.settings,
    this.error,
  });

  final bool initialized;
  final bool busy;
  final AuthSession? session;
  final MobileLookups? lookups;
  final List<HallSummary> halls;
  final List<Band> bands;
  final List<ReceivedBandInvitation> receivedInvitations;
  final List<Reservation> reservations;
  final Set<int> favoriteHallIds;
  final ProfileOverview? profile;
  final UserSettings? settings;
  final String? error;

  bool get authenticated => session != null;

  AppState copyWith({
    bool? initialized,
    bool? busy,
    AuthSession? session,
    bool clearSession = false,
    MobileLookups? lookups,
    List<HallSummary>? halls,
    List<Band>? bands,
    List<ReceivedBandInvitation>? receivedInvitations,
    List<Reservation>? reservations,
    Set<int>? favoriteHallIds,
    ProfileOverview? profile,
    UserSettings? settings,
    String? error,
    bool clearError = false,
  }) => AppState(
    initialized: initialized ?? this.initialized,
    busy: busy ?? this.busy,
    session: clearSession ? null : session ?? this.session,
    lookups: lookups ?? this.lookups,
    halls: halls ?? this.halls,
    bands: bands ?? this.bands,
    receivedInvitations: receivedInvitations ?? this.receivedInvitations,
    reservations: reservations ?? this.reservations,
    favoriteHallIds: favoriteHallIds ?? this.favoriteHallIds,
    profile: profile ?? this.profile,
    settings: settings ?? this.settings,
    error: clearError ? null : error ?? this.error,
  );
}

class AppController extends Notifier<AppState> {
  OpenAmpRepository get _repository => ref.read(repositoryProvider);
  SessionStore get _sessionStore => ref.read(sessionStoreProvider);

  @override
  AppState build() {
    Future.microtask(_bootstrap);
    return const AppState();
  }

  Future<void> _bootstrap() async {
    try {
      final session = await _sessionStore.load();
      state = state.copyWith(initialized: true, session: session);
      await loadCatalog();
      if (session != null) await refreshPrivateData();
    } catch (error) {
      state = state.copyWith(initialized: true, error: error.toString());
    }
  }

  Future<void> login(String email, String password) async {
    await _guard(() async {
      final session = await _repository.login(email, password);
      state = state.copyWith(session: session);
      await refreshPrivateData();
    });
  }

  Future<void> register({
    required String username,
    required String firstName,
    required String lastName,
    required String email,
    required String password,
    String? phone,
  }) async {
    await _guard(() async {
      final session = await _repository.register(
        username: username,
        firstName: firstName,
        lastName: lastName,
        email: email,
        password: password,
        phone: phone,
      );
      state = state.copyWith(session: session);
      await refreshPrivateData();
    });
  }

  Future<void> logout() async {
    await _sessionStore.clear();
    await LocalNotificationService.instance.syncReservations(
      enabled: false,
      reservations: const [],
    );
    state = AppState(
      initialized: true,
      lookups: state.lookups,
      halls: state.halls,
    );
  }

  Future<void> loadCatalog([
    SearchFilters filters = const SearchFilters(),
  ]) async {
    await _guard(() async {
      final values = await Future.wait([
        _repository.getLookups(),
        _repository.searchHalls(filters),
      ]);
      state = state.copyWith(
        lookups: values[0] as MobileLookups,
        halls: values[1] as List<HallSummary>,
      );
    }, showBusy: state.halls.isEmpty);
  }

  Future<void> refreshPrivateData() async {
    if (!state.authenticated) return;
    await _guard(() async {
      final values = await Future.wait([
        _repository.getMyBands(),
        _repository.getReceivedInvitations(),
        _repository.getMyReservations(),
        _repository.getProfile(),
        _repository.getFavoriteHallIds(),
        _repository.getSettings(),
      ]);
      state = state.copyWith(
        bands: values[0] as List<Band>,
        receivedInvitations: values[1] as List<ReceivedBandInvitation>,
        reservations: values[2] as List<Reservation>,
        profile: values[3] as ProfileOverview,
        favoriteHallIds: values[4] as Set<int>,
        settings: values[5] as UserSettings,
      );
      await LocalNotificationService.instance.syncReservations(
        enabled: state.settings?.pushNotifications ?? false,
        reservations: state.reservations,
      );
    });
  }

  Future<void> createBand(String name, int genreId, String? description) async {
    await _guard(() async {
      await _repository.createBand(
        name: name,
        genreId: genreId,
        description: description,
      );
      state = state.copyWith(bands: await _repository.getMyBands());
    });
  }

  Future<void> inviteMember(int bandId, String username) async {
    await _guard(() async {
      await _repository.inviteMember(bandId, username);
      state = state.copyWith(bands: await _repository.getMyBands());
    });
  }

  Future<void> respondToInvitation(
    int invitationId,
    bool accept,
    int? instrumentId,
  ) async {
    await _guard(() async {
      await _repository.respondToInvitation(
        invitationId: invitationId,
        accept: accept,
        instrumentId: instrumentId,
      );
      final values = await Future.wait([
        _repository.getMyBands(),
        _repository.getReceivedInvitations(),
      ]);
      state = state.copyWith(
        bands: values[0] as List<Band>,
        receivedInvitations: values[1] as List<ReceivedBandInvitation>,
      );
    });
  }

  Future<void> updateBand(
    int bandId,
    String name,
    int genreId,
    String? description,
  ) async {
    await _guard(() async {
      await _repository.updateBand(
        bandId: bandId,
        name: name,
        genreId: genreId,
        description: description,
      );
      state = state.copyWith(bands: await _repository.getMyBands());
    });
  }

  Future<void> updateBandMember(
    int bandId,
    int userId,
    int? instrumentId,
    String? role,
  ) async {
    await _guard(() async {
      await _repository.updateBandMember(
        bandId: bandId,
        userId: userId,
        instrumentId: instrumentId,
        role: role,
      );
      state = state.copyWith(bands: await _repository.getMyBands());
    });
  }

  Future<void> removeBandMember(int bandId, int userId) async {
    await _guard(() async {
      await _repository.removeBandMember(bandId, userId);
      state = state.copyWith(bands: await _repository.getMyBands());
    });
  }

  Future<void> setFavoriteHall(int hallId, bool saved) async {
    await _guard(() async {
      await _repository.setFavoriteHall(hallId, saved);
      final favorites = {...state.favoriteHallIds};
      saved ? favorites.add(hallId) : favorites.remove(hallId);
      state = state.copyWith(favoriteHallIds: favorites);
    }, showBusy: false);
  }

  Future<void> updateProfile({
    required String username,
    required String firstName,
    required String lastName,
    String? phone,
    String? imageUrl,
    required List<int> instrumentIds,
  }) async {
    await _guard(() async {
      await _repository.updateProfile(
        username: username,
        firstName: firstName,
        lastName: lastName,
        phone: phone,
        imageUrl: imageUrl,
        instrumentIds: instrumentIds,
      );
      state = state.copyWith(profile: await _repository.getProfile());
    });
  }

  Future<void> uploadProfilePhoto(String path) async {
    await _guard(() async {
      await _repository.uploadProfilePhoto(path);
      state = state.copyWith(profile: await _repository.getProfile());
    });
  }

  Future<void> uploadBandPhoto(int bandId, String path) async {
    await _guard(() async {
      await _repository.uploadBandPhoto(bandId, path);
      state = state.copyWith(bands: await _repository.getMyBands());
    });
  }

  Future<void> changePassword(String currentPassword, String newPassword) =>
      _guard(
        () => _repository.changePassword(
          currentPassword: currentPassword,
          newPassword: newPassword,
        ),
      );

  Future<void> updateSettings(UserSettings settings) async {
    await _guard(() async {
      state = state.copyWith(
        settings: await _repository.updateSettings(settings),
      );
    });
  }

  Future<void> reloadReservations() async {
    await _guard(() async {
      state = state.copyWith(
        reservations: await _repository.getMyReservations(),
        profile: await _repository.getProfile(),
      );
      await LocalNotificationService.instance.syncReservations(
        enabled: state.settings?.pushNotifications ?? false,
        reservations: state.reservations,
      );
    }, showBusy: false);
  }

  void clearError() => state = state.copyWith(clearError: true);

  Future<void> _guard(
    Future<void> Function() action, {
    bool showBusy = true,
  }) async {
    if (showBusy) state = state.copyWith(busy: true, clearError: true);
    try {
      await action();
    } catch (error) {
      state = state.copyWith(error: error.toString());
      rethrow;
    } finally {
      if (showBusy) state = state.copyWith(busy: false);
    }
  }
}
