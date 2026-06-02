using Google.Cloud.Firestore;

namespace PISO.Entities.Models;

[FirestoreData]
public class PlanConfig
{
    [FirestoreProperty]
    public string Name { get; set; } = string.Empty;

    [FirestoreProperty("maxCredits")]
    public long MaxCredits { get; set; }

    [FirestoreProperty]
    public long Price { get; set; }

    [FirestoreProperty("hourlyRateLimit")]
    public int HourlyRateLimit { get; set; }
}

[FirestoreData]
public class SystemConfig
{
    [FirestoreProperty]
    public PlanConfig Free { get; set; } = new();

    [FirestoreProperty]
    public PlanConfig Developer { get; set; } = new();
}
