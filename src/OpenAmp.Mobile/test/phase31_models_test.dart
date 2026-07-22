import 'package:flutter_test/flutter_test.dart';
import 'package:openamp_mobile/models/models.dart';

void main() {
  test('reservation parses phase 3.1 actions from API response', () {
    final reservation = Reservation.fromJson({
      'id': 9,
      'salaId': 2,
      'sala': 'Jazz Corner',
      'studio': 'OpenAmp Mostar',
      'bendId': 4,
      'bend': 'Blue Notes',
      'terminOdUtc': '2026-08-01T18:00:00Z',
      'terminDoUtc': '2026-08-01T20:00:00Z',
      'ukupnaCijena': 64.0,
      'status': 'Plaćena',
      'statusKod': 'PLACENA',
      'rowVersion': 'AQID',
      'slikaUrl': null,
      'mozeOtkazati': true,
      'mozeRecenzirati': false,
    });

    expect(reservation.hallId, 2);
    expect(reservation.bandId, 4);
    expect(reservation.statusCode, 'PLACENA');
    expect(reservation.canCancel, isTrue);
    expect(reservation.canReview, isFalse);
  });

  test('cancellation preview exposes studio refund policy', () {
    final preview = CancellationPreview.fromJson({
      'moguciPovrat': 32.0,
      'puniPovratDoSati': 24,
      'djelimicniPovratDoSati': 12,
      'djelimicniPovratPostotak': 50,
    });

    expect(preview.possibleRefund, 32.0);
    expect(preview.fullRefundHours, 24);
    expect(preview.partialRefundPercent, 50);
  });

  test('received band invitation reports pending status', () {
    final invitation = ReceivedBandInvitation.fromJson({
      'id': 3,
      'bendId': 7,
      'bend': 'Open Chords',
      'zanr': 'Rock',
      'pozvao': 'Imad Ali',
      'kod': 'ABC123',
      'status': 'Na čekanju',
      'kreiranaUtc': '2026-07-22T12:00:00Z',
      'isticeUtc': '2026-07-29T12:00:00Z',
    });

    expect(invitation.pending, isTrue);
  });
}
