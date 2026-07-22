import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_stripe/flutter_stripe.dart' hide Card;
import 'package:intl/intl.dart';
import 'package:openamp_mobile/core/config/app_config.dart';
import 'package:openamp_mobile/core/theme/app_theme.dart';
import 'package:openamp_mobile/models/models.dart';
import 'package:openamp_mobile/state/app_state.dart';
import 'package:openamp_mobile/widgets/common.dart';

class CheckoutScreen extends ConsumerStatefulWidget {
  const CheckoutScreen({super.key, required this.draft});
  final BookingDraft draft;

  @override
  ConsumerState<CheckoutScreen> createState() => _CheckoutScreenState();
}

class _CheckoutScreenState extends ConsumerState<CheckoutScreen> {
  bool _processing = false;
  int? _reservationId;
  String? _error;

  Future<void> _pay() async {
    if (AppConfig.stripePublishableKey.isEmpty) {
      setState(
        () => _error =
            'Stripe publishable key nije konfigurisan. Pokrenite aplikaciju sa '
            '--dart-define=STRIPE_PUBLISHABLE_KEY=pk_test_...',
      );
      return;
    }
    setState(() {
      _processing = true;
      _error = null;
    });
    try {
      final repository = ref.read(repositoryProvider);
      _reservationId ??= await repository.createReservation(widget.draft);
      final intent = await repository.createPaymentIntent(_reservationId!);
      await Stripe.instance.initPaymentSheet(
        paymentSheetParameters: SetupPaymentSheetParameters(
          paymentIntentClientSecret: intent.clientSecret,
          merchantDisplayName: 'OpenAmp',
          style: ThemeMode.system,
          appearance: const PaymentSheetAppearance(
            colors: PaymentSheetAppearanceColors(primary: AppColors.primary),
            shapes: PaymentSheetShape(borderRadius: 16),
          ),
        ),
      );
      await Stripe.instance.presentPaymentSheet();
      await ref.read(appControllerProvider.notifier).refreshPrivateData();
      if (!mounted) return;
      await showDialog<void>(
        context: context,
        barrierDismissible: false,
        builder: (context) => AlertDialog(
          icon: const Icon(
            Icons.check_circle,
            color: AppColors.success,
            size: 58,
          ),
          title: const Text('Rezervacija je plaćena'),
          content: const Text(
            'Potvrda će se pojaviti u historiji nakon Stripe webhooka.',
            textAlign: TextAlign.center,
          ),
          actions: [
            FilledButton(
              onPressed: () => Navigator.of(context).pop(),
              child: const Text('Gotovo'),
            ),
          ],
        ),
      );
      if (mounted) Navigator.of(context).popUntil((route) => route.isFirst);
    } on StripeException catch (error) {
      if (error.error.code != FailureCode.Canceled) {
        setState(() => _error = error.error.localizedMessage);
      }
    } catch (error) {
      setState(() => _error = error.toString());
    } finally {
      if (mounted) setState(() => _processing = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final draft = widget.draft;
    return Scaffold(
      appBar: AppBar(title: const Text('Plaćanje')),
      body: ListView(
        padding: const EdgeInsets.fromLTRB(18, 8, 18, 110),
        children: [
          Container(
            padding: const EdgeInsets.all(20),
            decoration: BoxDecoration(
              gradient: const LinearGradient(
                colors: [AppColors.primaryDark, AppColors.primary],
              ),
              borderRadius: BorderRadius.circular(22),
            ),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const Text(
                  'REZERVACIJA PROBE',
                  style: TextStyle(
                    color: Colors.white70,
                    fontWeight: FontWeight.w700,
                    letterSpacing: 1.2,
                  ),
                ),
                const SizedBox(height: 14),
                Text(
                  draft.hall.name,
                  style: const TextStyle(
                    color: Colors.white,
                    fontSize: 25,
                    fontWeight: FontWeight.w900,
                  ),
                ),
                Text(
                  draft.hall.studio + ' · ' + draft.band!.name,
                  style: const TextStyle(color: Colors.white70),
                ),
                const SizedBox(height: 16),
                Text(
                  DateFormat(
                        'dd.MM.yyyy. HH:mm',
                      ).format(draft.startsAt!.toLocal()) +
                      ' – ' +
                      DateFormat('HH:mm').format(draft.endsAt!.toLocal()),
                  style: const TextStyle(
                    color: Colors.white,
                    fontWeight: FontWeight.w700,
                  ),
                ),
              ],
            ),
          ),
          const SizedBox(height: 20),
          Text('Pregled cijene', style: Theme.of(context).textTheme.titleLarge),
          const SizedBox(height: 10),
          Card(
            child: Padding(
              padding: const EdgeInsets.all(16),
              child: Column(
                children: [
                  _CheckoutLine(label: 'Sala', value: draft.hallTotal),
                  _CheckoutLine(
                    label: 'Dodatna oprema',
                    value: draft.equipmentTotal,
                  ),
                  _CheckoutLine(
                    label: 'Potrošni artikli',
                    value: draft.storeTotal,
                  ),
                  const Divider(height: 28),
                  Row(
                    children: [
                      Text(
                        'Ukupno',
                        style: Theme.of(context).textTheme.titleLarge,
                      ),
                      const Spacer(),
                      Text(
                        money(draft.total),
                        style: const TextStyle(
                          color: AppColors.primary,
                          fontSize: 23,
                          fontWeight: FontWeight.w900,
                        ),
                      ),
                    ],
                  ),
                ],
              ),
            ),
          ),
          const SizedBox(height: 14),
          const Card(
            child: ListTile(
              leading: Icon(Icons.verified_user_outlined),
              title: Text('Sigurno Stripe plaćanje'),
              subtitle: Text(
                'Podaci kartice se unose u Stripe PaymentSheet. '
                '3D Secure se otvara automatski kada je potreban.',
              ),
            ),
          ),
          if (_error != null) ...[
            const SizedBox(height: 14),
            ErrorBanner(message: _error!),
          ],
        ],
      ),
      bottomNavigationBar: SafeArea(
        minimum: const EdgeInsets.fromLTRB(18, 10, 18, 14),
        child: FilledButton.icon(
          onPressed: _processing ? null : _pay,
          icon: _processing
              ? const SizedBox(
                  width: 20,
                  height: 20,
                  child: CircularProgressIndicator(
                    strokeWidth: 2,
                    color: Colors.white,
                  ),
                )
              : const Icon(Icons.lock_outline),
          label: Text(
            _processing ? 'Obrada...' : 'Plati ' + money(draft.total),
          ),
        ),
      ),
    );
  }
}

class _CheckoutLine extends StatelessWidget {
  const _CheckoutLine({required this.label, required this.value});
  final String label;
  final double value;

  @override
  Widget build(BuildContext context) => Padding(
    padding: const EdgeInsets.symmetric(vertical: 6),
    child: Row(
      children: [
        Expanded(child: Text(label)),
        Text(money(value), style: const TextStyle(fontWeight: FontWeight.w700)),
      ],
    ),
  );
}
