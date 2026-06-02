namespace PISO.Shared;

public static class PisoConstants
{
    public static class Plans
    {
        public const string Free = "free";
        public const string Developer = "developer";
    }

    public static class Credits
    {
        public const long FreePlanMax = 200;
        public const long DeveloperPlanMax = 500;
    }

    public static class UserStatus
    {
        public const string Active = "active";
        public const string Suspended = "suspended";
    }

    public static class ApiKey
    {
        public const string Prefix = "piso_";
    }
}
