class LookupItem {
  const LookupItem({required this.id, required this.code, required this.name});
  final int id;
  final String code;
  final String name;

  factory LookupItem.fromJson(Map<String, dynamic> json) => LookupItem(
    id: json['id'] as int,
    code: json['kod'] as String,
    name: json['naziv'] as String,
  );
}

class MobileLookups {
  const MobileLookups({
    required this.genres,
    required this.equipmentCategories,
    required this.instruments,
  });
  final List<LookupItem> genres;
  final List<LookupItem> equipmentCategories;
  final List<LookupItem> instruments;

  factory MobileLookups.fromJson(Map<String, dynamic> json) => MobileLookups(
    genres: _list(json['zanrovi'], LookupItem.fromJson),
    equipmentCategories: _list(json['kategorijeOpreme'], LookupItem.fromJson),
    instruments: _list(json['instrumenti'], LookupItem.fromJson),
  );
}

class HallSummary {
  const HallSummary({
    required this.id,
    required this.name,
    required this.studio,
    required this.city,
    required this.capacity,
    required this.hourlyPrice,
    required this.status,
    required this.imageUrl,
    required this.rating,
    required this.reviewCount,
    required this.equipment,
    required this.available,
  });
  final int id;
  final String name;
  final String studio;
  final String city;
  final int capacity;
  final double hourlyPrice;
  final String status;
  final String? imageUrl;
  final double rating;
  final int reviewCount;
  final List<String> equipment;
  final bool available;

  factory HallSummary.fromJson(Map<String, dynamic> json) => HallSummary(
    id: json['id'] as int,
    name: json['naziv'] as String,
    studio: json['studio'] as String,
    city: json['grad'] as String,
    capacity: json['kapacitet'] as int,
    hourlyPrice: (json['cijenaPoSatu'] as num).toDouble(),
    status: json['status'] as String,
    imageUrl: json['slikaUrl'] as String?,
    rating: (json['prosjecnaOcjena'] as num).toDouble(),
    reviewCount: json['brojRecenzija'] as int,
    equipment: List<String>.from(json['oprema'] as List),
    available: json['dostupna'] as bool,
  );
}

class EquipmentItem {
  const EquipmentItem({
    required this.id,
    required this.name,
    required this.category,
    required this.description,
    required this.hourlyPrice,
    required this.available,
  });
  final int id;
  final String name;
  final String category;
  final String? description;
  final double hourlyPrice;
  final bool available;

  factory EquipmentItem.fromJson(Map<String, dynamic> json) => EquipmentItem(
    id: json['id'] as int,
    name: json['naziv'] as String,
    category: json['kategorija'] as String,
    description: json['opis'] as String?,
    hourlyPrice: (json['cijenaPoSatu'] as num).toDouble(),
    available: json['dostupna'] as bool,
  );
}

class StoreItem {
  const StoreItem({
    required this.id,
    required this.name,
    required this.category,
    required this.description,
    required this.price,
    required this.stock,
  });
  final int id;
  final String name;
  final String category;
  final String? description;
  final double price;
  final int stock;

  factory StoreItem.fromJson(Map<String, dynamic> json) => StoreItem(
    id: json['id'] as int,
    name: json['naziv'] as String,
    category: json['kategorija'] as String,
    description: json['opis'] as String?,
    price: (json['cijena'] as num).toDouble(),
    stock: json['naStanju'] as int,
  );
}

class HallReview {
  const HallReview({
    required this.id,
    required this.rating,
    required this.comment,
    required this.user,
    required this.createdAt,
  });
  final int id;
  final int rating;
  final String? comment;
  final String user;
  final DateTime createdAt;

  factory HallReview.fromJson(Map<String, dynamic> json) => HallReview(
    id: json['id'] as int,
    rating: json['ocjena'] as int,
    comment: json['komentar'] as String?,
    user: json['korisnik'] as String,
    createdAt: DateTime.parse(json['kreiranaUtc'] as String).toUtc(),
  );
}

class HallDetails {
  const HallDetails({
    required this.id,
    required this.name,
    required this.studio,
    required this.city,
    required this.address,
    required this.capacity,
    required this.hourlyPrice,
    required this.description,
    required this.acoustics,
    required this.latitude,
    required this.longitude,
    required this.rating,
    required this.reviewCount,
    required this.gallery,
    required this.equipment,
    required this.storeItems,
    required this.reviews,
  });
  final int id;
  final String name;
  final String studio;
  final String city;
  final String address;
  final int capacity;
  final double hourlyPrice;
  final String? description;
  final String? acoustics;
  final double? latitude;
  final double? longitude;
  final double rating;
  final int reviewCount;
  final List<String> gallery;
  final List<EquipmentItem> equipment;
  final List<StoreItem> storeItems;
  final List<HallReview> reviews;

  factory HallDetails.fromJson(Map<String, dynamic> json) => HallDetails(
    id: json['id'] as int,
    name: json['naziv'] as String,
    studio: json['studio'] as String,
    city: json['grad'] as String,
    address: json['adresa'] as String,
    capacity: json['kapacitet'] as int,
    hourlyPrice: (json['cijenaPoSatu'] as num).toDouble(),
    description: json['opis'] as String?,
    acoustics: json['akustika'] as String?,
    latitude: (json['geografskaSirina'] as num?)?.toDouble(),
    longitude: (json['geografskaDuzina'] as num?)?.toDouble(),
    rating: (json['prosjecnaOcjena'] as num).toDouble(),
    reviewCount: json['brojRecenzija'] as int,
    gallery: List<String>.from(json['galerija'] as List),
    equipment: _list(json['oprema'], EquipmentItem.fromJson),
    storeItems: _list(json['artikli'], StoreItem.fromJson),
    reviews: _list(json['recenzije'], HallReview.fromJson),
  );
}

class BandMember {
  const BandMember({
    required this.userId,
    required this.fullName,
    required this.instrument,
    required this.role,
    required this.isFounder,
  });
  final int userId;
  final String fullName;
  final String? instrument;
  final String? role;
  final bool isFounder;

  factory BandMember.fromJson(Map<String, dynamic> json) => BandMember(
    userId: json['korisnikId'] as int,
    fullName: json['imePrezime'] as String,
    instrument: json['instrument'] as String?,
    role: json['uloga'] as String?,
    isFounder: json['osnivac'] as bool,
  );
}

class BandInvitation {
  const BandInvitation({
    required this.id,
    required this.email,
    required this.code,
    required this.status,
    required this.expiresAt,
  });
  final int id;
  final String email;
  final String code;
  final String status;
  final DateTime expiresAt;

  factory BandInvitation.fromJson(Map<String, dynamic> json) => BandInvitation(
    id: json['id'] as int,
    email: json['email'] as String,
    code: json['kod'] as String,
    status: json['status'] as String,
    expiresAt: DateTime.parse(json['isticeUtc'] as String).toUtc(),
  );
}

class Band {
  const Band({
    required this.id,
    required this.name,
    required this.genre,
    required this.description,
    required this.imageUrl,
    required this.isFounder,
    required this.reservationCount,
    required this.members,
    required this.invitations,
  });
  final int id;
  final String name;
  final String genre;
  final String? description;
  final String? imageUrl;
  final bool isFounder;
  final int reservationCount;
  final List<BandMember> members;
  final List<BandInvitation> invitations;

  factory Band.fromJson(Map<String, dynamic> json) => Band(
    id: json['id'] as int,
    name: json['naziv'] as String,
    genre: json['zanr'] as String,
    description: json['opis'] as String?,
    imageUrl: json['fotografijaUrl'] as String?,
    isFounder: json['jeOsnivac'] as bool,
    reservationCount: json['brojRezervacija'] as int,
    members: _list(json['clanovi'], BandMember.fromJson),
    invitations: _list(json['pozivnice'], BandInvitation.fromJson),
  );
}

class Reservation {
  const Reservation({
    required this.id,
    required this.hall,
    required this.studio,
    required this.band,
    required this.startsAt,
    required this.endsAt,
    required this.total,
    required this.status,
    required this.rowVersion,
    required this.imageUrl,
  });
  final int id;
  final String hall;
  final String studio;
  final String band;
  final DateTime startsAt;
  final DateTime endsAt;
  final double total;
  final String status;
  final String rowVersion;
  final String? imageUrl;

  factory Reservation.fromJson(Map<String, dynamic> json) => Reservation(
    id: json['id'] as int,
    hall: json['sala'] as String,
    studio: json['studio'] as String,
    band: json['bend'] as String,
    startsAt: DateTime.parse(json['terminOdUtc'] as String).toUtc(),
    endsAt: DateTime.parse(json['terminDoUtc'] as String).toUtc(),
    total: (json['ukupnaCijena'] as num).toDouble(),
    status: json['status'] as String,
    rowVersion: json['rowVersion'] as String,
    imageUrl: json['slikaUrl'] as String?,
  );
}

class ProfileOverview {
  const ProfileOverview({
    required this.id,
    required this.firstName,
    required this.lastName,
    required this.email,
    required this.phone,
    required this.imageUrl,
    required this.instruments,
    required this.bandCount,
    required this.reservationCount,
    required this.totalHours,
    required this.reviewCount,
    required this.favoriteHall,
    required this.topGenre,
  });
  final int id;
  final String firstName;
  final String lastName;
  final String email;
  final String? phone;
  final String? imageUrl;
  final List<String> instruments;
  final int bandCount;
  final int reservationCount;
  final double totalHours;
  final int reviewCount;
  final String? favoriteHall;
  final String? topGenre;

  String get fullName => '$firstName $lastName';

  factory ProfileOverview.fromJson(Map<String, dynamic> json) =>
      ProfileOverview(
        id: json['id'] as int,
        firstName: json['ime'] as String,
        lastName: json['prezime'] as String,
        email: json['email'] as String,
        phone: json['telefon'] as String?,
        imageUrl: json['fotografijaUrl'] as String?,
        instruments: List<String>.from(json['instrumenti'] as List),
        bandCount: json['brojBendova'] as int,
        reservationCount: json['brojRezervacija'] as int,
        totalHours: (json['ukupnoSati'] as num).toDouble(),
        reviewCount: json['brojRecenzija'] as int,
        favoriteHall: json['omiljenaSala'] as String?,
        topGenre: json['najcesciZanr'] as String?,
      );
}

class SearchFilters {
  const SearchFilters({
    this.text,
    this.genreCode,
    this.minimumCapacity,
    this.equipmentCategoryCode,
    this.startsAt,
    this.endsAt,
  });
  final String? text;
  final String? genreCode;
  final int? minimumCapacity;
  final String? equipmentCategoryCode;
  final DateTime? startsAt;
  final DateTime? endsAt;
}

class BookingDraft {
  const BookingDraft({
    required this.hall,
    this.band,
    this.startsAt,
    this.endsAt,
    this.equipmentQuantities = const {},
    this.storeItemQuantities = const {},
  });
  final HallDetails hall;
  final Band? band;
  final DateTime? startsAt;
  final DateTime? endsAt;
  final Map<int, int> equipmentQuantities;
  final Map<int, int> storeItemQuantities;

  double get durationHours {
    if (startsAt == null || endsAt == null) return 0;
    return endsAt!.difference(startsAt!).inMinutes / 60;
  }

  double get hallTotal => hall.hourlyPrice * durationHours;
  double get equipmentTotal => equipmentQuantities.entries.fold(0, (sum, e) {
    final item = hall.equipment.firstWhere((x) => x.id == e.key);
    return sum + item.hourlyPrice * durationHours * e.value;
  });
  double get storeTotal => storeItemQuantities.entries.fold(0, (sum, e) {
    final item = hall.storeItems.firstWhere((x) => x.id == e.key);
    return sum + item.price * e.value;
  });
  double get total => hallTotal + equipmentTotal + storeTotal;

  BookingDraft copyWith({
    Band? band,
    DateTime? startsAt,
    DateTime? endsAt,
    Map<int, int>? equipmentQuantities,
    Map<int, int>? storeItemQuantities,
  }) => BookingDraft(
    hall: hall,
    band: band ?? this.band,
    startsAt: startsAt ?? this.startsAt,
    endsAt: endsAt ?? this.endsAt,
    equipmentQuantities: equipmentQuantities ?? this.equipmentQuantities,
    storeItemQuantities: storeItemQuantities ?? this.storeItemQuantities,
  );
}

List<T> _list<T>(Object? value, T Function(Map<String, dynamic>) fromJson) =>
    (value as List<dynamic>? ?? const [])
        .map((item) => fromJson(item as Map<String, dynamic>))
        .toList();
