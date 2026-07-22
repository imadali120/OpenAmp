import 'package:intl/intl.dart';
import 'package:openamp_mobile/core/network/api_client.dart';
import 'package:openamp_mobile/core/storage/session_store.dart';
import 'package:openamp_mobile/models/models.dart';

class OpenAmpRepository {
  OpenAmpRepository(this._apiClient);
  final ApiClient _apiClient;

  Future<AuthSession> login(String email, String password) async {
    try {
      final response = await _apiClient.dio.post<Map<String, dynamic>>(
        '/api/auth/login',
        data: {'email': email.trim(), 'password': password},
      );
      final session = AuthSession.fromAuthResponse(response.data!);
      await _apiClient.sessionStore.save(session);
      return session;
    } catch (error) {
      _apiClient.throwApiError(error);
    }
  }

  Future<AuthSession> register({
    required String firstName,
    required String lastName,
    required String email,
    required String password,
    String? phone,
  }) async {
    try {
      final response = await _apiClient.dio.post<Map<String, dynamic>>(
        '/api/auth/register',
        data: {
          'ime': firstName.trim(),
          'prezime': lastName.trim(),
          'email': email.trim(),
          'password': password,
          'telefon': phone?.trim().isEmpty == true ? null : phone?.trim(),
        },
      );
      final session = AuthSession.fromAuthResponse(response.data!);
      await _apiClient.sessionStore.save(session);
      return session;
    } catch (error) {
      _apiClient.throwApiError(error);
    }
  }

  Future<MobileLookups> getLookups() async {
    try {
      final response = await _apiClient.dio.get<Map<String, dynamic>>(
        '/api/mobile/lookups',
      );
      return MobileLookups.fromJson(response.data!);
    } catch (error) {
      _apiClient.throwApiError(error);
    }
  }

  Future<List<HallSummary>> searchHalls(SearchFilters filters) async {
    try {
      final response = await _apiClient.dio.get<List<dynamic>>(
        '/api/salas',
        queryParameters: {
          if (filters.text?.isNotEmpty == true) 'search': filters.text,
          if (filters.genreCode != null) 'genre': filters.genreCode,
          if (filters.minimumCapacity != null)
            'capacity': filters.minimumCapacity,
          if (filters.equipmentCategoryCode != null)
            'equipmentCategory': filters.equipmentCategoryCode,
          if (filters.startsAt != null)
            'fromUtc': filters.startsAt!.toUtc().toIso8601String(),
          if (filters.endsAt != null)
            'toUtc': filters.endsAt!.toUtc().toIso8601String(),
        },
      );
      return response.data!
          .map((item) => HallSummary.fromJson(item as Map<String, dynamic>))
          .toList();
    } catch (error) {
      _apiClient.throwApiError(error);
    }
  }

  Future<HallDetails> getHall(int id) async {
    try {
      final response = await _apiClient.dio.get<Map<String, dynamic>>(
        '/api/salas/$id',
      );
      return HallDetails.fromJson(response.data!);
    } catch (error) {
      _apiClient.throwApiError(error);
    }
  }

  Future<List<DateTimeRangeValue>> getAvailability({
    required int hallId,
    required DateTime date,
    int durationMinutes = 60,
  }) async {
    try {
      final response = await _apiClient.dio.get<List<dynamic>>(
        '/api/reservations/availability',
        queryParameters: {
          'salaId': hallId,
          'date': DateFormat('yyyy-MM-dd').format(date),
          'durationMinutes': durationMinutes,
          'stepMinutes': 60,
        },
      );
      return response.data!
          .map(
            (item) => DateTimeRangeValue(
              DateTime.parse(
                (item as Map<String, dynamic>)['terminOdUtc'] as String,
              ).toUtc(),
              DateTime.parse(item['terminDoUtc'] as String).toUtc(),
            ),
          )
          .toList();
    } catch (error) {
      _apiClient.throwApiError(error);
    }
  }

  Future<List<Band>> getMyBands() async {
    try {
      final response = await _apiClient.dio.get<List<dynamic>>(
        '/api/bands/mine',
      );
      return response.data!
          .map((item) => Band.fromJson(item as Map<String, dynamic>))
          .toList();
    } catch (error) {
      _apiClient.throwApiError(error);
    }
  }

  Future<Band> createBand({
    required String name,
    required int genreId,
    String? description,
  }) async {
    try {
      final response = await _apiClient.dio.post<Map<String, dynamic>>(
        '/api/bands',
        data: {'naziv': name, 'zanrId': genreId, 'opis': description},
      );
      return Band.fromJson(response.data!);
    } catch (error) {
      _apiClient.throwApiError(error);
    }
  }

  Future<BandInvitation> inviteMember(int bandId, String email) async {
    try {
      final response = await _apiClient.dio.post<Map<String, dynamic>>(
        '/api/bands/$bandId/invitations',
        data: {'email': email.trim()},
      );
      return BandInvitation.fromJson(response.data!);
    } catch (error) {
      _apiClient.throwApiError(error);
    }
  }

  Future<List<Reservation>> getMyReservations() async {
    try {
      final response = await _apiClient.dio.get<List<dynamic>>(
        '/api/reservations/mine',
      );
      return response.data!
          .map((item) => Reservation.fromJson(item as Map<String, dynamic>))
          .toList();
    } catch (error) {
      _apiClient.throwApiError(error);
    }
  }

  Future<ProfileOverview> getProfile() async {
    try {
      final response = await _apiClient.dio.get<Map<String, dynamic>>(
        '/api/users/me/overview',
      );
      return ProfileOverview.fromJson(response.data!);
    } catch (error) {
      _apiClient.throwApiError(error);
    }
  }

  Future<int> createReservation(BookingDraft draft) async {
    if (draft.band == null || draft.startsAt == null || draft.endsAt == null) {
      throw const ApiException('Odaberite bend i termin prije rezervacije.');
    }
    try {
      final items = <Map<String, dynamic>>[
        ...draft.equipmentQuantities.entries
            .where((entry) => entry.value > 0)
            .map(
              (entry) => {
                'opremaId': entry.key,
                'artikalId': null,
                'kolicina': 1,
              },
            ),
        ...draft.storeItemQuantities.entries
            .where((entry) => entry.value > 0)
            .map(
              (entry) => {
                'opremaId': null,
                'artikalId': entry.key,
                'kolicina': entry.value,
              },
            ),
      ];
      final response = await _apiClient.dio.post<Map<String, dynamic>>(
        '/api/reservations',
        data: {
          'salaId': draft.hall.id,
          'bendId': draft.band!.id,
          'terminOdUtc': draft.startsAt!.toUtc().toIso8601String(),
          'terminDoUtc': draft.endsAt!.toUtc().toIso8601String(),
          'napomena': null,
          'stavke': items,
        },
      );
      return response.data!['id'] as int;
    } catch (error) {
      _apiClient.throwApiError(error);
    }
  }

  Future<PaymentIntentValue> createPaymentIntent(int reservationId) async {
    try {
      final response = await _apiClient.dio.post<Map<String, dynamic>>(
        '/api/payments/reservations/$reservationId/payment-intent',
      );
      final json = response.data!;
      return PaymentIntentValue(
        id: json['paymentIntentId'] as String,
        clientSecret: json['clientSecret'] as String,
        amount: json['iznosUNajmanjojJedinici'] as int,
        currency: json['valuta'] as String,
      );
    } catch (error) {
      _apiClient.throwApiError(error);
    }
  }
}

class DateTimeRangeValue {
  const DateTimeRangeValue(this.start, this.end);
  final DateTime start;
  final DateTime end;
}

class PaymentIntentValue {
  const PaymentIntentValue({
    required this.id,
    required this.clientSecret,
    required this.amount,
    required this.currency,
  });
  final String id;
  final String clientSecret;
  final int amount;
  final String currency;
}
