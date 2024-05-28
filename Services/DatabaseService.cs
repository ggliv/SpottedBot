using Discord;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace SpottedBot.Services;

public class DatabaseService(IConfiguration config)
{
    private const string SqliteCountSpotter = "SELECT COUNT(*) FROM spots WHERE guild=@guildId AND spotter=@userId";
    private const string SqliteCountSpotted = "SELECT COUNT(*) FROM spots WHERE guild=@guildId AND spotted=@userId";

    /// <summary>
    /// Create the database for this application if it doesn't already exist.
    /// </summary>
    public async Task EnsureDatabaseExists()
    {
        await using var connection = new SqliteConnection($"Data Source={config["db_path"]}");
        await connection.OpenAsync();
        var command = connection.CreateCommand();

        command.CommandText =
            """
            CREATE TABLE IF NOT EXISTS spots (
              id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
              guild TEXT NOT NULL,
              spotter TEXT NOT NULL,
              spotted TEXT NOT NULL,
              image TEXT NOT NULL,
              timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
            )
            """;
        await command.ExecuteNonQueryAsync();

        command.CommandText =
            """
            CREATE TABLE IF NOT EXISTS guilds (
               id TEXT NOT NULL PRIMARY KEY,
               spotted_channel TEXT,
               spotted_role TEXT,
               season_name TEXT,
               season_start DATETIME,
               season_end DATETIME
            )
            """;
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Dump the database's records for the given server to a local file and return that file's path.
    /// </summary>
    /// <remarks>
    /// This method does nothing to the file after returning. Users are responsible for deleting the provided file after
    /// use.
    /// </remarks>
    /// <param name="guild">The guild to dump the records for.</param>
    /// <returns>A path to the file's location on the local filesystem, or null if the dump could not be made.</returns>
    public async Task<string?> Dump(IGuild guild)
    {
        try
        {
            await using var connection = new SqliteConnection($"Data Source={config["db_path"]}");
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = $"SELECT * FROM spots WHERE guild='{guild.Id}'";

            await using var reader = await command.ExecuteReaderAsync();
            var tempFile = Path.GetTempFileName();
            await using var fs = File.CreateText(tempFile);

            while (reader.Read())
                await fs.WriteLineAsync(
                    $"{reader.GetString(2)}," +
                    $"{reader.GetString(3)}," +
                    $"{reader.GetString(4)}," +
                    $"{reader.GetDateTime(5)}");

            return tempFile;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Get the players who currently have a score in a guild.
    /// </summary>
    /// <param name="guild">The guild to get the players of</param>
    /// <returns>Enumeration of the players who have a score in the <paramref name="guild"/></returns>
    public async Task<IEnumerable<string>> GetPlayers(IGuild guild)
    {
        await using var connection = new SqliteConnection($"Data Source={config["db_path"]}");
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = $"""
                               WITH guild_spots AS (SELECT * FROM spots WHERE guild={guild.Id})
                               SELECT player
                               FROM (
                                 SELECT spotter AS player FROM guild_spots
                                 UNION
                                 SELECT spotted AS player FROM guild_spots
                               )
                               """;
        await using var reader = await command.ExecuteReaderAsync();
        List<string> players = [];
        while (reader.Read()) players.Add(reader.GetString(0));
        return players;
    }

    /// <summary>
    /// Get the scores currently set in a given guild.
    /// </summary>
    /// <param name="guild">The guild to get the scores for</param>
    /// <returns>Dictionary associating user IDs (key) to scores (value)</returns>
    public async Task<Dictionary<string, long>> GetScores(IGuild guild)
    {
        await using var connection = new SqliteConnection($"Data Source={config["db_path"]}");
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = $"""
                               WITH guild_spots AS (SELECT * FROM spots WHERE guild={guild.Id})
                               SELECT player, SUM(value) AS score
                               FROM (
                                 SELECT spotter AS player, 1 as value FROM guild_spots
                                 UNION ALL
                                 SELECT spotted AS player, -1 as value FROM guild_spots
                               )
                               GROUP BY player
                               """;

        Dictionary<string, long> scores = new();
        await using var reader = await command.ExecuteReaderAsync();
        while (reader.Read()) scores.Add(reader.GetString(0), reader.GetInt64(1));

        return scores;
    }

    /// <summary>
    /// Fetch from the database the number of times a user has spotted someone else in the provided guild.
    /// </summary>
    /// <param name="guild">The guild context.</param>
    /// <param name="user">The user to count the spots of.</param>
    /// <returns>Number of spots made by <paramref name="user" /> in <paramref name="guild" />.</returns>
    public async Task<long> GetSpotterCount(IGuild guild, IUser user)
    {
        await using var connection = new SqliteConnection($"Data Source={config["db_path"]}");
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = SqliteCountSpotter;
        command.Parameters.AddWithValue("@guildId", guild.Id);
        command.Parameters.AddWithValue("@userId", user.Id);
        return (long)(await command.ExecuteScalarAsync() ?? 0);
    }

    /// <summary>
    /// Fetch from the database the number of times a user has been spotted by someone else in the provided guild.
    /// </summary>
    /// <param name="guild">The guild context</param>
    /// <param name="user">The user to count the spots of</param>
    /// <returns>Number of times <paramref name="user" /> has been spotted in <paramref name="guild" /></returns>
    public async Task<long> GetSpottedCount(IGuild guild, IUser user)
    {
        await using var connection = new SqliteConnection($"Data Source={config["db_path"]}");
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = SqliteCountSpotted;
        command.Parameters.AddWithValue("@guildId", guild.Id);
        command.Parameters.AddWithValue("@userId", user.Id);
        return (long)(await command.ExecuteScalarAsync() ?? 0);
    }

    /// <summary>
    /// Log a single spot to the database.
    /// </summary>
    /// <param name="guildId">The guild this spot should be recorded under</param>
    /// <param name="spotterId">The user ID of the player who spotted the other player</param>
    /// <param name="spottedId">The user ID of the player who got spotted</param>
    /// <param name="imageUrl">Link to the image certifying the spot</param>
    private async Task LogSpot(ulong guildId, ulong spotterId, ulong spottedId, string imageUrl)
    {
        await using var connection = new SqliteConnection($"Data Source={config["db_path"]}");
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = $"""
                               INSERT INTO spots (
                                 guild, spotter, spotted, image
                               ) VALUES (
                                 '{guildId}', '{spotterId}', '{spottedId}', '{imageUrl}'
                               )
                               """;
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Log a new spot event to the database.
    /// </summary>
    /// <param name="guild">The guild where the spots took place</param>
    /// <param name="spotter">The player who spotted the other people</param>
    /// <param name="spots">The players who got spotted by the <paramref name="spotter"/></param>
    /// <param name="image">The image of the spot</param>
    public async Task LogSpots(IGuild guild, IUser spotter, IEnumerable<IUser> spots, IAttachment image)
    {
        foreach (var spot in spots) await LogSpot(guild.Id, spotter.Id, spot.Id, image.Url);
    }
}