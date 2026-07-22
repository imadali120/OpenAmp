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
    return Scaffold(
      appBar: AppBar(
        title: const OpenAmpLogo(compact: true),
        actions: [
          IconButton(
            tooltip: 'Osvježi',
            onPressed: state.busy ? null : _runSearch,
            icon: const Icon(Icons.refresh_rounded),
          ),
          const SizedBox(width: 8),
        ],
      ),
      body: RefreshIndicator(
        onRefresh: _runSearch,
        child: CustomScrollView(
          physics: const AlwaysScrollableScrollPhysics(),
          slivers: [
            SliverPadding(
              padding: const EdgeInsets.fromLTRB(18, 18, 18, 12),
              sliver: SliverToBoxAdapter(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'Pronađi prostor za sljedeću probu',
                      style: Theme.of(context).textTheme.headlineMedium,
                    ),
                    const SizedBox(height: 16),
                    TextField(
                      controller: _search,
                      textInputAction: TextInputAction.search,
                      onSubmitted: (_) => _runSearch(),
                      decoration: InputDecoration(
                        hintText: 'Naziv sale, studija ili grad...',
                        prefixIcon: const Icon(Icons.search),
                        suffixIcon: IconButton(
                          onPressed: _runSearch,
                          icon: const Icon(Icons.tune_rounded),
                        ),
                      ),
                    ),
                    const SizedBox(height: 14),
                    if (lookups != null)
                      SingleChildScrollView(
                        scrollDirection: Axis.horizontal,
                        child: Row(
                          children: lookups.genres
                              .map(
                                (genre) => Padding(
                                  padding: const EdgeInsets.only(right: 8),
                                  child: FilterChip(
                                    selected: _genre == genre.code,
                                    label: Text(genre.name),
                                    onSelected: (selected) {
                                      setState(
                                        () => _genre = selected
                                            ? genre.code
                                            : null,
                                      );
                                      _runSearch();
                                    },
                                  ),
                                ),
                              )
                              .toList(),
                        ),
                      ),
                    const SizedBox(height: 10),
                    Wrap(
                      spacing: 8,
                      runSpacing: 8,
                      children: [
                        ChoiceChip(
                          avatar: const Icon(Icons.groups_2_outlined, size: 18),
                          selected: _capacity == 4,
                          label: const Text('4+ člana'),
                          onSelected: (value) {
                            setState(() => _capacity = value ? 4 : null);
                            _runSearch();
                          },
                        ),
                        if (lookups != null)
                          ...lookups.equipmentCategories
                              .take(4)
                              .map(
                                (category) => ChoiceChip(
                                  selected: _equipmentCategory == category.code,
                                  label: Text(category.name),
                                  onSelected: (value) {
                                    setState(
                                      () => _equipmentCategory = value
                                          ? category.code
                                          : null,
                                    );
                                    _runSearch();
                                  },
                                ),
                              ),
                      ],
                    ),
                    const SizedBox(height: 22),
                    Row(
                      children: [
                        Expanded(
                          child: Text(
                            _genre == null
                                ? 'Preporučeno za vas'
                                : 'Sale za odabrani žanr',
                            style: Theme.of(context).textTheme.titleLarge,
                          ),
                        ),
                        Text(
                          state.halls.length.toString() + ' rezultata',
                          style: const TextStyle(
                            color: AppColors.textMuted,
                            fontWeight: FontWeight.w600,
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
            else if (state.halls.isEmpty)
              const SliverFillRemaining(
                hasScrollBody: false,
                child: Center(
                  child: Text('Nema sala koje odgovaraju filterima.'),
                ),
              )
            else
              SliverPadding(
                padding: const EdgeInsets.fromLTRB(18, 0, 18, 24),
                sliver: SliverList.separated(
                  itemCount: state.halls.length,
                  separatorBuilder: (_, _) => const SizedBox(height: 12),
                  itemBuilder: (context, index) =>
                      _HallCard(hall: state.halls[index]),
                ),
              ),
          ],
        ),
      ),
    );
  }
}

class _HallCard extends StatelessWidget {
  const _HallCard({required this.hall});
  final HallSummary hall;

  @override
  Widget build(BuildContext context) => Card(
    clipBehavior: Clip.antiAlias,
    child: InkWell(
      onTap: () => Navigator.of(context).push(
        MaterialPageRoute<void>(
          builder: (_) => HallDetailsScreen(hallId: hall.id),
        ),
      ),
      child: Padding(
        padding: const EdgeInsets.all(12),
        child: Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            HallImage(url: hall.imageUrl, width: 104, height: 118),
            const SizedBox(width: 14),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    hall.name,
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                    style: Theme.of(context).textTheme.titleMedium,
                  ),
                  const SizedBox(height: 4),
                  Text(
                    hall.studio + ' · ' + hall.city,
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                  ),
                  const SizedBox(height: 7),
                  Text(
                    hall.equipment.isEmpty
                        ? 'Oprema dostupna na upit'
                        : hall.equipment.join(', '),
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis,
                    style: const TextStyle(fontSize: 12),
                  ),
                  const SizedBox(height: 8),
                  Row(
                    children: [
                      Expanded(
                        child: Text(
                          money(hall.hourlyPrice) + '/h',
                          style: const TextStyle(
                            color: AppColors.primary,
                            fontSize: 18,
                            fontWeight: FontWeight.w900,
                          ),
                        ),
                      ),
                      StatusPill(
                        label: hall.available ? 'Slobodno' : 'Zauzeto',
                        positive: hall.available,
                      ),
                    ],
                  ),
                  const SizedBox(height: 5),
                  Row(
                    children: [
                      const Icon(
                        Icons.star_rounded,
                        color: AppColors.warning,
                        size: 17,
                      ),
                      Text(
                        hall.reviewCount == 0
                            ? ' Nova sala'
                            : ' ' +
                                  hall.rating.toStringAsFixed(1) +
                                  ' (' +
                                  hall.reviewCount.toString() +
                                  ')',
                        style: const TextStyle(fontSize: 12),
                      ),
                      const Spacer(),
                      const Icon(Icons.groups_2_outlined, size: 16),
                      Text(' do ' + hall.capacity.toString()),
                    ],
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    ),
  );
}
