# Rumana — برومبت تنفيذ Report0002

> **انسخ هذا الملف كاملاً إلى Cursor** على workspace يحتوي التطبيقات الثلاثة + Backend.
>
> **مصدر المتطلبات:** Report0002 (مرفق العميل)
>
> **دورك:** مهندس Backend + Flutter محترف — نفّذ كل بند أدناه بالترتيب دون تخطي.

---

## §0 — إعداد المسارات (عدّل قبل التنفيذ)

```
{BACKEND_ROOT}       = مسار RomanaWeb (ASP.NET Core 8)
{CUSTOMER_APP}       = مسار تطبيق الزبون (Flutter)
{DRIVER_APP}         = مسار تطبيق السائق / الدلفري (Flutter)
{RESTAURANT_APP}     = مسار تطبيق المتجر (Flutter)
{BASE_URL}           = https://YOUR-DOMAIN/
{DRIVER_SUPPORT_URL} = رابط دعم وتسجيل السائقين (واتساب / تيليغرام / صفحة ويب)
```

**قواعد عامة:**
- `statusCode` من `GET Orders/GetOrderFullDetails/{orderId}` = **مصدر الحقيقة** لحالة الطلب.
- `total` و `costDelivery` = **دينار عراقي كامل** (3000 وليس 3) — لا تقسم على 1000.
- SignalR: `{BASE_URL}hubs/orders?access_token={JWT}` — حدث `OrderUpdated`.
- **إخفاء تفاصيل ECA/LZA** من تطبيق الزبون والسائق والمتجر — اعرض «رسوم التوصيل» فقط.

---

# §1 — الداشبورد / Backend ({BACKEND_ROOT})

> **مرجع PDF — القسم 1**

## 1.1 المطلوب

**داخل الزون يجب أن يعمل مثل زون→زون** — نفس منطق التسعير (سعر الزون + ECA)، وليس مساراً مختصراً بسعر ثابت فقط.

## 1.2 الخلل المُبلَّغ عنه

| البند | الوصف |
|-------|--------|
| مسافة ECA الصحيحة | من **نقطة دخول زون الزبون → الزبون** = **1.73 كم** |
| مسافة ECA الخاطئة | عند التوصيل من **زون المدينة → زون الرحمانية** يظهر **2.72 كm** |
| المسافة الكلية | مطعم → زبون = **5.36 كm** (هذه ≠ مسافة ECA) |

**القاعدة:** مسافة الطريق لحساب ECA = `OSRM(نقطة_دخول_زون_الزبون → إحداثيات_الزبون)` — **ثابتة لنفس موقع الزبون** بغض النظر عن زون المطعm.

## 1.3 التنفيذ في Backend

### أ) `Helper/ZoneGeometryHelper.cs`

أضف `FindEntryPointOnBoundary(polygon, fromLng, fromLat, toLng, toLat)`:

- **المطعm خارج زون الزبون:** أول تقاطع لخط `pickup → dropoff` مع حدود مضلع زون الزبون.
- **المطعm داخل زون الزبون:** أقرب نقطة على حدود المضلع إلى **موقع الزبون** (وليس إلى المطعm).
- fallback: `ClosestPointOnBoundary`.

### ب) `Helper/Repository/PricingService.cs`

1. **أزل** الفرع الذي يُرجع `BaseDeliveryPrice` فقط عند `pickupZone == dropoffZone`.
2. **وحّد** مسار same-zone و inter-zone:
   ```
   zoneFee = ZonePrice matrix (from→to) ?? BaseDeliveryPrice
   routeKm = OSRM(entryPoint → dropoff)
   ECA_km = max(0, routeKm - LZA)
   ecaFee = min(ECA_km × EcaPricePerKm, MaxEcaFee)
   Total = zoneFee + ecaFee → تقريب 250 د.ع
   ```
3. استخدم `FindEntryPointOnBoundary` بدل `ClosestPointOnBoundary(pickup)`.

### ج) `Helper/Repository/OrdersService.cs`

- عند `Quote()` أرسل `forceZonePricing: true` حتى لا يتجاوز `RestaurantCity` تسعير الزون.

### د) اختبارات قبول Backend

| # | السينario | النتيجة |
|---|-----------|---------|
| B1 | مطعm + زبون في زون الرحمانية، نفس موقع الزبون | `routeDistanceKm ≈ 1.73` |
| B2 | مطعm زون المدينة + زبون زون الرحمانية، **نفس موقع الزبون** | `routeDistanceKm ≈ 1.73` (**ليس 2.72**) |
| B3 | same-zone + مسافة > LZA | `pricingSource = zone_eca` |

---

# §2 — تطبيق الزبون ({CUSTOMER_APP})

> **مرجع PDF — القسم 2**

## 2.1 إخفاء تفاصيل ECA

- **لا تعرض** للزبون: `fromZone`, `toZone`, `lzaKm`, `ecaKm`, `ecaFee`, `routeDistanceKm`.
- **اعرض فقط:** «رسوم التوصيل: {formatIqd(total)}».
- استخدم `POST pricing/quote` — اقرأ `data.total` فقط.

## 2.2 التصنيفات — إزالة المربعات

**المشكلة (PDF):** مربعات التصنيفات عند كثرتها تمنع الزبون من رؤية المنتجات.

**المطلوب:**
- **احذف** grid / cards / مربعات التصنيفات من شاشة تفاصيل المطعm.
- **اعتمد حصرياً** على **القائمة الأفقية العلوية** للتصنيفات.

## 2.3 مزامنة القائمة العلوية مع التصنيف المعروض

**المشكلة (PDF):** المنتجات تابعة لتصنيف «مندي» لكن القائمة العلوية لا تزال على «الأكثر طلباً».

**المطلوب:**
- عند scroll المنتجات أو اختيار تصنيف → **حدّث** `_selectedCategoryIndex`.
- **ظلّل / underline** التصنيف النشط في القائمة العلوية.
- **Scroll تلقائي** للقائمة العلوية لتُظهر التصنيف الحالي:

```dart
void syncCategoryTab(int index) {
  setState(() => _selectedIndex = index);
  _tabController?.animateTo(index);
  _categoryScrollController.animateTo(
    index * _tabWidth,
    duration: const Duration(milliseconds: 250),
    curve: Curves.easeOut,
  );
}

// عند scroll قائمة المنتجات — VisibilityDetector أو scroll listener
void onProductSectionVisible(int categoryIndex) {
  if (_selectedIndex != categoryIndex) syncCategoryTab(categoryIndex);
}
```

## 2.4 الحد الأدنى للطلب — 3000 دينار

**المشكلة (PDF):** منتج 1000 د.ع يُكمَل الطلب رغم أن الحد الأدنى 3000.

**المطلوب (UI + Backend):**

```dart
final minOrder = restaurant.minimumPrice > 0 ? restaurant.minimumPrice : 3000;
final subtotal = cart.items.fold<num>(0, (s, i) => s + i.price * i.qty);
final canCheckout = subtotal >= minOrder;

// زر إتمام الطلب
ElevatedButton(
  onPressed: canCheckout ? _submitOrder : null,
  ...
);
if (!canCheckout)
  Text('الحد الأدنى للطلب ${formatIqd(minOrder)}', style: errorStyle);
```

Backend يرفض أيضاً: `{ success: false, msg: "الحد الأدنى للطلب 3,000 د.ع" }`.

## 2.5 زر تتبع الطلب

**المشكلة (PDF):** لا يظهر زر تتبع الطلب بعد إكمال الطلب.

**المطلوب:**
- فور نجاح `Post` الطلب → اعرض زر/FAB **«تتبع الطلب»** أو انتقل لـ `OrderTrackingScreen(orderId)`.
- لا تُخفِ الزر بعد الإنشاء.

## 2.6 إضافة للسلة — qty=1 فوراً

**المشكلة (PDF):** الضغط على «إضافة للسلة» لا يُضيف دون الضغط على +.

**المطلوب:**

```dart
void onAddToCart(Product p) {
  cart.addItem(productId: p.id, qty: 1); // مباشرة — بدون فتح عداد
  showSnackBar('تمت الإضافة للسلة');
}
```

## 2.7 تزامن حالة الطلب مع السائق

**المشكلة (PDF):**
- السائق في مرحلة «التوصيل من المطعm → الزبون» (statusCode **6**).
- تطبيق الزبون لا يزال على «تمت الموافقة» (statusCode **1**).

**المطلوب:**

```dart
class OrderStatusHelper {
  static const labels = {
    0: 'انتظار موافقة المطعm',
    1: 'تمت الموافقة',
    3: 'تم تعيين سائق',
    4: 'السائق في الطريق لاستلام الطلب',
    5: 'تم استلام الطلب من المطعm',
    6: 'السائق في الطريق إليك',
    8: 'تم تسليم الطلب',
    9: 'ملغي',
  };
}

// عند SignalR / OneSignal / فتح شاشة التتبع:
Future<void> refreshOrder(int orderId) async {
  final res = await api.get('Orders/GetOrderFullDetails/$orderId');
  final code = res.data['statusCode'] as int;
  setState(() => _statusCode = code); // من statusCode فقط — لا من نص الإشعار
}

hub.on('OrderUpdated', (args) {
  final orderId = args[0]['orderId'] as int;
  if (_currentOrderId == orderId) refreshOrder(orderId);
});
```

| statusCode | يجب أن يرى الزبون |
|------------|-------------------|
| 1 | تمت الموافقة |
| 6 | السائق في الطريق إليك |
| **ممنوع** | عرض 6 عندما API يُرجع 1 |

---

# §3 — تطبيق الدلفري / السائق ({DRIVER_APP})

> **مرجع PDF — القسم 3**

## 3.1 زر الدعم والتسجيل

**المطلوب (PDF):** زر «يرجى الضغط هنا» يودّي لدعm السائقين.

```dart
// في شاشة Login أو Home
TextButton.icon(
  icon: const Icon(Icons.support_agent),
  label: const Text('للدعم والتسجيل — يرجى الضغط هنا'),
  onPressed: () => launchUrl(Uri.parse(DRIVER_SUPPORT_URL), mode: LaunchMode.externalApplication),
)
```

## 3.2 الطلب لا يظهر في «قيد التوصيل»

**المشكلة (PDF):** يتطلب الخروج من التطبيق والرجوع لظهور الطلب في خانة الطلبات قيد التوصيل.

**المطلوب:**

```dart
// بعد كل تغيير حالة
Future<void> setOutForDelivery(int orderId) async {
  await api.post('Orders/SetOutForDelivery/$orderId');
  await loadOrders(type: inDeliveryTab, force: true);
  await loadOrderDetails(orderId, force: true);
}

// SignalR — تحديث فوري
hub.on('OrderUpdated', (_) => loadOrders(force: true));

// Pull-to-refresh
RefreshIndicator(
  onRefresh: () => loadOrders(force: true),
  child: ListView.builder(
    physics: const AlwaysScrollableScrollPhysics(),
    ...
  ),
)
```

API: `GET Orders/GetOrdersByOrderNoAndSaleManId/{orderNo},{saleManId},{type}`

## 3.3 إخفاء تفاصيل ECA

- **لا تعرض** breakdown ECA/LZA/zoneFee في أي شاشة.
- اعرض رسوم التوصيل الإجمالية فقط إن لزم.

## 3.4 صوت قوي عند وصول طلب

**المطلوب (PDF — القسم 4 يشمل السائق أيضاً):** صوت **مزعج وقوي** عند وصول طلب.

```dart
Future<void> playOrderAlert() async {
  final player = AudioPlayer();
  await player.setVolume(1.0);
  await player.play(AssetSource('sounds/loud_order_alert.mp3'));
}
// استدعِها عند new_order / driver_assigned / OrderUpdated ذات صلة
```

---

# §4 — تطبيق المتجر ({RESTAURANT_APP})

> **مرجع PDF — القسم 4**

## 4.1 إشعار بصوت قوي عند وصول طلب

**المشكلة (PDF):** لا يظهر إشعار بصوت قوي مثل تطبيق الدلفري عند وصول طلب للمتجر.

**المطلوب:**
- عند **طلب جديد** → إشعار + **صوت مزعج وقوي** (نفس مستوى تطبيق السائق).
- استخدم `audioplayers` أو `flutter_ringtone_player` — volume = 1.0.

```dart
hub.on('OrderUpdated', (args) {
  final data = args[0] as Map<String, dynamic>;
  if (data['audience'] != 'restaurant') return;
  if (data['statusKey'] == 'new_order' || data['statusCode'] == 0) {
    playOrderAlert();
    showFullScreenNewOrderDialog(orderId: data['orderId']);
  }
});
```

## 4.2 الضغط على الإشعار → تفاصيل الطلب مباشرة

**المطلوب (PDF):** عند الضغط على إشعار الطلب → الانتقال **مباشرة** إلى شاشة **تفاصيل الطلب**.

```dart
// OneSignal
OneSignal.Notifications.addClickListener((event) {
  final data = event.notification.additionalData ?? {};
  if (data['type'] == 'order_status') {
    final orderId = int.parse(data['orderId'].toString());
    navigatorKey.currentState?.push(
      MaterialPageRoute(builder: (_) => OrderDetailsScreen(orderId: orderId)),
    );
  }
});

// Firebase
FirebaseMessaging.onMessageOpenedApp.listen((message) {
  final orderId = int.tryParse(message.data['orderId'] ?? '');
  if (orderId != null) _openOrderDetails(orderId);
});
```

---

# §5 — معايير القبول النهائية (من PDF)

| # | القسم | السينario | النتيجة المتوقعة |
|---|-------|-----------|-----------------|
| 1 | §1 | زبون زون الرحمانية — مطعm داخل/خارج الزون | `routeDistanceKm = 1.73` (ثابت) |
| 2 | §1 | مطعm زون المدينة → زبون زون الرحمانية | `routeDistanceKm = 1.73` (**≠ 2.72**) |
| 3 | §2 | checkout | لا يُكمَل طلب 1000 د.ع — الحد 3000 |
| 4 | §2 | تصنيف مندي | القائمة العلوية تُظلّل «مندي» |
| 5 | §2 | إضافة للسلة | qty=1 فوراً |
| 6 | §2 | بعد الطلب | زر تتبع الطلب ظاهر |
| 7 | §2 | سائق statusCode=6 | زبون يرى «السائق في الطريق إليك» |
| 8 | §2/3/4 | كل التطبيقات | **لا** تفاصيل ECA |
| 9 | §3 | out for delivery | يظهر في «قيد التوصيل» بدون restart |
| 10 | §3 | شاشة Login | زر دعm السائقين موجود |
| 11 | §4 | طلب جديد للمتجر | صوت قوي + إشعار |
| 12 | §4 | ضغط الإشعار | → تفاصيل الطلب مباشرة |

---

# §6 — ترتيب التنفيذ

1. **{BACKEND_ROOT}** — §1 (تسعير الزون + ECA)
2. **{CUSTOMER_APP}** — §2 (كل بنود PDF)
3. **{DRIVER_APP}** — §3 (دعm + قيد التوصيل + ECA + صوت)
4. **{RESTAURANT_APP}** — §4 (صوت + deep link)
5. اختبار end-to-end على سيناريوهات PDF

---

# §7 — Checklist

### Backend / Dashboard
- [ ] same-zone = zone→zone (ECA موحّد)
- [ ] `routeDistanceKm` ثابت 1.73 لنفس الزبون
- [ ] حد أدنى 3000 د.ع server-side

### تطبيق الزبون
- [ ] إخفاء ECA
- [ ] حذف مربعات التصنيف — قائمة علوية فقط
- [ ] sync + highlight التصنيف النشط
- [ ] min order 3000
- [ ] زر تتبع الطلب
- [ ] add to cart qty=1
- [ ] statusCode sync مع السائق

### تطبيق الدلفري
- [ ] زر دعm وتسجيل
- [ ] قيد التوصيل — refresh فوري
- [ ] إخفاء ECA
- [ ] صوت قوي

### تطبيق المتجر
- [ ] صوت قوي عند طلب جديد
- [ ] deep link → تفاصيل الطلب

---

**نهاية البرومبت — Report0002**
