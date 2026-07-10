# Rumana — برومبت تنفيذ موحّد Report0002

> **الغرض:** برومبت واحد لنسخه إلى Cursor وتطبيقه على **RomanaWeb (Backend)** + **تطبيق الزبون** + **تطبيق المتجر** + **تطبيق السائق**.
>
> **مرجع العميل:** Report0002 — تسعير الزونات، UI الزبون، إشعارات المتجر/السائق، تزامن حالة الطلب.
>
> **توثيق Backend:** `docs/Rumana_Flutter_Unified_Prompt_AR.md` · `docs/Flutter_Fixes_Prompt_AR.md`

---

## §0 — إعداد المسارات

```
{BACKEND_ROOT}     = مسار RomanaWeb (ASP.NET Core 8)
{CUSTOMER_APP}     = مسار تطبيق الزبون (Flutter)
{RESTAURANT_APP}   = مسار تطبيق المتجر (Flutter)
{DRIVER_APP}       = مسار تطبيق السائق (Flutter)
{BASE_URL}         = https://YOUR-DOMAIN/     ← بدون api/ في النهاية
{DRIVER_SUPPORT_URL} = رابط واتساب/تيليغرام دعم السائقين
```

**Migration SQL (على السيرفر):**
- `Migrations/SQL/2026_ZonePricing_LZA_ECA.sql`

---

## §1 — قواعد ذهبية (إلزامية)

1. **`statusCode` من `GET Orders/GetOrderFullDetails/{id}` = مصدر الحقيقة** — لا تعتمد على نص Push أو `statusKey` وحده.
2. **`total` و `costDelivery` = دينار عراقي كامل** (مثال: `3000` وليس `3`) — **لا تقسم على 1000**.
3. SignalR: `{BASE_URL}hubs/orders?access_token={JWT}` — event `OrderUpdated`.
4. **إخفاء breakdown ECA/LZA** في التطبيقات الثلاثة — اعرض «رسوم التوصيل: X د.ع» فقط.
5. Flutter يرسل **إحداثيات GPS** فقط — حساب LZA/ECA ومسافة الطريق (OSRM) على Backend.
6. عند إنشاء الطلب: Backend يستخدم `forceZonePricing: true` — لا تعتمد على `RestaurantCity` إذا كانت الزونات مفعّلة.

---

## §2 — Backend ({BACKEND_ROOT}) — إصلاح تسعير داخل الزون

### 2.1 المشكلة (Report0002)

| # | الخلل | التأثير |
|---|-------|---------|
| A | same-zone كان يُرجع `BaseDeliveryPrice` فقط بدون ECA | تكلفة داخل الزون ≠ تكلفة زون→زون |
| B | نقطة الدخول = `ClosestPointOnBoundary(pickup)` | مسافة ECA 2.72 كم بدل 1.73 كm لنفس الزبون |
| C | `RestaurantCity` يتجاوز تسعير الزون | سعر مدينة ثابت بدل LZA/ECA |

### 2.2 المعادلة (PDF Rev.0001)

```
routeKm = OSRM(نقطة_دخول_زون_الزبون → dropoff)
ECA_km = max(0, routeKm - LZA)
ecaFee = min(ECA_km × EcaPricePerKm, MaxEcaFee)
Total = ZonePrice + ecaFee  → تقريب لأقرب 250 د.ع
```

**نقطة الدخول:**
- من خارج الزون: أول تقاطع لخط `pickup→dropoff` مع حدود مضلع زون الزبون.
- من داخل الزون: أقرب نقطة على الحدود إلى `dropoff` (وليس إلى `pickup`).

### 2.3 الملفات المُحدَّثة

| ملف | التغيير |
|-----|---------|
| `Helper/ZoneGeometryHelper.cs` | `FindEntryPointOnBoundary(from, to, polygon)` |
| `Helper/Repository/PricingService.cs` | مسار موحّد same/inter-zone + ECA |
| `Helper/Repository/OrdersService.cs` | `ForceZonePricing = true` + حد أدنى للطلب |
| `Tests/RomanaWeb.Tests/PricingServiceTests.cs` | اختبارات 1.73 vs 2.72 |

### 2.4 حد أدنى للطلب

- **Backend:** `OrdersService.Post` يرفض الطلب إذا `itemsTotal < MinimumPrice` (أو `AppSettings.DefaultOrderCost` = 3000).
- **Response:** `{ success: false, msg: "الحد الأدنى للطلب 3,000 د.ع" }`

### 2.5 API التسعير

**`POST {BASE_URL}pricing/quote`**

```json
{
  "restaurantId": 1,
  "cityId": 5,
  "pickupLat": 30.508,
  "pickupLng": 47.784,
  "dropoffLat": 30.520,
  "dropoffLng": 47.820,
  "forceZonePricing": true
}
```

**Response (`data`):**

```json
{
  "total": 3250,
  "pricingSource": "zone_eca",
  "fromZone": "قضاء المدينة",
  "toZone": "الرحمانية",
  "zoneFee": 3000,
  "routeDistanceKm": 1.73,
  "ecaKm": 0,
  "ecaFee": 0
}
```

| `pricingSource` | المعنى | عرض التطبيق |
|-----------------|--------|-------------|
| `zone_eca` | زون + ECA | **total فقط** |
| `zone` | زون بدون ECA (route ≤ LZA) | **total فقط** |
| `near_restaurant` | قريب من المطعm | **total فقط** |

---

## §3 — تطبيق الزبون ({CUSTOMER_APP})

### 3.1 التصنيفات (Report0002 §2)

- **احذف** grid/m boxes للتصنيفات في شاشة المطعm.
- **اعتمد** قائمة أفقية علوية (TabBar / horizontal ListView) فقط.
- عند scroll المنتجات أو اختيار تصنيف:
  - `ScrollController.animateTo` لمزامنة القائمة العلوية.
  - highlight / underline للتصنيف النشط (`selectedCategoryIndex`).

```dart
void onCategoryVisible(int index) {
  if (_selectedIndex == index) return;
  setState(() => _selectedIndex = index);
  _categoryScrollController.animateTo(
    index * 96.0,
    duration: const Duration(milliseconds: 250),
    curve: Curves.easeOut,
  );
}
```

### 3.2 السلة

- زر «إضافة للسلة» → `cart.add(product, qty: 1)` **فوراً** (بدون الضغط على +).
- **حد أدنى 3000 د.ع:** disable زر «إتمام الطلب» + SnackBar إذا `subtotal < minOrder`.

```dart
final minOrder = restaurant.minimumPrice > 0 ? restaurant.minimumPrice : 3000;
final canCheckout = cartSubtotal >= minOrder;
```

### 3.3 التسعير — إخفاء ECA

```dart
Future<num> fetchDeliveryFee(...) async {
  final r = await api.postJson('pricing/quote', {
    'restaurantId': restaurantId,
    'cityId': cityId,
    'pickupLat': pickupLat, 'pickupLng': pickupLng,
    'dropoffLat': dropoffLat, 'dropoffLng': dropoffLng,
    'forceZonePricing': true,
  });
  api.throwIfFailed(r);
  return r.data['total'] as num; // لا تعرض zoneFee/ecaKm/LZA
}
```

```dart
String formatIqd(num amount) =>
  '${NumberFormat('#,##0', 'ar_IQ').format(amount.round())} د.ع';
```

### 3.4 تتبع الطلب

- بعد `Post` order ناجح → `Navigator.push(OrderTrackingScreen(orderId))` أو FAB «تتبع الطلب».
- `OrderStatusHelper` — انسخ من `docs/Flutter_Fixes_Prompt_AR.md` §1.1–1.5.
- عند SignalR/OneSignal → `GetOrderFullDetails` → حدّث UI من `statusCode` فقط.

| statusCode | النص |
|------------|------|
| 1 | تمت الموافقة |
| 5 | تم استلام الطلب من المطعم |
| 6 | السائق في الطريق إليك |

**ممنوع:** عرض حالة 6 عندما `statusCode == 5`.

---

## §4 — تطبيق السائق ({DRIVER_APP})

### 4.1 زر الدعم (Report0002 §3)

```dart
TextButton(
  onPressed: () => launchUrl(Uri.parse(DRIVER_SUPPORT_URL)),
  child: const Text('للدعم والتسجيل — يرجى الضغط هنا'),
)
```

### 4.2 قائمة «قيد التوصيل» (Report0002 §3.1)

المشكلة: الطلب لا يظهر حتى إعادة فتح التطبيق.

**الحل:**

```dart
hub.on('OrderUpdated', (_) => orderController.loadOrders(force: true));

Future<void> setOutForDelivery(int orderId) async {
  await api.post('Orders/SetOutForDelivery/$orderId');
  await orderController.loadOrders(force: true);
  await orderController.loadOrderDetails(orderId, force: true);
}
```

- `RefreshIndicator` على قائمة الطلبات وتفاصيلها (`AlwaysScrollableScrollPhysics`).
- API: `GET Orders/GetOrdersByOrderNoAndSaleManId/{orderNo},{saleManId},{type}`

### 4.3 صوت + إخفاء ECA

- **لا** تعرض breakdown ECA في تفاصيل الطلب.
- `audioplayers` — صوت قوي عند `new_order` / `driver_assigned`:

```dart
await player.play(AssetSource('sounds/loud_order_alert.mp3'), volume: 1.0);
```

---

## §5 — تطبيق المتجر ({RESTAURANT_APP})

### 5.1 إشعار بصوت قوي (Report0002 §4)

```dart
bool shouldShowFullScreenOrderAlert(Map<String, dynamic> data) {
  final mode = data['displayMode']?.toString() ?? '';
  final key = data['status']?.toString() ?? data['statusKey']?.toString() ?? '';
  return mode == 'fullscreen' || key == 'new_order';
}
```

| statusKey | displayMode | السلوك |
|-----------|-------------|--------|
| `new_order` | `fullscreen` | overlay + **صوت قوي** |
| `driver_en_route` | `banner` | toast فقط |
| `picked_up` | `banner` | toast فقط |

### 5.2 Deep link من الإشعار → تفاصيل الطلب

```dart
void onNotificationOpened(Map<String, dynamic> data) {
  if (data['type'] != 'order_status') return;
  final orderId = int.parse(data['orderId'].toString());
  navigatorKey.currentState?.push(
    MaterialPageRoute(builder: (_) => OrderDetailsScreen(orderId: orderId)),
  );
}
```

OneSignal: `OneSignal.Notifications.addClickListener` · Firebase: `onMessageOpenedApp`.

### 5.3 SignalR

```dart
hub.on('OrderUpdated', (args) {
  final data = args[0] as Map<String, dynamic>;
  if (data['audience'] != 'restaurant') return;
  if (shouldShowFullScreenOrderAlert(data)) {
    playLoudAlert();
    showFullScreenNewOrderDialog(orderId: data['orderId']);
  } else {
    showBannerNotification(title: data['title'], body: data['message']);
  }
});
```

---

## §6 — معايير القبول (Report0002)

| # | السينario | النتيجة المتوقعة |
|---|-----------|-----------------|
| 1 | مطعم + زبون في زون الرحمانية | `routeDistanceKm ≈ 1.73`؛ ECA محسوب |
| 2 | مطعm زون المدينة + زبون زون الرحمانية (نفس موقع 1) | `routeDistanceKm ≈ 1.73` (**ليس** 2.72) |
| 3 | طلب 1000 د.ع | مرفوض — «الحد الأدنى 3,000 د.ع» |
| 4 | سائق → out_for_delivery | زبون `statusCode = 6` |
| 5 | طلب جديد للمتجر | صوت قوي + deep link لتفاصيل الطلب |
| 6 | سائق — out for delivery | يظهر في «قيد التوصيل» **بدون restart** |
| 7 | checkout زبون | «3,000 د.ع» وليس «3 د.ع» |
| 8 | كل التطبيقات | **لا** يظهر breakdown ECA/LZA |
| 9 | زبون — تصنيف مندي | القائمة العلوية تُظلّل «مندي» |
| 10 | زبون — إضافة للسلة | qty=1 فوراً |

---

## §7 — ترتيب التنفيذ

1. **Backend:** `ZoneGeometryHelper` + `PricingService` + `OrdersService` + tests
2. **Deploy** API على السيرفر
3. **{CUSTOMER_APP}:** تصنيفات + سلة + تسعير + تتبع + statusCode
4. **{DRIVER_APP}:** refresh + SignalR + دعم + صوت
5. **{RESTAURANT_APP}:** إشعارات + صوت + deep link
6. **اختبار end-to-end** على سيناريوهات Report0002

---

## §8 — Checklist

### Backend
- [ ] `FindEntryPointOnBoundary` — تقاطع pickup→dropoff مع الحدود
- [ ] same-zone يستخدم ECA مثل inter-zone
- [ ] `ForceZonePricing` في `OrdersService.Post`
- [ ] حد أدنى 3000 د.ع
- [ ] Unit tests: `Quote_InterZone_EntryDistanceConsistentForSameDropoff`

### تطبيق الزبون
- [ ] قائمة تصنيفات علوية فقط + sync
- [ ] إضافة للسلة qty=1
- [ ] حد أدنى + formatIqd
- [ ] OrderStatusHelper + تتبع الطلب
- [ ] إخفاء ECA

### تطبيق السائق
- [ ] زر دعم
- [ ] SignalR + refresh قيد التوصيل
- [ ] صوت + إخفاء ECA

### تطبيق المتجر
- [ ] full-screen + صوت للطلب الجديد فقط
- [ ] deep link → تفاصيل الطلب
- [ ] banner لباقي الحالات

---

## §9 — مراجع API

| Endpoint | Method | الاستخدام |
|----------|--------|-----------|
| `pricing/quote` | POST | حساب التوصيل |
| `Orders/GetOrderFullDetails/{id}` | GET | حالة الطلب (مصدر الحقيقة) |
| `Orders/Post` | POST | إنشاء طلب (حد أدنى server-side) |
| `Orders/SetOutForDelivery/{id}` | POST | السائق → للزبون |
| SignalR `/hubs/orders` | WS | `OrderUpdated` |

---

**ملاحظة:** تطبيقات Flutter في folder منفصل عن RomanaWeb. نفّذ Backend أولاً ثم Flutter.
