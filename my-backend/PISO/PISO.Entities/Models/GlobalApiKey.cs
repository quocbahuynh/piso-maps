using Google.Cloud.Firestore;

namespace PISO.Entities.Models;

[FirestoreData]
public class GlobalApiKey
{
    [FirestoreProperty]
    public string UserId { get; set; } = string.Empty;
}
