using Microsoft.EntityFrameworkCore;
using QTKD.Api.Entities;

namespace QTKD.Api.Data;

/// <summary>
/// Nạp dữ liệu mẫu vào PostgreSQL.
/// Chỉ chạy ở môi trường Development, chỉ khi database còn trống và đã migrate.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QtkdDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

        var pending = await db.Database.GetPendingMigrationsAsync();
        if (pending.Any())
        {
            logger.LogWarning("Database chưa áp dụng migration, bỏ qua seed. Chạy: dotnet ef database update --project src/QTKD.Api");
            return;
        }

        if (await db.Customers.AnyAsync())
        {
            return; // đã có dữ liệu, không seed lại
        }

        var minhPhat = new Customer { Name = "Công ty Minh Phát", Status = CustomerStatus.Active };
        var anKhang = new Customer { Name = "Công ty An Khang", Status = CustomerStatus.Active };
        db.Customers.AddRange(minhPhat, anKhang);

        // Hợp đồng Draft cố ý chưa có ngày hiệu lực/kết thúc để tuần 06 demo được
        // "gửi khi thiếu dữ liệu (thất bại) -> bổ sung -> gửi lại (thành công)".
        db.Contracts.Add(new Contract
        {
            ContractNo = "HD-2026-018",
            Customer = minhPhat,
            StartDate = null,
            EndDate = null,
            Status = ContractStatus.Draft
        });

        // Mã dịch vụ và đơn giá dưới đây chỉ là dữ liệu mẫu để thực hành.
        db.PriceLists.Add(new PriceList
        {
            Name = "Bảng giá 2026",
            EffectiveDate = new DateOnly(2026, 1, 1),
            Items =
            {
                new PriceItem { ServiceCode = "SVC-CONSULT", UnitPrice = 1_500_000m },
                new PriceItem { ServiceCode = "SVC-SUPPORT", UnitPrice = 500_000m }
            }
        });

        await db.SaveChangesAsync();
        logger.LogInformation("Đã seed dữ liệu mẫu: 2 Customer, 1 Contract Draft, 1 PriceList (2 PriceItem).");
    }
}
