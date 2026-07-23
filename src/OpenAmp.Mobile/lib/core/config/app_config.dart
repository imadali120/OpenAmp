abstract final class AppConfig {
  static const apiBaseUrl = String.fromEnvironment(
    'OPENAMP_API_URL',
    defaultValue: 'http://10.0.2.2:5264',
  );

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
