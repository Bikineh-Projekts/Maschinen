# راهنمای دیپلوی در Render

## ساختار نهایی پروژه در GitHub

```
your-repo/
├── Maschin/                        # پروژه .NET
│   ├── Controllers/
│   ├── Models/
│   ├── Views/
│   └── MaschinenDataein.csproj
├── market_pipeline/                # پروژه Python (جدید)
│   ├── collectors/
│   │   ├── __init__.py
│   │   ├── finnhub.py
│   │   ├── alphavantage.py
│   │   └── twelvedata.py
│   ├── db/
│   │   ├── __init__.py
│   │   └── connection.py
│   ├── main.py
│   └── requirements.txt
├── Dockerfile                      # برای هر دو پروژه
├── supervisord.conf                # مدیریت همزمان
├── start.sh                        # اسکریپت شروع
├── .gitignore
├── .env.example
└── README.md
```

## مراحل دیپلوی

### 1️⃣ آپلود فایل‌ها به GitHub

```bash
# در پوشه اصلی پروژه:
git add .
git commit -m "Add Python market pipeline + multi-app Dockerfile"
git push origin main
```

### 2️⃣ ایجاد PostgreSQL Database در Render

1. برو به Render Dashboard
2. کلیک **New +** → **PostgreSQL**
3. نام: `market-database` (یا هر نام دلخواه)
4. منطقه: Frankfurt یا نزدیک‌ترین
5. Plan: **Free**
6. کلیک **Create Database**
7. بعد از ساخت، **Internal Database URL** رو کپی کن

### 3️⃣ تنظیم Environment Variables

در Web Service خودت، برو به **Environment** tab و این متغیرها رو اضافه کن:

#### برای .NET App:
```
ConnectionStrings__DefaultConnection=Host=dpg-xxxxx.frankfurt-postgres.render.com;Database=market_db;Username=market_user;Password=xxxxx;Port=5432;SSL Mode=Require;Trust Server Certificate=true;
```

#### برای Python Pipeline:
```
PGHOST=dpg-xxxxx.frankfurt-postgres.render.com
PGPORT=5432
PGDATABASE=market_db
PGUSER=market_user
PGPASSWORD=xxxxx

FINNHUB_API_KEY=d6uofkpr01qig545a5i0d6uofkpr01qig545a5ig
ALPHAVANTAGE_API_KEY=L9QID1181GO59813
TWELVEDATA_API_KEY=2e7a3e33e3ce4e30931e52c90e10a24f

SYMBOLS=AAPL,MSFT,GOOGL,TSLA
POLL_INTERVAL_SECONDS=3600
ALPHA_MAX_RECORDS=10
TWELVE_OUTPUTSIZE=30
```

⚠️ **مهم:** از همان PostgreSQL برای هر دو استفاده کن!

### 4️⃣ دیپلوی

1. Render به صورت خودکار دیپلوی می‌کنه
2. Build حدود 5-10 دقیقه طول می‌کشه
3. بعد از موفقیت، هر دو برنامه شروع می‌شن:
   - ✅ .NET Web App روی Port 8080
   - ✅ Python Pipeline هر ساعت داده جمع می‌کنه

### 5️⃣ بررسی Logs

در Render Dashboard → Logs:

```
Starting Multi-Application Container
→ .NET Web App: MaschinenDataein (Port 8080)
→ Python Pipeline: Market Data Collector

[DB] Star Schema created / verified.
Cycle #1  |  2026-03-20 15:30:00  |  4 symbol(s)

[1/4] AAPL
  [Finnhub] Quote → AAPL @ 175.23
  [Finnhub] Fundamentals → AAPL
  [TwelveData] 1day → AAPL (30 candles)
...
```

## چک کردن عملکرد

### .NET App:
```
https://your-app.onrender.com
```

### Python Pipeline:
- Logs رو در Render Dashboard ببین
- یا به Database متصل شو و جداول رو چک کن:

```sql
SELECT COUNT(*) FROM fact_market_quote;
SELECT COUNT(*) FROM fact_market_timeseries;
SELECT * FROM dim_symbol;
```

## مشکلات متداول

### ❌ Build Failed
- بررسی کن `.csproj` با `net8.0` باشه (نه `net10.0`)
- بررسی کن `using X.PagedList.Extensions;` نداشته باشی

### ❌ .NET App Running, Python Crashed
- Environment Variables رو چک کن
- `PGHOST`, `PGDATABASE`, `PGUSER`, `PGPASSWORD` باید درست باشن
- API Keys باید معتبر باشن

### ❌ Database Connection Failed
- مطمئن شو **Internal Database URL** استفاده می‌کنی (نه External)
- `SSL Mode=Require;Trust Server Certificate=true;` رو فراموش نکن

## هزینه‌ها

- **Web Service Free Tier:** 750 ساعت/ماه رایگان
- **PostgreSQL Free Tier:** 1 GB رایگان
- **بعد از Free Tier:**
  - Web Service: ~$7/ماه
  - PostgreSQL: ~$7/ماه

## نکات بهینه‌سازی

1. **تنظیم `POLL_INTERVAL_SECONDS=3600`** (هر 1 ساعت)
   - Alpha Vantage فقط 25 req/day رایگان
   
2. **محدود کردن SYMBOLS**
   - هر symbol = چند API call
   - مثال: 5 symbol = ~20 calls per cycle

3. **Monitoring**
   - Logs در Render Dashboard
   - PostgreSQL queries برای بررسی داده‌ها

## پشتیبانی

اگه مشکلی پیش اومد:
1. Logs رو بررسی کن
2. Environment Variables رو چک کن
3. Database connection string رو تست کن
