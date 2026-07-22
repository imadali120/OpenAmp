import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:openamp_mobile/core/theme/app_theme.dart';

class OpenAmpLogo extends StatelessWidget {
  const OpenAmpLogo({super.key, this.compact = false, this.onDark = false});

  final bool compact;
  final bool onDark;

  @override
  Widget build(BuildContext context) => Text.rich(
    TextSpan(
      children: [
        const TextSpan(text: 'Open'),
        TextSpan(
          text: 'Amp',
          style: TextStyle(
            color: onDark ? AppColors.signal : AppColors.primary,
          ),
        ),
      ],
    ),
    style: TextStyle(
      color: onDark ? Colors.white : AppColors.ink,
      fontSize: compact ? 21 : 29,
      height: 1,
      fontWeight: FontWeight.w900,
      letterSpacing: compact ? -0.8 : -1.2,
    ),
  );
}

class OpenAmpLoader extends StatelessWidget {
  const OpenAmpLoader({super.key});

  @override
  Widget build(BuildContext context) => const Column(
    mainAxisSize: MainAxisSize.min,
    children: [
      OpenAmpLogo(),
      SizedBox(height: 24),
      SizedBox(
        width: 44,
        child: LinearProgressIndicator(
          minHeight: 4,
          color: AppColors.signal,
          backgroundColor: AppColors.paperMuted,
          borderRadius: BorderRadius.all(Radius.circular(3)),
        ),
      ),
    ],
  );
}

class SectionEyebrow extends StatelessWidget {
  const SectionEyebrow(this.label, {super.key, this.color});

  final String label;
  final Color? color;

  @override
  Widget build(BuildContext context) => Row(
    mainAxisSize: MainAxisSize.min,
    children: [
      Container(
        width: 18,
        height: 5,
        decoration: BoxDecoration(
          color: color ?? AppColors.signal,
          borderRadius: BorderRadius.circular(2),
        ),
      ),
      const SizedBox(width: 8),
      Text(
        label.toUpperCase(),
        style: TextStyle(
          color: color ?? AppColors.textMuted,
          fontSize: 11,
          fontWeight: FontWeight.w900,
          letterSpacing: 1.35,
        ),
      ),
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
        color: AppColors.ink,
        borderRadius: BorderRadius.circular(borderRadius),
      ),
      child: Stack(
        children: [
          Positioned.fill(
            child: CustomPaint(painter: const _SignalPatternPainter()),
          ),
          const Center(
            child: Icon(
              Icons.speaker_group_outlined,
              size: 40,
              color: AppColors.signal,
            ),
          ),
        ],
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
  Widget build(BuildContext context) {
    final color = positive ? AppColors.success : AppColors.danger;
    final background = positive ? AppColors.successSoft : AppColors.dangerSoft;
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
      decoration: BoxDecoration(
        color: background,
        borderRadius: BorderRadius.circular(AppRadii.small),
        border: Border.all(color: color.withValues(alpha: .38)),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Container(
            width: 6,
            height: 6,
            decoration: BoxDecoration(color: color, shape: BoxShape.circle),
          ),
          const SizedBox(width: 6),
          Text(
            label.toUpperCase(),
            style: TextStyle(
              color: color,
              fontSize: 10,
              fontWeight: FontWeight.w900,
              letterSpacing: .65,
            ),
          ),
        ],
      ),
    );
  }
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
      color: AppColors.dangerSoft,
      borderRadius: BorderRadius.circular(AppRadii.medium),
      border: Border.all(color: AppColors.danger),
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

class SignalButton extends StatelessWidget {
  const SignalButton({
    super.key,
    required this.label,
    required this.onPressed,
    this.loading = false,
    this.icon = Icons.arrow_forward_rounded,
  });

  final String label;
  final VoidCallback? onPressed;
  final bool loading;
  final IconData icon;

  @override
  Widget build(BuildContext context) => FilledButton(
    onPressed: loading ? null : onPressed,
    style: FilledButton.styleFrom(
      backgroundColor: AppColors.signal,
      foregroundColor: AppColors.ink,
    ),
    child: loading
        ? const SizedBox(
            width: 21,
            height: 21,
            child: CircularProgressIndicator(
              strokeWidth: 2,
              color: AppColors.ink,
            ),
          )
        : Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [Text(label), Icon(icon, size: 20)],
          ),
  );
}

class _SignalPatternPainter extends CustomPainter {
  const _SignalPatternPainter();

  @override
  void paint(Canvas canvas, Size size) {
    final line = Paint()
      ..color = Colors.white.withValues(alpha: .055)
      ..strokeWidth = 1;
    const gap = 18.0;
    for (double x = -size.height; x < size.width; x += gap) {
      canvas.drawLine(Offset(x, size.height), Offset(x + size.height, 0), line);
    }
    final signal = Paint()
      ..color = AppColors.primary.withValues(alpha: .3)
      ..strokeWidth = 3
      ..strokeCap = StrokeCap.round;
    final center = size.height * .5;
    for (var i = 0; i < 5; i++) {
      final h = 8.0 + (2 - (i - 2).abs()) * 6;
      final x = size.width - 22 - (4 - i) * 7;
      canvas.drawLine(
        Offset(x, center - h / 2),
        Offset(x, center + h / 2),
        signal,
      );
    }
  }

  @override
  bool shouldRepaint(covariant CustomPainter oldDelegate) => false;
}

String money(num value) => '${NumberFormat('0.00').format(value)} KM';

String initials(String value) {
  final parts = value.trim().split(RegExp(r'\s+'));
  return parts.take(2).where((x) => x.isNotEmpty).map((x) => x[0]).join();
}
