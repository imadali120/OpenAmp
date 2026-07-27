import 'dart:convert';

import 'package:flutter_secure_storage/flutter_secure_storage.dart';

class AuthSession {
  const AuthSession({
    required this.accessToken,
    required this.refreshToken,
    required this.userId,
    required this.username,
    required this.firstName,
    required this.lastName,
    required this.email,
    required this.role,
  });

  final String accessToken;
  final String refreshToken;
  final int userId;
  final String username;
  final String firstName;
  final String lastName;
  final String email;
  final String role;

  bool get canUseDesktop =>
      role.toUpperCase() == 'ADMIN' ||
      role.toUpperCase() == 'ZAPOSLENIK' ||
      role.toUpperCase() == 'ADMINISTRATOR';

  factory AuthSession.fromAuthResponse(Map<String, dynamic> json) {
    final user = json['korisnik'] as Map<String, dynamic>;
    return AuthSession(
      accessToken: json['accessToken'] as String,
      refreshToken: json['refreshToken'] as String,
      userId: user['id'] as int,
      username: user['username'] as String,
      firstName: user['ime'] as String,
      lastName: user['prezime'] as String,
      email: user['email'] as String,
      role: user['uloga'] as String? ?? 'MUZICAR',
    );
  }

  Map<String, dynamic> toJson() => {
    'accessToken': accessToken,
    'refreshToken': refreshToken,
    'userId': userId,
    'username': username,
    'firstName': firstName,
    'lastName': lastName,
    'email': email,
    'role': role,
  };

  factory AuthSession.fromJson(Map<String, dynamic> json) => AuthSession(
    accessToken: json['accessToken'] as String,
    refreshToken: json['refreshToken'] as String,
    userId: json['userId'] as int,
    username: json['username'] as String? ?? '',
    firstName: json['firstName'] as String,
    lastName: json['lastName'] as String,
    email: json['email'] as String,
    role: json['role'] as String? ?? 'MUZICAR',
  );
}

class SessionStore {
  SessionStore({FlutterSecureStorage? storage})
    : _storage = storage ?? const FlutterSecureStorage();

  static const _key = 'openamp.auth.session';
  final FlutterSecureStorage _storage;
  AuthSession? _memory;

  AuthSession? get current => _memory;

  Future<AuthSession?> load() async {
    final value = await _storage.read(key: _key);
    if (value == null) return null;
    _memory = AuthSession.fromJson(jsonDecode(value) as Map<String, dynamic>);
    return _memory;
  }

  Future<void> save(AuthSession session) async {
    _memory = session;
    await _storage.write(key: _key, value: jsonEncode(session.toJson()));
  }

  Future<void> clear() async {
    _memory = null;
    await _storage.delete(key: _key);
  }
}
