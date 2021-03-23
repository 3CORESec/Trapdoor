using System.Collections.Generic;
using System.Threading.Tasks;

namespace Trapdoor
{
    public interface ISender
    {
        Task<string> SendAlert((string, Dictionary<string, dynamic>) res, string sourceIp, string path,
            string guid);

        Task StoreLogs(string id, string data);
    }
}
