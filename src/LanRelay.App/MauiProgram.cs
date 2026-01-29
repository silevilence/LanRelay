using Microsoft.Extensions.Logging;
using LanRelay.Core.State;
using LanRelay.Core.FileTransfer;

namespace LanRelay.App;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();

		// Register State Services (Singleton for State Container Pattern)
		builder.Services.AddSingleton<DeviceListState>();
		builder.Services.AddSingleton<ChatState>();
		builder.Services.AddSingleton<TransferState>();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
