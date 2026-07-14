CREATE TABLE encounters (
    Id TEXT PRIMARY KEY,
    Start TEXT NOT NULL,                   -- ISO8601 Date String
    Stop TEXT,
    Patient TEXT NOT NULL,                 -- Foreign Key to patients(Id)
    Organization TEXT NOT NULL,
    Provider TEXT NOT NULL,
    Payer TEXT NOT NULL,
    EncounterClass TEXT NOT NULL,
    Code TEXT NOT NULL,
    Description TEXT NOT NULL,
    Base_Encounter_Cost REAL NOT NULL,
    Total_Claim_Cost REAL NOT NULL,
    Payer_Coverage REAL NOT NULL,
    ReasonCode TEXT,
    ReasonDescription TEXT,
    FOREIGN KEY (Patient) REFERENCES patients(Id)
);