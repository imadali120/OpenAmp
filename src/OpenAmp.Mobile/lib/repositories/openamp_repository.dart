import 'package:dio/dio.dart';
import 'package:intl/intl.dart';
import 'package:openamp_mobile/core/network/api_client.dart';
import 'package:openamp_mobile/core/storage/session_store.dart';
import 'package:openamp_mobile/models/models.dart';

class OpenAmpRepository {
  OpenAmpRepository(this._apiClient);
  final ApiClient _apiClient;

  Future<AuthSession> login(String identifier, String password) async {
    try {
      final response = await _apiClient.dio.post<Map<String, dynamic>>(
        '/api/auth/login',
        data: {'email': identifier.trim(), 'password': password},
      );
      final session = AuthSession.fromAuthResponse(response.data!);
      await _apiClient.sessionStore.save(session);
      return session;
    } catch (error) {
      _apiClient.throwApiError(error);
    }
  }

  Future<AuthSession> register({
    required String username,
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
          'username': username.trim(),
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

  Future<BandInvitation> inviteMember(int bandId, String username) async {
    try {
      final response = await _apiClient.dio.post<Map<String, dynamic>>(
        '/api/bands/$bandId/invitations',
        data: {'username': username.trim()},
      );
      return BandInvitation.fromJson(response.data!);
    } catch (error) {
      _apiClient.throwApiError(error);
    }
  }

  Future<List<ReceivedBandInvitation>> getReceivedInvitations() async {
    try {
      final response = await _apiClient.dio.get<List<dynamic>>(
        '/api/bands/invitations/received',
      );
      return response.data!
          .map(
            (item) =>
                ReceivedBandInvitation.fromJson(item as Map<String, dynamic>),
          )
          .toList();
    } catch (error) {
      _apiClient.throwApiError(error);
    }
  }

  Future<void> respondToInvitation({
    required int invitationId,
    required bool accept,
    int? instrumentId,
  }) async {
    try {
      await _apiClient.dio.post<Map<String, dynamic>>(
        '/api/bands/invitations/$invitationId/respond',
        data: {'prihvati': accept, 'instrumentId': instrumentId},
      );
    } catch (error) {
      _apiClient.throwApiError(error);
    }
  }

  Future<Band> updateBand({
    required int bandId,
    required String name,
    required int genreId,
    String? description,
  }) async {
    try {
      final response = await _apiClient.dio.put<Map<String, dynamic>>(
        '/api/bands/$bandId',
        data: {'naziv': name, 'zanrId': genreId, 'opis': description},
      );
      return Band.fromJson(response.data!);
    } catch (error) {
      _apiClient.throwApiError(error);
    }
  }

  Future<Band> updateBandMember({
    required int bandId,
    required int userId,
    int? instrumentId,
    String? role,
  }) async {
    try {
      final response = await _apiClient.dio.put<Map<String, dynamic>>(
        '/api/bands/$bandId/members/$userId',
        data: {'instrumentId': instrumentId, 'uloga': role},
      );
      return Band.fromJson(response.data!);
    } catch (error) {
      _apiClient.throwApiError(error);
    }
  }

  Future<void> removeBandMember(int bandId, int userId) async {
    try {
      await _apiClient.dio.delete<Map<String, dynamic>>(
        '/api/bands/$bandId/members/$userId',
      );
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

  Future<ReservationDetails> getReservation(int id) async {
    try {
      final response = await _apiClient.dio.get<Map<String, dynamic>>(
        '/api/reservations/$id',
      );
      return ReservationDetails.fromJson(response.data!);
    } catch (error) {
      _apiClient.throwApiError(error);
    }
  }

  Future<ReservationDetails> updateReservation({
    required int id,
    required DateTime startsAt,
    required DateTime endsAt,
    required String rowVersion,
  }) async {
    try {
      final response = await _apiClient.dio.put<Map<String, dynamic>>(
        '/api/reservations/$id',
        data: {
          'terminOdUtc': startsAt.toUtc().toIso8601String(),
          'terminDoUtc': endsAt.toUtc().toIso8601String(),
          'rowVersion': rowVersion,
        },
      );
      return ReservationDetails.fromJson(response.data!);
    } catch (error) {
      _apiClient.throwApiError(error);
    }
  }

  Future<CancellationResult> cancelReservation({
    required int id,
    required String rowVersion,
    String? reason,
  }) async {
    try {
      final response = await _apiClient.dio.post<Map<String, dynamic>>(
        '/api/reservations/$id/cancel',
        data: {'rowVersion': rowVersion, 'razlog': reason},
      );
      return CancellationResult.fromJson(response.data!);
    } catch (error) {
      _apiClient.throwApiError(error);
    }
  }

  Future<CancellationPreview> getCancellationPreview(int id) async {
    try {
      final response = await _apiClient.dio.get<Map<String, dynamic>>(
        '/api/reservations/$id/cancellation-preview',
      );
      return CancellationPreview.fromJson(response.data!);
    } catch (error) {
      _apiClient.throwApiError(error);
    }
  }

  Future<void> createReview({
    required int reservationId,
    required int rating,
    String? comment,
  }) async {
    try {
      await _apiClient.dio.post<Map<String, dynamic>>(
        '/api/reservations/$reservationId/review',
        data: {'ocjena': rating, 'komentar': comment},
      );
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

  Future<void> updateProfile({
    required String username,
    required String firstName,
    required String lastName,
    String? phone,
    String? imageUrl,
    required List<int> instrumentIds,
  }) async {
    try {
      await _apiClient.dio.put<Map<String, dynamic>>(
        '/api/users/me',
        data: {
          'username': username,
          'ime': firstName,
          'prezime': lastName,
          'telefon': phone,
          'fotografijaUrl': imageUrl,
          'instrumentIds': instrumentIds,
        },
      );
    } catch (error) {
      _apiClient.throwApiError(error);
    }
  }

  Future<void> uploadProfilePhoto(String path) async {
    try {
      await _apiClient.dio.post<Map<String, dynamic>>(
        '/api/images/profile',
        data: FormData.fromMap({
          'file': await MultipartFile.fromFile(
            path,
            contentType: _imageContentType(path),
          ),
        }),
      );
    } catch (error) {
      _apiClient.throwApiError(error);
    }
  }

  Future<void> uploadBandPhoto(int bandId, String path) async {
    try {
      await _apiClient.dio.post<Map<String, dynamic>>(
        '/api/images/bands/$bandId',
        data: FormData.fromMap({
          'file': await MultipartFile.fromFile(
            path,
            contentType: _imageContentType(path),
          ),
        }),
      );
    } catch (error) {
      _apiClient.throwApiError(error);
    }
  }

  Future<void> changePassword({
    required String currentPassword,
    required String newPassword,
  }) async {
    try {
      await _apiClient.dio.post<void>(
        '/api/users/me/change-password',
        data: {'trenutnaLozinka': currentPassword, 'novaLozinka': newPassword},
      );
    } catch (error) {
      _apiClient.throwApiError(error);
    }
  }

  Future<Set<int>> getFavoriteHallIds() async {
    try {
      final response = await _apiClient.dio.get<List<dynamic>>(
        '/api/users/me/favorite-halls',
      );
      return response.data!.cast<int>().toSet();
    } catch (error) {
      _apiClient.throwApiError(error);
    }
  }

  Future<void> setFavoriteHall(int hallId, bool saved) async {
    try {
      if (saved) {
        await _apiClient.dio.put<bool>('/api/users/me/favorite-halls/$hallId');
      } else {
        await _apiClient.dio.delete<bool>(
          '/api/users/me/favorite-halls/$hallId',
        );
      }
    } catch (error) {
      _apiClient.throwApiError(error);
    }
  }

  Future<UserSettings> getSettings() async {
    try {
      final response = await _apiClient.dio.get<Map<String, dynamic>>(
        '/api/users/me/settings',
      );
      return UserSettings.fromJson(response.data!);
    } catch (error) {
      _apiClient.throwApiError(error);
    }
  }

  Future<UserSettings> updateSettings(UserSettings settings) async {
    try {
      final response = await _apiClient.dio.put<Map<String, dynamic>>(
        '/api/users/me/settings',
        data: {
          'pushNotifikacije': settings.pushNotifications,
          'emailNotifikacije': settings.emailNotifications,
          'jezik': settings.language,
          'profilJavan': settings.publicProfile,
        },
      );
      return UserSettings.fromJson(response.data!);
    } catch (error) {
      _apiClient.throwApiError(error);
    }
  }

  Future<ReservationDetails> createReservation(BookingDraft draft) async {
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
      return ReservationDetails.fromJson(response.data!);
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
        customerId: json['customerId'] as String,
        customerSessionClientSecret:
            json['customerSessionClientSecret'] as String,
      );
    } catch (error) {
      _apiClient.throwApiError(error);
    }
  }

  static DioMediaType _imageContentType(String path) {
    final extension = path.toLowerCase().split('.').last;
    return switch (extension) {
      'png' => DioMediaType('image', 'png'),
      'webp' => DioMediaType('image', 'webp'),
      _ => DioMediaType('image', 'jpeg'),
    };
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
    required this.customerId,
    required this.customerSessionClientSecret,
  });
  final String id;
  final String clientSecret;
  final int amount;
  final String currency;
  final String customerId;
  final String customerSessionClientSecret;
}
