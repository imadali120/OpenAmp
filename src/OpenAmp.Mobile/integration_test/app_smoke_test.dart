import 'package:flutter_test/flutter_test.dart';
import 'package:integration_test/integration_test.dart';
import 'package:openamp_mobile/app.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:openamp_mobile/widgets/common.dart';

void main() {
  IntegrationTestWidgetsFlutterBinding.ensureInitialized();

  testWidgets('OpenAmp starts and renders the authentication experience', (
    tester,
  ) async {
    await tester.pumpWidget(const ProviderScope(child: OpenAmpApp()));
    await tester.pump(const Duration(seconds: 2));

    expect(find.byType(OpenAmpLogo), findsWidgets);
    expect(find.text('Prijava'), findsWidgets);
  });
}
