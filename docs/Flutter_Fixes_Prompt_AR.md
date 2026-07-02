# Rumana — برومبت إصلاحات موحّد (زبون + متجر + سائق)

> **الغرض:** برومبت واحد دقيق لنسخه إلى Cursor وتطبيقه على **تطبيق الزبون** و**تطبيق المتجر** و**تطبيق السائق** بعد تحديث باكند RomanaWeb.
>
> **Base URL:** `https://{YOUR_DOMAIN}/`
>
> **قاعدة ذهبية:** `statusCode` من `GET Orders/GetOrderFullDetails/{orderId}` هو **مصدر الحقيقة الوحيد** لحالة الطلب. لا تعتمد على نص الإشعار أو عنوان Push.

---

## 0) ما تغيّر في الباكند (اقرأ قبل التنفيذ)

### 0.1 SignalR — حدث `OrderUpdated`

```json
{
  "title": "عنوان الإشعار",
  "message": "تفاصيل الإشعار",
  "orderId": 456,
  "statusKey": "picked_up",
  "statusCode": 5,
  "displayMode": "banner",
  "audience": "user",
  "audienceId": 123,
  "at": "2026-06-19T10:00:00Z"
}
```

| statusKey | statusCode | displayMode | المعنى |
|-----------|------------|-------------|--------|
| `new_order` | 0 | `fullscreen` (للمطعم فقط) | طلب جديد |
| `approved` | 1 | `banner` | موافقة المطعم |
| `driver_assigned` | 3 | `banner` | تعيين سائق |
| `driver_en_route` | 4 | `banner` | السائق متجه للمطعم |
| `picked_up` | 5 | `banner` | استلام من المطعم |
| `out_for_delivery` | 6 | `banner` | في الطريق للزبون |
| `confirmed` | 8 | `banner` | تم التسليم |

### 0.2 OneSignal — payload موحّد

```json
{
  "type": "order_status",
  "orderId": "456",
  "status": "picked_up",
  "statusText": "تم الاستلام من المطعم",
  "statusCode": "5",
  "displayMode": "banner"
}
```

### 0.3 سعر التوصيل

- `POST /pricing/quote` — أرسل `restaurantId` + `cityId` + الإحداثيات
- إذا وُجد سعر مدينة في `RestaurantCity` → `total = 3000` (دينار كامل، **بدون** قسمة)
- `pricingSource`: `"city"` | `"zone"` | `"distance"` | `"minimum"`
- Fallback: `GET RestaurantCity/GetDeliveryFee/{restaurantId}/{cityId}`
- Fallback: `GET RestaurantCity/GetByResId/{restaurantId}`

---

## 1) مشكلة تسبيق حالة الطلب — تطبيق الزبون 👤

### المشكلة
عند ضغط السائق «استلام من المطعم» يظهر للزبون «في الطريق إليك» (حالة 6) بدل «تم الاستلام من المطعم» (حالة 5).

### المطلوب

#### 1.1 إنشاء `OrderStatusHelper` (ملف مشترك)

```dart
class OrderStatusHelper {
  static const labels = {
    0: 'انتظار موافقة المطعم',
    1: 'تمت الموافقة',
    3: 'تم تعيين سائق',
    4: 'السائق في الطريق لاستلام الطلب',
    5: 'تم استلام الطلب من المطعم',
    6: 'السائق في الطريق إليك',
    8: 'تم تسليم الطلب',
    9: 'ملغي',
  };

  static const statusKeyToCode = {
    'pending': 0,
    'approved': 1,
    'driver_assigned': 3,
    'driver_en_route': 4,
    'picked_up': 5,
    'out_for_delivery': 6,
    'confirmed': 8,
    'cancel': 9,
  };

  static String labelFor(int code) => labels[code] ?? 'غير معروف';
}
```

#### 1.2 قواعد العرض — **إلزامية**

| statusCode | النص المعروض | ممنوع |
|------------|--------------|-------|
| 4 | السائق في الطريق لاستلام الطلب | — |
| 5 | تم استلام الطلب من المطعم | **لا** تعرض «في الطريق إليك» |
| 6 | السائق في الطريق إليك | **لا** تعرض «تم الاستلام» |

#### 1.3 مصدر التحديث

عند **أي** من:
- SignalR `OrderUpdated`
- OneSignal foreground/background (`data.type == "order_status"`)
- فتح شاشة تتبع الطلب

**نفّذ:**
```dart
final res = await api.get('Orders/GetOrderFullDetails/$orderId');
final statusCode = res.data['statusCode'] as int;
// حدّث UI من statusCode فقط — تجاهل title/message للحالة
```

#### 1.4 Progress Stepper (اختياري موصى)

خطوات مرئية بعد الموافقة: `3 → 4 → 5 → 6 → 8`

```dart
int activeStepFromCode(int code) {
  if (code <= 3) return 0;
  if (code == 4) return 1;
  if (code == 5) return 2;
  if (code == 6) return 3;
  if (code >= 8) return 4;
  return 0;
}
```

#### 1.5 SignalR handler

```dart
hub.on('OrderUpdated', (args) {
  final data = args[0] as Map;
  final orderId = data['orderId'] as int;
  // prefer statusCode from payload; fallback statusKey map
  final code = data['statusCode'] as int? ??
      OrderStatusHelper.statusKeyToCode[data['statusKey']] ?? 0;
  orderController.refreshOrderDetails(orderId, expectedCode: code);
});
```

---

## 2) مشكلة سعر التوصيل 3000 → 3 — تطبيق الزبون 👤

### المشكلة
مطعم في الهوير، طلب من المدينة، سعر مُعدّ 3000 د.ع يظهر 3 د.ع.

### المطلوب

#### 2.1 حساب السعر عند Checkout

```dart
Future<decimal> fetchDeliveryFee({
  required int restaurantId,
  required int cityId,
  required double pickupLat,
  required double pickupLng,
  required double dropoffLat,
  required double dropoffLng,
}) async {
  final res = await api.post('pricing/quote', body: {
    'restaurantId': restaurantId,
    'cityId': cityId,
    'pickupLat': pickupLat,
    'pickupLng': pickupLng,
    'dropoffLat': dropoffLat,
    'dropoffLng': dropoffLng,
  });
  if (res.success) {
    return (res.data['total'] as num).toDecimal(); // استخدم total — ليس distanceKm
  }
  // fallback
  final cityRes = await api.get('RestaurantCity/GetDeliveryFee/$restaurantId/$cityId');
  return (cityRes.data['costDelivery'] as num).toDecimal();
}
```

#### 2.2 Formatter — **بدون قسمة**

```dart
import 'package:intl/intl.dart';

String formatIqd(num amount) {
  return '${NumberFormat('#,##0', 'ar_IQ').format(amount)} د.ع';
}
// 3000 → "3,000 د.ع" — وليس "3 د.ع"
```

#### 2.3 أخطاء شائعة — **تجنّبها**

| خطأ | النتيجة |
|-----|---------|
| عرض `distanceKm` (3.0) كسعر | يظهر 3 |
| `amount / 1000` | 3000 → 3 |
| parse `"3.000"` كـ double | 3 بدل 3000 |
| تجاهل `restaurantId` في quote | يحسب بالمسافة فقط |

#### 2.4 Debug

```dart
debugPrint('API total: ${res.data['total']}, source: ${res.data['pricingSource']}');
```

---

## 3) تكبير بطاقات الأصناف — تطبيق الزبون 👤

### المطلوب (شاشة تفاصيل المطعم)

عدّل widget بطاقة الصنف (`ProductCard` / `MenuItemTile`):

```dart
Container(
  constraints: const BoxConstraints(minHeight: 110),
  padding: const EdgeInsets.all(12),
  decoration: BoxDecoration(
    borderRadius: BorderRadius.circular(12),
    border: Border.all(color: Colors.grey.shade300, width: 1.2),
    boxShadow: [
      BoxShadow(
        color: Colors.black.withOpacity(0.06),
        blurRadius: 6,
        offset: const Offset(0, 2),
      ),
    ],
  ),
  child: Row(
    children: [
      ClipRRect(
        borderRadius: BorderRadius.circular(10),
        child: Image.network(imageUrl, width: 72, height: 72, fit: BoxFit.cover),
      ),
      const SizedBox(width: 12),
      Expanded(
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Text(name, style: const TextStyle(fontSize: 16, fontWeight: FontWeight.w600)),
            const SizedBox(height: 4),
            Text(formatIqd(price), style: const TextStyle(fontSize: 15, fontWeight: FontWeight.w700)),
          ],
        ),
      ),
    ],
  ),
)
```

Grid (إن وُجد):
```dart
GridView.builder(
  gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
    crossAxisCount: 2,
    childAspectRatio: 0.82,
    crossAxisSpacing: 10,
    mainAxisSpacing: 10,
  ),
  // ...
)
```

---

## 4) إشعارات المتجر — full-screen vs banner — تطبيق المتجر 🏪

### المشكلة
«طلب جديد» → full-screen ✅ صحيح  
«السائق متجه للمطعم» → full-screen ❌ يجب أن يكون إشعار banner فقط

### المطلوب

#### 4.1 دالة قرار العرض

```dart
bool shouldShowFullScreenOrderAlert(Map<String, dynamic> data) {
  final displayMode = data['displayMode']?.toString() ?? '';
  final statusKey = data['status']?.toString() ?? data['statusKey']?.toString() ?? '';
  return displayMode == 'fullscreen' || statusKey == 'new_order';
}
```

#### 4.2 SignalR handler (المطعم)

```dart
hub.on('OrderUpdated', (args) {
  final data = args[0] as Map<String, dynamic>;
  if (data['audience'] != 'restaurant') return;

  if (shouldShowFullScreenOrderAlert(data)) {
    showFullScreenNewOrderDialog(orderId: data['orderId'], message: data['message']);
  } else {
    showBannerNotification(title: data['title'], body: data['message']);
    // أو SnackBar / local notification — بدون modal ولا navigation إجباري
  }
});
```

#### 4.3 OneSignal foreground

```dart
void onNotificationReceived(OSNotification notification) {
  final data = notification.additionalData ?? {};
  if (data['type'] != 'order_status') return;

  if (shouldShowFullScreenOrderAlert(data)) {
    showFullScreenNewOrderDialog(...);
  } else {
    showBannerNotification(...);
  }
}
```

#### 4.4 جدول سلوك

| statusKey | displayMode | السلوك |
|-----------|-------------|--------|
| `new_order` | `fullscreen` | شاشة كاملة + صوت |
| `driver_en_route` | `banner` | toast/banner فقط |
| `picked_up` | `banner` | toast/banner فقط |
| `approved` | `banner` | toast/banner فقط |

---

## 5) Pull-to-Refresh — تطبيق السائق 🚗

### المشكلة
عند تحديث الحالة أو عرض التفاصيل لا يُحدَّث المحتوى.

### المطلوب

#### 5.1 APIs للـ refresh

| الشاشة | API |
|--------|-----|
| قائمة الطلبات | `GET Orders/GetOrdersByOrderNoAndSaleManId/{orderNo},{saleManId},{type}` |
| تفاصيل طلب | `GET Orders/GetOrderFullDetails/{orderId}` |
| طلبات قريبة | `GET Orders/GetNearbyDriverOrders/{saleManId},{lat},{lng},{radiusKm}` |

#### 5.2 RefreshIndicator — قائمة الطلبات

```dart
RefreshIndicator(
  onRefresh: () async {
    await orderController.loadOrders(saleManId: currentDriverId, force: true);
  },
  child: ListView.builder(
    physics: const AlwaysScrollableScrollPhysics(),
    itemCount: orders.length,
    itemBuilder: ...
  ),
)
```

#### 5.3 RefreshIndicator — تفاصيل الطلب

```dart
RefreshIndicator(
  onRefresh: () async {
    await orderController.loadOrderDetails(orderId, force: true);
  },
  child: SingleChildScrollView(
    physics: const AlwaysScrollableScrollPhysics(),
    child: OrderDetailsBody(...),
  ),
)
```

#### 5.4 Auto-refresh بعد تحديث الحالة

```dart
Future<void> updateStatus(int orderId, String endpoint) async {
  await api.post(endpoint);
  await loadOrderDetails(orderId, force: true);
  await loadOrders(force: true);
}
```

Endpoints:
- `POST Orders/SetDriverEnRouteToPickup/{orderId}`
- `POST Orders/SetPickedUpFromRestaurant/{orderId}`
- `POST Orders/SetOutForDelivery/{orderId}`
- `POST Orders/SetDeliveryConfirmed/{orderId}`

#### 5.5 SignalR

```dart
hub.on('OrderUpdated', (args) {
  final orderId = args[0]['orderId'];
  if (orderController.activeOrderId == orderId) {
    orderController.loadOrderDetails(orderId, force: true);
  }
  orderController.loadOrders(force: true);
});
```

---

## 6) معايير القبول (Acceptance Criteria)

| # | السينario | النتيجة المتوقعة |
|---|-----------|-----------------|
| 1 | سائق يضغط «استلام من المطعم» | زبون يرى statusCode **5** — «تم استلام الطلب من المطعم» |
| 2 | سائق يضغط «في الطريق للزبون» | زبون يرى statusCode **6** — «السائق في الطريق إليك» |
| 3 | مطعم هوير + مدينة بسعر 3000 | زبون يرى **3,000 د.ع** في checkout |
| 4 | متجر — طلب جديد | full-screen overlay |
| 5 | متجر — سائق متجه للمطعم | banner/toast فقط — **لا** full-screen |
| 6 | سائق — pull down على القائمة | تُحدَّث الطلبات والحالات |
| 7 | سائق — pull down على التفاصيل | تُحدَّث تفاصيل الطلب |

---

## 7) checklist تنفيذ

### تطبيق الزبون 👤
- [ ] `OrderStatusHelper` + عرض من `statusCode`
- [ ] SignalR/OneSignal → refresh `GetOrderFullDetails`
- [ ] `pricing/quote` مع `restaurantId` + `cityId`
- [ ] `formatIqd` بدون قسمة
- [ ] تكبير بطاقات الأصناف في تفاصيل المطعم

### تطبيق المتجر 🏪
- [ ] `shouldShowFullScreenOrderAlert` — full-screen فقط لـ `new_order`
- [ ] `driver_en_route` → banner فقط
- [ ] SignalR + OneSignal يستخدمان `displayMode`

### تطبيق السائق 🚗
- [ ] `RefreshIndicator` على القائمة والتفاصيل والرئيسية
- [ ] auto-refresh بعد POST تحديث الحالة
- [ ] SignalR → invalidate + reload

---

## 8) مراجع API

| Endpoint | Method |
|----------|--------|
| `Orders/GetOrderFullDetails/{orderId}` | GET |
| `pricing/quote` | POST |
| `RestaurantCity/GetDeliveryFee/{restaurantId}/{cityId}` | GET |
| `RestaurantCity/GetByResId/{restaurantId}` | GET |
| SignalR `/hubs/orders` — event `OrderUpdated` | WebSocket |

---

**ملاحظة:** لا تفترض أن `GetOrdersWithDetailAll` يعيد array — الشكل: `{ order, details, driver, statusCode }`.
