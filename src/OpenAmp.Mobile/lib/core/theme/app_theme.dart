import 'package:flutter/material.dart';

abstract final class AppColors {
  static const ink = Color(0xFF101114);
  static const inkSoft = Color(0xFFD0D1D5);
  static const canvas = Color(0xFF0C0D10);
  static const paper = Color(0xFF17191E);
  static const paperMuted = Color(0xFF24272E);
  static const line = Color(0xFF363A43);
  static const primary = Color(0xFFFF633D);
  static const primarySoft = Color(0xFF3A211B);
  static const signal = Color(0xFFFF633D);
  static const signalSoft = Color(0xFF3A211B);
  static const text = Color(0xFFF4F1EA);
  static const textMuted = Color(0xFFA3A7B0);
  static const success = Color(0xFF42C995);
  static const successSoft = Color(0xFF17352B);
  static const danger = Color(0xFFFF667D);
  static const dangerSoft = Color(0xFF3B1D24);
  static const warning = Color(0xFFF1AE3D);

  // Kept for existing screens while the refreshed names above communicate intent.
  static const primaryDark = ink;
  static const background = canvas;
  static const surfaceMuted = paperMuted;
}

abstract final class AppRadii {
  static const small = 8.0;
  static const medium = 13.0;
  static const large = 18.0;
}

abstract final class OpenAmpTheme {
  static ThemeData get dark {
    const scheme = ColorScheme.dark(
      primary: AppColors.primary,
      onPrimary: AppColors.ink,
      primaryContainer: AppColors.primarySoft,
      onPrimaryContainer: AppColors.text,
      secondary: AppColors.signal,
      onSecondary: AppColors.ink,
      secondaryContainer: AppColors.signalSoft,
      onSecondaryContainer: AppColors.text,
      error: AppColors.danger,
      surface: AppColors.paper,
      onSurface: AppColors.text,
      outline: AppColors.line,
      outlineVariant: AppColors.paperMuted,
    );

    const textTheme = TextTheme(
      displaySmall: TextStyle(
        color: AppColors.text,
        fontSize: 38,
        height: 0.98,
        fontWeight: FontWeight.w900,
        letterSpacing: -1.8,
      ),
      headlineMedium: TextStyle(
        color: AppColors.text,
        fontSize: 29,
        height: 1.04,
        fontWeight: FontWeight.w900,
        letterSpacing: -1.1,
      ),
      titleLarge: TextStyle(
        color: AppColors.text,
        fontSize: 20,
        height: 1.1,
        fontWeight: FontWeight.w900,
        letterSpacing: -0.45,
      ),
      titleMedium: TextStyle(
        color: AppColors.text,
        fontSize: 16,
        height: 1.2,
        fontWeight: FontWeight.w800,
        letterSpacing: -0.15,
      ),
      bodyLarge: TextStyle(color: AppColors.text, fontSize: 16, height: 1.45),
      bodyMedium: TextStyle(
        color: AppColors.textMuted,
        fontSize: 14,
        height: 1.42,
      ),
      labelLarge: TextStyle(
        color: AppColors.text,
        fontSize: 15,
        fontWeight: FontWeight.w800,
        letterSpacing: 0.1,
      ),
    );

    OutlineInputBorder inputBorder(Color color, [double width = 1]) =>
        OutlineInputBorder(
          borderRadius: BorderRadius.circular(AppRadii.medium),
          borderSide: BorderSide(color: color, width: width),
        );

    return ThemeData(
      useMaterial3: true,
      colorScheme: scheme,
      scaffoldBackgroundColor: AppColors.canvas,
      canvasColor: AppColors.canvas,
      fontFamily: 'Roboto',
      textTheme: textTheme,
      dividerColor: AppColors.line,
      splashFactory: InkSparkle.splashFactory,
      appBarTheme: const AppBarTheme(
        backgroundColor: AppColors.canvas,
        foregroundColor: AppColors.text,
        surfaceTintColor: Colors.transparent,
        elevation: 0,
        scrolledUnderElevation: 0,
        centerTitle: false,
        titleSpacing: 18,
        titleTextStyle: TextStyle(
          color: AppColors.text,
          fontSize: 19,
          fontWeight: FontWeight.w900,
          letterSpacing: -0.35,
        ),
      ),
      cardTheme: CardThemeData(
        color: AppColors.paper,
        surfaceTintColor: Colors.transparent,
        margin: EdgeInsets.zero,
        elevation: 0,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(AppRadii.large),
          side: const BorderSide(color: AppColors.line),
        ),
      ),
      filledButtonTheme: FilledButtonThemeData(
        style: FilledButton.styleFrom(
          minimumSize: const Size.fromHeight(54),
          backgroundColor: AppColors.primary,
          foregroundColor: AppColors.ink,
          disabledBackgroundColor: AppColors.paperMuted,
          disabledForegroundColor: AppColors.textMuted,
          elevation: 0,
          textStyle: const TextStyle(
            fontSize: 15,
            fontWeight: FontWeight.w900,
            letterSpacing: 0.1,
          ),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(AppRadii.medium),
          ),
        ),
      ),
      outlinedButtonTheme: OutlinedButtonThemeData(
        style: OutlinedButton.styleFrom(
          minimumSize: const Size.fromHeight(52),
          foregroundColor: AppColors.text,
          side: const BorderSide(color: AppColors.line, width: 1.3),
          textStyle: const TextStyle(fontWeight: FontWeight.w800),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(AppRadii.medium),
          ),
        ),
      ),
      textButtonTheme: TextButtonThemeData(
        style: TextButton.styleFrom(
          foregroundColor: AppColors.primary,
          textStyle: const TextStyle(fontWeight: FontWeight.w800),
        ),
      ),
      inputDecorationTheme: InputDecorationTheme(
        filled: true,
        fillColor: AppColors.paper,
        isDense: false,
        contentPadding: const EdgeInsets.symmetric(
          horizontal: 16,
          vertical: 16,
        ),
        labelStyle: const TextStyle(color: AppColors.textMuted),
        hintStyle: const TextStyle(color: Color(0xFF938B96)),
        prefixIconColor: AppColors.textMuted,
        suffixIconColor: AppColors.textMuted,
        border: inputBorder(AppColors.line),
        enabledBorder: inputBorder(AppColors.line),
        focusedBorder: inputBorder(AppColors.primary, 2),
        errorBorder: inputBorder(AppColors.danger),
        focusedErrorBorder: inputBorder(AppColors.danger, 2),
      ),
      chipTheme: ChipThemeData(
        backgroundColor: AppColors.paper,
        selectedColor: AppColors.primary,
        disabledColor: AppColors.paperMuted,
        side: const BorderSide(color: AppColors.line),
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(AppRadii.small),
        ),
        labelStyle: const TextStyle(fontWeight: FontWeight.w700),
        secondaryLabelStyle: const TextStyle(
          color: Colors.white,
          fontWeight: FontWeight.w800,
        ),
        padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 7),
      ),
      snackBarTheme: SnackBarThemeData(
        backgroundColor: AppColors.paperMuted,
        contentTextStyle: const TextStyle(color: Colors.white),
        behavior: SnackBarBehavior.floating,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(AppRadii.medium),
        ),
      ),
      tabBarTheme: const TabBarThemeData(
        labelColor: AppColors.text,
        unselectedLabelColor: AppColors.textMuted,
        indicatorColor: AppColors.signal,
        indicatorSize: TabBarIndicatorSize.label,
        dividerColor: AppColors.line,
        labelStyle: TextStyle(fontWeight: FontWeight.w900),
      ),
      dialogTheme: DialogThemeData(
        backgroundColor: AppColors.paper,
        surfaceTintColor: Colors.transparent,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(AppRadii.large),
          side: const BorderSide(color: AppColors.line),
        ),
      ),
    );
  }
}
