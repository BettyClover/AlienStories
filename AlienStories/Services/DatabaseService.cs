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
        // База данных будет в папке с приложением
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AlienStories",
            "aliens.db"
        );

        // Создаём папку, если её нет
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

        _connectionString = $"Data Source={dbPath}";

        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Таблица каталога существ
        var createCatalog = @"
            CREATE TABLE IF NOT EXISTS CreatureCatalog (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Planet TEXT NOT NULL,
                Rarity INTEGER NOT NULL,
                ColorHex TEXT NOT NULL,
                Emoji TEXT NOT NULL,
                Description TEXT NOT NULL,
                Story TEXT NOT NULL
            )";

        // Таблица пойманных существ
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
                FOREIGN KEY(CatalogId) REFERENCES CreatureCatalog(Id)
            )";

        using var cmd1 = new SqliteCommand(createCatalog, connection);
        cmd1.ExecuteNonQuery();

        using var cmd2 = new SqliteCommand(createCaptured, connection);
        cmd2.ExecuteNonQuery();

        // Заполняем каталог, если он пуст
        SeedCatalog(connection);
    }

    private void SeedCatalog(SqliteConnection connection)
    {
        // Проверяем, есть ли данные
        using var checkCmd = new SqliteCommand("SELECT COUNT(*) FROM CreatureCatalog", connection);
        var count = Convert.ToInt32(checkCmd.ExecuteScalar());
        if (count > 0) return;

        var aliens = GetDefaultAliens();

        foreach (var alien in aliens)
        {
            using var cmd = new SqliteCommand(@"
                INSERT INTO CreatureCatalog (Name, Planet, Rarity, ColorHex, Emoji, Description, Story)
                VALUES (@name, @planet, @rarity, @color, @emoji, @desc, @story)
            ", connection);

            cmd.Parameters.AddWithValue("@name", alien.Name);
            cmd.Parameters.AddWithValue("@planet", alien.Planet);
            cmd.Parameters.AddWithValue("@rarity", alien.Rarity);
            cmd.Parameters.AddWithValue("@color", alien.ColorHex);
            cmd.Parameters.AddWithValue("@emoji", alien.Emoji);
            cmd.Parameters.AddWithValue("@desc", alien.Description);
            cmd.Parameters.AddWithValue("@story", alien.Story);
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
                Story = "Я родился в туманности, где звёзды танцуют. Однажды я увидел свет от твоего окна и понял — там тепло. Можно я останусь?"
            },
            new() {
                Name = "Светлячок",
                Planet = "Звездная пыль",
                Rarity = 2,
                ColorHex = "#FFD700",
                Emoji = "✨",
                Description = "Светится в темноте",
                Story = "Когда темно, я свечусь, чтобы другие не боялись. Но иногда мне самому нужен кто-то, кто посветит для меня."
            },
            new() {
                Name = "Облачко",
                Planet = "Небесная высь",
                Rarity = 1,
                ColorHex = "#87CEEB",
                Emoji = "☁️",
                Description = "Плавающий зефир",
                Story = "Я летал между галактик и видел много чудес. Но самое прекрасное — это когда меня обнимают."
            },
            new() {
                Name = "Мохнатик",
                Planet = "Колючий лес",
                Rarity = 2,
                ColorHex = "#98FB98",
                Emoji = "🌿",
                Description = "Весь в колючках, но нежный внутри",
                Story = "Я колючий только снаружи. Внутри я мягкий и пушистый. Просто я стесняюсь."
            },
            new() {
                Name = "Звёздный кот",
                Planet = "Млечный путь",
                Rarity = 3,
                ColorHex = "#9370DB",
                Emoji = "🐈",
                Description = "Кот, который живёт среди звёзд",
                Story = "Я путешествую между мирами. В каждом мире я нахожу друзей. Ты — мой самый любимый друг."
            },
            new() {
                Name = "Ириска",
                Planet = "Сахарная галактика",
                Rarity = 1,
                ColorHex = "#FF69B4",
                Emoji = "🍬",
                Description = "Сладкая и нежная",
                Story = "Я сделана из звёздного сахара. Если ты меня обнимешь, я стану ещё слаще!"
            },
            new() {
                Name = "Туманник",
                Planet = "Туманность Ориона",
                Rarity = 3,
                ColorHex = "#4169E1",
                Emoji = "🌌",
                Description = "Загадочный и молчаливый",
                Story = "Я вижу все тайны вселенной. Но твоя доброта — самая большая тайна, которую я хочу разгадать."
            },
            new() {
                Name = "Плюшевый",
                Planet = "Мягкая планета",
                Rarity = 2,
                ColorHex = "#DDA0DD",
                Emoji = "🧸",
                Description = "Мягкий, как игрушка",
                Story = "Меня создали из облаков и любви. Я здесь, чтобы дарить тепло."
            },
            new() {
                Name = "Лунный заяц",
                Planet = "Луна",
                Rarity = 3,
                ColorHex = "#C0C0C0",
                Emoji = "🌙",
                Description = "Пришёл с Луны",
                Story = "Я прыгал по лунным кратерам и увидел, как ты улыбаешься. Теперь я хочу быть рядом."
            },
            new() {
                Name = "Искринка",
                Planet = "Пылающая звезда",
                Rarity = 4,
                ColorHex = "#FF4500",
                Emoji = "🔥",
                Description = "Огненная, но тёплая",
                Story = "Я родилась в сердце звезды. Когда я с тобой, я чувствую себя дома."
            },
            new() {
                Name = "Соня",
                Planet = "Сонная галактика",
                Rarity = 1,
                ColorHex = "#B0C4DE",
                Emoji = "😴",
                Description = "Всегда хочет спать",
                Story = "Я сплю 23 часа в сутки. Но ради объятий я готова проснуться."
            },
            new() {
                Name = "Радужка",
                Planet = "Цветной мир",
                Rarity = 2,
                ColorHex = "#FF1493",
                Emoji = "🌈",
                Description = "Переливается всеми цветами",
                Story = "Я — частичка радуги. Когда ты грустишь, я прихожу, чтобы раскрасить твой день."
            },
            new() {
                Name = "Тихоня",
                Planet = "Тихая планета",
                Rarity = 2,
                ColorHex = "#4682B4",
                Emoji = "🤫",
                Description = "Говорит шёпотом",
                Story = "Я не люблю шум. Но с тобой я чувствую себя спокойно и уютно."
            },
            new() {
                Name = "Звёздный странник",
                Planet = "Вечность",
                Rarity = 4,
                ColorHex = "#FFD700",
                Emoji = "⭐",
                Description = "Путешествует сквозь время",
                Story = "Я видел рождение и смерть звёзд. Но твоя доброта — вечна."
            },
            new() {
                Name = "Пушистик",
                Planet = "Мягкая планета",
                Rarity = 1,
                ColorHex = "#F5DEB3",
                Emoji = "🐾",
                Description = "Пушистый и добрый",
                Story = "Я просто хочу, чтобы меня гладили. Это всё, что мне нужно."
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
                Story = reader.GetString(7)
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
            (CatalogId, Nickname, CaptureDate, Size, IsShiny, Hunger, TimesHugged, TimesFed, TimesHeardStory)
            VALUES (@catalogId, @nickname, @date, @size, @shiny, @hunger, @hugged, @fed, @stories)
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
                Catalog = new CreatureCatalog
                {
                    Id = reader.GetInt32(12),
                    Name = reader.GetString(13),
                    Planet = reader.GetString(14),
                    Rarity = reader.GetInt32(15),
                    ColorHex = reader.GetString(16),
                    Emoji = reader.GetString(17),
                    Description = reader.GetString(18),
                    Story = reader.GetString(19)
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

    public PlayerProgress GetProgress()
    {
        var all = GetAllCaptured();
        return new PlayerProgress
        {
            TotalFriends = all.Count,
            TotalHugs = all.Sum(c => c.TimesHugged),
            TotalFed = all.Sum(c => c.TimesFed),
            TotalStoriesHeard = all.Sum(c => c.TimesHeardStory),
            RarestCaught = all.Any()
                ? all.OrderByDescending(c => c.Catalog!.Rarity).First().Catalog?.Name
                : null
        };
    }

    public void DeleteCaptured(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = new SqliteCommand("DELETE FROM CapturedCreatures WHERE Id = @id", connection);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }
}