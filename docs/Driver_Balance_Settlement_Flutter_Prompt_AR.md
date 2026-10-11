# برومبت Flutter — رصيد المندوب وتصفير الحساب (Backend جاهز)

استخدم هذا البرومبت مع مشروع **تطبيق المندوب** فقط. الـ Backend موجود في RomanaWeb؛ **لا تغيّر الـ Backend**.

---

## الفكرة

لكل مندوب رصيدان منفصلان يظهران كمربعين في واجهة التطبيق:

1. **مبلغ الأكل الصافي** (`foodNetBalance`) — بدون أجور توصيل
2. **مستحق أجور التوصيل** (`deliveryFeeBalance`) — أجور التوصيل فقط

عند تسليم الطلب (تأكيد التوصيل) يزيد الرصيدان تلقائياً من جهة السيرفر.
تصفير الحساب يتم من **لوحة التحكم فقط** (زر «تصفير الحساب»). التطبيق يعرض الرصيد الحالي فقط.

---

## §1 — استبدل المسارات

```
{DRIVER_APP_ROOT}  = مسار تطبيق المندوب
{BASE_URL}         = https://YOUR-DOMAIN/     ← بدون api/ في النهاية
```

JWT role = `sal`

---

## §2 — API الرصيد

### 2.1 رصيدي الحالي (بعد الدخول / عند فتح الشاشة الرئيسية / pull-to-refresh)

```
GET {BASE_URL}SaleMan/MyBalance
Authorization: Bearer {token}
```

**Response (`data`):**
```json
{
  "saleManId": 7,
  "name": "علي",
  "phone": "07701112222",
  "foodNetBalance": 45000,
  "deliveryFeeBalance": 6500,
  "openOrdersCount": 12,
  "updatedAt": "2026-10-11T20:00:00"
}
```

| حقل | المعنى | العرض |
|-----|--------|--------|
| `foodNetBalance` | مبلغ الأكل الصافي المطلوب تسليمه للمدير | مربع 1 |
| `deliveryFeeBalance` | مستحق أجور التوصيل | مربع 2 |
| `openOrdersCount` | عدد الطلبات الداخلة في الرصيد الحالي (قبل التصفير) | اختياري تحت المربعين |

المبالغ بـ **دينار عراقي** مباشرة — استخدم `formatIqd` الموجود (عرض `45,000 د.ع` وليس قسمة على 1000).

### 2.2 الرصيد أيضاً مع Login

```
GET {BASE_URL}SaleMan/Login?Phone=...&password=...
```

`data` يتضمن الآن أيضاً:
```json
{
  "saleManId": 7,
  "name": "علي",
  "token": "eyJ...",
  "foodNetBalance": 45000,
  "deliveryFeeBalance": 6500,
  "...": "..."
}
```

اعرض المربعين فوراً من Login، ثم حدّثهما من `MyBalance` عند refresh.

---

## §3 — واجهة التطبيق (مربعان)

### 3.1 مكان العرض

في الشاشة الرئيسية للمندوب (Home / Dashboard) — **فوق قائمة الطلبات** أو تحت شريط التوفر (يعمل / متوقف):

```
┌─────────────────────────┐  ┌─────────────────────────┐
│ مبلغ الأكل الصافي       │  │ مستحق أجور التوصيل      │
│ 45,000 د.ع              │  │ 6,500 د.ع               │
└─────────────────────────┘  └─────────────────────────┘
```

### 3.2 قواعد UI

- مربعان جنباً إلى جنب على الشاشات العريضة؛ تحت بعض على الموبايل الضيق.
- لا تستخدم أزرار تصفير داخل التطبيق — التصفير من الأدمن فقط.
- عند `foodNetBalance == 0 && deliveryFeeBalance == 0` اعرض الأصفار بشكل طبيعي (ليس empty state).
- بعد كل `SetDeliveryConfirmed` ناجح: استدعِ `MyBalance` وحدّث المربعين.
- Pull-to-refresh على الشاشة الرئيسية يعيد جلب `MyBalance` مع الطلبات.

### 3.3 نموذج بيانات مقترح

```dart
class DriverBalance {
  final int saleManId;
  final double foodNetBalance;
  final double deliveryFeeBalance;
  final int openOrdersCount;

  factory DriverBalance.fromJson(Map<String, dynamic> j) => DriverBalance(
    saleManId: j['saleManId'] ?? 0,
    foodNetBalance: (j['foodNetBalance'] as num?)?.toDouble() ?? 0,
    deliveryFeeBalance: (j['deliveryFeeBalance'] as num?)?.toDouble() ?? 0,
    openOrdersCount: j['openOrdersCount'] ?? 0,
  );
}
```

---

## §4 — متى يتغير الرصيد؟

| الحدث | من يفعله | أثر على الرصيد |
|-------|----------|----------------|
| تأكيد التوصيل `SetDeliveryConfirmed` | المندوب | يزيد الأكل الصافي + أجور التوصيل |
| تصفير الحساب | الأدمن من لوحة التحكم | يصبح الرصيدان = 0 (البيانات والطلبات تبقى) |

التطبيق **لا يستدعي** API التصفير.

---

## §5 — Checklist تنفيذ

1. أضف `DriverBalance` model + repository method `getMyBalance()`.
2. خزّن الرصيد في state الشاشة الرئيسية (أو provider/bloc موجود).
3. ابنِ ويدجت مربعين `DriverBalanceBoxes`.
4. اربطها بـ Login response + `GET SaleMan/MyBalance`.
5. حدّث بعد `SetDeliveryConfirmed` وبعد pull-to-refresh.
6. لا تغيّر Backend ولا تضف endpoint جديد.

---

## §6 — أخطاء شائعة

| الخطأ | الصحيح |
|-------|--------|
| جمع الأكل + التوصيل في رقم واحد | مربعان منفصلان |
| قسمة المبلغ على 1000 | عرض مباشر بـ `formatIqd` |
| زر تصفير داخل التطبيق | التصفير من الداشبورد فقط |
| الاعتماد على Login فقط بدون refresh | بعد التسليم استدعِ `MyBalance` |
