import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:openamp_mobile/core/theme/app_theme.dart';
import 'package:openamp_mobile/screens/auth/auth_screen.dart';
import 'package:openamp_mobile/screens/main_shell.dart';
import 'package:openamp_mobile/state/app_state.dart';
import 'package:openamp_mobile/widgets/common.dart';

class OpenAmpApp extends ConsumerWidget {
  const OpenAmpApp({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final state = ref.watch(appControllerProvider);
    return MaterialApp(
      title: 'OpenAmp',
      debugShowCheckedModeBanner: false,
      theme: OpenAmpTheme.dark,
      darkTheme: OpenAmpTheme.dark,
      themeMode: ThemeMode.dark,
      home: !state.initialized
          ? const Scaffold(body: Center(child: OpenAmpLoader()))
          : state.authenticated
          ? const MainShell()
          : const AuthScreen(),
    );
  }
}
