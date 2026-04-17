var connectionString =
    Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? "Host=localhost;Port=5432;Database=vetcrm;Username=postgres;Password=CHANGE_ME";
