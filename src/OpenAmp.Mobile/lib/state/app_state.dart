import 'dart:async';

import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:openamp_mobile/core/network/api_client.dart';
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
    this.reservations = const [],
    this.profile,
    this.error,
  });

  final bool initialized;
  final bool busy;
  final AuthSession? session;
  final MobileLookups? lookups;
  final List<HallSummary> halls;
  final List<Band> bands;
  final List<Reservation> reservations;
  final ProfileOverview? profile;
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
    List<Reservation>? reservations,
    ProfileOverview? profile,
    String? error,
    bool clearError = false,
  }) => AppState(
    initialized: initialized ?? this.initialized,
    busy: busy ?? this.busy,
    session: clearSession ? null : session ?? this.session,
    lookups: lookups ?? this.lookups,
    halls: halls ?? this.halls,
    bands: bands ?? this.bands,
    reservations: reservations ?? this.reservations,
    profile: profile ?? this.profile,
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
    required String firstName,
    required String lastName,
    required String email,
    required String password,
    String? phone,
  }) async {
    await _guard(() async {
      final session = await _repository.register(
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
        _repository.getMyReservations(),
        _repository.getProfile(),
      ]);
      state = state.copyWith(
        bands: values[0] as List<Band>,
        reservations: values[1] as List<Reservation>,
        profile: values[2] as ProfileOverview,
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

  Future<void> inviteMember(int bandId, String email) async {
    await _guard(() async {
      await _repository.inviteMember(bandId, email);
      state = state.copyWith(bands: await _repository.getMyBands());
    });
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
