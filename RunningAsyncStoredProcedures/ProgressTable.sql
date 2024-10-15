CREATE TABLE ProgressTable (
    Id INT IDENTITY(1,1) PRIMARY KEY, -- Auto-incrementing primary key
    TaskName NVARCHAR(50) NOT NULL,   -- Name of the task or operation being monitored
    ProgressPercent INT NOT NULL,     -- Progress percentage (0 to 100)
    LastUpdated DATETIME NOT NULL DEFAULT GETDATE() -- Timestamp of the last update
);