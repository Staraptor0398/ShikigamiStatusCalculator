using ShikigamiDataAuditor.Model;
using System.Threading.Tasks;

namespace ShikigamiDataAuditor.External
{
	public interface IShikigamiStatusSource
	{
		Task<ExternalStatusResult> GetStatusAsync(ExternalNameMapEntry nameMapEntry);
	}
}
