using System;
using System.Data;
using System.Data.SqlClient;

class Program
{
    static async Task Main()
    {
        string connectionString = "Put your Connection String here";

        // Generate the task name using date and time
        string taskName = "Task_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string storedProcedureName = "LongRunningProcedureAsync";

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            try
            {
                // Open the connection
                await connection.OpenAsync();

                // Execute the stored procedure in a separate task
                var procedureTask = Task.Run(() => ExecuteStoredProcedureAsync(connection, storedProcedureName, taskName));

                // Monitor the progress in a separate task
                var monitorTask = MonitorProgressAsync(connectionString, taskName);

                // Wait for the completion of both tasks
                await Task.WhenAll(procedureTask, monitorTask);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }
       
    // Asynchronous method to execute the stored procedure, passing the task name as a parameter
    static async Task ExecuteStoredProcedureAsync(SqlConnection connection, string storedProcedureName, string taskName)
    {
        using (SqlCommand command = new SqlCommand(storedProcedureName, connection))
        {
            command.CommandType = CommandType.StoredProcedure;

            // Pass the task name to the stored procedure
            command.Parameters.AddWithValue("@TaskName", taskName);

            // Increase the execution timeout of the stored procedure (for example, 5 minutes)
            command.CommandTimeout = 300; // Timeout of 300 seconds (5 minutes)

            Console.WriteLine("Calling the stored procedure...");

            // Execute the stored procedure asynchronously
            await command.ExecuteNonQueryAsync();

            Console.WriteLine("Stored procedure finished.");
        }
    }
    
    // Asynchronous method to monitor progress
    static async Task MonitorProgressAsync(string connectionString, string taskName)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();

            // Store the last displayed progress
            int? lastProgress = null;

            while (true)
            {
                using (SqlCommand command = new SqlCommand("SELECT ProgressPercent FROM ProgressTable WHERE TaskName = @TaskName", connection))
                {
                    command.Parameters.AddWithValue("@TaskName", taskName);

                    // Execute the query asynchronously
                    var progress = (int?)await command.ExecuteScalarAsync();

                    if (progress.HasValue)
                    {
                        // Only display progress if it is different from the last displayed value
                        if (progress != lastProgress)
                        {
                            Console.WriteLine($"Progress of task '{taskName}': {progress}%");
                            lastProgress = progress;
                        }

                        // If the progress reached 100%, the task is complete
                        if (progress == 100)
                        {
                            break;
                        }
                    }
                }

                // Wait a few seconds before checking again
                await Task.Delay(1000); // 1 second
            }
        }
    }

    // Asynchronous method to delete old records from ProgressTable
    static async Task ClearOldTaskData(SqlConnection connection, string taskName)
    {
        using (SqlCommand command = new SqlCommand("DELETE FROM ProgressTable WHERE TaskName = @TaskName", connection))
        {
            command.Parameters.AddWithValue("@TaskName", taskName);
            await command.ExecuteNonQueryAsync();
        }
    }

}