namespace DannyGoodacre.InviteOnly.Tests.Infrastructure;

/// <summary>
/// A SQLite file in a temporary folder, for tests that need a database to outlive one <see cref="TestApp"/>.
/// </summary>
public sealed class TempDatabase : IDisposable
{
    private readonly string _directory = Directory.CreateTempSubdirectory("DannyGoodacre.InviteOnly.Tests-").FullName;

    public string Path => System.IO.Path.Combine(_directory, "identity.db");

    public string DirectoryPath => _directory;

    public void Dispose()
    {
        try
        {
            Directory.Delete(_directory, recursive: true);
        }
        catch (IOException)
        {
        }
    }
}
