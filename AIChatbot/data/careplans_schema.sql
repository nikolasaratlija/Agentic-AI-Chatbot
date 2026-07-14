CREATE TABLE care_plans (
    Id TEXT PRIMARY KEY,
    Start TEXT NOT NULL,                  -- SQLite stores YYYY-MM-DD as TEXT
    Stop TEXT,
    Patient TEXT NOT NULL,                -- Foreign Key to patients(Id)
    Encounter TEXT NOT NULL,              -- Foreign Key to encounters(Id)
    Code TEXT NOT NULL,
    Description TEXT NOT NULL,
    ReasonCode TEXT,
    ReasonDescription TEXT,
    FOREIGN KEY (Patient) REFERENCES patients(Id),
    FOREIGN KEY (Encounter) REFERENCES encounters(Id)
);