import 'package:flutter/material.dart';
import 'package:openamp_mobile/core/theme/app_theme.dart';
import 'package:openamp_mobile/screens/bands/bands_screen.dart';
import 'package:openamp_mobile/screens/halls/hall_search_screen.dart';
import 'package:openamp_mobile/screens/profile/profile_screen.dart';
import 'package:openamp_mobile/screens/reservations/reservations_screen.dart';

class MainShell extends StatefulWidget {
  const MainShell({super.key});

  @override
  State<MainShell> createState() => _MainShellState();
}

class _MainShellState extends State<MainShell> {
  int _index = 0;

  static const _screens = [
    HallSearchScreen(),
    ReservationsScreen(),
    BandsScreen(),
    ProfileScreen(),
  ];

  static const _items = [
    (Icons.meeting_room_outlined, Icons.meeting_room_rounded, 'Sale'),
    (Icons.calendar_today_outlined, Icons.calendar_month_rounded, 'Termini'),
    (Icons.groups_outlined, Icons.groups_rounded, 'Bendovi'),
    (Icons.person_outline_rounded, Icons.person_rounded, 'Profil'),
  ];

  @override
  Widget build(BuildContext context) => Scaffold(
    body: IndexedStack(index: _index, children: _screens),
    bottomNavigationBar: Container(
      decoration: const BoxDecoration(
        color: AppColors.ink,
        border: Border(top: BorderSide(color: Color(0xFF38313E))),
      ),
      child: SafeArea(
        top: false,
        minimum: const EdgeInsets.fromLTRB(8, 5, 8, 7),
        child: Row(
          children: List.generate(_items.length, (index) {
            final item = _items[index];
            final selected = index == _index;
            return Expanded(
              child: Semantics(
                selected: selected,
                button: true,
                label: item.$3,
                child: InkWell(
                  borderRadius: BorderRadius.circular(10),
                  onTap: () => setState(() => _index = index),
                  child: Padding(
                    padding: const EdgeInsets.symmetric(vertical: 6),
                    child: Column(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        AnimatedContainer(
                          duration: const Duration(milliseconds: 170),
                          width: selected ? 28 : 5,
                          height: 3,
                          decoration: BoxDecoration(
                            color: selected
                                ? AppColors.signal
                                : Colors.transparent,
                            borderRadius: BorderRadius.circular(3),
                          ),
                        ),
                        const SizedBox(height: 5),
                        Icon(
                          selected ? item.$2 : item.$1,
                          size: 21,
                          color: selected ? Colors.white : Colors.white54,
                        ),
                        const SizedBox(height: 3),
                        Text(
                          item.$3.toUpperCase(),
                          style: TextStyle(
                            color: selected ? Colors.white : Colors.white54,
                            fontSize: 9,
                            fontWeight: FontWeight.w900,
                            letterSpacing: .75,
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
              ),
            );
          }),
        ),
      ),
    ),
  );
}
