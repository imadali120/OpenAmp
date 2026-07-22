import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:openamp_mobile/widgets/common.dart';

void main() {
  testWidgets('OpenAmp logo renders brand name', (tester) async {
    await tester.pumpWidget(
      const MaterialApp(home: Scaffold(body: OpenAmpLogo())),
    );

    expect(find.text('OpenAmp'), findsOneWidget);
    expect(find.byIcon(Icons.graphic_eq_rounded), findsOneWidget);
  });
}
