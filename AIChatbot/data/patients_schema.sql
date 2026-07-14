CREATE TABLE patients (
    Id TEXT PRIMARY KEY,
    BirthDate TEXT NOT NULL,       -- SQLite stores dates as TEXT (YYYY-MM-DD)
    DeathDate TEXT,
    SSN TEXT NOT NULL,
    Drivers TEXT,
    Passport TEXT,
    Prefix TEXT,
    First TEXT NOT NULL,
    Middle TEXT,
    Last TEXT NOT NULL,
    Suffix TEXT,
    Maiden TEXT,
    Marital TEXT,
    Race TEXT NOT NULL,
    Ethnicity TEXT NOT NULL,
    Gender TEXT NOT NULL,
    BirthPlace TEXT NOT NULL,
    Address TEXT NOT NULL,
    City TEXT NOT NULL,
    State TEXT NOT NULL,
    County TEXT,
    FIPS_County_Code TEXT,         -- Replaced spaces with underscores
    Zip TEXT,
    Lat REAL,                      -- SQLite uses REAL for Numeric/Floating points
    Lon REAL,
    Healthcare_Expenses REAL NOT NULL,
    Healthcare_Coverage REAL NOT NULL,
    Income REAL NOT NULL
);