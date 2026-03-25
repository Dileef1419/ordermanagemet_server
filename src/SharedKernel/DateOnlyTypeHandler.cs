using System.Data;
using Dapper;

namespace SharedKernel;

/// <summary>
/// Dapper type handler for DateOnly, which is not natively supported by Dapper.
/// Converts between DateOnly (CLR) and DateTime (SQL Server DATE column).
/// </summary>
public sealed class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.DbType = DbType.Date;
        parameter.Value = value.ToDateTime(TimeOnly.MinValue);
    }

    public override DateOnly Parse(object value) => value switch
    {
        DateTime dt => DateOnly.FromDateTime(dt),
        DateTimeOffset dto => DateOnly.FromDateTime(dto.DateTime),
        _ => throw new InvalidCastException($"Cannot convert {value.GetType()} to DateOnly")
    };
}
