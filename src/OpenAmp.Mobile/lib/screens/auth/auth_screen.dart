import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:openamp_mobile/core/theme/app_theme.dart';
import 'package:openamp_mobile/state/app_state.dart';
import 'package:openamp_mobile/widgets/common.dart';

class AuthScreen extends ConsumerStatefulWidget {
  const AuthScreen({super.key});

  @override
  ConsumerState<AuthScreen> createState() => _AuthScreenState();
}

class _AuthScreenState extends ConsumerState<AuthScreen> {
  final _formKey = GlobalKey<FormState>();
  final _firstName = TextEditingController();
  final _lastName = TextEditingController();
  final _email = TextEditingController();
  final _password = TextEditingController();
  final _phone = TextEditingController();
  bool _register = false;
  bool _obscure = true;

  @override
  void dispose() {
    _firstName.dispose();
    _lastName.dispose();
    _email.dispose();
    _password.dispose();
    _phone.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    final controller = ref.read(appControllerProvider.notifier);
    try {
      if (_register) {
        await controller.register(
          firstName: _firstName.text,
          lastName: _lastName.text,
          email: _email.text,
          password: _password.text,
          phone: _phone.text,
        );
      } else {
        await controller.login(_email.text, _password.text);
      }
    } catch (_) {}
  }

  void _setRegister(bool value) {
    if (_register == value) return;
    ref.read(appControllerProvider.notifier).clearError();
    setState(() => _register = value);
  }

  @override
  Widget build(BuildContext context) {
    final state = ref.watch(appControllerProvider);
    return Scaffold(
      body: SafeArea(
        child: SingleChildScrollView(
          keyboardDismissBehavior: ScrollViewKeyboardDismissBehavior.onDrag,
          child: Column(
            children: [
              Container(
                width: double.infinity,
                padding: const EdgeInsets.fromLTRB(22, 22, 22, 28),
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
                    const OpenAmpLogo(onDark: true),
                    const SizedBox(height: 34),
                    const SectionEyebrow(
                      'Rehearsal booking, bez šuma',
                      color: AppColors.signal,
                    ),
                    const SizedBox(height: 13),
                    Text(
                      'Pojačaj probu.\nNe komplikuj termin.',
                      style: Theme.of(
                        context,
                      ).textTheme.displaySmall?.copyWith(color: Colors.white),
                    ),
                    const SizedBox(height: 13),
                    const Text(
                      'Sala, oprema i bend na jednom mjestu — tačno kad vam treba.',
                      style: TextStyle(
                        color: Colors.white70,
                        fontSize: 15,
                        height: 1.4,
                      ),
                    ),
                  ],
                ),
              ),
              Padding(
                padding: const EdgeInsets.fromLTRB(20, 20, 20, 30),
                child: ConstrainedBox(
                  constraints: const BoxConstraints(maxWidth: 480),
                  child: Form(
                    key: _formKey,
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.stretch,
                      children: [
                        _ModeSwitch(
                          register: _register,
                          onChanged: _setRegister,
                        ),
                        const SizedBox(height: 26),
                        const SectionEyebrow('Member access'),
                        const SizedBox(height: 9),
                        Text(
                          _register
                              ? 'Novi član postave'
                              : 'Vrati bend u ritam',
                          style: Theme.of(context).textTheme.headlineMedium,
                        ),
                        const SizedBox(height: 6),
                        Text(
                          _register
                              ? 'Kreiraj profil i rezerviši prvi termin.'
                              : 'Unesi podatke i nastavi gdje ste stali.',
                        ),
                        const SizedBox(height: 20),
                        if (state.error != null)
                          ErrorBanner(
                            message: state.error!,
                            onRetry: ref
                                .read(appControllerProvider.notifier)
                                .clearError,
                          ),
                        AnimatedSize(
                          duration: const Duration(milliseconds: 200),
                          alignment: Alignment.topCenter,
                          child: _register
                              ? Column(
                                  children: [
                                    Row(
                                      children: [
                                        Expanded(
                                          child: TextFormField(
                                            controller: _firstName,
                                            textInputAction:
                                                TextInputAction.next,
                                            decoration: const InputDecoration(
                                              labelText: 'Ime',
                                            ),
                                            validator: _required,
                                          ),
                                        ),
                                        const SizedBox(width: 10),
                                        Expanded(
                                          child: TextFormField(
                                            controller: _lastName,
                                            textInputAction:
                                                TextInputAction.next,
                                            decoration: const InputDecoration(
                                              labelText: 'Prezime',
                                            ),
                                            validator: _required,
                                          ),
                                        ),
                                      ],
                                    ),
                                    const SizedBox(height: 11),
                                    TextFormField(
                                      controller: _phone,
                                      keyboardType: TextInputType.phone,
                                      textInputAction: TextInputAction.next,
                                      decoration: const InputDecoration(
                                        labelText: 'Telefon (opcionalno)',
                                        prefixIcon: Icon(Icons.phone_outlined),
                                      ),
                                    ),
                                    const SizedBox(height: 11),
                                  ],
                                )
                              : const SizedBox.shrink(),
                        ),
                        TextFormField(
                          controller: _email,
                          keyboardType: TextInputType.emailAddress,
                          textInputAction: TextInputAction.next,
                          autofillHints: const [AutofillHints.email],
                          decoration: const InputDecoration(
                            labelText: 'Email',
                            prefixIcon: Icon(Icons.alternate_email_rounded),
                          ),
                          validator: (value) =>
                              value != null && value.contains('@')
                              ? null
                              : 'Unesite ispravan email.',
                        ),
                        const SizedBox(height: 11),
                        TextFormField(
                          controller: _password,
                          obscureText: _obscure,
                          textInputAction: TextInputAction.done,
                          onFieldSubmitted: (_) => _submit(),
                          autofillHints: const [AutofillHints.password],
                          decoration: InputDecoration(
                            labelText: 'Lozinka',
                            prefixIcon: const Icon(Icons.lock_outline_rounded),
                            suffixIcon: IconButton(
                              onPressed: () =>
                                  setState(() => _obscure = !_obscure),
                              icon: Icon(
                                _obscure
                                    ? Icons.visibility_outlined
                                    : Icons.visibility_off_outlined,
                              ),
                            ),
                          ),
                          validator: (value) => (value?.length ?? 0) >= 10
                              ? null
                              : 'Lozinka mora imati najmanje 10 znakova.',
                        ),
                        const SizedBox(height: 16),
                        SignalButton(
                          label: _register ? 'Kreiraj profil' : 'Uđi u OpenAmp',
                          onPressed: _submit,
                          loading: state.busy,
                        ),
                        const SizedBox(height: 13),
                        Text(
                          _register
                              ? 'Registracijom prihvataš pravila korištenja platforme.'
                              : 'Tvoj sljedeći termin je bliže nego što misliš.',
                          textAlign: TextAlign.center,
                          style: const TextStyle(
                            color: AppColors.textMuted,
                            fontSize: 11,
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  String? _required(String? value) =>
      value == null || value.trim().length < 2 ? 'Obavezno polje.' : null;
}

class _ModeSwitch extends StatelessWidget {
  const _ModeSwitch({required this.register, required this.onChanged});

  final bool register;
  final ValueChanged<bool> onChanged;

  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.all(4),
    decoration: BoxDecoration(
      color: AppColors.paperMuted,
      borderRadius: BorderRadius.circular(AppRadii.medium),
      border: Border.all(color: AppColors.line),
    ),
    child: Row(
      children: [
        _ModeOption(
          label: 'Prijava',
          selected: !register,
          onTap: () => onChanged(false),
        ),
        _ModeOption(
          label: 'Registracija',
          selected: register,
          onTap: () => onChanged(true),
        ),
      ],
    ),
  );
}

class _ModeOption extends StatelessWidget {
  const _ModeOption({
    required this.label,
    required this.selected,
    required this.onTap,
  });

  final String label;
  final bool selected;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) => Expanded(
    child: InkWell(
      borderRadius: BorderRadius.circular(9),
      onTap: onTap,
      child: AnimatedContainer(
        duration: const Duration(milliseconds: 170),
        padding: const EdgeInsets.symmetric(vertical: 11),
        decoration: BoxDecoration(
          color: selected ? AppColors.ink : Colors.transparent,
          borderRadius: BorderRadius.circular(9),
        ),
        child: Text(
          label.toUpperCase(),
          textAlign: TextAlign.center,
          style: TextStyle(
            color: selected ? Colors.white : AppColors.textMuted,
            fontSize: 11,
            fontWeight: FontWeight.w900,
            letterSpacing: .85,
          ),
        ),
      ),
    ),
  );
}
