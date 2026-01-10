Funkčné požiadavky:
- Systém umožňuje registráciu používateľov prostredníctvom e-mailu, hesla a platnej pozvánky
- Systém overí platnosť pozvánky pred povolením registrácie
- Podpora prihlásenia (email, heslo, prípadne 2fa) a odhlásenia sa
- Integrácia TOTP s podporou Google Authentifikátora
- Resetovanie hesla pomocou emailu
- Generovanie a validácia pozvánok cez backend, automatická deaktivácia po použití
- Refresh Token autentifikácia pri spúšťaní aplikácie
- JWT (JSON Web Token) autentifikácia pri volaní citlivých API
- Používateľ môže vytvoriť novú krypto peňaženku v rámci aplikácie
- Používateľ môže importovať existujúcu peňaženku pomocou 12 slovnej frázy
- Aplikácia šifruje a bezpečne ukladá (leveldb) seed frázu
- V danom čase môže byť aktívna iba jedna peňaženka pre používateľský účet
- Používateľ môže posielať a prijímať kryptomeny podporované peňaženkou
- Aplikácia zobrazuje históriu transakcií a aktuálny zostatok
- Pred odoslaním transakcie používateľ potvrdí sumu a adresu
- Všetky citlivé údaje (fráza, seed a privátne kľúče) sú šifrované lokálne

Nefunkčné požiadavky:
- Šifrovanie dát pomocou AES-256 GCM alebo CBC, hashovanie hesiel Argon2Id
- Žiadne citlivé údaje nie sú odosielané na server
- Ochrana proti brute-force útokom pri prihlásovaní (30 sekundový timeout)
- Intuitívne rozhranie s jasnými návodmi pre nových používateľov
- Podpora iba pre Windows
- Kompatibilita s hlavnými kryptomenami, napr. Bitcoin, Ethereum a Litecoin
- Nízka latencia API volaní (<500ms)
- Logovanie použivateľskej aktivity (lokálne aj vzdialené)