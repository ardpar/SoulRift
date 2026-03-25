# Surge Warning System

> **Status**: Designed
> **Author**: user + claude
> **Last Updated**: 2026-03-25
> **Implements Pillar**: Core Risk/Reward — Overflow'a bilincli gecis karari

## 1. Overview

**Surge Warning System**, oyuncunun Ruh seviyesi %85–90 araligina (Surge Warning state) girdiginde aktive olan bir uyari ve karar penceresi sistemidir. Gorsel efektler (turuncu ekran vignette'i), ses efektleri (dusuk frekansli ugultu) ve bir countdown timer ile oyuncuya "Overflow'a yaklasiyorsun — simdi karar ver" sinyali verir. Timer mekanik bir zorlama degil, psikolojik bir urgency araci olarak calisir — gercek Overflow gecisi sadece Ruh >= %90 oldugunda olur. Surge Warning olmadan Overflow "kazara girilen bir tuzak" olurdu; bu sistemle "bilincli alinan bir risk" haline gelir.

---

## 2. Player Fantasy

"Kalp atisinin hizlandigi o 3 saniye." Oyuncu Surging'in zirvesinde guclu hissederken aniden ekranin kenarlari turuncu titrer, derin bir ugultu baslar ve sayac geriye saymaya baslar. Bu an **bilincli bir secim noktasidir**: "Ruh harcayip guvenlige mi doneyim, yoksa bu gucu sonuna kadar mi zorlayayim?" Her Warning penceresi kucuk bir kumar — bazen kaybedersin, ama o adrenalin seni tekrar oynamaya ceker.

---

## 3. Detailed Design

### 3.1 Core Rules

**SW1. Aktivasyon**
- Soul System state == SurgeWarning (%85–90) oldugunda Surge Warning System aktive olur
- `OnSoulStateChanged(_, SurgeWarning)` event'i ile tetiklenir

**SW2. Timer**
- Aktivasyonda `warning_timer` baslar (varsayilan 3.5 sn)
- Timer countdown olarak calisir (3.5 → 0)
- Timer sona erdiginde: **mekanik hicbir sey olmaz**
- Timer sifirlanir ve tekrar baslar (loop) — gorsel urgency surekliler devam eder
- The Vessel: timer suresi = `warning_timer + vessel_bonus` (3.5 + 2.0 = 5.5 sn)

**SW3. Gorsel/Ses Feedback**
- Aktivasyonda gorsel + ses efektleri baslar (detay Section 9'da)
- Timer ilerledikce efektler yogunlasir (urgency ramp)
- Timer son 1 saniyede: efektler maksimum yogunluk
- Timer looplandiktan sonra tekrar ramp baslar

**SW4. Deaktivasyon**
- Oyuncu Surge Warning state'den cikarsa (Ruh < %85 veya Ruh >= %90) sistem deaktive olur
- Tum gorsel/ses efektleri fade-out ile biter (0.3 sn)
- Timer sifirlanir

**SW5. Shop Pause**
- Shop acikken timer DURAKLAR (Soul System E7 ile uyumlu)
- Gorsel efektler duraklatilmis haliyle kalir (donmus vignette)
- Shop kapatilinca timer kaldigi yerden devam eder

### 3.2 States and Transitions

| State | Kosul | Davranis |
|-------|-------|----------|
| **Inactive** | Soul state != SurgeWarning | Sistem kapali, tum efektler kapali |
| **Active — Ramping** | Soul state == SurgeWarning, timer > 1 sn | Timer geriye sayiyor, efektler yogunlasiyor |
| **Active — Peak** | Soul state == SurgeWarning, timer <= 1 sn | Maksimum urgency efektleri |
| **Active — Looping** | Soul state == SurgeWarning, timer sona erdi | Timer sifirlanir, ramp yeniden baslar |
| **Paused** | Soul state == SurgeWarning, shop acik | Timer durmus, efektler donmus |

| Gecis | Tetikleyici |
|-------|-------------|
| Inactive → Active Ramping | `OnSoulStateChanged` → SurgeWarning |
| Active Ramping → Active Peak | timer <= 1.0 sn |
| Active Peak → Active Looping | timer <= 0 |
| Active Looping → Active Ramping | timer reset → yeni countdown baslar |
| Active * → Paused | Shop acildi |
| Paused → Active * | Shop kapandi (onceki state'e don) |
| Active * → Inactive | Soul state degisti (< %85 veya >= %90) |

### 3.3 Interactions with Other Systems

| Sistem | Yon | Veri Akisi | Arayuz |
|--------|-----|-----------|--------|
| **Soul System** | Upstream | State degisim eventi | `OnSoulStateChanged` — SurgeWarning giris/cikis |
| **Soul System** | Upstream | Ruh yuzdesi (urgency seviyesi icin) | `GetSoulPercent()` — %85-90 arasi gradyan |
| **Shop System** | Upstream | Shop acik/kapali durumu | `OnShopOpened`, `OnShopClosed` — timer pause/resume |
| **Camera System** | Downstream | Vignette yogunlugu, renk | `GetWarningIntensity()` — 0.0–1.0 float |
| **VFX/Aura** | Downstream | Uyari state aktif mi? | `IsWarningActive()`, `GetWarningIntensity()` |
| **Audio System** | Downstream | Ugultu yogunlugu, kalp atisi | `GetWarningIntensity()`, `OnWarningPeak()` |
| **UI/HUD** | Downstream | Timer countdown degeri | `GetTimerRemaining()`, `GetTimerTotal()` |
| **Character System** | Upstream | Vessel timer bonusu | `SetTimerBonus(seconds)` — run baslangicinda |

---

## 4. Formulas

### SWF1. Warning Intensity (Urgency Ramp)
```
timer_progress = 1.0 - (timer_remaining / warning_timer_total)  // 0.0 → 1.0
soul_proximity = (soul_percent - 0.85) / 0.05                    // 0.0 → 1.0 (%85→%90)
warning_intensity = max(timer_progress, soul_proximity)           // en yuksek urgency kazanir
```

**Degiskenler:**
- `timer_remaining`: float, 0.0 – warning_timer_total
- `warning_timer_total`: float, 3.5 sn (Vessel: 5.5 sn)
- `soul_percent`: float, 0.85 – 0.90 (SurgeWarning araliginda)
- `warning_intensity`: float, 0.0 – 1.0 (gorsel/ses sistemlerine gonderilir)

**Not:** Iki kaynak urgency yaratir: timer ilerlemesi VE Ruh'un %90'a yakinligi. Hangisi daha yuksekse o baskin olur. Bu sayede %89 Ruh'ta timer basindayken bile urgency yuksek hissedilir.

### SWF2. Gorsel Efekt Yogunluklari
```
vignette_opacity = lerp(0.15, 0.6, warning_intensity)
vignette_pulse_speed = lerp(1.0, 4.0, warning_intensity)  // Hz
screen_shake_amplitude = warning_intensity * 0.5            // piksel, son 1 sn'de aktif
```

### SWF3. Ses Efekt Yogunluklari
```
hum_volume = lerp(0.2, 0.7, warning_intensity)
hum_pitch = lerp(0.8, 1.2, warning_intensity)              // pitch yukselir
heartbeat_bpm = lerp(80, 160, warning_intensity)            // son 1 sn'de aktif
```

---

## 5. Edge Cases

| # | Durum | Ne Olur | Gerekce |
|---|-------|---------|---------|
| SWE1 | Ruh tam %85'te, bir orb daha toplarsa %90'i gecer | Warning aktive olur ama cok kisa surede Overflow'a gecilebilir. Timer yetismeyebilir — kasitli. | Hizli Overflow gecisi risk/reward'un parcasi |
| SWE2 | Warning aktifken Hunger patlamasi | Ruh %90'i gecer, Overflow'a gecis tetiklenir, Warning deaktive olur. Timer irrelevant. | Hunger + Warning etkilesimi dogal akis |
| SWE3 | Warning aktifken hasar alma | Ruh duser, eger < %85 ise Surging'e donus, Warning deaktive. | Hasar = dogal "cikis yolu" |
| SWE4 | Warning'de shop acilir, Hunger stack biriktir | Timer kaldigi yerden devam. Hunger stack biriktirmek mumkun (Hunger HE6 ile uyumlu). | Shop pause sadece timer'i etkiler |
| SWE5 | Vessel timer bonusu runtime'da item ile degisir | Aktif timer: `remaining = remaining * (new_total / old_total)`. Oranli olarak yeni sure uygulanir. | Ani timer atlama onlenir |
| SWE6 | Warning state'e girip ayni frame'de cikmak (%85 → %91) | Warning aktive → hemen deaktive. Gorsel efekt gosterilmez (cok kisa). | Tek frame gecisleri gorsel gurultu yaratmamali |
| SWE7 | Her iki intensity kaynagi da dusuk (timer basta, Ruh %85'te) | `warning_intensity ≈ 0.0`. Efektler baslangicta subtle. | Kademeli ramp istenen davranis |

---

## 6. Dependencies

### Hard Dependencies

| Sistem | Yon | Arayuz | Notlar |
|--------|-----|--------|--------|
| **Soul System** | Upstream | `OnSoulStateChanged`, `GetSoulPercent()` | State tespiti ve urgency hesabi — bu olmadan Warning calismaz |

### Soft Dependencies

| Sistem | Yon | Arayuz | Notlar |
|--------|-----|--------|--------|
| **Shop System** | Upstream | `OnShopOpened`, `OnShopClosed` | Timer pause. Olmazsa timer durmaz — kabul edilebilir. |
| **Character System** | Upstream | `SetTimerBonus(seconds)` | Vessel bonusu. Olmazsa varsayilan timer kullanilir. |
| **Camera System** | Downstream | `GetWarningIntensity()` | Vignette efekti |
| **VFX/Aura** | Downstream | `IsWarningActive()`, `GetWarningIntensity()` | Uyari gorselleri |
| **Audio System** | Downstream | `GetWarningIntensity()`, `OnWarningPeak()` | Ugultu + kalp atisi |
| **UI/HUD** | Downstream | `GetTimerRemaining()` | Countdown gostergesi |

### Bidirectional Referanslar
- Soul System GDD E7: "Surge Warning'deyken shop acilir → timer DURAKLAR" → **burada SW5 ile uyumlu**
- Soul System GDD Tuning: `warning_timer = 3.5` → **bu sistemin `warning_timer_total` degiskeni**

---

## 7. Tuning Knobs

Tum degerler Soul System'in `SoulSystemConfig` ScriptableObject'i uzerinden yonetilir (`warning_timer`). Surge Warning'e ozel ekstra degerler `SurgeWarningConfig` altinda.

| Parametre | Varsayilan | Guvenli Aralik | Cok Dusukse | Cok Yuksekse | Etkilesim |
|-----------|------------|----------------|-------------|-------------|-----------|
| `warning_timer_total` | 3.5 sn | 2.0–6.0 | Cok az urgency zamani, panik | Cok rahat, gerilim yok | Soul System'den okunur |
| `vessel_timer_bonus` | 2.0 sn | 1.0–4.0 | Vessel pasifi zayif | Vessel cok rahat | Karakter kimlik dengesi |
| `vignette_max_opacity` | 0.6 | 0.3–0.8 | Uyari farkedilmez | Ekran cok kapali | Gorsel okunaklilik |
| `hum_max_volume` | 0.7 | 0.3–1.0 | Ses uyarisi zayif | Rahatsiz edici | Oyuncu ses tercihleri |
| `intensity_ramp_curve` | Linear | Linear/EaseIn/EaseOut | — | — | Playtest ile belirle |
| `peak_threshold` | 1.0 sn | 0.5–2.0 | Peak efektleri kisa | Peak yorucu | Timer loop hissi |
| `fade_out_duration` | 0.3 sn | 0.1–0.5 | Ani kesme hissi | Efekt cok yavas kaybolur | Gorsel akicilik |

---

## 8. Acceptance Criteria

### Fonksiyonel Testler

| # | Test | Beklenen Sonuc | Oncelik |
|---|------|----------------|---------|
| SWT1 | Ruh %84 → %86 | Warning aktive, timer baslar, gorsel/ses efektler baslar | P0 |
| SWT2 | Warning aktifken Ruh %86 → %83 (hasar) | Warning deaktive, efektler fade-out (0.3 sn) | P0 |
| SWT3 | Warning aktifken Ruh %89 → %91 | Warning deaktive, Overflow state tetiklenir | P0 |
| SWT4 | Timer countdown 3.5 → 0 | Timer sifirlanir, loop baslar, mekanik degisiklik yok | P0 |
| SWT5 | Warning aktifken shop acilir | Timer duraklar, efektler donmus | P0 |
| SWT6 | Shop kapanir | Timer kaldigi yerden devam | P0 |
| SWT7 | The Vessel: timer suresi | 5.5 sn (3.5 + 2.0) | P1 |
| SWT8 | Intensity hesabi (%87.5 Ruh, timer yarida) | `soul_proximity = 0.5`, `timer_progress = 0.5`, intensity = 0.5 | P1 |
| SWT9 | Tek frame'de giris-cikis (%85 → %91) | Warning gorsel gosterilmez | P1 |

### Playtest Kriterleri (Hafta 2)

| # | Kriter | Olcum |
|---|--------|-------|
| SWP1 | Oyuncu Warning'i fark ediyor | "Overflow'a girmeden once uyari gordun mu?" — hedef %90+ |
| SWP2 | Warning gerilim yaratiyor | Oyuncu Warning'de duraksiyor/karar veriyor mu? |
| SWP3 | Warning gorsel olarak okunakli | UI'a bakmadan (vignette + ses) durumu anlayabiliyor mu? |

---

## 9. Visual/Audio Requirements

### Gorsel Efektler

| Efekt | Baslangic (intensity 0.0) | Zirve (intensity 1.0) | Uygulama |
|-------|---------------------------|----------------------|----------|
| **Ekran Vignette** | Turuncu (#FB923C), %15 opacity | %60 opacity, pulse | Camera post-processing |
| **Vignette Pulse** | 1 Hz (yavas nabiz) | 4 Hz (hizli nabiz) | Sin wave modulation |
| **Screen Shake** | Yok | 0.5 px amplitude, son 1 sn | Camera transform jitter |
| **Aura Renk Gecisi** | Altin (Surging) → Turuncu'ya blend | Tam turuncu, parlak | Sprite shader lerp |

### Ses Efektleri

| Efekt | Baslangic | Zirve | Notlar |
|-------|-----------|-------|--------|
| **Dusuk Frekansli Ugultu** | Vol %20, pitch 0.8 | Vol %70, pitch 1.2 | Surekliler, loop, Warning boyunca |
| **Kalp Atisi** | 80 BPM | 160 BPM | Son 1 sn'de (peak) baslar |
| **Giris Stinger** | Tek seferlik "bwommm" | — | Warning'e ilk giriste bir kez calar |
| **Cikis (guvenli)** | Relief chime | — | Warning'den Surging'e donuste |
| **Cikis (Overflow)** | Derin bass hit | — | Overflow'a geciste (Soul System Audio'ya devredilir) |

### Fade-Out Kurali
Tum gorsel/ses efektleri deaktivasyon sirasinda 0.3 sn icinde fade-out olur. Ani kesme yok.

---

## 10. UI Requirements

### Timer Countdown
- Soul metrenin ustunde kucuk countdown: "3.5... 3.0... 2.5..."
- Font: Share Tech Mono, turuncu renk
- Son 1 sn'de kirmiziya doner ve font buyur (1.3x scale)
- Timer loop'landiktan sonra tekrar normal boyut ve turuncu renk

### State Gostergesi
- Soul metredeki state text "WARNING" olarak gosterilir
- Turuncu renk, blink animasyonu (2 Hz)

### Minimal Yaklasim
Surge Warning'in asil feedback'i gorsel (vignette) ve ses (ugultu/kalp atisi). UI countdown destekleyici ama ikincil — oyuncu ekrana bakarak (vignette) ve dinleyerek (ses) durumu anlamali.

---

## 11. Open Questions

| # | Soru | Sahip | Hedef Cozum |
|---|------|-------|-------------|
| SWQ1 | intensity_ramp_curve Linear mi EaseIn mi daha iyi hissettiriyor? | Game Designer | Hafta 2 playtest — A/B test |
| SWQ2 | Timer gorsel olarak gosterilmeli mi yoksa sadece gorsel/ses urgency yeterli mi? | UX Designer | Hafta 2 playtest — countdown olmadan dene |
| SWQ3 | Accessibility: renk koru oyuncular icin vignette'e alternatif sinyal? | UX Designer | Post-MVP erisilebilirlik gecisi |
