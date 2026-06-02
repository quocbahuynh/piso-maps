using Google.Cloud.Firestore;

namespace PISO.Repository;

public class RepositoryContext
{
    private readonly FirestoreDb _db;

    public RepositoryContext(FirestoreDb db)
    {
        _db = db;
    }

    public CollectionReference Users => _db.Collection("users");
    public CollectionReference SystemConfigs => _db.Collection("system_configs");
    public CollectionReference GlobalApiKeys => _db.Collection("global_api_keys");

    public CollectionReference Balances(string userId) =>
        Users.Document(userId).Collection("balances");

    public CollectionReference DailyUsage(string userId) =>
        Users.Document(userId).Collection("daily_usage");
}
