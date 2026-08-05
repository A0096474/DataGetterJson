# DataGetterJson

DataGetterJson is a cTrader cBot that collects market candle data from a financial data provider and saves it in JSONL format for easy AI ingestion and data analysis.

It records:

- candle open, high, low, and close
- tick volume
- timestamps in UTC and Singapore time
- one JSON object per line, which makes the file simple to stream, parse, and analyze

The robot writes data to a user-selected folder and requests full file access so it can save the export directly on disk.

## Output format

Each candle is stored as a single JSON line, which works well for Python, notebooks, LLM pipelines, and general analytics tools.

## Purpose

The goal of this project is to make raw trading data easy to reuse in:

- AI training or evaluation pipelines
- time-series analysis
- research notebooks
- lightweight data export workflows
