import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:openamp_mobile/core/theme/app_theme.dart';
import 'package:openamp_mobile/models/models.dart';
import 'package:openamp_mobile/screens/halls/hall_details_screen.dart';
import 'package:openamp_mobile/state/app_state.dart';
import 'package:openamp_mobile/widgets/common.dart';

class HallSearchScreen extends ConsumerStatefulWidget {
  const HallSearchScreen({super.key});

  @override
  ConsumerState<HallSearchScreen> createState() => _HallSearchScreenState();
}

class _HallSearchScreenState extends ConsumerState<HallSearchScreen> {
  final _search = TextEditingController();
  String? _genre;
  String? _equipmentCategory;
  int? _capacity;
  bool _favoritesOnly = false;

  @override
  void dispose() {
    _search.dispose();
    super.dispose();
  }

  Future<void> _runSearch() => ref
      .read(appControllerProvider.notifier)
      .loadCatalog(
        SearchFilters(
          text: _search.text.trim(),
          genreCode: _genre,
          minimumCapacity: _capacity,
          equipmentCategoryCode: _equipmentCategory,
        ),
      );

  @override
  Widget build(BuildContext context) {
    final state = ref.watch(appControllerProvider);
    final lookups = state.lookups;
    final visibleHalls = _favoritesOnly
        ? state.halls
              .where((hall) => state.favoriteHallIds.contains(hall.id))
              .toList()
        : state.halls;
    return Scaffold(
      body: SafeArea(
        bottom: false,
        child: RefreshIndicator(
          color: AppColors.signal,
          onRefresh: _runSearch,
          child: CustomScrollView(
            physics: const AlwaysScrollableScrollPhysics(),
            slivers: [
              SliverToBoxAdapter(
                child: Container(
                  padding: const EdgeInsets.fromLTRB(18, 17, 18, 24),
                  decoration: const BoxDecoration(
                    color: AppColors.ink,
                    borderRadius: BorderRadius.only(
                      bottomLeft: Radius.circular(24),
                      bottomRight: Radius.circular(24),
                    ),
                  ),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Row(
                        children: [
                          const OpenAmpLogo(compact: true, onDark: true),
                          const Spacer(),
                          IconButton.filledTonal(
                            tooltip: 'Osvježi',
                            onPressed: state.busy ? null : _runSearch,
                            style: IconButton.styleFrom(
                              backgroundColor: Colors.white10,
                              foregroundColor: Colors.white,
                            ),
                            icon: const Icon(Icons.refresh_rounded),
                          ),
                        ],
                      ),
                      const SizedBox(height: 25),
                      const SectionEyebrow(
                        'Live room finder',
                        color: AppColors.signal,
                      ),
                      const SizedBox(height: 9),
                      Text(
                        'Šta sviramo\nvečeras?',
                        style: Theme.of(
                          context,
                        ).textTheme.displaySmall?.copyWith(color: Colors.white),
                      ),
                      const SizedBox(height: 17),
                      TextField(
                        controller: _search,
                        textInputAction: TextInputAction.search,
                        onSubmitted: (_) => _runSearch(),
                        style: const TextStyle(color: AppColors.ink),
                        decoration: InputDecoration(
                          hintText: 'Sala, studio ili grad',
                          prefixIcon: const Icon(Icons.search_rounded),
                          suffixIcon: IconButton(
                            tooltip: 'Pretraži',
                            onPressed: _runSearch,
                            icon: const Icon(Icons.arrow_forward_rounded),
                          ),
                        ),
                      ),
                    ],
                  ),
                ),
              ),
              SliverPadding(
                padding: const EdgeInsets.fromLTRB(18, 18, 18, 12),
                sliver: SliverToBoxAdapter(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      const SectionEyebrow('Brzi filteri'),
                      const SizedBox(height: 11),
                      if (lookups != null)
                        SingleChildScrollView(
                          scrollDirection: Axis.horizontal,
                          child: Row(
                            children: [
                              ...lookups.genres.map(
                                (genre) => Padding(
                                  padding: const EdgeInsets.only(right: 8),
                                  child: _FilterPill(
                                    label: genre.name,
                                    selected: _genre == genre.code,
                                    onTap: () {
                                      setState(
                                        () => _genre == genre.code
                                            ? _genre = null
                                            : _genre = genre.code,
                                      );
                                      _runSearch();
                                    },
                                  ),
                                ),
                              ),
                            ],
                          ),
                        ),
                      const SizedBox(height: 8),
                      SingleChildScrollView(
                        scrollDirection: Axis.horizontal,
                        child: Row(
                          children: [
                            _FilterPill(
                              icon: Icons.groups_2_outlined,
                              label: '4+ člana',
                              selected: _capacity == 4,
                              onTap: () {
                                setState(
                                  () => _capacity = _capacity == 4 ? null : 4,
                                );
                                _runSearch();
                              },
                            ),
                            const SizedBox(width: 8),
                            _FilterPill(
                              icon: Icons.favorite_outline_rounded,
                              label: 'Sačuvane',
                              selected: _favoritesOnly,
                              onTap: () => setState(
                                () => _favoritesOnly = !_favoritesOnly,
                              ),
                            ),
                            if (lookups != null)
                              ...lookups.equipmentCategories
                                  .take(4)
                                  .map(
                                    (category) => Padding(
                                      padding: const EdgeInsets.only(left: 8),
                                      child: _FilterPill(
                                        label: category.name,
                                        selected:
                                            _equipmentCategory == category.code,
                                        onTap: () {
                                          setState(
                                            () =>
                                                _equipmentCategory ==
                                                    category.code
                                                ? _equipmentCategory = null
                                                : _equipmentCategory =
                                                      category.code,
                                          );
                                          _runSearch();
                                        },
                                      ),
                                    ),
                                  ),
                          ],
                        ),
                      ),
                      const SizedBox(height: 24),
                      Row(
                        crossAxisAlignment: CrossAxisAlignment.end,
                        children: [
                          Expanded(
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                const SectionEyebrow('Aktivni prostori'),
                                const SizedBox(height: 7),
                                Text(
                                  _genre == null
                                      ? 'Spremno za probu'
                                      : 'U tvom žanru',
                                  style: Theme.of(context).textTheme.titleLarge,
                                ),
                              ],
                            ),
                          ),
                          Text(
                            "${visibleHalls.length.toString().padLeft(2, '0')} ROOMS",
                            style: const TextStyle(
                              color: AppColors.primary,
                              fontSize: 11,
                              fontWeight: FontWeight.w900,
                              letterSpacing: 1,
                            ),
                          ),
                        ],
                      ),
                    ],
                  ),
                ),
              ),
              if (state.error != null)
                SliverPadding(
                  padding: const EdgeInsets.symmetric(horizontal: 18),
                  sliver: SliverToBoxAdapter(
                    child: ErrorBanner(
                      message: state.error!,
                      onRetry: _runSearch,
                    ),
                  ),
                ),
              if (state.busy && state.halls.isEmpty)
                const SliverFillRemaining(
                  hasScrollBody: false,
                  child: Center(child: CircularProgressIndicator()),
                )
              else if (visibleHalls.isEmpty)
                const SliverFillRemaining(
                  hasScrollBody: false,
                  child: Center(child: Text('Nema sala za ove filtere.')),
                )
              else
                SliverPadding(
                  padding: const EdgeInsets.fromLTRB(18, 0, 18, 26),
                  sliver: SliverList.separated(
                    itemCount: visibleHalls.length,
                    separatorBuilder: (_, _) => const SizedBox(height: 13),
                    itemBuilder: (context, index) =>
                        _HallCard(hall: visibleHalls[index], number: index + 1),
                  ),
                ),
            ],
          ),
        ),
      ),
    );
  }
}

class _FilterPill extends StatelessWidget {
  const _FilterPill({
    required this.label,
    required this.selected,
    required this.onTap,
    this.icon,
  });

  final String label;
  final bool selected;
  final VoidCallback onTap;
  final IconData? icon;

  @override
  Widget build(BuildContext context) => InkWell(
    borderRadius: BorderRadius.circular(AppRadii.small),
    onTap: onTap,
    child: AnimatedContainer(
      duration: const Duration(milliseconds: 160),
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 9),
      decoration: BoxDecoration(
        color: selected ? AppColors.ink : AppColors.paper,
        borderRadius: BorderRadius.circular(AppRadii.small),
        border: Border.all(color: selected ? AppColors.ink : AppColors.line),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          if (icon != null) ...[
            Icon(
              icon,
              size: 16,
              color: selected ? AppColors.signal : AppColors.textMuted,
            ),
            const SizedBox(width: 6),
          ],
          Text(
            label,
            style: TextStyle(
              color: selected ? Colors.white : AppColors.ink,
              fontSize: 12,
              fontWeight: FontWeight.w800,
            ),
          ),
        ],
      ),
    ),
  );
}

class _HallCard extends StatelessWidget {
  const _HallCard({required this.hall, required this.number});

  final HallSummary hall;
  final int number;

  @override
  Widget build(BuildContext context) => Card(
    clipBehavior: Clip.antiAlias,
    child: InkWell(
      onTap: () => Navigator.of(context).push(
        MaterialPageRoute<void>(
          builder: (_) => HallDetailsScreen(hallId: hall.id),
        ),
      ),
      child: Stack(
        children: [
          Padding(
            padding: const EdgeInsets.all(11),
            child: Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Stack(
                  children: [
                    HallImage(
                      url: hall.imageUrl,
                      width: 112,
                      height: 142,
                      borderRadius: 12,
                    ),
                    Positioned(
                      left: 8,
                      top: 8,
                      child: Container(
                        padding: const EdgeInsets.symmetric(
                          horizontal: 7,
                          vertical: 4,
                        ),
                        decoration: BoxDecoration(
                          color: AppColors.paper,
                          borderRadius: BorderRadius.circular(5),
                        ),
                        child: Text(
                          "#${number.toString().padLeft(2, '0')}",
                          style: const TextStyle(
                            color: AppColors.ink,
                            fontSize: 9,
                            fontWeight: FontWeight.w900,
                            letterSpacing: .7,
                          ),
                        ),
                      ),
                    ),
                  ],
                ),
                const SizedBox(width: 13),
                Expanded(
                  child: SizedBox(
                    height: 142,
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          '${hall.studio} / ${hall.city}'.toUpperCase(),
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                          style: const TextStyle(
                            color: AppColors.primary,
                            fontSize: 9,
                            fontWeight: FontWeight.w900,
                            letterSpacing: .9,
                          ),
                        ),
                        const SizedBox(height: 5),
                        Text(
                          hall.name,
                          maxLines: 2,
                          overflow: TextOverflow.ellipsis,
                          style: Theme.of(context).textTheme.titleMedium,
                        ),
                        const SizedBox(height: 6),
                        Text(
                          hall.equipment.isEmpty
                              ? 'Oprema dostupna na upit'
                              : hall.equipment.take(3).join(' · '),
                          maxLines: 2,
                          overflow: TextOverflow.ellipsis,
                          style: const TextStyle(fontSize: 11, height: 1.35),
                        ),
                        const Spacer(),
                        Row(
                          children: [
                            const Icon(
                              Icons.star_rounded,
                              color: AppColors.warning,
                              size: 16,
                            ),
                            Text(
                              hall.reviewCount == 0
                                  ? ' Novo'
                                  : ' ${hall.rating.toStringAsFixed(1)}',
                              style: const TextStyle(
                                fontSize: 11,
                                fontWeight: FontWeight.w800,
                              ),
                            ),
                            const SizedBox(width: 9),
                            const Icon(Icons.groups_2_outlined, size: 15),
                            Text(
                              ' ${hall.capacity}',
                              style: const TextStyle(fontSize: 11),
                            ),
                          ],
                        ),
                        const SizedBox(height: 7),
                        Row(
                          children: [
                            Expanded(
                              child: Text.rich(
                                TextSpan(
                                  children: [
                                    TextSpan(text: money(hall.hourlyPrice)),
                                    const TextSpan(
                                      text: ' / h',
                                      style: TextStyle(
                                        fontSize: 11,
                                        color: AppColors.textMuted,
                                      ),
                                    ),
                                  ],
                                ),
                                style: const TextStyle(
                                  color: AppColors.ink,
                                  fontSize: 17,
                                  fontWeight: FontWeight.w900,
                                  letterSpacing: -.4,
                                ),
                              ),
                            ),
                            StatusPill(
                              label: hall.available ? 'Slobodno' : 'Zauzeto',
                              positive: hall.available,
                            ),
                          ],
                        ),
                      ],
                    ),
                  ),
                ),
              ],
            ),
          ),
          Positioned(
            right: 0,
            top: 0,
            child: Container(width: 4, height: 42, color: AppColors.signal),
          ),
        ],
      ),
    ),
  );
}
