import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:openamp_mobile/core/theme/app_theme.dart';

class OpenAmpLogo extends StatelessWidget {
  const OpenAmpLogo({super.key, this.compact = false});
  final bool compact;

  @override
  Widget build(BuildContext context) => Row(
    mainAxisSize: MainAxisSize.min,
    children: [
      Container(
        width: compact ? 30 : 42,
        height: compact ? 30 : 42,
        decoration: BoxDecoration(
          color: AppColors.primary,
          borderRadius: BorderRadius.circular(compact ? 9 : 13),
        ),
        child: Icon(
          Icons.graphic_eq_rounded,
          color: Colors.white,
          size: compact ? 18 : 25,
        ),
      ),
      const SizedBox(width: 10),
      Text(
        'OpenAmp',
        style: TextStyle(
          color: AppColors.text,
          fontSize: compact ? 19 : 27,
          fontWeight: FontWeight.w900,
          letterSpacing: -0.7,
        ),
      ),
    ],
  );
}

class OpenAmpLoader extends StatelessWidget {
  const OpenAmpLoader({super.key});

  @override
  Widget build(BuildContext context) => const Column(
    mainAxisSize: MainAxisSize.min,
    children: [
      OpenAmpLogo(),
      SizedBox(height: 28),
      CircularProgressIndicator(color: AppColors.primary),
    ],
  );
}

class HallImage extends StatelessWidget {
  const HallImage({
    super.key,
    required this.url,
    this.height = 150,
    this.width = double.infinity,
    this.borderRadius = 16,
  });
  final String? url;
  final double height;
  final double width;
  final double borderRadius;

  @override
  Widget build(BuildContext context) {
    final placeholder = Container(
      width: width,
      height: height,
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(borderRadius),
        gradient: const LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [Color(0xFF2A255C), AppColors.primary],
        ),
      ),
      child: const Icon(
        Icons.music_note_rounded,
        size: 42,
        color: Colors.white70,
      ),
    );
    if (url == null || url!.isEmpty || url!.contains('example.openamp.local')) {
      return placeholder;
    }
    return ClipRRect(
      borderRadius: BorderRadius.circular(borderRadius),
      child: CachedNetworkImage(
        imageUrl: url!,
        width: width,
        height: height,
        fit: BoxFit.cover,
        placeholder: (_, _) => placeholder,
        errorWidget: (_, _, _) => placeholder,
      ),
    );
  }
}

class StatusPill extends StatelessWidget {
  const StatusPill({super.key, required this.label, required this.positive});
  final String label;
  final bool positive;

  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 7),
    decoration: BoxDecoration(
      color: positive ? AppColors.success : AppColors.danger,
      borderRadius: BorderRadius.circular(999),
    ),
    child: Text(
      label,
      style: const TextStyle(
        color: Colors.white,
        fontSize: 12,
        fontWeight: FontWeight.w800,
      ),
    ),
  );
}

class ErrorBanner extends StatelessWidget {
  const ErrorBanner({super.key, required this.message, this.onRetry});
  final String message;
  final VoidCallback? onRetry;

  @override
  Widget build(BuildContext context) => Container(
    margin: const EdgeInsets.only(bottom: 16),
    padding: const EdgeInsets.all(14),
    decoration: BoxDecoration(
      color: const Color(0xFFFFEAED),
      borderRadius: BorderRadius.circular(14),
    ),
    child: Row(
      children: [
        const Icon(Icons.error_outline, color: AppColors.danger),
        const SizedBox(width: 10),
        Expanded(child: Text(message)),
        if (onRetry != null)
          TextButton(onPressed: onRetry, child: const Text('Ponovi')),
      ],
    ),
  );
}

String money(num value) => NumberFormat('0.00').format(value) + ' KM';

String initials(String value) {
  final parts = value.trim().split(RegExp(r'\s+'));
  return parts.take(2).where((x) => x.isNotEmpty).map((x) => x[0]).join();
}
