# Input System

> **Status**: Tasarlandi
> **Son Guncelleme**: 2026-03-25
> **Katman**: Foundation
> **Oncelik**: MVP

## Genel Bakis

Unity Input System paketi uzerine kurulu, top-down twin-stick kontrol sistemi. Hareket (WASD/sol stick), nisan (mouse/sag stick) ve ates (mouse click/trigger) girdilerini isler. Rebindable kontroller destekler.

## Oyuncu Fantezisi

Kontroller sezgisel ve tepkili. Oyuncu dustugunde "kontroller beni yarı yolda bıraktı" degil, "ben hata yaptım" hissi.

## Detayli Tasarim

### Temel Kurallar

1. Unity Input System paketi (`com.unity.inputsystem`) kullanilir — legacy Input sinifi kullanilmaz
2. Input Actions asset'i Editor'de tanimlanir, C# sinifi otomatik generate edilir
3. Tum girdiler action-based: dogrudan Keyboard/Mouse API'si yerine InputAction kullanilir
4. Controller ve klavye/mouse ayni anda desteklenir (scheme switching)

### Action Map

| Action Map | Action | Binding (KB/Mouse) | Binding (Gamepad) | Tip |
|------------|--------|---------------------|-------------------|-----|
| **Gameplay** | Move | WASD | Left Stick | Value (Vector2) |
| **Gameplay** | Aim | Mouse Position | Right Stick | Value (Vector2) |
| **Gameplay** | Fire | Left Mouse Button | Right Trigger | Button |
| **Gameplay** | Dash | Space | Left Bumper | Button |
| **Gameplay** | Interact | E | A Button | Button |
| **UI** | Navigate | Arrow Keys | D-Pad / Left Stick | Value (Vector2) |
| **UI** | Confirm | Enter / Left Click | A Button | Button |
| **UI** | Cancel | Escape | B Button | Button |

### Kontrol Modlari

| Mod | Aktif Map | Ne Zaman |
|-----|-----------|----------|
| **Gameplay** | Gameplay | Wave sirasinda |
| **Shop** | UI | Shop ekraninda |
| **Pause** | UI | Pause menusunde |

### Diger Sistemlerle Etkilesim

- **Player Controller**: Move, Aim, Fire, Dash action'larini dinler
- **Shop System**: UI action map'ine gecer, Navigate/Confirm/Cancel kullanir
- **HUD/UI**: Pause toggle, menu navigasyonu

## Formuller

| Parametre | Deger |
|-----------|-------|
| Mouse aim: ekran pozisyonundan dunya pozisyonuna | `Camera.main.ScreenToWorldPoint(mousePos)` |
| Gamepad aim: stick yonu | `aimDirection = rightStick.normalized` |
| Dead zone (gamepad) | 0.15 (varsayilan) |

## Kenar Durumlar

- **Mouse ve gamepad ayni anda kullanilirsa**: Son kullanilan cihaza gecis yap (scheme auto-switch)
- **Aim sifir vektoru (gamepad stick merkez)**: Son bilinen yonu koru, ates etme
- **Shop acikken gameplay input gelirse**: Gameplay action map deaktif, input yutulur
- **Pause sirasinda**: Tum gameplay input'lari deaktif
- **Rebind sirasinda cakisma**: Uyari goster, eski binding'i geri yukle

## Bagimliliklar

| Yonu | Sistem | Arayuz |
|------|--------|--------|
| Bagimlilik yok | — | Foundation katmani |
| Bagimli olan | Player Controller | Move, Aim, Fire, Dash action'larini okur |
| Bagimli olan | Combat/Weapon | Fire action'ini okur |
| Bagimli olan | Shop System | UI action map'ini kullanir |
| Bagimli olan | HUD/UI | Pause, navigasyon action'larini okur |

## Ayar Dugumleri

| Dugum | Varsayilan | Guvenli Aralik | Etki |
|-------|-----------|----------------|------|
| Gamepad dead zone | 0.15 | 0.05 - 0.30 | Cok dusuk = drift, cok yuksek = tepkisiz |
| Mouse sensitivity | 1.0 | 0.5 - 2.0 | Aim hissi |
| Stick aim assist | 0 (yok) | 0 - 0.5 | Gamepad icin hafif yapiskanlık |

## Kabul Kriterleri

- [ ] WASD ile 8 yonlu hareket calisiyor
- [ ] Mouse ile aim calisiyor (karakter mouse yonune bakiyor)
- [ ] Gamepad ile hareket ve aim calisiyor
- [ ] Fire action ates mekanigini tetikliyor
- [ ] Shop acikken gameplay input'lari engelleniyor
- [ ] Kontrol scheme'i otomatik gecis yapiyor (KB/Mouse ↔ Gamepad)
- [ ] Rebind sistemi calisiyor (opsiyonel — MVP sonrasi olabilir)
