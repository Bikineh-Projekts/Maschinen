# Market Data Pipeline

**Automated market data collection from multiple sources into PostgreSQL**

## Overview

This Python pipeline collects real-time and historical market data from:
- **Finnhub** - Real-time quotes, company fundamentals, earnings calendar
- **Alpha Vantage** - Technical indicators (RSI, MACD, EMA, SMA)
- **Twelve Data** - OHLCV candlestick data

All data is stored in a **Star Schema** PostgreSQL database for analysis.

## Features

- ✅ Automatic data collection every hour (configurable)
- ✅ Star Schema data warehouse design
- ✅ API rate limit handling
- ✅ Error recovery and logging
- ✅ Upsert logic (no duplicates)

## Project Structure

```
market_pipeline/
├── collectors/
│   ├── finnhub.py          # Finnhub API collector
│   ├── alphavantage.py     # Alpha Vantage API collector
│   └── twelvedata.py       # Twelve Data API collector
├── db/
│   └── connection.py       # PostgreSQL connection & schema
├── main.py                 # Entry point
└── requirements.txt        # Python dependencies
```

## Database Schema

**Dimension Tables:**
- `dim_source` - Data sources
- `dim_symbol` - Stock symbols
- `dim_interval` - Time intervals
- `dim_indicator` - Technical indicators

**Fact Tables:**
- `fact_market_quote` - Real-time quotes
- `fact_market_indicator` - Technical indicators
- `fact_market_timeseries` - OHLCV candlesticks
- `fact_company_fundamental` - Company fundamentals
- `fact_earnings_calendar` - Earnings dates

## Setup

1. Get free API keys from:
   - [Finnhub](https://finnhub.io/register) - 60 req/min
   - [Alpha Vantage](https://www.alphavantage.co/support/#api-key) - 25 req/day
   - [Twelve Data](https://twelvedata.com/pricing) - 800 req/day

2. Set environment variables in Render:

```bash
# API Keys
FINNHUB_API_KEY=your_key
ALPHAVANTAGE_API_KEY=your_key
TWELVEDATA_API_KEY=your_key

# PostgreSQL (same database as .NET app)
PGHOST=your-postgres-host.render.com
PGPORT=5432
PGDATABASE=Marketdb
PGUSER=your_user
PGPASSWORD=your_password

# Configuration
SYMBOLS=AAPL,MSFT,GOOGL,TSLA
POLL_INTERVAL_SECONDS=3600
```

## Running

The pipeline runs automatically via Supervisor alongside the .NET app.

**Manual execution:**
```bash
# Run once and exit
python3 main.py --once

# Run continuously
python3 main.py

# Custom symbols
python3 main.py --symbols AAPL,MSFT,TSLA
```

## API Rate Limits

| Source | Free Tier | Used For |
|--------|-----------|----------|
| Finnhub | 60 req/min | Quotes, fundamentals, earnings |
| Alpha Vantage | 25 req/day | Technical indicators (every 10 cycles) |
| Twelve Data | 800 req/day | OHLCV candlesticks |

**Recommended:** Set `POLL_INTERVAL_SECONDS=3600` (1 hour)

## Notes

- Database schema is created automatically on first run
- Duplicate data is handled via `ON CONFLICT` upserts
- API errors are logged but don't stop the pipeline
- Each cycle logs to console for monitoring
