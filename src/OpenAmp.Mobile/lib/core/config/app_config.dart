abstract final class AppConfig {
  static const apiBaseUrl = String.fromEnvironment(
    'OPENAMP_API_URL',
    defaultValue: 'http://10.0.2.2:5264',
  );

  static const stripePublishableKey = String.fromEnvironment(
    'STRIPE_PUBLISHABLE_KEY',
  );
}
