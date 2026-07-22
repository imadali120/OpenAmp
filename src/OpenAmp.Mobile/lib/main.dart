import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_stripe/flutter_stripe.dart';
import 'package:intl/date_symbol_data_local.dart';
import 'package:openamp_mobile/app.dart';
import 'package:openamp_mobile/core/config/app_config.dart';

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();
  await initializeDateFormatting('bs');
  if (AppConfig.stripePublishableKey.isNotEmpty) {
    Stripe.publishableKey = AppConfig.stripePublishableKey;
    Stripe.urlScheme = 'openamp';
    await Stripe.instance.applySettings();
  }
  runApp(const ProviderScope(child: OpenAmpApp()));
}
