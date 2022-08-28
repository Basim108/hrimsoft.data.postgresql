using System;
using Hrimsoft.StringCases;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Hrimsoft.Data.PostgreSql.ValueConverters;

public static class EnumToSnakeCaseConverter<TEnum> 
    where TEnum : struct {
    public static ValueConverter<TEnum, string> Get() =>
        new(item => item.ToString().ToSnakeCase(),
            str => (TEnum)Enum.Parse(typeof(TEnum), str.ToPascalCase()));

    public static ValueConverter<TEnum?, string> GetNullable()=>
        new(item => item.HasValue
                        ? item.Value.ToString().ToSnakeCase()
                        : "",
            str => string.IsNullOrWhiteSpace(str)
                       ? null
                       : (TEnum?)Enum.Parse(typeof(TEnum), str.ToPascalCase()));
}