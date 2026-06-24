using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AlienStories.Models;
using Microsoft.Data.Sqlite;

namespace AlienStories.Services;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService()
    {
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AlienStories",
            "aliens.db"
        );

        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

        _connectionString = $"Data Source={dbPath}";

        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var createCatalog = @"
            CREATE TABLE IF NOT EXISTS CreatureCatalog (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL,
            Planet TEXT NOT NULL,
            Rarity INTEGER NOT NULL,
            ColorHex TEXT NOT NULL,
            Emoji TEXT NOT NULL,
            Description TEXT NOT NULL,
            Story TEXT NOT NULL,
            ShapeType INTEGER NOT NULL DEFAULT 0
        )";

        var createCaptured = @"
            CREATE TABLE IF NOT EXISTS CapturedCreatures (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            CatalogId INTEGER NOT NULL,
            Nickname TEXT NOT NULL,
            CaptureDate TEXT NOT NULL,
            Size REAL NOT NULL,
            IsShiny INTEGER NOT NULL,
            Hunger INTEGER NOT NULL DEFAULT 100,
            TimesHugged INTEGER NOT NULL DEFAULT 0,
            TimesFed INTEGER NOT NULL DEFAULT 0,
            TimesHeardStory INTEGER NOT NULL DEFAULT 0,
            LastFed TEXT,
            LastHugged TEXT,
            ShapeType INTEGER NOT NULL DEFAULT 0,
            FOREIGN KEY(CatalogId) REFERENCES CreatureCatalog(Id)
        )";

        // Таблица для игровых данных (звёздная пыль)
        var createGameData = @"
            CREATE TABLE IF NOT EXISTS GameData (
                Key TEXT PRIMARY KEY,
                Value TEXT
            )";

        using var cmd1 = new SqliteCommand(createCatalog, connection);
        cmd1.ExecuteNonQuery();

        using var cmd2 = new SqliteCommand(createCaptured, connection);
        cmd2.ExecuteNonQuery();

        using var cmd3 = new SqliteCommand(createGameData, connection);
        cmd3.ExecuteNonQuery();

        SeedCatalog(connection);
    }

    private void SeedCatalog(SqliteConnection connection)
    {
        using var checkCmd = new SqliteCommand("SELECT COUNT(*) FROM CreatureCatalog", connection);
        var count = Convert.ToInt32(checkCmd.ExecuteScalar());
        if (count > 0) return;

        var aliens = GetDefaultAliens();

        foreach (var alien in aliens)
        {
            using var cmd = new SqliteCommand(@"
            INSERT INTO CreatureCatalog (Name, Planet, Rarity, ColorHex, Emoji, Description, Story, ShapeType)
            VALUES (@name, @planet, @rarity, @color, @emoji, @desc, @story, @shapeType)
        ", connection);

            cmd.Parameters.AddWithValue("@name", alien.Name);
            cmd.Parameters.AddWithValue("@planet", alien.Planet);
            cmd.Parameters.AddWithValue("@rarity", alien.Rarity);
            cmd.Parameters.AddWithValue("@color", alien.ColorHex);
            cmd.Parameters.AddWithValue("@emoji", alien.Emoji);
            cmd.Parameters.AddWithValue("@desc", alien.Description);
            cmd.Parameters.AddWithValue("@story", alien.Story);
            cmd.Parameters.AddWithValue("@shapeType", alien.ShapeType);
            cmd.ExecuteNonQuery();
        }
    }

    private List<CreatureCatalog> GetDefaultAliens()
    {
        return new List<CreatureCatalog>
    {
        new() {
            Name = "Пылевой жуй",
            Planet = "Туманность Андромеды",
            Rarity = 1,
            ColorHex = "#FFB6C1",
            Emoji = "🐱",
            Description = "Маленький пушистый комочек",
            Story = "Я родился в туманности, где звёзды танцуют. Однажды я увидел свет от твоего окна и понял — там тепло. Можно я останусь?",
            ShapeType = 0
        },
        new() {
            Name = "Светлячок",
            Planet = "Звездная пыль",
            Rarity = 2,
            ColorHex = "#FFD700",
            Emoji = "✨",
            Description = "Светится в темноте",
            Story = "Когда темно, я свечусь, чтобы другие не боялись. Но иногда мне самому нужен кто-то, кто посветит для меня.",
            ShapeType = 3
        },
        new() {
            Name = "Облачко",
            Planet = "Небесная высь",
            Rarity = 1,
            ColorHex = "#87CEEB",
            Emoji = "☁️",
            Description = "Плавающий зефир",
            Story = "Я летал между галактик и видел много чудес. Но самое прекрасное — это когда меня обнимают.",
            ShapeType = 4
        },
        new() {
            Name = "Мохнатик",
            Planet = "Колючий лес",
            Rarity = 2,
            ColorHex = "#98FB98",
            Emoji = "🌿",
            Description = "Весь в колючках, но нежный внутри",
            Story = "Я колючий только снаружи. Внутри я мягкий и пушистый. Просто я стесняюсь.",
            ShapeType = 1
        },
        new() {
            Name = "Звёздный кот",
            Planet = "Млечный путь",
            Rarity = 3,
            ColorHex = "#9370DB",
            Emoji = "🐈",
            Description = "Кот, который живёт среди звёзд",
            Story = "Я путешествую между мирами. В каждом мире я нахожу друзей. Ты — мой самый любимый друг.",
            ShapeType = 0
        },
        new() {
            Name = "Ириска",
            Planet = "Сахарная галактика",
            Rarity = 1,
            ColorHex = "#FF69B4",
            Emoji = "🍬",
            Description = "Сладкая и нежная",
            Story = "Я сделана из звёздного сахара. Если ты меня обнимешь, я стану ещё слаще!",
            ShapeType = 2
        },
        new() {
            Name = "Туманник",
            Planet = "Туманность Ориона",
            Rarity = 3,
            ColorHex = "#4169E1",
            Emoji = "🌌",
            Description = "Загадочный и молчаливый",
            Story = "Я вижу все тайны вселенной. Но твоя доброта — самая большая тайна, которую я хочу разгадать.",
            ShapeType = 4
        },
        new() {
            Name = "Плюшевый",
            Planet = "Мягкая планета",
            Rarity = 2,
            ColorHex = "#DDA0DD",
            Emoji = "🧸",
            Description = "Мягкий, как игрушка",
            Story = "Меня создали из облаков и любви. Я здесь, чтобы дарить тепло.",
            ShapeType = 0
        },
        new() {
            Name = "Лунный заяц",
            Planet = "Луна",
            Rarity = 3,
            ColorHex = "#C0C0C0",
            Emoji = "🌙",
            Description = "Пришёл с Луны",
            Story = "Я прыгал по лунным кратерам и увидел, как ты улыбаешься. Теперь я хочу быть рядом.",
            ShapeType = 0
        },
        new() {
            Name = "Искринка",
            Planet = "Пылающая звезда",
            Rarity = 4,
            ColorHex = "#FF4500",
            Emoji = "🔥",
            Description = "Огненная, но тёплая",
            Story = "Я родилась в сердце звезды. Когда я с тобой, я чувствую себя дома.",
            ShapeType = 0
        },
        new() {
            Name = "Соня",
            Planet = "Сонная галактика",
            Rarity = 3,
            ColorHex = "#B0C4DE",
            Emoji = "😴",
            Description = "Всегда хочет спать",
            Story = "Я сплю 23 часа в сутки. Но ради объятий я готова проснуться.",
            ShapeType = 0
        },
        new() {
            Name = "Радужка",
            Planet = "Цветной мир",
            Rarity = 2,
            ColorHex = "#FF1493",
            Emoji = "🌈",
            Description = "Переливается всеми цветами",
            Story = "Я — частичка радуги. Когда ты грустишь, я прихожу, чтобы раскрасить твой день.",
            ShapeType = 2
        },
        new() {
            Name = "Тихоня",
            Planet = "Тихая планета",
            Rarity = 2,
            ColorHex = "#4682B4",
            Emoji = "🤫",
            Description = "Говорит шёпотом",
            Story = "Я не люблю шум. Но с тобой я чувствую себя спокойно и уютно.",
            ShapeType = 1
        },
        new() {
            Name = "Звёздный странник",
            Planet = "Вечность",
            Rarity = 4,
            ColorHex = "#FFD700",
            Emoji = "⭐",
            Description = "Путешествует сквозь время",
            Story = "Я видел рождение и смерть звёзд. Но твоя доброта — вечна.",
            ShapeType = 0
        },
        new() {
            Name = "Пушистик",
            Planet = "Мягкая планета",
            Rarity = 1,
            ColorHex = "#F5DEB3",
            Emoji = "🐾",
            Description = "Пушистый и добрый",
            Story = "Я просто хочу, чтобы меня гладили. Это всё, что мне нужно.",
            ShapeType = 0
        }
    };
    }

    // ===== CRUD методы =====

    public List<CreatureCatalog> GetAllCatalog()
    {
        var result = new List<CreatureCatalog>();
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = new SqliteCommand("SELECT * FROM CreatureCatalog", connection);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            result.Add(new CreatureCatalog
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Planet = reader.GetString(2),
                Rarity = reader.GetInt32(3),
                ColorHex = reader.GetString(4),
                Emoji = reader.GetString(5),
                Description = reader.GetString(6),
                Story = reader.GetString(7),
                ShapeType = reader.IsDBNull(8) ? 0 : reader.GetInt32(8)
            });
        }
        return result;
    }

    public void AddCaptured(CapturedCreature creature)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = new SqliteCommand(@"
        INSERT INTO CapturedCreatures 
        (CatalogId, Nickname, CaptureDate, Size, IsShiny, Hunger, TimesHugged, TimesFed, TimesHeardStory, ShapeType)
        VALUES (@catalogId, @nickname, @date, @size, @shiny, @hunger, @hugged, @fed, @stories, @shapeType)
    ", connection);

        cmd.Parameters.AddWithValue("@catalogId", creature.CatalogId);
        cmd.Parameters.AddWithValue("@nickname", creature.Nickname);
        cmd.Parameters.AddWithValue("@date", creature.CaptureDate.ToString("yyyy-MM-dd HH:mm:ss"));
        cmd.Parameters.AddWithValue("@size", creature.Size);
        cmd.Parameters.AddWithValue("@shiny", creature.IsShiny ? 1 : 0);
        cmd.Parameters.AddWithValue("@hunger", creature.Hunger);
        cmd.Parameters.AddWithValue("@hugged", creature.TimesHugged);
        cmd.Parameters.AddWithValue("@fed", creature.TimesFed);
        cmd.Parameters.AddWithValue("@stories", creature.TimesHeardStory);
        cmd.Parameters.AddWithValue("@shapeType", creature.ShapeType);

        cmd.ExecuteNonQuery();
    }

    public List<CapturedCreature> GetAllCaptured()
    {
        var result = new List<CapturedCreature>();
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = new SqliteCommand(@"
        SELECT c.*, cat.* 
        FROM CapturedCreatures c
        JOIN CreatureCatalog cat ON c.CatalogId = cat.Id
        ORDER BY c.CaptureDate DESC
    ", connection);

        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            var creature = new CapturedCreature
            {
                Id = reader.GetInt32(0),
                CatalogId = reader.GetInt32(1),
                Nickname = reader.GetString(2),
                CaptureDate = DateTime.Parse(reader.GetString(3)),
                Size = reader.GetDouble(4),
                IsShiny = reader.GetInt32(5) == 1,
                Hunger = reader.GetInt32(6),
                TimesHugged = reader.GetInt32(7),
                TimesFed = reader.GetInt32(8),
                TimesHeardStory = reader.GetInt32(9),
                LastFed = reader.IsDBNull(10) ? null : DateTime.Parse(reader.GetString(10)),
                LastHugged = reader.IsDBNull(11) ? null : DateTime.Parse(reader.GetString(11)),
                ShapeType = reader.IsDBNull(12) ? 0 : reader.GetInt32(12), // 👈 ДОБАВИТЬ
                Catalog = new CreatureCatalog
                {
                    Id = reader.GetInt32(13),
                    Name = reader.GetString(14),
                    Planet = reader.GetString(15),
                    Rarity = reader.GetInt32(16),
                    ColorHex = reader.GetString(17),
                    Emoji = reader.GetString(18),
                    Description = reader.GetString(19),
                    Story = reader.GetString(20),
                    ShapeType = reader.IsDBNull(21) ? 0 : reader.GetInt32(21)
                }
            };
            result.Add(creature);
        }
        return result;
    }

    public void UpdateCreature(CapturedCreature creature)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = new SqliteCommand(@"
            UPDATE CapturedCreatures 
            SET Nickname = @nickname,
                Hunger = @hunger,
                TimesHugged = @hugged,
                TimesFed = @fed,
                TimesHeardStory = @stories,
                LastFed = @lastFed,
                LastHugged = @lastHugged
            WHERE Id = @id
        ", connection);

        cmd.Parameters.AddWithValue("@nickname", creature.Nickname);
        cmd.Parameters.AddWithValue("@hunger", creature.Hunger);
        cmd.Parameters.AddWithValue("@hugged", creature.TimesHugged);
        cmd.Parameters.AddWithValue("@fed", creature.TimesFed);
        cmd.Parameters.AddWithValue("@stories", creature.TimesHeardStory);
        cmd.Parameters.AddWithValue("@lastFed", creature.LastFed?.ToString("yyyy-MM-dd HH:mm:ss") ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@lastHugged", creature.LastHugged?.ToString("yyyy-MM-dd HH:mm:ss") ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@id", creature.Id);

        cmd.ExecuteNonQuery();
    }

    public void DeleteCaptured(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = new SqliteCommand("DELETE FROM CapturedCreatures WHERE Id = @id", connection);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    // ===== Звёздная пыль =====

    public void SaveStarDust(int amount)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = new SqliteCommand(@"
            INSERT OR REPLACE INTO GameData (Key, Value)
            VALUES ('StarDust', @value)
        ", connection);
        cmd.Parameters.AddWithValue("@value", amount.ToString());
        cmd.ExecuteNonQuery();
    }

    public int GetStarDust()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = new SqliteCommand(@"
        SELECT Value FROM GameData WHERE Key = 'StarDust'
    ", connection);

        var result = cmd.ExecuteScalar();

        return result != null && result != DBNull.Value ? int.Parse(result.ToString()!) : 0;
    }
}