using System.IO;

namespace MisterSpider
{
    public interface INetConnection
    {
        Stream? Read(Url url);

        Stream? Read(string absoluteUri);
    }
}
