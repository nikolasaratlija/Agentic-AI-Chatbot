CREATE TABLE medications (
    Start TEXT NOT NULL,             -- ISO8601 Date String
    Stop TEXT,
    Patient TEXT NOT NULL,           -- Foreign Key to patients(Id)
    Payer TEXT NOT NULL,
    Encounter TEXT NOT NULL,
    Code TEXT NOT NULL,
    Description TEXT NOT NULL,
    Base_Cost REAL NOT NULL,
    Payer_Coverage REAL NOT NULL,
    Dispenses INTEGER NOT NULL,      -- Changed to INTEGER since it's a count
    TotalCost REAL NOT NULL,
    ReasonCode TEXT,
    ReasonDescription TEXT,
    FOREIGN KEY (Patient) REFERENCES patients(Id)
);