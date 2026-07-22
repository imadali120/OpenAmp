import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_stripe/flutter_stripe.dart' hide Card;
import 'package:intl/intl.dart';
import 'package:openamp_mobile/core/config/app_config.dart';
import 'package:openamp_mobile/core/theme/app_theme.dart';
import 'package:openamp_mobile/models/models.dart';
import 'package:openamp_mobile/repositories/openamp_repository.dart';
import 'package:openamp_mobile/screens/booking/slot_selection_screen.dart';
import 'package:openamp_mobile/state/app_state.dart';
import 'package:openamp_mobile/widgets/common.dart';

class ReservationDetailsScreen extends ConsumerStatefulWidget {
  const ReservationDetailsScreen({super.key, required this.reservation});

  final Reservation reservation;

  @override
  ConsumerState<ReservationDetailsScreen> createState() =>
      _ReservationDetailsScreenState();
}

class _ReservationDetailsScreenState
    extends ConsumerState<ReservationDetailsScreen> {
  ReservationDetails? _details;
  bool _busy = false;
  String? _error;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    try {
      final details = await ref
          .read(repositoryProvider)
          .getReservation(widget.reservation.id);
      if (mounted) setState(() => _details = details);
    } catch (error) {
      if (mounted) setState(() => _error = error.toString());
    }
  }

  Future<void> _cancel() async {
    final details = _details;
    if (details == null) return;
    CancellationPreview preview;
    try {
      preview = await ref
          .read(repositoryProvider)
          .getCancellationPreview(details.id);
    } catch (error) {
      if (mounted) setState(() => _error = error.toString());
      return;
    }
    if (!mounted) return;
    final reason = TextEditingController();
    final accepted = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Otkazati rezervaciju?'),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Mogući povrat sada: ${money(preview.possibleRefund)}. '
              'Puni povrat vrijedi do ${preview.fullRefundHours} h prije termina, '
              'a ${preview.partialRefundPercent}% do ${preview.partialRefundHours} h prije termina.',
            ),
            const SizedBox(height: 14),
            TextField(
              controller: reason,
              maxLines: 3,
              decoration: const InputDecoration(
                labelText: 'Razlog (opcionalno)',
              ),
            ),
          ],
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: const Text('Zadrži termin'),
          ),
          FilledButton(
            onPressed: () => Navigator.pop(context, true),
            child: const Text('Otkaži'),
          ),
        ],
      ),
    );
    if (accepted != true) return;
    await _run(() async {
      final result = await ref
          .read(repositoryProvider)
          .cancelReservation(
            id: details.id,
            rowVersion: details.rowVersion,
            reason: reason.text.trim().isEmpty ? null : reason.text.trim(),
          );
      _details = result.reservation;
      await ref.read(appControllerProvider.notifier).reloadReservations();
      if (!mounted) return;
      await showDialog<void>(
        context: context,
        builder: (context) => AlertDialog(
          icon: const Icon(Icons.event_available_rounded, size: 48),
          title: const Text('Rezervacija je otkazana'),
          content: Text(
            result.refundedAmount > 0
                ? 'Stripe povrat: ${money(result.refundedAmount)}. Sredstva će biti vidljiva prema rokovima banke.'
                : 'Za ovaj termin nema povrata sredstava prema politici studija.',
            textAlign: TextAlign.center,
          ),
          actions: [
            FilledButton(
              onPressed: () => Navigator.pop(context),
              child: const Text('U redu'),
            ),
          ],
        ),
      );
    });
  }

  Future<void> _changeSlot() async {
    final details = _details;
    if (details == null) return;
    final date = await showDatePicker(
      context: context,
      initialDate: details.startsAt.toLocal().isAfter(DateTime.now())
          ? details.startsAt.toLocal()
          : DateTime.now().add(const Duration(days: 1)),
      firstDate: DateTime.now(),
      lastDate: DateTime.now().add(const Duration(days: 180)),
    );
    if (date == null || !mounted) return;
    await _run(() async {
      final duration = details.endsAt.difference(details.startsAt).inMinutes;
      final slots = await ref
          .read(repositoryProvider)
          .getAvailability(
            hallId: details.hallId,
            date: date,
            durationMinutes: duration,
          );
      if (!mounted) return;
      final selected = await showModalBottomSheet<DateTimeRangeValue>(
        context: context,
        showDragHandle: true,
        builder: (context) => SafeArea(
          child: ListView(
            padding: const EdgeInsets.fromLTRB(18, 4, 18, 24),
            children: [
              Text(
                'Odaberi novi termin',
                style: Theme.of(context).textTheme.titleLarge,
              ),
              const SizedBox(height: 10),
              if (slots.isEmpty)
                const Padding(
                  padding: EdgeInsets.symmetric(vertical: 28),
                  child: Text('Nema slobodnih termina za odabrani datum.'),
                )
              else
                ...slots.map(
                  (slot) => ListTile(
                    leading: const Icon(Icons.schedule_rounded),
                    title: Text(
                      '${DateFormat('HH:mm').format(slot.start.toLocal())} – ${DateFormat('HH:mm').format(slot.end.toLocal())}',
                    ),
                    onTap: () => Navigator.pop(context, slot),
                  ),
                ),
            ],
          ),
        ),
      );
      if (selected == null) return;
      _details = await ref
          .read(repositoryProvider)
          .updateReservation(
            id: details.id,
            startsAt: selected.start,
            endsAt: selected.end,
            rowVersion: details.rowVersion,
          );
      await ref.read(appControllerProvider.notifier).reloadReservations();
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Termin je uspješno promijenjen.')),
        );
      }
    });
  }

  Future<void> _review() async {
    var rating = 5;
    final comment = TextEditingController();
    final accepted = await showDialog<bool>(
      context: context,
      builder: (context) => StatefulBuilder(
        builder: (context, setDialogState) => AlertDialog(
          title: const Text('Ocijeni probu'),
          content: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: List.generate(
                  5,
                  (index) => IconButton(
                    onPressed: () => setDialogState(() => rating = index + 1),
                    icon: Icon(
                      index < rating
                          ? Icons.star_rounded
                          : Icons.star_border_rounded,
                      color: AppColors.warning,
                    ),
                  ),
                ),
              ),
              TextField(
                controller: comment,
                maxLines: 4,
                decoration: const InputDecoration(labelText: 'Komentar'),
              ),
            ],
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.pop(context, false),
              child: const Text('Kasnije'),
            ),
            FilledButton(
              onPressed: () => Navigator.pop(context, true),
              child: const Text('Objavi'),
            ),
          ],
        ),
      ),
    );
    if (accepted != true) return;
    await _run(() async {
      await ref
          .read(repositoryProvider)
          .createReview(
            reservationId: widget.reservation.id,
            rating: rating,
            comment: comment.text.trim().isEmpty ? null : comment.text.trim(),
          );
      await ref.read(appControllerProvider.notifier).reloadReservations();
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Hvala! Recenzija je objavljena.')),
        );
        Navigator.pop(context);
      }
    });
  }

  Future<void> _pay() async {
    final details = _details;
    if (details == null) return;
    if (AppConfig.stripePublishableKey.isEmpty) {
      setState(
        () => _error = 'Stripe testni publishable key nije konfigurisan.',
      );
      return;
    }
    await _run(() async {
      final repository = ref.read(repositoryProvider);
      final intent = await repository.createPaymentIntent(details.id);
      await Stripe.instance.initPaymentSheet(
        paymentSheetParameters: SetupPaymentSheetParameters(
          paymentIntentClientSecret: intent.clientSecret,
          customerId: intent.customerId,
          customerSessionClientSecret: intent.customerSessionClientSecret,
          merchantDisplayName: 'OpenAmp',
          returnURL: 'openamp://redirect',
          style: ThemeMode.system,
        ),
      );
      await Stripe.instance.presentPaymentSheet();
      for (var attempt = 0; attempt < 8; attempt++) {
        _details = await repository.getReservation(details.id);
        if (!_details!.status.toLowerCase().contains('čekanju')) break;
        await Future<void>.delayed(const Duration(seconds: 1));
      }
      await ref.read(appControllerProvider.notifier).reloadReservations();
      if (mounted) setState(() {});
    });
  }

  Future<void> _bookAgain() async {
    await _run(() async {
      final hall = await ref
          .read(repositoryProvider)
          .getHall(widget.reservation.hallId);
      if (!mounted) return;
      await Navigator.of(context).push(
        MaterialPageRoute<void>(
          builder: (_) => SlotSelectionScreen(hall: hall),
        ),
      );
    });
  }

  Future<void> _run(Future<void> Function() action) async {
    if (_busy) return;
    setState(() {
      _busy = true;
      _error = null;
    });
    try {
      await action();
    } on StripeException catch (error) {
      if (error.error.code != FailureCode.Canceled && mounted) {
        setState(() => _error = error.error.localizedMessage);
      }
    } catch (error) {
      if (mounted) setState(() => _error = error.toString());
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final details = _details;
    final statusCode = details?.statusCode ?? widget.reservation.statusCode;
    final canEdit = statusCode == 'NA_CEKANJU' && widget.reservation.canCancel;
    return Scaffold(
      appBar: AppBar(title: Text('Rezervacija #${widget.reservation.id}')),
      body: details == null && _error == null
          ? const Center(child: CircularProgressIndicator())
          : ListView(
              padding: const EdgeInsets.fromLTRB(18, 8, 18, 30),
              children: [
                if (_error != null) ...[
                  ErrorBanner(message: _error!),
                  const SizedBox(height: 12),
                ],
                if (details != null) ...[
                  Container(
                    padding: const EdgeInsets.all(18),
                    decoration: BoxDecoration(
                      color: AppColors.ink,
                      borderRadius: BorderRadius.circular(AppRadii.large),
                    ),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        const SectionEyebrow(
                          'Session ticket',
                          color: AppColors.signal,
                        ),
                        const SizedBox(height: 14),
                        Text(
                          details.hall,
                          style: const TextStyle(
                            color: Colors.white,
                            fontSize: 25,
                            fontWeight: FontWeight.w900,
                          ),
                        ),
                        const SizedBox(height: 5),
                        Text(
                          details.band,
                          style: const TextStyle(color: Colors.white70),
                        ),
                        const SizedBox(height: 16),
                        Text(
                          DateFormat(
                            'EEEE, dd.MM.yyyy. · HH:mm',
                            'bs',
                          ).format(details.startsAt.toLocal()),
                          style: const TextStyle(
                            color: Colors.white,
                            fontWeight: FontWeight.w800,
                          ),
                        ),
                        Text(
                          'do ${DateFormat('HH:mm').format(details.endsAt.toLocal())}',
                          style: const TextStyle(color: Colors.white60),
                        ),
                      ],
                    ),
                  ),
                  const SizedBox(height: 18),
                  Row(
                    children: [
                      Expanded(
                        child: Text(
                          'Stavke',
                          style: Theme.of(context).textTheme.titleLarge,
                        ),
                      ),
                      Text(
                        money(details.total),
                        style: const TextStyle(
                          fontSize: 20,
                          fontWeight: FontWeight.w900,
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 8),
                  _Line(
                    label: 'Najam sale',
                    value:
                        details.total -
                        details.items.fold(0, (sum, item) => sum + item.total),
                  ),
                  ...details.items.map(
                    (item) => _Line(
                      label: '${item.name} × ${item.quantity}',
                      value: item.total,
                    ),
                  ),
                  const SizedBox(height: 22),
                  if (statusCode == 'NA_CEKANJU')
                    FilledButton.icon(
                      onPressed: _busy ? null : _pay,
                      icon: const Icon(Icons.credit_card_rounded),
                      label: const Text('Nastavi plaćanje'),
                    ),
                  if (canEdit) ...[
                    const SizedBox(height: 8),
                    OutlinedButton.icon(
                      onPressed: _busy ? null : _changeSlot,
                      icon: const Icon(Icons.edit_calendar_outlined),
                      label: const Text('Promijeni termin'),
                    ),
                  ],
                  if (widget.reservation.canCancel) ...[
                    const SizedBox(height: 8),
                    OutlinedButton.icon(
                      onPressed: _busy ? null : _cancel,
                      icon: const Icon(Icons.event_busy_outlined),
                      label: const Text('Otkaži rezervaciju'),
                    ),
                  ],
                  if (widget.reservation.canReview) ...[
                    const SizedBox(height: 8),
                    FilledButton.tonalIcon(
                      onPressed: _busy ? null : _review,
                      icon: const Icon(Icons.star_outline_rounded),
                      label: const Text('Ostavi recenziju'),
                    ),
                  ],
                  const SizedBox(height: 8),
                  TextButton.icon(
                    onPressed: _busy ? null : _bookAgain,
                    icon: const Icon(Icons.replay_rounded),
                    label: const Text('Rezerviši ovu salu ponovo'),
                  ),
                ],
              ],
            ),
    );
  }
}

class _Line extends StatelessWidget {
  const _Line({required this.label, required this.value});
  final String label;
  final double value;

  @override
  Widget build(BuildContext context) => Padding(
    padding: const EdgeInsets.symmetric(vertical: 8),
    child: Row(
      children: [
        Expanded(child: Text(label)),
        Text(money(value), style: const TextStyle(fontWeight: FontWeight.w800)),
      ],
    ),
  );
}
