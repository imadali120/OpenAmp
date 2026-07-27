import 'dart:typed_data';

import 'package:dio/dio.dart';
import 'package:openamp_mobile/core/network/api_client.dart';

class AdminRepository {
  AdminRepository(this._client);

  final ApiClient _client;

  Future<Map<String, dynamic>> dashboard({int? studioId}) =>
      _getMap('/api/desktop/dashboard', {'studioId': studioId});

  Future<Map<String, dynamic>> lookups() => _getMap('/api/desktop/lookups');

  Future<List<Map<String, dynamic>>> halls({String? search}) =>
      _getList('/api/desktop/halls', {'search': search});

  Future<Map<String, dynamic>> saveHall(Map<String, dynamic> data, {int? id}) =>
      _save('/api/desktop/halls', data, id: id);

  Future<void> deleteHall(int id) async {
    try {
      await _client.dio.delete<void>('/api/desktop/halls/$id');
    } catch (error) {
      _client.throwApiError(error);
    }
  }

  Future<List<Map<String, dynamic>>> equipment() =>
      _getList('/api/desktop/equipment');

  Future<Map<String, dynamic>> saveEquipment(
    Map<String, dynamic> data, {
    int? id,
  }) => _save('/api/desktop/equipment', data, id: id);

  Future<List<Map<String, dynamic>>> articles({
    String? search,
    bool lowStockOnly = false,
  }) async {
    final values = await _getList('/api/desktop/articles', {
      'lowStockOnly': lowStockOnly,
    });
    final query = search?.trim().toLowerCase();
    if (query == null || query.isEmpty) return values;
    return values
        .where(
          (x) => '${x['naziv']} ${x['inventarskiBroj']} ${x['studio']}'
              .toLowerCase()
              .contains(query),
        )
        .toList();
  }

  Future<Map<String, dynamic>> saveArticle(
    Map<String, dynamic> data, {
    int? id,
  }) => _save('/api/desktop/articles', data, id: id);

  Future<List<Map<String, dynamic>>> reservations({
    required DateTime from,
    required DateTime to,
  }) => _getList('/api/desktop/reservations', {
    'fromUtc': from.toUtc().toIso8601String(),
    'toUtc': to.toUtc().toIso8601String(),
  });

  Future<Map<String, dynamic>> createReservation(Map<String, dynamic> data) =>
      _save('/api/desktop/reservations', data);

  Future<Map<String, dynamic>> updateReservation(
    int id,
    Map<String, dynamic> data,
  ) => _save('/api/desktop/reservations', data, id: id);

  Future<List<Map<String, dynamic>>> bands({String? search}) =>
      _getList('/api/desktop/bands', {'search': search});

  Future<Map<String, dynamic>> updateBand(int id, Map<String, dynamic> data) =>
      _save('/api/desktop/bands', data, id: id);

  Future<List<Map<String, dynamic>>> users({String? search}) =>
      _getList('/api/desktop/users', {'search': search});

  Future<Map<String, dynamic>> updateUser(int id, Map<String, dynamic> data) =>
      _save('/api/desktop/users', data, id: id);

  Future<List<String>> referenceTypes() async {
    try {
      final response = await _client.dio.get<List<dynamic>>(
        '/api/desktop/reference-data/types',
      );
      return response.data!.cast<String>();
    } catch (error) {
      _client.throwApiError(error);
    }
  }

  Future<List<Map<String, dynamic>>> references(String type) =>
      _getList('/api/desktop/reference-data/$type');

  Future<Map<String, dynamic>> saveReference(
    String type,
    Map<String, dynamic> data, {
    int? id,
  }) => _save('/api/desktop/reference-data/$type', data, id: id);

  Future<void> deleteReference(String type, int id) async {
    try {
      await _client.dio.delete<void>('/api/desktop/reference-data/$type/$id');
    } catch (error) {
      _client.throwApiError(error);
    }
  }

  Future<List<Map<String, dynamic>>> studios({String? search}) =>
      _getList('/api/desktop/reference-data/studios/all', {'search': search});

  Future<Map<String, dynamic>> saveStudio(
    Map<String, dynamic> data, {
    int? id,
  }) => _save('/api/desktop/reference-data/studios/all', data, id: id);

  Future<void> deleteStudio(int id) async {
    try {
      await _client.dio.delete<void>(
        '/api/desktop/reference-data/studios/all/$id',
      );
    } catch (error) {
      _client.throwApiError(error);
    }
  }

  Future<Map<String, dynamic>> report({
    required DateTime from,
    required DateTime to,
    int? hallId,
    int? genreId,
  }) => _getMap('/api/desktop/reports', {
    'fromUtc': from.toUtc().toIso8601String(),
    'toUtc': to.toUtc().toIso8601String(),
    'hallId': hallId,
    'genreId': genreId,
  });

  Future<Uint8List> reportPdf({
    required DateTime from,
    required DateTime to,
    int? hallId,
    int? genreId,
  }) async {
    try {
      final response = await _client.dio.get<List<int>>(
        '/api/desktop/reports/pdf',
        queryParameters: _query({
          'fromUtc': from.toUtc().toIso8601String(),
          'toUtc': to.toUtc().toIso8601String(),
          'hallId': hallId,
          'genreId': genreId,
        }),
        options: Options(responseType: ResponseType.bytes),
      );
      return Uint8List.fromList(response.data!);
    } catch (error) {
      _client.throwApiError(error);
    }
  }

  Future<Map<String, dynamic>> _getMap(
    String path, [
    Map<String, dynamic>? query,
  ]) async {
    try {
      final response = await _client.dio.get<Map<String, dynamic>>(
        path,
        queryParameters: _query(query),
      );
      return response.data!;
    } catch (error) {
      _client.throwApiError(error);
    }
  }

  Future<List<Map<String, dynamic>>> _getList(
    String path, [
    Map<String, dynamic>? query,
  ]) async {
    try {
      final response = await _client.dio.get<List<dynamic>>(
        path,
        queryParameters: _query(query),
      );
      return response.data!.cast<Map<String, dynamic>>().toList(
        growable: false,
      );
    } catch (error) {
      _client.throwApiError(error);
    }
  }

  Future<Map<String, dynamic>> _save(
    String path,
    Map<String, dynamic> data, {
    int? id,
  }) async {
    try {
      final response = id == null
          ? await _client.dio.post<Map<String, dynamic>>(path, data: data)
          : await _client.dio.put<Map<String, dynamic>>(
              '$path/$id',
              data: data,
            );
      return response.data!;
    } catch (error) {
      _client.throwApiError(error);
    }
  }

  Map<String, dynamic>? _query(Map<String, dynamic>? source) {
    if (source == null) return null;
    return Map.fromEntries(
      source.entries.where((entry) => entry.value != null),
    );
  }
}
