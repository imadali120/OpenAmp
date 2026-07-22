import 'package:flutter/material.dart';

abstract final class AppColors {
  static const ink = Color(0xFF17131F);
  static const inkSoft = Color(0xFF2B2632);
  static const canvas = Color(0xFFF3F0E9);
  static const paper = Color(0xFFFFFCF6);
  static const paperMuted = Color(0xFFE8E3DA);
  static const line = Color(0xFFCEC7BC);
  static const primary = Color(0xFF6847F5);
  static const primarySoft = Color(0xFFE8E1FF);
  static const signal = Color(0xFFFF633D);
  static const signalSoft = Color(0xFFFFE3DA);
  static const text = ink;
  static const textMuted = Color(0xFF6D6672);
  static const success = Color(0xFF14855E);
  static const successSoft = Color(0xFFDDF3E8);
  static const danger = Color(0xFFD63D55);
  static const dangerSoft = Color(0xFFFFE0E5);
  static const warning = Color(0xFFE49318);

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
  static ThemeData get light {
    const scheme = ColorScheme.light(
      primary: AppColors.primary,
      onPrimary: Colors.white,
      primaryContainer: AppColors.primarySoft,
      onPrimaryContainer: AppColors.ink,
      secondary: AppColors.signal,
      onSecondary: AppColors.ink,
      secondaryContainer: AppColors.signalSoft,
      onSecondaryContainer: AppColors.ink,
      error: AppColors.danger,
      surface: AppColors.paper,
      onSurface: AppColors.ink,
      outline: AppColors.line,
      outlineVariant: AppColors.paperMuted,
    );

    const textTheme = TextTheme(
      displaySmall: TextStyle(
        color: AppColors.ink,
        fontSize: 38,
        height: 0.98,
        fontWeight: FontWeight.w900,
        letterSpacing: -1.8,
      ),
      headlineMedium: TextStyle(
        color: AppColors.ink,
        fontSize: 29,
        height: 1.04,
        fontWeight: FontWeight.w900,
        letterSpacing: -1.1,
      ),
      titleLarge: TextStyle(
        color: AppColors.ink,
        fontSize: 20,
        height: 1.1,
        fontWeight: FontWeight.w900,
        letterSpacing: -0.45,
      ),
      titleMedium: TextStyle(
        color: AppColors.ink,
        fontSize: 16,
        height: 1.2,
        fontWeight: FontWeight.w800,
        letterSpacing: -0.15,
      ),
      bodyLarge: TextStyle(color: AppColors.ink, fontSize: 16, height: 1.45),
      bodyMedium: TextStyle(
        color: AppColors.textMuted,
        fontSize: 14,
        height: 1.42,
      ),
      labelLarge: TextStyle(
        color: AppColors.ink,
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
        foregroundColor: AppColors.ink,
        surfaceTintColor: Colors.transparent,
        elevation: 0,
        scrolledUnderElevation: 0,
        centerTitle: false,
        titleSpacing: 18,
        titleTextStyle: TextStyle(
          color: AppColors.ink,
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
          backgroundColor: AppColors.ink,
          foregroundColor: Colors.white,
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
          foregroundColor: AppColors.ink,
          side: const BorderSide(color: AppColors.ink, width: 1.3),
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
        selectedColor: AppColors.ink,
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
        backgroundColor: AppColors.ink,
        contentTextStyle: const TextStyle(color: Colors.white),
        behavior: SnackBarBehavior.floating,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(AppRadii.medium),
        ),
      ),
      tabBarTheme: const TabBarThemeData(
        labelColor: AppColors.ink,
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
          side: const BorderSide(color: AppColors.ink),
        ),
      ),
    );
  }
}
