import 'package:dio/dio.dart';
import 'package:openamp_mobile/core/config/app_config.dart';
import 'package:openamp_mobile/core/storage/session_store.dart';

class ApiException implements Exception {
  const ApiException(this.message, {this.statusCode});
  final String message;
  final int? statusCode;

  @override
  String toString() => message;
}

class ApiClient {
  ApiClient(this.sessionStore)
    : dio = Dio(
        BaseOptions(
          baseUrl: AppConfig.apiBaseUrl,
          connectTimeout: const Duration(seconds: 12),
          receiveTimeout: const Duration(seconds: 20),
          headers: {'Accept': 'application/json'},
        ),
      ) {
    dio.interceptors.add(
      InterceptorsWrapper(
        onRequest: (options, handler) {
          final token = sessionStore.current?.accessToken;
          if (token != null) {
            options.headers['Authorization'] = 'Bearer $token';
          }
          handler.next(options);
        },
        onError: (error, handler) async {
          final request = error.requestOptions;
          final canRefresh =
              error.response?.statusCode == 401 &&
              request.extra['retried'] != true &&
              sessionStore.current?.refreshToken != null;
          if (canRefresh) {
            try {
              final response =
                  await Dio(
                    BaseOptions(baseUrl: AppConfig.apiBaseUrl),
                  ).post<Map<String, dynamic>>(
                    '/api/auth/refresh',
                    data: {'refreshToken': sessionStore.current!.refreshToken},
                  );
              final refreshed = AuthSession.fromAuthResponse(response.data!);
              await sessionStore.save(refreshed);
              request.extra['retried'] = true;
              request.headers['Authorization'] =
                  'Bearer ' + refreshed.accessToken;
              handler.resolve(await dio.fetch<dynamic>(request));
              return;
            } on DioException {
              await sessionStore.clear();
            }
          }
          handler.next(error);
        },
      ),
    );
  }

  final SessionStore sessionStore;
  final Dio dio;

  Never throwApiError(Object error) {
    if (error is DioException) {
      final data = error.response?.data;
      final message = data is Map<String, dynamic>
          ? (data['detail'] ?? data['title'])?.toString()
          : null;
      throw ApiException(
        message ?? 'Server trenutno nije dostupan. Pokušajte ponovo.',
        statusCode: error.response?.statusCode,
      );
    }
    throw ApiException(error.toString());
  }
}
