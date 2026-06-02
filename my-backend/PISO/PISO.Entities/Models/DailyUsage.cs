using Google.Cloud.Firestore;

namespace PISO.Entities.Models;

[FirestoreData]
public class DailyUsage
{
    [FirestoreProperty]
    public string Date { get; set; } = string.Empty;

    [FirestoreProperty]
    public long SuccessCount { get; set; }

    [FirestoreProperty]
    public long FailedCount { get; set; }
}
