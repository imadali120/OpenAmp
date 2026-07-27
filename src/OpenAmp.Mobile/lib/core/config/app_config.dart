import 'package:flutter/foundation.dart';

abstract final class AppConfig {
  static String get apiBaseUrl {
    const configured = String.fromEnvironment('OPENAMP_API_URL');
    if (configured.isNotEmpty) return configured;
    return defaultTargetPlatform == TargetPlatform.android
        ? 'http://10.0.2.2:5264'
        : 'http://127.0.0.1:5264';
  }

  static const stripePublishableKey = String.fromEnvironment(
    'STRIPE_PUBLISHABLE_KEY',
  );

  static String? resolveMediaUrl(String? value) {
    if (value == null || value.isEmpty) return null;
    final uri = Uri.parse(value);
    if (uri.hasScheme) return value;
    return Uri.parse(apiBaseUrl).resolve(value).toString();
  }
}
