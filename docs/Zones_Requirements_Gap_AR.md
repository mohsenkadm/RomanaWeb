# فحص المتطلبات — PDF Rev.0001 مقابل Backend

آخر مراجعة: 2026-07-03  
المصدر: `docs/Rummana Area price Rev.0001.pdf`

---

## ✅ مكتمل

| الفئة | البنود |
|-------|--------|
| **تسعير LZA/ECA** | Zone، مصفوفة، OSRM، محاكي، APIs |
| **زبون** | OTP، coverage، طلب خدمة |
| **مطاعم↔زونات (PDF §2.2)** | `RestaurantZone` + فلترة `GetByUserLocation` + **واجهة في `/Home/Restaurant`** |
| **مندوبين↔زونات (PDF §2.3.1)** | `SaleManZone` + dispatch + **واجهة في `/Home/SaleMan`** |
| **نشاط المندوبين** | `/Home/DriverActivity` |
| **لوحة الطوارئ** | `/Home/EmergencyOrders` |
| **دورة الطلب** | SignalR، workflow، تتبع موقع |
| **برومبت Flutter** | `docs/Rumana_Flutter_Unified_Prompt_AR.md` |

---

## كيفية الاستخدام (Admin)

### مطعم — زونات الخدمة
1. `/Home/Restaurant` → تعديل مطعم
2. قسم **زونات الخدمة** — اختر الزونات (مثال: المدينة، الصادق، الهوير)
3. عند إدخال Lat/Long يُقترح زون موقع المطعم تلقائياً
4. الجدول يعرض عمود **زونات الخدمة**

### مندوب — زونات العمل
1. `/Home/SaleMan` → تعديل مندوب
2. قسم **زونات العمل** — اختر الزونات التي يعمل بها
3. الجدول يعرض عمود **زونات العمل**

---

## Migrations SQL

1. `2026_ZonePricing_LZA_ECA.sql`
2. `2026_Remove_AppSettings_Menu.sql`
3. `2026_DriverActivity_Permission.sql`
4. `2026_EmergencyOrders_Permission.sql`
