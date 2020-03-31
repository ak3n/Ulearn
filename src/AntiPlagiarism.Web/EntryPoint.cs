using System.Threading.Tasks;
using Ulearn.Common.Api;
using Vostok.Hosting;

namespace AntiPlagiarism.Web
{
	public static class EntryPoint
	{
		public static async Task Main(string[] args)
		{
			var application = new WebApplication();
			var setupBuilder = new EnvironmentSetupBuilder("AntiPlagiarism.Web", args);
			var hostSettings = new VostokHostSettings(application, setupBuilder.EnvironmentSetup);
			var host = new VostokHost(hostSettings);
			await host.WithConsoleCancellation().RunAsync();
		}
	}
}