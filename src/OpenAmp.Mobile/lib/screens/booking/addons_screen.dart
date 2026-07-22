import 'package:flutter/material.dart';
import 'package:openamp_mobile/core/theme/app_theme.dart';
import 'package:openamp_mobile/models/models.dart';
import 'package:openamp_mobile/screens/booking/checkout_screen.dart';
import 'package:openamp_mobile/widgets/common.dart';

class AddonsScreen extends StatefulWidget {
  const AddonsScreen({super.key, required this.initialDraft});
  final BookingDraft initialDraft;

  @override
  State<AddonsScreen> createState() => _AddonsScreenState();
}

class _AddonsScreenState extends State<AddonsScreen>
    with SingleTickerProviderStateMixin {
  late final TabController _tabs;
  late Map<int, int> _equipment;
  late Map<int, int> _storeItems;

  BookingDraft get _draft => widget.initialDraft.copyWith(
    equipmentQuantities: _equipment,
    storeItemQuantities: _storeItems,
  );

  @override
  void initState() {
    super.initState();
    _tabs = TabController(length: 3, vsync: this);
    _equipment = {...widget.initialDraft.equipmentQuantities};
    _storeItems = {...widget.initialDraft.storeItemQuantities};
  }

  @override
  void dispose() {
    _tabs.dispose();
    super.dispose();
  }

  void _equipmentToggle(EquipmentItem item) {
    setState(() {
      if (_equipment.containsKey(item.id)) {
        _equipment.remove(item.id);
      } else {
        _equipment[item.id] = 1;
      }
    });
  }

  void _changeStoreItem(StoreItem item, int delta) {
    setState(() {
      final value = (_storeItems[item.id] ?? 0) + delta;
      if (value <= 0) {
        _storeItems.remove(item.id);
      } else {
        _storeItems[item.id] = value.clamp(0, item.stock);
      }
    });
  }

  @override
  Widget build(BuildContext context) => Scaffold(
    appBar: AppBar(title: const Text('Dodatna oprema')),
    body: Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Padding(
          padding: const EdgeInsets.fromLTRB(18, 8, 18, 12),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const Text('Unajmi uz probu'),
              Text(
                widget.initialDraft.band!.name +
                    ' · ' +
                    widget.initialDraft.durationHours.toStringAsFixed(0) +
                    ' h',
                style: const TextStyle(
                  color: AppColors.primary,
                  fontWeight: FontWeight.w800,
                ),
              ),
              const SizedBox(height: 12),
              Container(
                decoration: BoxDecoration(
                  color: Colors.white,
                  borderRadius: BorderRadius.circular(16),
                ),
                child: TabBar(
                  controller: _tabs,
                  indicatorSize: TabBarIndicatorSize.tab,
                  dividerColor: Colors.transparent,
                  indicator: BoxDecoration(
                    color: AppColors.primary,
                    borderRadius: BorderRadius.circular(14),
                  ),
                  labelColor: Colors.white,
                  unselectedLabelColor: AppColors.textMuted,
                  tabs: const [
                    Tab(text: 'Oprema'),
                    Tab(text: 'Artikli'),
                    Tab(text: 'Sažetak'),
                  ],
                ),
              ),
            ],
          ),
        ),
        Expanded(
          child: TabBarView(
            controller: _tabs,
            children: [
              _EquipmentTab(
                items: widget.initialDraft.hall.equipment
                    .where((x) => x.available)
                    .toList(),
                quantities: _equipment,
                onToggle: _equipmentToggle,
              ),
              _StoreTab(
                items: widget.initialDraft.hall.storeItems,
                quantities: _storeItems,
                onChange: _changeStoreItem,
              ),
              _SummaryTab(draft: _draft),
            ],
          ),
        ),
        SafeArea(
          minimum: const EdgeInsets.fromLTRB(18, 10, 18, 14),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              Card(
                child: Padding(
                  padding: const EdgeInsets.all(14),
                  child: Row(
                    children: [
                      const Expanded(child: Text('Sala + oprema + artikli')),
                      Text(
                        money(_draft.total),
                        style: Theme.of(context).textTheme.titleLarge,
                      ),
                    ],
                  ),
                ),
              ),
              const SizedBox(height: 10),
              FilledButton(
                onPressed: () => Navigator.of(context).push(
                  MaterialPageRoute<void>(
                    builder: (_) => CheckoutScreen(draft: _draft),
                  ),
                ),
                child: const Text('Nastavi na plaćanje'),
              ),
            ],
          ),
        ),
      ],
    ),
  );
}

class _EquipmentTab extends StatelessWidget {
  const _EquipmentTab({
    required this.items,
    required this.quantities,
    required this.onToggle,
  });
  final List<EquipmentItem> items;
  final Map<int, int> quantities;
  final ValueChanged<EquipmentItem> onToggle;

  @override
  Widget build(BuildContext context) => ListView.separated(
    padding: const EdgeInsets.fromLTRB(18, 8, 18, 18),
    itemCount: items.length,
    separatorBuilder: (_, _) => const SizedBox(height: 10),
    itemBuilder: (_, index) {
      final item = items[index];
      final selected = quantities.containsKey(item.id);
      return Card(
        child: ListTile(
          leading: const CircleAvatar(
            backgroundColor: Color(0xFFECE9FF),
            child: Icon(Icons.music_note, color: AppColors.primary),
          ),
          title: Text(item.name),
          subtitle: Text(
            item.category + ' · ' + money(item.hourlyPrice) + '/h',
          ),
          trailing: selected
              ? IconButton.filled(
                  onPressed: () => onToggle(item),
                  icon: const Icon(Icons.check),
                )
              : FilledButton.tonal(
                  onPressed: () => onToggle(item),
                  child: const Text('Dodaj'),
                ),
        ),
      );
    },
  );
}

class _StoreTab extends StatelessWidget {
  const _StoreTab({
    required this.items,
    required this.quantities,
    required this.onChange,
  });
  final List<StoreItem> items;
  final Map<int, int> quantities;
  final void Function(StoreItem, int) onChange;

  @override
  Widget build(BuildContext context) => ListView.separated(
    padding: const EdgeInsets.fromLTRB(18, 8, 18, 18),
    itemCount: items.length,
    separatorBuilder: (_, _) => const SizedBox(height: 10),
    itemBuilder: (_, index) {
      final item = items[index];
      final quantity = quantities[item.id] ?? 0;
      return Card(
        child: ListTile(
          title: Text(item.name),
          subtitle: Text(
            item.category +
                ' · ' +
                money(item.price) +
                ' · ' +
                item.stock.toString() +
                ' na stanju',
          ),
          trailing: Row(
            mainAxisSize: MainAxisSize.min,
            children: [
              IconButton(
                onPressed: quantity == 0 ? null : () => onChange(item, -1),
                icon: const Icon(Icons.remove_circle_outline),
              ),
              Text(
                quantity.toString(),
                style: const TextStyle(fontWeight: FontWeight.w800),
              ),
              IconButton(
                onPressed: quantity >= item.stock
                    ? null
                    : () => onChange(item, 1),
                icon: const Icon(Icons.add_circle, color: AppColors.primary),
              ),
            ],
          ),
        ),
      );
    },
  );
}

class _SummaryTab extends StatelessWidget {
  const _SummaryTab({required this.draft});
  final BookingDraft draft;

  @override
  Widget build(BuildContext context) => ListView(
    padding: const EdgeInsets.fromLTRB(18, 8, 18, 18),
    children: [
      _PriceLine(label: draft.hall.name, value: draft.hallTotal),
      ...draft.equipmentQuantities.keys.map((id) {
        final item = draft.hall.equipment.firstWhere((x) => x.id == id);
        return _PriceLine(
          label: item.name,
          value: item.hourlyPrice * draft.durationHours,
        );
      }),
      ...draft.storeItemQuantities.entries.map((entry) {
        final item = draft.hall.storeItems.firstWhere((x) => x.id == entry.key);
        return _PriceLine(
          label: item.name + ' × ' + entry.value.toString(),
          value: item.price * entry.value,
        );
      }),
    ],
  );
}

class _PriceLine extends StatelessWidget {
  const _PriceLine({required this.label, required this.value});
  final String label;
  final double value;

  @override
  Widget build(BuildContext context) => Padding(
    padding: const EdgeInsets.symmetric(vertical: 9),
    child: Row(
      children: [
        Expanded(child: Text(label)),
        Text(money(value), style: const TextStyle(fontWeight: FontWeight.w800)),
      ],
    ),
  );
}
