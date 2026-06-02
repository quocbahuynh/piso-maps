namespace PISO.GoogleMapService;

public static class GoogleMapsRequestBuilder
{
    private const string CommonBrowserOptions =
        "!12m25!1m5!18b1!30b1!31m1!1b1!34e1!2m4!5m1!6e2!20e3!39b1!10b1!12b1!13b1!16b1!17m1!3e1" +
        "!20m3!5e2!6b1!14b1!46m1!1b0!96b1!99b1!19m4!2m3!1i360!2i120!4i8";

    private const string CommonMapPanelOptions =
        "!2m2!1i203!2i100!3m2!2i4!5b1!6m6!1m2!1i86!2i86!1m2!1i408!2i240";

    private const string CommonReviewOptions =
        "!65m5!3m4!1m3!1m2!1i224!2i298" +
        "!72m22!1m8!2b1!5b1!7b1!12m4!1b1!2b1!4m1!1e1!4b1" +
        "!8m10!1m6!4m1!1e1!4m1!1e3!4m1!1e4!3sother_user_google_review_posts__and__hotel_and_vr_partner_review_posts!6m1!1e1!9b1";

    public static string BuildSearchPb(string latStr, string lngStr)
    {
        return
            $"!4m12!1m3!1d1054.5317069146395!2d{lngStr}!3d{latStr}" +
            "!2m3!1f0!2f0!3f0!3m2!1i585!2i827!4f13.1!7i20!10b1" +
            CommonBrowserOptions +
            "!20m65" + CommonMapPanelOptions +
            "!7m33!1m3!1e1!2b0!3e3!1m3!1e2!2b1!3e2!1m3!1e2!2b0!3e3!1m3!1e8!2b0!3e3!1m3!1e10!2b0!3e3" +
            "!1m3!1e10!2b1!3e2!1m3!1e10!2b0!3e4!1m3!1e9!2b1!3e2!2b1!9b0" +
            "!15m16!1m7!1m2!1m1!1e2!2m2!1i195!2i195!3i20!1m7!1m2!1m1!1e2!2m2!1i195!2i195!3i20" +
            "!22m6!1s-MoLaufnDZyMseMPxJHfoQw%3A3!2s1i%3A0%2Ct%3A11887%2Cp%3A-MoLaufnDZyMseMPxJHfoQw%3A3!7e81!12e5!17s-MoLaufnDZyMseMPxJHfoQw%3A86!18e15" +
            "!24m107!1m27!13m9!2b1!3b1!4b1!6i1!8b1!9b1!14b1!20b1!25b1" +
            "!18m16!3b1!4b1!5b1!6b1!9b1!13b1!14b1!17b1!20b1!21b1!22b1!32b1!33m1!1b1!34b1!36e2" +
            "!10m1!8e3!11m1!3e1!17b1!20m2!1e3!1e6!24b1!25b1!26b1!27b1!29b1!30m1!2b1!36b1!37b1" +
            "!39m3!2m2!2i1!3i1!43b1!52b1!55b1!56m1!1b1!61m2!1m1!1e1" +
            CommonReviewOptions +
            "!89b1!90m2!1m1!1e2!98m3!1b1!2b1!3b1!103b1!113b1!114m3!1b1!2m1!1b1!117b1!122m1!1b1!126b1!127b1!128m1!1b0" +
            "!26m4!2m3!1i80!2i92!4i8" +
            "!30m0" +
            "!34m19!2b1!3b1!4b1!6b1!8m6!1b1!3b1!4b1!5b1!6b1!7b1!9b1!12b1!14b1!20b1!23b1!25b1!26b1!31b1" +
            "!37m1!1e81!42b1!47m0!49m10!3b1!6m2!1b1!2b1!7m2!1e3!2b1!8b1!9b1!10e2!50m4!2e2!3m2!1b1!3b1" +
            "!67m5!7b1!10b1!14b1!15m1!1b0!69i780!77b1";
    }

    public static string BuildPlacePb(string googleId, string latStr, string lngStr)
    {
        return
            $"!1m14!1s{googleId}" +
            $"!3m12!1m3!1d15668.304202086909!2d{lngStr}!3d{latStr}" +
            "!2m3!1f0.0!2f0.0!3f0.0!3m2!1i1024!2i768!4f13.1" +
            "!12m4!2m3!1i360!2i120!4i8" +
            "!13m53" + CommonMapPanelOptions +
            "!7m29!1m3!1e1!2b0!3e3!1m3!1e2!2b1!3e2!1m3!1e2!2b0!3e3!1m3!1e8!2b0!3e3!1m3!1e10!2b0!3e3" +
            "!1m3!1e10!2b1!3e2!1m3!1e10!2b0!3e4!2b1!9b0" +
            "!15m8!1m7!1m2!1m1!1e2!2m2!1i195!2i195!3i20" +
            "!14m3!1s3Z4MarO5Fsfl2roP0rDX2Qo!7e81!15i10112" +
            "!15m108!1m26!13m9!2b1!3b1!4b1!6i1!8b1!9b1!14b1!20b1!25b1" +
            "!18m15!3b1!4b1!5b1!6b1!13b1!14b1!17b1!21b1!22b1!30b1!32b1!33m1!1b1!34b1!36e2" +
            "!10m1!8e3!11m1!3e1!17b1!20m2!1e3!1e6!24b1!25b1!26b1!27b1!29b1!30m1!2b1!36b1!37b1" +
            "!39m3!2m2!2i1!3i1!43b1!52b1!54m1!1b1!55b1!56m1!1b1!61m2!1m1!1e1" +
            CommonReviewOptions +
            "!89b1!90m2!1m1!1e2!98m3!1b1!2b1!3b1!103b1!113b1!114m3!1b1!2m1!1b1!117b1!122m1!1b1!126b1!127b1!128m1!1b1" +
            "!21m0!22m1!1e81!30m8!3b1!6m2!1b1!2b1!7m2!1e3!2b1!9b1" +
            "!34m5!7b1!10b1!14b1!15m1!1b0!37i779";
    }

    public static string BuildPb(string latStr, string lngStr)
    {
        return
            $"!2i8!4m12!1m3!1d6895.626398710887!2d{lngStr}!3d{latStr}" +
            "!2m3!1f0!2f0!3f0!3m2!1i1512!2i169!4f13.1!7i20!10b1" +
            CommonBrowserOptions +
            "!20m57" + CommonMapPanelOptions +
            "!7m33!1m3!1e1!2b0!3e3!1m3!1e2!2b1!3e2!1m3!1e2!2b0!3e3!1m3!1e8!2b0!3e3!1m3!1e10!2b0!3e3" +
            "!1m3!1e10!2b1!3e2!1m3!1e10!2b0!3e4!1m3!1e9!2b1!3e2!2b1!9b0" +
            "!15m8!1m7!1m2!1m1!1e2!2m2!1i195!2i195!3i20" +
            "!22m3!1seqsJap3cHqak2roPpICloAI!7e81!17seqsJap3cHqak2roPpICloAI%3A69!23m2!4b1!10b1" +
            "!24m107!1m27!13m9!2b1!3b1!4b1!6i1!8b1!9b1!14b1!20b1!25b1" +
            "!18m16!3b1!4b1!5b1!6b1!9b1!13b1!14b1!17b1!20b1!21b1!22b1!32b1!33m1!1b1!34b1!36e2" +
            "!10m1!8e3!11m1!3e1!17b1!20m2!1e3!1e6!24b1!25b1!26b1!27b1!29b1!30m1!2b1!36b1!37b1" +
            "!39m3!2m2!2i1!3i1!43b1!52b1!55b1!56m1!1b1!61m2!1m1!1e1" +
            CommonReviewOptions +
            "!89b1!90m2!1m1!1e2!98m3!1b1!2b1!3b1!103b1!113b1!114m3!1b1!2m1!1b1!117b1!122m1!1b1!126b1!127b1!128m1!1b0" +
            "!26m4!2m3!1i80!2i92!4i8" +
            "!34m19!2b1!3b1!4b1!6b1!8m6!1b1!3b1!4b1!5b1!6b1!7b1!9b1!12b1!14b1!20b1!23b1!25b1!26b1!31b1" +
            "!37m1!1e81!47m0!49m10!3b1!6m2!1b1!2b1!7m2!1e3!2b1!8b1!9b1!10e2!61b1" +
            "!67m5!7b1!10b1!14b1!15m1!1b0!69i779!77b1";
    }
}
