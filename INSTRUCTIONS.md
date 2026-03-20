# 🚀 فایل‌های آماده برای اضافه کردن Python Pipeline

## ✅ فایل‌هایی که باید به repository اضافه کنی:

### 📁 در ROOT پروژه (کنار پوشه Maschin):

1. **Dockerfile** (جایگزین Dockerfile قبلی)
2. **supervisord.conf** (جدید)
3. **start.sh** (جدید)
4. **.gitignore** (به‌روز شده)
5. **.env.example** (جدید - template بدون API keys واقعی)
6. **DEPLOY_GUIDE.md** (راهنمای کامل دیپلوی)

### 📁 پوشه market_pipeline/ (کل این پوشه جدید است):

```
market_pipeline/
├── collectors/
│   ├── __init__.py
│   ├── alphavantage.py
│   ├── finnhub.py
│   └── twelvedata.py
├── db/
│   ├── __init__.py
│   └── connection.py
├── main.py
├── requirements.txt
└── README.md
```

## 📋 مراحل کار:

### 1️⃣ آپلود به GitHub

```bash
# کپی همه فایل‌ها به repository خودت
# ساختار نهایی:
your-repo/
├── Maschin/              # موجود قبلی
├── market_pipeline/      # جدید
├── Dockerfile            # جایگزین
├── supervisord.conf      # جدید
├── start.sh              # جدید
├── .gitignore            # به‌روز
└── .env.example          # جدید

# Push به GitHub:
git add .
git commit -m "Add Python market pipeline with multi-app Dockerfile"
git push origin main
```

### 2️⃣ تنظیم Environment Variables در Render

برو به Web Service → **Environment** tab:

```bash
# .NET Connection String (همون قبلی):
ConnectionStrings__DefaultConnection=Host=your-db;Database=your-db;Username=user;Password=pass;Port=5432;SSL Mode=Require;Trust Server Certificate=true;

# Python Database Config (همون PostgreSQL):
PGHOST=your-postgres-host.render.com
PGPORT=5432
PGDATABASE=your-db-name
PGUSER=your-user
PGPASSWORD=your-password

# API Keys (از فایل _env که دادی):
FINNHUB_API_KEY=d6uofkpr01qig545a5i0d6uofkpr01qig545a5ig
ALPHAVANTAGE_API_KEY=L9QID1181GO59813
TWELVEDATA_API_KEY=2e7a3e33e3ce4e30931e52c90e10a24f

# Pipeline Config:
SYMBOLS=AAPL,MSFT,GOOGL,TSLA
POLL_INTERVAL_SECONDS=3600
ALPHA_MAX_RECORDS=10
TWELVE_OUTPUTSIZE=30
```

### 3️⃣ دیپلوی

Render خودکار شروع به Build می‌کنه. بعد از 5-10 دقیقه:
- ✅ .NET App روی Port 8080
- ✅ Python Pipeline هر ساعت اجرا می‌شه

## 🎯 تفاوت با قبل:

| قبل | حالا |
|-----|------|
| فقط .NET App | .NET App + Python Pipeline |
| 1 Container | 1 Container (هر دو با Supervisor) |
| 1 هزینه | همون 1 هزینه! |

## ⚠️ نکات مهم:

1. **PostgreSQL یکی باشه:** هر دو از همون database استفاده می‌کنن
2. **API Keys:** در .env.example نذار! فقط در Render Environment Variables
3. **POLL_INTERVAL:** توصیه = 3600 (1 ساعت) به خاطر محدودیت Alpha Vantage
4. **.gitignore:** اضافه شده تا .env واقعی commit نشه

## 📊 بررسی عملکرد:

### Logs در Render:
```
Starting Multi-Application Container
→ .NET Web App: MaschinenDataein (Port 8080)
→ Python Pipeline: Market Data Collector

Cycle #1  |  2026-03-20 15:30:00  |  4 symbol(s)
[1/4] AAPL
  [Finnhub] Quote → AAPL @ 175.23
  [TwelveData] 1day → AAPL (30 candles)
...
✓ Cycle #1 complete.
⏱ Next run in 3600 seconds
```

### چک Database:
```sql
-- تعداد داده‌های جمع‌آوری شده
SELECT COUNT(*) FROM fact_market_quote;
SELECT COUNT(*) FROM fact_market_timeseries;
SELECT * FROM dim_symbol;
```

## 🆘 مشکلات متداول:

### Build Failed:
- چک کن: `.csproj` فایل `net8.0` باشه (نه `net10.0`)
- چک کن: `using X.PagedList.Extensions;` نباشه

### Python Crashed:
- Environment Variables رو بررسی کن
- PGHOST باید Internal Database URL باشه

### Database Connection Failed:
- SSL Mode=Require رو فراموش نکن
- Internal URL استفاده کن (نه External)

## 📚 مستندات بیشتر:

- **DEPLOY_GUIDE.md** - راهنمای کامل دیپلوی
- **market_pipeline/README.md** - توضیحات Python Pipeline

## ✅ چک‌لیست نهایی:

- [ ] همه فایل‌ها در repository آپلود شدن
- [ ] .env.example موجوده (بدون API keys واقعی)
- [ ] Environment Variables در Render تنظیم شدن
- [ ] PostgreSQL ساخته شده و Internal URL کپی شده
- [ ] Push به GitHub و منتظر Build
- [ ] Logs رو چک کن که هر دو برنامه شروع شدن

---

**موفق باشی! 🚀**
