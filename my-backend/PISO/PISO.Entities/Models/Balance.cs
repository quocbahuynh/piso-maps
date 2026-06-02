using Google.Cloud.Firestore;

namespace PISO.Entities.Models;

[FirestoreData]
public class Balance
{
    [FirestoreProperty]
    public long RemainingCredits { get; set; }

    [FirestoreProperty]
    public Timestamp UpdatedAt { get; set; }
}
