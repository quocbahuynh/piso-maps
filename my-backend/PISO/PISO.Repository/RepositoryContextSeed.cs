using Google.Cloud.Firestore;
using PISO.Repository.Configuration;

namespace PISO.Repository;

public static class RepositoryContextSeed
{
    public static async Task SeedAsync(FirestoreDb db)
    {
        var docRef = db.Collection("system_configs").Document("plans");
        var snapshot = await docRef.GetSnapshotAsync();

        if (!snapshot.Exists)
        {
            await docRef.SetAsync(SystemConfigConfiguration.GetSeedData());
        }
    }
}
