# FlowOps — نسخة تشغيل مبسطة بـ 2 Microservices

هذه النسخة مخصصة للتشغيل خطوة بخطوة محليًا أولًا، قبل الدخول في Kubernetes و ArgoCD.

## الخدمات الموجودة

```text
FlowOps Portal      → ASP.NET Core MVC Dashboard
user-service        → Microservice لإدارة المستخدمين
product-service     → Microservice لإدارة المنتجات
```

## أول تشغيل باستخدام Docker Compose

نفذ الأوامر من داخل فولدر المشروع:

```bash
cd FlowOps_2Microservices_FIXED

docker compose up --build
```

افتح الروابط التالية:

```text
Portal:          http://localhost:5000
User Service:    http://localhost:5101/version
Product Service: http://localhost:5102/version
```

## تشغيل بدون Docker

افتح 3 terminals.

Terminal 1:

```bash
cd FlowOps.Web
dotnet run --urls http://localhost:5000
```

Terminal 2:

```bash
cd services/user-service
dotnet run --urls http://localhost:5101
```

Terminal 3:

```bash
cd services/product-service
dotnet run --urls http://localhost:5102
```

## اختبار الـ APIs

```bash
curl http://localhost:5101/health
curl http://localhost:5101/api/users
curl http://localhost:5102/health
curl http://localhost:5102/api/products
```

## ماذا يحدث في الـ Portal الآن؟

الـ Portal يعمل بـ Mock integrations حتى يشتغل بدون Kubernetes أو ArgoCD.
هذا مقصود في المرحلة الأولى حتى نثبت أن الواجهة والـ flow يعملان.

بعد ذلك نربط نفس الفكرة بـ Kubernetes و ArgoCD.
