using System.Diagnostics;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

record Parcel(string TrackingNumber, string Carrier, string City, string Status, double WeightKg, int DaysInTransit);

record Order(string OrderId, string Customer, string TrackingNumber);

class Program
{
    static List<Parcel> parcels = new();

    static JsonSerializerOptions jsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };

    static List<Order> orders = new()
    {
        new("ORD-001", "Іваненко Олена", "SP-1001"),
        new("ORD-002", "Петренко Андрій", "SP-1004"),
        new("ORD-003", "Коваль Марія", "ED-2002"),
        new("ORD-004", "Іваненко Олена", "ED-2005"),
        new("ORD-005", "Шевчук Дмитро", "P24-3001"),
        new("ORD-006", "Бондар Ірина", "P24-3004"),
        new("ORD-007", "Петренко Андрій", "SP-1007"),
        new("ORD-008", "Ткаченко Сергій", "ED-2008"),
        new("ORD-009", "Коваль Марія", "P24-3007"),
        new("ORD-010", "Шевчук Дмитро", "SP-1090"),
        new("ORD-011", "Іваненко Олена", "ED-2999"),
    };

    static async Task Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        bool run = true;

        while (run)
        {
            Console.WriteLine();
            Console.WriteLine("Меню:");
            Console.WriteLine("0. Вихід");
            Console.WriteLine("1. Завантажити дані служб доставки (одночасно)");
            Console.WriteLine("2. Завантажити дані служб доставки (послідовно)");
            Console.WriteLine("3. Статистика за службами");
            Console.WriteLine("4. Середній термін доставки");
            Console.WriteLine("5. Затримані посилки");
            Console.WriteLine("6. Замовлення клієнта");
            Console.WriteLine("7. Зберегти звіт у JSON");

            int choice = ReadInt("Виберіть пункт: ", 0, 7);

            switch (choice)
            {
                case 0:
                    run = false;
                    break;
                case 1:
                    await LoadData(true);
                    break;
                case 2:
                    await LoadData(false);
                    break;
                case 3:
                    ShowStats();
                    break;
                case 4:
                    ShowAvgDays();
                    break;
                case 5:
                    ShowDelayed();
                    break;
                case 6:
                    ShowCustomerOrders();
                    break;
                case 7:
                    await SaveReport();
                    break;
            }
        }
    }

    static async Task<List<Parcel>> LoadShvydkoPoshtaAsync()
    {
        await Task.Delay(2000);

        return new List<Parcel>
        {
            new("SP-1001", "ШвидкоПошта", "Київ", "Отримано", 1.2, 2),
            new("SP-1002", "ШвидкоПошта", "Львів", "В дорозі", 3.5, 6),
            new("SP-1003", "ШвидкоПошта", "Вінниця", "У відділенні", 0.8, 3),
            new("SP-1004", "ШвидкоПошта", "Одеса", "В дорозі", 5.0, 9),
            new("SP-1005", "ШвидкоПошта", "Київ", "Отримано", 2.1, 3),
            new("SP-1006", "ШвидкоПошта", "Харків", "Отримано", 0.5, 4),
            new("SP-1007", "ШвидкоПошта", "Вінниця", "В дорозі", 1.7, 2),
            new("SP-1008", "ШвидкоПошта", "Дніпро", "У відділенні", 4.3, 5),
        };
    }
    static async Task<List<Parcel>> LoadExpressDostavkaAsync()
    {
        await Task.Delay(1500);

        return new List<Parcel>
        {
            new("ED-2001", "ЕкспресДоставка", "Київ", "Отримано", 0.9, 1),
            new("ED-2002", "ЕкспресДоставка", "Вінниця", "У відділенні", 2.4, 2),
            new("ED-2003", "ЕкспресДоставка", "Полтава", "В дорозі", 7.8, 7),
            new("ED-2004", "ЕкспресДоставка", "Львів", "Отримано", 1.1, 2),
            new("ED-2005", "ЕкспресДоставка", "Київ", "В дорозі", 3.0, 4),
            new("ED-2006", "ЕкспресДоставка", "Одеса", "Отримано", 6.2, 3),
            new("ED-2007", "ЕкспресДоставка", "Харків", "В дорозі", 0.4, 1),
            new("ED-2008", "ЕкспресДоставка", "Вінниця", "Отримано", 2.6, 2),
            new("ED-2009", "ЕкспресДоставка", "Житомир", "У відділенні", 1.5, 3),
        };
    }
    static async Task<List<Parcel>> LoadPosylka24Async()
    {
        await Task.Delay(1000);

        return new List<Parcel>
        {
            new("P24-3001", "Посилка24", "Київ", "В дорозі", 2.2, 8),
            new("P24-3002", "Посилка24", "Вінниця", "Отримано", 1.0, 5),
            new("P24-3003", "Посилка24", "Львів", "Отримано", 3.3, 6),
            new("P24-3004", "Посилка24", "Чернівці", "У відділенні", 0.7, 4),
            new("P24-3005", "Посилка24", "Київ", "Отримано", 4.8, 4),
            new("P24-3006", "Посилка24", "Одеса", "В дорозі", 9.5, 11),
            new("P24-3007", "Посилка24", "Харків", "Отримано", 1.4, 7),
        };
    }
    static async Task LoadData(bool parallel)
    {
        Console.WriteLine("Завантаження...");
        Stopwatch sw = Stopwatch.StartNew();
        List<Parcel>[] result;

        if (parallel)
        {
            result = await Task.WhenAll(LoadShvydkoPoshtaAsync(), LoadExpressDostavkaAsync(), LoadPosylka24Async());
        }
        else
        {
            var a = await LoadShvydkoPoshtaAsync();
            var b = await LoadExpressDostavkaAsync();
            var c = await LoadPosylka24Async();
            result = new[] { a, b, c };
        }

        sw.Stop();

        parcels = result.SelectMany(x => x).ToList();

        Console.WriteLine($"Завантажено {parcels.Count} посилок за {sw.ElapsedMilliseconds} мс");
    }
    static void ShowStats()
    {
        if (!CheckData()) return;

        var stats = parcels
            .GroupBy(p => p.Carrier)
            .Select(g => new
            {
                Carrier = g.Key,
                InWay = g.Count(p => p.Status == "В дорозі"),
                InBranch = g.Count(p => p.Status == "У відділенні"),
                Received = g.Count(p => p.Status == "Отримано"),
                Weight = g.Sum(p => p.WeightKg)
            })
            .OrderBy(s => s.Carrier);

        foreach (var s in stats)
        {
            Console.WriteLine($"{s.Carrier}: в дорозі - {s.InWay}, у відділенні - {s.InBranch}, отримано - {s.Received}, вага - {s.Weight} кг");
        }
    }
    static void ShowAvgDays()
    {
        if (!CheckData()) return;

        var avg = parcels
            .Where(p => p.Status == "Отримано")
            .GroupBy(p => p.Carrier)
            .Select(g => new { Carrier = g.Key, Days = g.Average(p => p.DaysInTransit) })
            .OrderBy(x => x.Days)
            .ThenBy(x => x.Carrier)
            .ToList();

        if (!avg.Any())
        {
            Console.WriteLine("Немає отриманих посилок");
            return;
        }

        foreach (var x in avg)
        {
            Console.WriteLine($"{x.Carrier}: {x.Days:F1} дн.");
        }
    }

    static void ShowDelayed()
    {
        if (!CheckData()) return;

        int n = ReadInt("Введіть кількість днів: ", 1, 100);

        var delayed = (from p in parcels
                       where p.Status == "В дорозі" && p.DaysInTransit > n
                       orderby p.DaysInTransit descending, p.Carrier
                       select new { p.TrackingNumber, p.Carrier, p.City, p.DaysInTransit }).ToList();

        if (!delayed.Any())
        {
            Console.WriteLine("Затриманих посилок немає");
            return;
        }

        foreach (var p in delayed)
        {
            Console.WriteLine($"{p.TrackingNumber} ({p.Carrier}, {p.City}) - {p.DaysInTransit} дн.");
        }
    }
    static void ShowCustomerOrders()
    {
        if (!CheckData()) return;

        Console.Write("Введіть ім'я клієнта: ");
        string name = (Console.ReadLine() ?? "").Trim().ToLower();

        var customerOrders = orders.Where(o => name != "" && o.Customer.ToLower().Contains(name)).ToList();

        if (!customerOrders.Any())
        {
            Console.WriteLine("Замовлень не знайдено");
            return;
        }

        // GroupJoin щоб не загубити замовлення без посилки
        var result = customerOrders.GroupJoin(
            parcels,
            o => o.TrackingNumber,
            p => p.TrackingNumber,
            (o, ps) => new
            {
                o.OrderId,
                o.Customer,
                Status = ps.Any() ? ps.First().Status : "статус невідомий",
                City = ps.Any() ? ps.First().City : "-"
            });

        foreach (var r in result)
        {
            Console.WriteLine($"{r.OrderId} {r.Customer}: {r.Status}, місто: {r.City}");
        }
    }
    static async Task SaveReport()
    {
        if (!CheckData()) return;

        var report = orders.GroupJoin(
            parcels,
            o => o.TrackingNumber,
            p => p.TrackingNumber,
            (o, ps) => new
            {
                o.OrderId,
                o.Customer,
                o.TrackingNumber,
                Carrier = ps.Any() ? ps.First().Carrier : "-",
                Status = ps.Any() ? ps.First().Status : "статус невідомий",
                City = ps.Any() ? ps.First().City : "-"
            }).ToList();

        string json = JsonSerializer.Serialize(report, jsonOptions);

        try
        {
            await File.WriteAllTextAsync("report.json", json);
            Console.WriteLine("Звіт збережено в report.json");
        }
        catch (IOException ex)
        {
            Console.WriteLine("Помилка збереження: " + ex.Message);
        }
    }
    static bool CheckData()
    {
        if (parcels.Count == 0)
        {
            Console.WriteLine("Спочатку завантажте дані (пункт 1 або 2)");
            return false;
        }
        return true;
    }
    static int ReadInt(string text, int min, int max)
    {
        while (true)
        {
            Console.Write(text);
            if (int.TryParse(Console.ReadLine(), out int x) && x >= min && x <= max)
                return x;
            Console.WriteLine($"Введіть число від {min} до {max}");
        }
    }
}