using Editor;
using Sandbox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PolyHaven;

public class PolySettings
{
	public const string SETTINGS_FILE = "hdri_porting_setitings.json";

	[JsonIgnore]
	private static PolySettings? _instance;

	public static PolySettings Instance
	{
		get
		{
			if ( _instance == null )
				_instance = LoadFromDisk();
			return _instance;
		}
	}

	public string BlenderPath { get; set; } = "";

	private LocalProject? _activeProject;

	[JsonIgnore]
	public LocalProject? ActiveProject
	{
		get => _activeProject; set
		{
			_activeProject = value;
		}
	}


	public string ActiveProjectPath
	{
		get => ActiveProject != null ? ActiveProject.Path : "";
		set => ActiveProject = FindLocalProject( value );
	}

	public void ReloadActiveProject()
	{
		_activeProject = FindLocalProject( ActiveProjectPath );
	}

	private static LocalProject? FindLocalProject( string path )
	{
		Log.Trace( path );
		foreach ( var proj in Utility.Projects.GetAll() )
		{
			Log.Trace( proj.Path );
			if ( proj.Active && proj.Path == path )
				return proj;
		}
		return null;
	}

	public static PolySettings LoadFromDisk()
	{
		if ( !File.Exists( SETTINGS_FILE ) )
			return CreateDefaultSettings();

		var jsonInput = File.ReadAllText( SETTINGS_FILE );
		var settings = JsonSerializer.Deserialize<PolySettings>( jsonInput );
		if ( settings == null )
			throw new InvalidOperationException( "Unable to load poly haven settings" );

		settings.ActiveProject ??= Utility.Projects.GetAll().Where( x => x.Active ).FirstOrDefault();
		settings.SaveToDisk();

		return settings;
	}

	public static PolySettings Reload()
	{
		_instance = LoadFromDisk();
		return _instance;
	}

	public void SaveToDisk()
	{
		var jsonOutput = JsonSerializer.Serialize( this );
		File.WriteAllText( SETTINGS_FILE, jsonOutput );
	}

	private static PolySettings CreateDefaultSettings()
	{
		var settings = new PolySettings();
		settings.SaveToDisk();
		return settings;
	}
}
