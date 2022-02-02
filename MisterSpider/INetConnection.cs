namespace MisterSpider
{
    public interface INetConnection
    {
        string Go(Url url);

        string Go(string absoluteUri);
    }
}
