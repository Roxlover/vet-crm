using System.IO;
using System.Threading.Tasks;

namespace VetCrm.Infrastructure.Storage;

public interface IR2Storage
{
    Task<string> UploadVisitImageAsync(int visitId, Stream stream, string contentType);
}
