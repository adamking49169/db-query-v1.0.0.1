using Newtonsoft.Json;
using System.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

public class Q : IDisposable
{
    private readonly ILogger<Q> _logger;
    private DataTable _df;
    private string _model;
    private string _apiKey;
    private bool disposedValue;
    private readonly IConfiguration _configuration;

    // Remove the second constructor and DataTable property
    // Remove ILogger<HomeController> as it's not needed here

    // Single constructor with DI-compatible parameters
    public Q(ILogger<Q> logger, IConfiguration configuration, string model = "Model.GPT3_5_Turbo")
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _model = model;

        SetupLogging();
        SetupApiKey();

        // Initialize empty DataTable if needed
        _df = new DataTable();
    }
    private void SetupLogging()
    {
        _logger.LogInformation("Logging setup initialized.");
    }

    private void SetupApiKey()
    {
        //_apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        _apiKey = "sk-proj-TQTlyjvM1QCZskEB4sCtT3BlbkFJmJxQLcOt1pOBPTpX9W1X";
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new InvalidOperationException("OPENAI_API_KEY not found in environment variables");
        }
    }
    //private void SetupDatabase()
    //{
    //    try
    //    {
    //        // Retrieve the connection string using _configuration
    //        string connString = _configuration.GetConnectionString("DefaultConnection");

    //        _connection = new NpgsqlConnection(connString);
    //        _connection.Open();

    //        _logger?.LogInformation("Database connection established.");
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger?.LogError(ex, "Error establishing database connection.");
    //        throw;
    //    }
    //}
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~Q()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    // ... rest of your existing methods ...
}