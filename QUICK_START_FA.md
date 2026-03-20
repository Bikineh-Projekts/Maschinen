# 🎯 خلاصه سریع - چیکار کنم؟

## فایل‌ها رو آپلود کن:

### 1. فایل‌های ROOT (کنار پوشه Maschin):
```
✅ Dockerfile (جایگزین کن)
✅ supervisord.conf (جدید)
✅ start.sh (جدید)
✅ .gitignore (جایگزین کن)
✅ .env.example (جدید)
```

### 2. پوشه market_pipeline (کل پوشه جدیده):
```
✅ کل پوشه market_pipeline/ رو کپی کن
```

## Git Commands:

```bash
git add .
git commit -m "Add Python pipeline"
git push origin main
```

## Environment Variables در Render:

```bash
# .NET (همون قبلی):
ConnectionStrings__DefaultConnection=Host=...;Database=...;Username=...;Password=...

# Python Database (همون PostgreSQL):
PGHOST=dpg-xxxxx.render.com
PGPORT=5432
PGDATABASE=نام-دیتابیس
PGUSER=یوزر
PGPASSWORD=پسورد

# API Keys:
FINNHUB_API_KEY=d6uofkpr01qig545a5i0d6uofkpr01qig545a5ig
ALPHAVANTAGE_API_KEY=L9QID1181GO59813
TWELVEDATA_API_KEY=2e7a3e33e3ce4e30931e52c90e10a24f

# تنظیمات:
SYMBOLS=AAPL,MSFT,GOOGL
POLL_INTERVAL_SECONDS=3600
```

## اتمام!

Render خودش Build می‌کنه. بعد:
- ✅ .NET App کار می‌کنه
- ✅ Python Pipeline هر ساعت داده جمع می‌کنه

**هزینه:** همون یک Service = یک هزینه!

برای جزئیات بیشتر: **INSTRUCTIONS.md** و **DEPLOY_GUIDE.md**
