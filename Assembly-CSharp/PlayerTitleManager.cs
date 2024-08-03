using System.Collections.Generic;

#if !VANILLA && !SERVER
// Custom titles
public class PlayerTitleManager
{
    public TitleFetcher titleFetcher;
    public List<PlayerTitle> PlayerTitles = new List<PlayerTitle>();
    private Dictionary<string, List<PlayerTitle>> cachedTitlesByHandle;
    private static PlayerTitleManager m_instance;

    public static PlayerTitleManager GetInstance()
    {
        if (m_instance == null)
        {
            m_instance = new PlayerTitleManager();
        }
        return m_instance;
    }

    private PlayerTitleManager()
    {
    }

    public void Init()
    {
        titleFetcher = new TitleFetcher();
        titleFetcher.TitlesFetched += OnTitlesFetched;
        CoroutineRunner.Instance.RunCoroutine(titleFetcher.FetchTitlesFromApi());
    }

    private void OnTitlesFetched(Dictionary<string, List<PlayerTitle>> titlesByHandle)
    {
        if (titlesByHandle != null)
        {
            cachedTitlesByHandle = titlesByHandle;
        }
    }

    public string GetTitle(int titleID, string handle, string returnOnEmptyOverride = "", int titleLevel = -1)
    {
        string normalizedHandle = handle.Split('#')[0].Trim();
        if (cachedTitlesByHandle == null)
        {
            Log.Info("cachedTitlesByHandle is null.");
            return returnOnEmptyOverride;
        }

        foreach (var kvp in cachedTitlesByHandle)
        {
            string normalizedKey = kvp.Key.Split('#')[0].Trim();
            if (normalizedKey == normalizedHandle)
            {
                foreach (PlayerTitle title in kvp.Value)
                {
                    string normalizedTitleHandle = title.Handle.Split('#')[0].Trim();
                    if (normalizedTitleHandle == normalizedHandle)
                    {
                        return title.TitleText;
                    }
                }
                break;
            }
        }

        return returnOnEmptyOverride;
    }
}
#endif