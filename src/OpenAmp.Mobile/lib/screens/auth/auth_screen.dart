import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
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
  final _username = TextEditingController();
  final _email = TextEditingController();
  final _password = TextEditingController();
  final _phone = TextEditingController();
  bool _register = false;
  bool _obscure = true;

  @override
  void dispose() {
    _firstName.dispose();
    _lastName.dispose();
    _username.dispose();
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
          username: _username.text,
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
        child: Center(
          child: SingleChildScrollView(
            padding: const EdgeInsets.fromLTRB(24, 28, 24, 32),
            keyboardDismissBehavior: ScrollViewKeyboardDismissBehavior.onDrag,
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: 420),
              child: Form(
                key: _formKey,
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    const Align(
                      alignment: Alignment.center,
                      child: OpenAmpLogo(onDark: true, withMark: true),
                    ),
                    const SizedBox(height: 44),
                    Text(
                      _register ? 'Kreiraj račun' : 'Prijava',
                      style: Theme.of(context).textTheme.headlineMedium,
                    ),
                    const SizedBox(height: 22),
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
                                TextFormField(
                                  controller: _username,
                                  textInputAction: TextInputAction.next,
                                  autocorrect: false,
                                  enableSuggestions: false,
                                  autofillHints: const [
                                    AutofillHints.newUsername,
                                  ],
                                  decoration: const InputDecoration(
                                    labelText: 'Username',
                                    prefixIcon: Icon(
                                      Icons.alternate_email_rounded,
                                    ),
                                    helperText:
                                        '3–30 znakova: mala slova, brojevi, . i _',
                                  ),
                                  validator: _validateUsername,
                                ),
                                const SizedBox(height: 11),
                                Row(
                                  children: [
                                    Expanded(
                                      child: TextFormField(
                                        controller: _firstName,
                                        textInputAction: TextInputAction.next,
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
                                        textInputAction: TextInputAction.next,
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
                                  validator: _validatePhone,
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
                      keyboardType: _register
                          ? TextInputType.emailAddress
                          : TextInputType.text,
                      textInputAction: TextInputAction.next,
                      autofillHints: const [AutofillHints.email],
                      decoration: InputDecoration(
                        labelText: _register ? 'Email' : 'Email ili username',
                        prefixIcon: const Icon(Icons.alternate_email_rounded),
                      ),
                      validator: (value) {
                        final text = value?.trim() ?? '';
                        if (_register) {
                          return text.contains('@')
                              ? null
                              : 'Unesite ispravan email.';
                        }
                        return text.length >= 3
                            ? null
                            : 'Unesite email ili username.';
                      },
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
                          onPressed: () => setState(() => _obscure = !_obscure),
                          icon: Icon(
                            _obscure
                                ? Icons.visibility_outlined
                                : Icons.visibility_off_outlined,
                          ),
                        ),
                      ),
                      validator: (value) => _register
                          ? _validatePassword(value)
                          : _requiredPassword(value),
                    ),
                    const SizedBox(height: 16),
                    SignalButton(
                      label: _register ? 'Kreiraj račun' : 'Prijavi se',
                      onPressed: _submit,
                      loading: state.busy,
                    ),
                    const SizedBox(height: 12),
                    TextButton(
                      onPressed: () => _setRegister(!_register),
                      child: Text(
                        _register
                            ? 'Već imaš račun? Prijavi se'
                            : 'Nemaš račun? Registruj se',
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }

  String? _required(String? value) =>
      value == null || value.trim().length < 2 ? 'Obavezno polje.' : null;

  String? _requiredPassword(String? value) =>
      value == null || value.isEmpty ? 'Unesite lozinku.' : null;

  String? _validatePhone(String? value) {
    final phone = value?.trim() ?? '';
    if (phone.isEmpty) return null;
    return RegExp(r'^\+?[0-9][0-9 ()-]{6,18}$').hasMatch(phone)
        ? null
        : 'Unesite ispravan broj telefona.';
  }

  String? _validateUsername(String? value) {
    final username = value?.trim().toLowerCase() ?? '';
    return RegExp(r'^[a-z0-9](?:[a-z0-9._]{1,28}[a-z0-9])?$').hasMatch(username)
        ? null
        : 'Username nije ispravan.';
  }

  String? _validatePassword(String? value) {
    final password = value ?? '';
    final valid =
        password.length >= 10 &&
        password.length <= 128 &&
        password.contains(RegExp('[A-Z]')) &&
        password.contains(RegExp('[a-z]')) &&
        password.contains(RegExp('[0-9]')) &&
        password.contains(RegExp(r'[^A-Za-z0-9]'));
    return valid
        ? null
        : '10+ znakova, veliko i malo slovo, broj i poseban znak.';
  }
}
