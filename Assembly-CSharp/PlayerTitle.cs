
#if !VANILLA && !SERVER
// Custom titles
public class PlayerTitle
{
    public int ID { get; set; }
    public string TitleText { get; set; }
    public string Handle { get; set; }

    public string GetTitleText(int titleLevel)
    {
        // Adjust title text based on titleLevel if needed
        return TitleText;
    }
}
#endif