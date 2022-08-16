using System;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Hrimsoft.Data.PostgreSql.ValueConverters; 

public static class UtcDateTimeConverter
{
    public static ValueConverter<DateTime, DateTime> Get() =>
        new ValueConverter<DateTime, DateTime>
            (v => v.ToUniversalTime(),
             v => v.ToUniversalTime());

    public static ValueConverter<DateTime?, DateTime?> GetNullable() =>
        new ValueConverter<DateTime?, DateTime?>
            (v => v.HasValue ? v.Value.ToUniversalTime() : v,
             v => v.HasValue ? v.Value.ToUniversalTime() : v);
}