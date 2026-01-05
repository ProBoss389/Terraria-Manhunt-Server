using Microsoft.Xna.Framework;
using ReLogic.Peripherals.RGB;
using Terraria.IO;

namespace Terraria.Initializers;

public static class ChromaInitializer
{
	private static ChromaEngine _engine;
	private const string GAME_NAME_ID = "TERRARIA";
	private static float _rgbUpdateRate;
	private static bool _useRazer;
	private static bool _useCorsair;
	private static bool _useLogitech;
	private static bool _useSteelSeries;
	private static VendorColorProfile _razerColorProfile;
	private static VendorColorProfile _corsairColorProfile;
	private static VendorColorProfile _logitechColorProfile;
	private static VendorColorProfile _steelSeriesColorProfile;

	public static void BindTo(Preferences preferences)
	{
		preferences.OnSave += Configuration_OnSave;
		preferences.OnLoad += Configuration_OnLoad;
	}

	private static void Configuration_OnLoad(Preferences obj)
	{
		_useRazer = obj.Get("UseRazerRGB", defaultValue: true);
		_useCorsair = obj.Get("UseCorsairRGB", defaultValue: true);
		_useLogitech = obj.Get("UseLogitechRGB", defaultValue: true);
		_useSteelSeries = obj.Get("UseSteelSeriesRGB", defaultValue: true);
		_razerColorProfile = obj.Get("RazerColorProfile", new VendorColorProfile(new Vector3(1f, 0.765f, 0.568f)));
		_corsairColorProfile = obj.Get("CorsairColorProfile", new VendorColorProfile());
		_logitechColorProfile = obj.Get("LogitechColorProfile", new VendorColorProfile());
		_steelSeriesColorProfile = obj.Get("SteelSeriesColorProfile", new VendorColorProfile());
		_rgbUpdateRate = obj.Get("RGBUpdatesPerSecond", 45f);
		if (_rgbUpdateRate <= 1E-07f)
			_rgbUpdateRate = 45f;
	}

	private static void Configuration_OnSave(Preferences preferences)
	{
		preferences.Put("RGBUpdatesPerSecond", _rgbUpdateRate);
		preferences.Put("UseRazerRGB", _useRazer);
		preferences.Put("RazerColorProfile", _razerColorProfile);
		preferences.Put("UseCorsairRGB", _useCorsair);
		preferences.Put("CorsairColorProfile", _corsairColorProfile);
		preferences.Put("UseLogitechRGB", _useLogitech);
		preferences.Put("LogitechColorProfile", _logitechColorProfile);
		preferences.Put("UseSteelSeriesRGB", _useSteelSeries);
		preferences.Put("SteelSeriesColorProfile", _steelSeriesColorProfile);
	}

	public static void DisableAllDeviceGroups()
	{
		if (_engine != null)
			_engine.DisableAllDeviceGroups();
	}

	public static void Load()
	{
		_engine = Main.Chroma;
	}

	public static void UpdateEvents()
	{
	}
}
