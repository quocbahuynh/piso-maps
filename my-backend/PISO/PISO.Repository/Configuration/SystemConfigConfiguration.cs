using PISO.Entities.Models;

namespace PISO.Repository.Configuration;

public class SystemConfigConfiguration
{
    public static SystemConfig GetSeedData() => new()
    {
        Free = new PlanConfig
        {
            Name = "Gói Miễn Phí",
            MaxCredits = 200,
            Price = 0,
            HourlyRateLimit = 100
        },
        Developer = new PlanConfig
        {
            Name = "Gói Developer",
            MaxCredits = 1000,
            Price = 500000,
            HourlyRateLimit = 1000
        }
    };
}
