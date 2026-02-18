using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace ClietStockHub.Application.Services;

public class OrderQueries
{
    private readonly string _connStr;
    public OrderQueries(IConfiguration config)
    {
        _connStr = config.GetConnectionString("DefaultConnection")!;
    }

public async Task<IEnumerable<dynamic>> ListOrdersAsync()
{
    await using var conn = new NpgsqlConnection(_connStr);

    var sql = @"
        SELECT 
            o.id,
            o.customer_id,
            c.""Name"" AS customer_name,
            o.""TotalAmount"",
            o.""Status"",
            o.""CreatedAt"",
            COALESCE(
                json_agg(
                    json_build_object(
                        'product_id', p.id,
                        'product_name', p.""Name"",
                        'unit_price', oi.""UnitPrice"",
                        'quantity', oi.""Quantity"",
                        'line_total', oi.""LineTotal""
                    )
                ) FILTER (WHERE oi.id IS NOT NULL),
                '[]'
            ) AS items
        FROM public.orders o
        JOIN public.customers c 
            ON c.id = o.customer_id
        LEFT JOIN public.order_items oi 
            ON oi.order_id = o.id
        LEFT JOIN public.products p 
            ON p.id = oi.product_id
        GROUP BY 
            o.id, c.""Name""
        ORDER BY 
            o.""CreatedAt"" DESC;
    ";

    return await conn.QueryAsync(sql);
}

}
