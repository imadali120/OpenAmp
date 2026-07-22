import 'package:flutter_test/flutter_test.dart';
import 'package:openamp_mobile/models/models.dart';

void main() {
  test('booking draft calculates hall, equipment and store totals', () {
    const hall = HallDetails(
      id: 1,
      name: 'Marshall Room',
      studio: 'OpenAmp Mostar',
      city: 'Mostar',
      address: 'Test 1',
      capacity: 6,
      hourlyPrice: 30,
      description: null,
      acoustics: null,
      latitude: null,
      longitude: null,
      rating: 0,
      reviewCount: 0,
      gallery: [],
      equipment: [
        EquipmentItem(
          id: 10,
          name: 'Mikrofon',
          category: 'Mikrofon',
          description: null,
          hourlyPrice: 5,
          available: true,
        ),
      ],
      storeItems: [
        StoreItem(
          id: 20,
          name: 'Trzalica',
          category: 'Dodaci',
          description: null,
          price: 2,
          stock: 10,
        ),
      ],
      reviews: [],
    );
    final draft = BookingDraft(
      hall: hall,
      startsAt: DateTime.utc(2026, 8, 1, 18),
      endsAt: DateTime.utc(2026, 8, 1, 21),
      equipmentQuantities: const {10: 1},
      storeItemQuantities: const {20: 2},
    );

    expect(draft.durationHours, 3);
    expect(draft.hallTotal, 90);
    expect(draft.equipmentTotal, 15);
    expect(draft.storeTotal, 4);
    expect(draft.total, 109);
  });
}
