CREATE OR ALTER PROCEDURE LongRunningProcedureAsync
@TaskName NVARCHAR(50) -- Receives the task name
AS
BEGIN
    DECLARE @progress INT = 0;

    -- Check if a record already exists for the task and insert if not
    IF NOT EXISTS (SELECT 1 FROM ProgressTable WHERE TaskName = @taskName)
    BEGIN
        INSERT INTO ProgressTable (TaskName, ProgressPercent, LastUpdated)
        VALUES (@TaskName, 0, GETDATE());
    END

    -- Simulate task progress in a loop
    WHILE @progress < 100
    BEGIN
        -- Increase progress
        SET @progress = @progress + 10;

        -- Update the ProgressTable
        UPDATE ProgressTable
        SET ProgressPercent = @progress, LastUpdated = GETDATE()
        WHERE TaskName = @TaskName;

        -- Simulate a delay between task steps (2 seconds here)
        WAITFOR DELAY '00:00:02'; -- 2-second delay to simulate work being done
    END
END;