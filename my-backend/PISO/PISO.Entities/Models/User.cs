using Google.Cloud.Firestore;

namespace PISO.Entities.Models;

[FirestoreData]
public class User
{
    [FirestoreProperty]
    public string UserId { get; set; } = string.Empty;

    [FirestoreProperty]
    public string Email { get; set; } = string.Empty;

    [FirestoreProperty]
    public string Plan { get; set; } = string.Empty;

    [FirestoreProperty]
    public long MaxCredits { get; set; }

    [FirestoreProperty]
    public string ApiKey { get; set; } = string.Empty;

    [FirestoreProperty]
    public string Status { get; set; } = string.Empty;

    [FirestoreProperty]
    public Timestamp CreatedAt { get; set; }
}
