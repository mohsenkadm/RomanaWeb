# Rumana — برومبت تكامل تطبيق الموبايل (API 2026)

> **الغرض:** هذا الملف برومبت جاهز لنسخه إلى Cursor  / فريق الموبايل لتنفيذ كل الميزات الجديدة على تطبيق **الزبون** و**المطعم** و**السائق** ضد باكند RomanaWeb.
>
> **Base URL:** `https://{YOUR_DOMAIN}/` 

---

## 0) قواعد عامة (اقرأها أولاً)

### 0.1 شكل الاستجابة الموحّد

كل الـ APIs ترجع JSON:

```json
{
  "success": true,
  "msg": "رسالة اختيارية",
  "data": { }
}
```

### 0.2 المصادقة JWT

| الهيدر | القيمة |
|--------|--------|
| `Authorization` | `Bearer {token}` |

**محتوى التوكن (UserManager):**

```json
{ "id": 123, "name": "اسم", "role": "user" }
```

| role | التطبيق |
|------|---------|
| `user` | زبون |
| `res` | مطعم (Merchant) |
| `admin` | لوحة ويب (ليس موبايل) |
| بدون role / حسب السائق | تطبيق السائق (يستخدم `id` = `saleManId`) |

### 0.3 رموز الجمهور في هذا الملف

| الرمز | المعنى |
|-------|--------|
| 👤 | **زبون** — Customer App |
| 🏪 | **مطعم** — Restaurant / Merchant App |
| 🚗 | **سائق** — Driver App |
| 👥 | **مشترك** — أكثر من تطبيق |
| 🌐 | **عام** — بدون توكن (AllowAnonymous) |
| 🖥️ | **أدمن ويب** — Dashboard فقط (للمرجع) |

### 0.4 رقم الهاتف (عراقي)

- **11 رقم** إلزامياً في OTP (مثال: `07701234567`).
- الباكند يرسل OTP عبر **واتساب** (VerifyWay).

### 0.5 تغيير Breaking مهم

`GET Orders/GetOrdersWithDetailAll?Id={orderId}` **لم يعد يعيد مصفوفة فقط.**

يعيد كائناً:

```json
{
  "order": { },
  "details": [ ],
  "driver": { } | null,
  "statusCode": 4
}
```

استخدم أيضاً: `GET Orders/GetOrderFullDetails/{orderId}` — نفس الشكل.

---

## 1) SignalR — إشعارات فورية 👥

| البند | القيمة |
|-------|--------|
| **الجمهور** | 👤 🏪 🚗 |
| **الرابط** | `{BASE_URL}hubs/orders?access_token={JWT}` |
| **المصادقة** | JWT في query `access_token` أو Bearer |
| **الحدث** | `OrderUpdated` |

### 1.1 الانضمام للمجموعات (بعد `StartAsync`)

| الاستدعاء | التطبيق |
|-----------|---------|
| `JoinUser(userId)` | 👤 زبون |
| `JoinRestaurant(restaurantId)` | 🏪 مطعم |
| `JoinDriver(saleManId)` | 🚗 سائق |
| `JoinAllDrivers()` | 🚗 سائق (طلبات جديدة قريبة) |

### 1.2 Payload الحدث `OrderUpdated`

```json
{
  "title": "طلبك",
  "message": "نص الإشعار",
  "orderId": 456,
  "statusKey": "preparing",
  "audience": "user",
  "audienceId": 123,
  "at": "2026-05-15T10:00:00Z"
}
```

**statusKey شائعة:** `new_order`, `approved`, `preparing`, `driver_en_route`, `picked_up`, `out_for_delivery`, `confirmed`, `cancel`, `done`

> **ملاحظة:** OneSignal ما زال يعمل — لا تلغِ Push؛ أضف SignalR للتحديث الفوري داخل الشاشة.

---

## 2) تسجيل الدخول OTP واتساب 👤

### 2.1 إرسال كود OTP

| البند | التفاصيل |
|-------|----------|
| **الجمهور** | 👤 زبون |
| **Method** | `POST` |
| **الرابط الكامل** | `{BASE_URL}Users/Login/SendOtp/{Phone}` |
| **مثال** | `POST https://api.rumana.iq/Users/Login/SendOtp/07701234567` |
| **Auth** | 🌐 لا يحتاج توكن |
| **Path Params** | `Phone` — 11 رقم |

**Response نجاح:**

```json
{ "success": true, "msg": "تم ارسال كود التحقق عبر الواتساب" }
```

**Response فشل (أمثلة):**

```json
{ "success": false, "msg": "يجب كتابة رقم الهاتف 11 رقما" }
{ "success": false, "msg": "حسابك محظور، يرجى التواصل مع الادارة" }
{ "success": false, "msg": "حسابك غير فعال" }
```

**سلوك الباكند:** إن لم يوجد مستخدم → يُنشأ تلقائياً ثم يُرسل OTP.

**مهمة UI:** شاشة «أدخل رقم الهاتف» → زر «إرسال الكود».

---

### 2.2 التحقق من OTP وتسجيل الدخول

| البند | التفاصيل |
|-------|----------|
| **الجمهور** | 👤 زبون |
| **Method** | `POST` |
| **الرابط الكامل** | `{BASE_URL}Users/Login/VerifyOtp/{Phone},{Code}` |
| **مثال** | `POST https://api.rumana.iq/Users/Login/VerifyOtp/07701234567,4829` |
| **Auth** | 🌐 |
| **Path Params** | `Phone` (11 رقم), `Code` (4 أرقام) |

**Response نجاح — `data`:**

```json
{
  "userId": 15,
  "name": "07701234567",
  "phone": "07701234567",
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "isConfirm": true,
  "isActive": true
}
```

**Response فشل:**

```json
{ "success": false, "msg": "كود التحقق غير صحيح" }
```

**مهمة UI:**

1. احفظ `token` في SecureStorage.
2. أرسل `Authorization: Bearer {token}` في كل طلب.
3. أزل تسجيل الدخول بكلمة المرور للزبون (Legacy: `GET Users/Login?UserName=&password=`).

---

### 2.3 تجديد التوكن (اختياري)

| البند | التفاصيل |
|-------|----------|
| **الجمهور** | 👤 زبون |
| **Method** | `POST` |
| **الرابط** | `{BASE_URL}Users/RefreshToken/{Id}` |
| **Auth** | 🌐 أو Bearer |
| **مثال** | `POST .../Users/RefreshToken/15` |

---

## 3) شاشة افتتاح التطبيق (Splash) 👤 🌐

### 3.1 جلب الشاشة للتطبيق

| البند | التفاصيل |
|-------|----------|
| **الجمهور** | 👤 زبون (عند فتح التطبيق) |
| **Method** | `GET` |
| **الرابط الكامل** | `{BASE_URL}AppSplash/GetForApp` |
| **Auth** | 🌐 بدون توكن |

**Response — معروضة:**

```json
{
  "success": true,
  "data": {
    "imageUrl": "https://domain/Uplouds/image-xxx.jpg",
    "details": "عرض خاص — مرحباً بكم",
    "isVisible": true
  }
}
```

**Response — غير معروضة أو لا يوجد سجل:**

```json
{ "success": true, "data": null }
```

**مهمة UI:**

- استدعِ عند `main()` / بعد Splash native.
- إن `data != null && isVisible` → Full-screen dialog مع صورة + نص + زر «متابعة».
- صف **واحد فقط** في DB (الأدمن يديره من الويب).

### 3.2 إدارة الشاشة (مرجع — 🖥️ أدمن)

| الرابط | الجمهور |
|--------|---------|
| `GET AppSplash/GetAdmin` | 🖥️ |
| `POST AppSplash/Save` (form: Image, Details, IsVisible) | 🖥️ |

---

## 4) المنتجات — حقول وروابط جديدة

### 4.1 حقول JSON جديدة على `Products`

| الحقل | النوع | الوصف |
|-------|------|--------|
| `preparationTimeMinutes` | int | وقت التحضير بالدقائق (افتراضي 15) |
| `isAvailable` | bool | `true` = متوفر |

---

### 4.2 قائمة منتجات المطعم (موجود — مع الحقول الجديدة)

| البند | التفاصيل |
|-------|----------|
| **الجمهور** | 👤 زبون |
| **Method** | `GET` |
| **الرابط** | `{BASE_URL}Products/GetByRestaurantId/{RestaurantId},{SubCategoriesId},{prodname}` |
| **مثال** | `GET .../Products/GetByRestaurantId/5,0,بيتزا` |
| **Auth** | 🌐 |
| **ملاحظة** | فلتر `isAvailable == false` في التطبيق — لا تسمح بالإضافة للسلة |

**مهمة UI 👤:** اعرض `preparationTimeMinutes` في بطاقة المنتج.

---

### 4.3 الأكثر طلباً لكل مطعم ⭐ جديد

| البند | التفاصيل |
|-------|----------|
| **الجمهور** | 👤 زبون |
| **Method** | `GET` |
| **الرابط** | `{BASE_URL}Products/GetTopSellingByRestaurant/{restaurantId}?take=20` |
| **مثال** | `GET .../Products/GetTopSellingByRestaurant/5?take=10` |
| **Auth** | 🌐 |

**Response `data` (مصفوفة):**

```json
[
  {
    "productsId": 101,
    "totalOrdered": 340,
    "productsName": "برجر كلاسيك",
    "productsPrice": 8000,
    "preparationTimeMinutes": 20,
    "isAvailable": true,
    "productsImageFirst": "https://..."
  }
]
```

**مهمة UI 👤:** قسم «الأكثر طلباً» أعلى صفحة المطعم.

---

### 4.4 تغيير توفر المنتج ⭐ جديد

| البند | التفاصيل |
|-------|----------|
| **الجمهور** | 🏪 مطعم |
| **Method** | `POST` |
| **الرابط** | `{BASE_URL}Products/SetIsAvailable/{Id}/{isAvailable}` |
| **مثال** | `POST .../Products/SetIsAvailable/101/false` |
| **Auth** | Bearer — `role=res` |
| **Path** | `Id` = productsId, `isAvailable` = `true` أو `false` |

**Response:**

```json
{ "success": true, "msg": "المنتج غير متوفر", "data": { /* Products */ } }
```

**قيود:** المطعم يعدّل منتجاته فقط (`RestaurantId` من التوكن).

**مهمة UI 🏪:** Toggle «متوفر / غير متوفر» في قائمة المنتجات.

---

## 5) البرومو كود — حد الاستخدام للشخص 👤

### 5.1 التحقق من الكود

| البند | التفاصيل |
|-------|----------|
| **الجمهور** | 👤 زبون |
| **Method** | `GET` |
| **الرابط** | `{BASE_URL}PromoCode/ValidatePromoCode?promoName={code}&restaurantId={id}&userId={userId}` |
| **مثال** | `GET .../PromoCode/ValidatePromoCode?promoName=RAMADAN&restaurantId=5&userId=15` |
| **Auth** | 🌐 أو Bearer (إن أرسلت Bearer و`userId=0` يأخذ Id من التوكن) |

**Query:**

| Param | إلزامي | الوصف |
|-------|--------|--------|
| `promoName` | نعم | نص الكود |
| `restaurantId` | نعم | مطعم الطلب |
| `userId` | **نعم للتحقق من الحد** | `userId` الزبون |

**فشل حد الشخص:**

```json
{ "success": false, "msg": "لقد استخدمت هذا الكود الحد الأقصى المسموح لك" }
```

**حقل جديد في Promo:** `maxUsagePerUser` (افتراضي 1، 0 = غير محدود للشخص).

---

### 5.2 تطبيق الخصم على الطلب

| البند | التفاصيل |
|-------|----------|
| **الجمهور** | 👤 زبون |
| **Method** | `POST` |
| **الرابط** | `{BASE_URL}PromoCode/ApplyPromoCode?promoName={code}&restaurantId={id}&orderTotal={amount}&userId={userId}` |
| **مثال** | `POST .../PromoCode/ApplyPromoCode?promoName=RAMADAN&restaurantId=5&orderTotal=50000&userId=15` |
| **Auth** | 🌐 أو Bearer |

**Response `data` (مختصر):**

```json
{
  "promoName": "RAMADAN",
  "discountValue": 5000,
  "netAmount": 45000,
  "originalTotal": 50000,
  "fundedByStore": false
}
```

**مهمة UI 👤:** أرسل `userId` دائماً في Validate و Apply.

---

## 6) الطلبات — تفاصيل + سائق + حالات جديدة

### 6.1 آلة الحالات `statusCode` 👥

يُحسب في الباكند بهذا الترتيب:

| code | الحالة بالعربي | متى |
|------|----------------|-----|
| 0 | انتظار موافقة المطعم | افتراضي بعد الإنشاء |
| 1 | موافق عليه | `isApporve=true` |
| 2 | قيد التحضير | `isPreparing=true` |
| 3 | السائق قبل الطلب | `isSaleManApprove=true` |
| 4 | السائق في الطريق لاستلام الطلب | `isDriverEnRouteToPickup=true` |
| 5 | تم الاستلام من المطعم | `isPickedUpFromRestaurant=true` |
| 6 | قيد التوصيل للزبون | `isOutForDelivery=true` |
| 7 | تم التوصيل | `isDelivered=true` |
| 8 | تم تأكيد الاستلام | `isDeliveryConfirmed=true` |
| 9 | ملغي | `isCancel=true` |

**حقول Boolean جديدة على `order`:**

```json
{
  "isPreparing": false,
  "isDriverEnRouteToPickup": false,
  "isPickedUpFromRestaurant": false,
  "isOutForDelivery": false,
  "isDeliveryConfirmed": false
}
```

---

### 6.2 تفاصيل الطلب الكاملة (مع السائق) ⭐ جديد / معدّل

| البند | التفاصيل |
|-------|----------|
| **الجمهور** | 👤 🏪 🚗 |
| **Method** | `GET` |
| **الرابط 1** | `{BASE_URL}Orders/GetOrderFullDetails/{OrderId}` |
| **الرابط 2** | `{BASE_URL}Orders/GetOrdersWithDetailAll?Id={OrderId}` |
| **مثال** | `GET .../Orders/GetOrderFullDetails/456` |
| **Auth** | Bearer (مُستحسن) |

**Response `data`:**

```json
{
  "order": {
    "orderId": 456,
    "orderNo": 1205,
    "restaurantId": 5,
    "userId": 15,
    "saleManId": 8,
    "netAmount": 25000,
    "isApporve": true,
    "isPreparing": true,
    "isDriverEnRouteToPickup": false,
    "phone": "07701234567",
    "address": "..."
  },
  "details": [
    {
      "orderDetailId": 1,
      "productsName": "برجر",
      "count": 2,
      "price": 8000
    }
  ],
  "driver": {
    "saleManId": 8,
    "name": "أحمد",
    "phone": "07709876543",
    "lat": "33.31",
    "long": "44.36",
    "isAvailable": true
  },
  "statusCode": 2
}
```

`driver` = `null` إن لم يُعيَّن سائق.

**مهمة UI:**

- 👤 شاشة تتبع الطلب: اعرض `statusCode` + بيانات `driver` (اسم، هاتف، اتصال).
- 🏪 تفاصيل الطلب في لوحة المطعم.
- 🚗 تفاصيل قبل التوصيل.

---

### 6.3 إنشاء طلب 👤

| البند | التفاصيل |
|-------|----------|
| **الجمهور** | 👤 زبون |
| **Method** | `POST` |
| **الرابط** | `{BASE_URL}Orders` |
| **Body** | `OrdersModel` (JSON) — كما كان |
| **Auth** | Bearer |

**بعد النجاح:** إشعار 🏪 + 🚗 (SignalR `new_order` + OneSignal).

---

### 6.4 موافقة المطعم على الطلب

| البند | التفاصيل |
|-------|----------|
| **الجمهور** | 🏪 مطعم |
| **Method** | `POST` |
| **الرابط** | `{BASE_URL}Orders/SetIsApporve/{OrderId}` |
| **مثال** | `POST .../Orders/SetIsApporve/456` |
| **Auth** | Bearer `role=res` |

**يُشعر:** 👤 (موافقة) — قد يبدأ dispatch للسائقين تلقائياً.

---

### 6.5 قيد التحضير ⭐ جديد

| البند | التفاصيل |
|-------|----------|
| **الجمهور** | 🏪 مطعم |
| **Method** | `POST` |
| **الرابط** | `{BASE_URL}Orders/SetIsPreparing/{OrderId}` |
| **مثال** | `POST .../Orders/SetIsPreparing/456` |
| **Auth** | Bearer `role=res` |
| **شرط** | الطلب يجب أن يكون `isApporve=true` |

**يُشعر:** 👤 — «طلبك قيد التحضير» (Push + SignalR `preparing`).

**مهمة UI 🏪:** زر «بدء التحضير» بعد الموافقة.

---

### 6.6 إلغاء الطلب

| البند | التفاصيل |
|-------|----------|
| **الجمهور** | 🏪 مطعم (أو حسب صلاحياتكم) |
| **Method** | `DELETE` |
| **الرابط** | `{BASE_URL}Orders/SetIsCancel/{OrderId}` |
| **يُشعر** | 👤 |

---

### 6.7 قبول السائق للطلب

| البند | التفاصيل |
|-------|----------|
| **الجمهور** | 🚗 سائق |
| **Method** | `POST` |
| **الرابط** | `{BASE_URL}Orders/ApproveOrderBySaleMan/{OrderId},{SaleManId}` |
| **مثال** | `POST .../Orders/ApproveOrderBySaleMan/456,8` |
| **Auth** | Bearer |

**يُشعر:** 🏪 + 👤

**مهمة UI 🚗:** زر «قبول الطلب» من قائمة الطلبات القريبة.

---

### 6.8 السائق في الطريق لاستلام الطلب ⭐ جديد

| البند | التفاصيل |
|-------|----------|
| **الجمهور** | 🚗 سائق |
| **Method** | `POST` |
| **الرابط** | `{BASE_URL}Orders/SetDriverEnRouteToPickup/{OrderId}` |
| **مثال** | `POST .../Orders/SetDriverEnRouteToPickup/456` |
| **Auth** | Bearer |
| **شرط** | يوجد `saleManId` معيّن |

**يُشعر:** 👤 + 🏪 — SignalR `driver_en_route`

**مهمة UI 🚗:** زر «في الطريق لاستلام الطلب من المطعم».

---

### 6.9 تم استلام الطلب من المطعم ⭐ جديد

| البند | التفاصيل |
|-------|----------|
| **الجمهور** | 🚗 سائق |
| **Method** | `POST` |
| **الرابط** | `{BASE_URL}Orders/SetPickedUpFromRestaurant/{OrderId}` |
| **يُشعر** | 👤 + 🏪 — `picked_up` |

**مهمة UI 🚗:** زر «استلمت الطلب من المطعم».

---

### 6.10 قيد التوصيل للزبون ⭐ جديد

| البند | التفاصيل |
|-------|----------|
| **الجمهور** | 🚗 سائق |
| **Method** | `POST` |
| **الرابط** | `{BASE_URL}Orders/SetOutForDelivery/{OrderId}` |
| **يُشعر** | 👤 — `out_for_delivery` |

**مهمة UI 🚗:** زر «في الطريق للزبون».

---

### 6.11 تأكيد استلام الزبون ⭐ جديد

| البند | التفاصيل |
|-------|----------|
| **الجمهور** | 🚗 سائق (أو 👤 إن أضفتم UX تأكيد من الزبون لاحقاً) |
| **Method** | `POST` |
| **الرابط** | `{BASE_URL}Orders/SetDeliveryConfirmed/{OrderId}` |
| **يُشعر** | 👤 + 🏪 — `confirmed` |
| **يضبط أيضاً** | `isDelivered=true`, `isDone=true` |

**مهمة UI 🚗:** زر «تم التسليم / تأكيد الاستلام».

---

### 6.12 طلبات قريبة للسائق

| البند | التفاصيل |
|-------|----------|
| **الجمهور** | 🚗 سائق |
| **Method** | `GET` |
| **الرابط** | `{BASE_URL}Orders/GetNearbyDriverOrders/{SaleManId},{Lat},{Lng},{RadiusKm}` |
| **مثال** | `GET .../Orders/GetNearbyDriverOrders/8,33.31,44.36,5` |
| **Auth** | 🌐 (حسب إعدادكم — تحقق من السيرفر) |

---

### 6.13 قوائم الطلبات (موجودة — مرجع)

| الرابط | الجمهور | الاستخدام |
|--------|---------|-----------|
| `GET Orders/GetOrdersByOrderNoAndUserId/{OrderNo},{PersonId}` | 👤 | طلباتي |
| `GET Orders/GetOrdersByOrderNoAndRestaurantId/{OrderNo},{RestaurantId},{Type}` | 🏪 | طلبات المطعم |
| `GET Orders/GetOrdersByOrderNoAndSaleManId/{OrderNo},{SaleManId},{Type}` | 🚗 | طلبات السائق |
| `GET Orders/GetAll?OrderNo=&RestaurantName=&datefrom=&dateto=&Phone=&orderStatus=` | 🖥️🏪 | فلترة (ويب/مطعم) |

**Query جديد للفلترة:** `Phone`, `orderStatus` (0–9 كما §6.1).

---

### 6.14 Endpoints قديمة ما زالت مستخدمة

| الرابط | الجمهور | ملاحظة |
|--------|---------|--------|
| `POST Orders/SetIsDelivered/{OrderId}` | 🚗 | توصيل (قديم) |
| `POST Orders/SetIsDone/{OrderId}` | 🏪/🚗 | إنهاء |
| `POST Orders/SetIsSaleManApprove/{OrderId}` | 🚗 | موافقة سائق |
| `POST Orders/SetIsSaleManCancel/{OrderId}` | 🚗 | رفض |
| `POST Orders/SetIsNotDelivered/{OrderId}/{Reason}` | 🚗 | فشل توصيل |
| `POST Orders/SetIsWaiting/{OrderId}/{Reason2}` | 🚗 | تأجيل |

---

## 7) السائق — تسجيل دخول وتوفر 🚗

> **ملاحظة:** السائق **غير مربوط بمطعم** في المنطق الجديد — `restaurantId` اختياري في DB فقط.

### 7.1 تسجيل دخول السائق (كلمة مرور — لم يتغير)

| البند | التفاصيل |
|-------|----------|
| **الجمهور** | 🚗 سائق |
| **Method** | `GET` |
| **الرابط** | `{BASE_URL}SaleMan/Login?Phone={phone}&password={pass}` |
| **Auth** | 🌐 |

---

### 7.2 تبديل حالة العمل (متاح / متوقف)

| البند | التفاصيل |
|-------|----------|
| **الجمهور** | 🚗 سائق |
| **Method** | `POST` |
| **الرابط** | `{BASE_URL}SaleMan/ToggleMyAvailability` |
| **Auth** | Bearer |

(أو `SaleMan/SetAvailability` — راجع Swagger)

**مهمة UI 🚗:** زر «متاح للعمل» — عند `false` لا يستلم dispatch.

---

## 8) مخطط تدفق الطلب (للمطور)

```
👤 ينشئ طلب          POST /Orders
        ↓
🏪 يوافق             POST /Orders/SetIsApporve/{id}
        ↓
🏪 قيد التحضير       POST /Orders/SetIsPreparing/{id}     ← جديد
        ↓
🚗 يقبل              POST /Orders/ApproveOrderBySaleMan/{id},{saleManId}
        ↓
🚗 في الطريق للمطعم POST /Orders/SetDriverEnRouteToPickup/{id}   ← جديد
        ↓
🚗 استلم من المطعم  POST /Orders/SetPickedUpFromRestaurant/{id} ← جديد
        ↓
🚗 في الطريق للزبون POST /Orders/SetOutForDelivery/{id}         ← جديد
        ↓
🚗 تأكيد التسليم     POST /Orders/SetDeliveryConfirmed/{id}      ← جديد
```

في كل خطوة: استمع لـ `OrderUpdated` على SignalR + حدّث UI.

---

## 9) Checklist تنفيذ حسب التطبيق

### 👤 تطبيق الزبون

- [ ] `POST Users/Login/SendOtp/{phone}`
- [ ] `POST Users/Login/VerifyOtp/{phone},{code}` + حفظ token
- [ ] `GET AppSplash/GetForApp` عند البدء
- [ ] `GET Products/GetTopSellingByRestaurant/{id}`
- [ ] فلترة `isAvailable` + عرض `preparationTimeMinutes`
- [ ] `GET PromoCode/ValidatePromoCode` + `ApplyPromoCode` مع `userId`
- [ ] `POST Orders` إنشاء طلب
- [ ] `GET Orders/GetOrderFullDetails/{id}` + عرض driver + statusCode
- [ ] SignalR: `JoinUser` + `OrderUpdated`

### 🏪 تطبيق المطعم

- [ ] `POST Orders/SetIsApporve/{id}`
- [ ] `POST Orders/SetIsPreparing/{id}`
- [ ] `POST Products/SetIsAvailable/{id}/{bool}`
- [ ] `GET Orders/GetOrderFullDetails/{id}`
- [ ] SignalR: `JoinRestaurant`

### 🚗 تطبيق السائق

- [ ] `GET SaleMan/Login`
- [ ] `GET Orders/GetNearbyDriverOrders/...`
- [ ] `POST Orders/ApproveOrderBySaleMan/{orderId},{saleManId}`
- [ ] `POST Orders/SetDriverEnRouteToPickup/{id}`
- [ ] `POST Orders/SetPickedUpFromRestaurant/{id}`
- [ ] `POST Orders/SetOutForDelivery/{id}`
- [ ] `POST Orders/SetDeliveryConfirmed/{id}`
- [ ] SignalR: `JoinDriver` + `JoinAllDrivers`

---

## 10) أوامر اختبار سريعة (cURL)

استبدل `BASE` و `TOKEN` و `PHONE`:

```bash
# OTP
curl -X POST "BASE/Users/Login/SendOtp/07701234567"
curl -X POST "BASE/Users/Login/VerifyOtp/07701234567,1234"

# Splash
curl "BASE/AppSplash/GetForApp"

# Top products
curl "BASE/Products/GetTopSellingByRestaurant/5?take=10"

# Order details
curl -H "Authorization: Bearer TOKEN" "BASE/Orders/GetOrderFullDetails/1"

# Preparing (restaurant token)
curl -X POST -H "Authorization: Bearer RES_TOKEN" "BASE/Orders/SetIsPreparing/1"
```

---

## 11) تعليمات لـ AI المطور (انسخ كقواعد)

1. نفّذ فقط الروابط المرتبطة بجمهور التطبيق (👤/🏪/🚗) حسب الجدول.
2. لا تفترض أن `GetOrdersWithDetailAll` يعيد array — تعامل مع `{ order, details, driver, statusCode }`.
3. أرسل `userId` في كل استدعاء Promo.
4. رقم الهاتف 11 رقم في OTP.
5. أبقِ OneSignal و أضف SignalR.
6. اعرض حالة الطلب للزبون من `statusCode` مع نص عربي واضح.
7. السائق: أزرار متسلسلة حسب §6.8–6.11 لا تتخطى الخطوات.
8. المنتجات: `isAvailable=false` غير قابلة للطلب.
9. استخدم Swagger على السيرفر: `{BASE_URL}swagger` للتحقق النهائي.

---

*آخر تحديث: ميزات RomanaWeb 2026 — بعد سكربت `Migrations/SQL/2026_Features_Upgrade.sql`*
