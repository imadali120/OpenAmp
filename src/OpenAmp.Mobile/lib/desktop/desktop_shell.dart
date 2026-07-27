import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:openamp_mobile/core/theme/app_theme.dart';
import 'package:openamp_mobile/desktop/admin_repository.dart';
import 'package:openamp_mobile/desktop/desktop_pages.dart';
import 'package:openamp_mobile/state/app_state.dart';
import 'package:openamp_mobile/widgets/common.dart';

final adminRepositoryProvider = Provider<AdminRepository>(
  (ref) => AdminRepository(ref.read(apiClientProvider)),
);

class DesktopShell extends ConsumerStatefulWidget {
  const DesktopShell({super.key});

  @override
  ConsumerState<DesktopShell> createState() => _DesktopShellState();
}

class _DesktopShellState extends ConsumerState<DesktopShell> {
  int selected = 0;

  static const _allDestinations = <(IconData, String)>[
    (Icons.grid_view_rounded, 'Pregled'),
    (Icons.meeting_room_outlined, 'Sale'),
    (Icons.speaker_group_outlined, 'Oprema'),
    (Icons.calendar_month_outlined, 'Rezervacije'),
    (Icons.groups_2_outlined, 'Bendovi'),
    (Icons.inventory_2_outlined, 'Artikli'),
    (Icons.people_alt_outlined, 'Korisnici'),
    (Icons.query_stats_outlined, 'Izvještaji'),
    (Icons.tune_outlined, 'Šifarnici'),
  ];

  @override
  Widget build(BuildContext context) {
    final app = ref.watch(appControllerProvider);
    final repository = ref.read(adminRepositoryProvider);
    final isAdmin =
        app.session!.role.toUpperCase() == 'ADMIN' ||
        app.session!.role.toUpperCase() == 'ADMINISTRATOR';
    final allPages = <Widget>[
      DashboardPage(repository: repository),
      HallsPage(repository: repository),
      EquipmentPage(repository: repository),
      ReservationsPage(repository: repository),
      BandsPage(repository: repository),
      ArticlesPage(repository: repository),
      UsersPage(repository: repository),
      ReportsPage(repository: repository),
      ReferenceDataPage(repository: repository),
    ];
    final allowedIndexes = isAdmin
        ? List<int>.generate(_allDestinations.length, (index) => index)
        : <int>[0, 1, 2, 3, 4, 5, 7];
    final destinations = allowedIndexes
        .map((index) => _allDestinations[index])
        .toList(growable: false);
    final pages = allowedIndexes
        .map((index) => allPages[index])
        .toList(growable: false);

    return Scaffold(
      body: Row(
        children: [
          Container(
            width: 230,
            decoration: const BoxDecoration(
              color: Color(0xFF0A0B0E),
              border: Border(right: BorderSide(color: AppColors.line)),
            ),
            child: Column(
              children: [
                const Padding(
                  padding: EdgeInsets.fromLTRB(24, 28, 24, 30),
                  child: Align(
                    alignment: Alignment.centerLeft,
                    child: OpenAmpLogo(compact: true, onDark: true),
                  ),
                ),
                Expanded(
                  child: ListView.separated(
                    padding: const EdgeInsets.symmetric(horizontal: 12),
                    itemCount: destinations.length,
                    separatorBuilder: (_, _) => const SizedBox(height: 4),
                    itemBuilder: (context, index) {
                      final item = destinations[index];
                      final active = selected == index;
                      return Material(
                        color: active
                            ? AppColors.primarySoft
                            : Colors.transparent,
                        borderRadius: BorderRadius.circular(10),
                        child: InkWell(
                          borderRadius: BorderRadius.circular(10),
                          onTap: () => setState(() => selected = index),
                          child: Padding(
                            padding: const EdgeInsets.symmetric(
                              horizontal: 14,
                              vertical: 12,
                            ),
                            child: Row(
                              children: [
                                Icon(
                                  item.$1,
                                  size: 20,
                                  color: active
                                      ? AppColors.primary
                                      : AppColors.textMuted,
                                ),
                                const SizedBox(width: 12),
                                Text(
                                  item.$2,
                                  style: TextStyle(
                                    color: active
                                        ? AppColors.text
                                        : AppColors.textMuted,
                                    fontWeight: active
                                        ? FontWeight.w800
                                        : FontWeight.w600,
                                  ),
                                ),
                              ],
                            ),
                          ),
                        ),
                      );
                    },
                  ),
                ),
                const Divider(height: 1),
                Padding(
                  padding: const EdgeInsets.all(16),
                  child: Row(
                    children: [
                      CircleAvatar(
                        radius: 18,
                        backgroundColor: AppColors.paperMuted,
                        child: Text(
                          '${app.session!.firstName[0]}${app.session!.lastName[0]}',
                          style: const TextStyle(
                            color: AppColors.text,
                            fontSize: 12,
                            fontWeight: FontWeight.w900,
                          ),
                        ),
                      ),
                      const SizedBox(width: 10),
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              '${app.session!.firstName} ${app.session!.lastName}',
                              maxLines: 1,
                              overflow: TextOverflow.ellipsis,
                              style: const TextStyle(
                                fontSize: 13,
                                fontWeight: FontWeight.w800,
                              ),
                            ),
                            Text(
                              app.session!.role,
                              style: const TextStyle(
                                color: AppColors.textMuted,
                                fontSize: 11,
                              ),
                            ),
                          ],
                        ),
                      ),
                      IconButton(
                        tooltip: 'Odjava',
                        onPressed: () =>
                            ref.read(appControllerProvider.notifier).logout(),
                        icon: const Icon(Icons.logout, size: 19),
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ),
          Expanded(
            child: ColoredBox(
              color: AppColors.canvas,
              child: IndexedStack(index: selected, children: pages),
            ),
          ),
        ],
      ),
    );
  }
}
