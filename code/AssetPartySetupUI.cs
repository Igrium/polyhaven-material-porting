using Editor;
using PolyHaven.API;
using PolyHaven.Meta;
using PolyHaven.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolyHavenp;

[Dock( "Editor", "Asset Party Setup", "snippet_folder" )]
public class AssetPartySetupUI : Widget
{
	protected Label Label { get; private set; }

	protected LineEdit TagsField { get; private set; }
	protected TextEdit DescriptionField { get; private set; }
	protected PropertyRowError AssetError { get; private set; }
	protected Button OpenPreviewButton { get; private set; }

	public AssetMeta? CurrentAsset { get; private set; }
	public Asset? SBoxAsset { get; private set; }

#nullable disable
	public AssetPartySetupUI( Widget parent ) : base( parent )
	{
		Utility.OnInspect += StartInspecting;
		CreateUI();
	}
#nullable enable

	protected virtual void CreateUI()
	{
		SetLayout( LayoutMode.TopToBottom );
		Layout.Margin = 16;
		Label = new Label("No object selected.");
		Layout.Add( Label );
		Layout.AddSpacingCell( 8 );

		{
			var sheet = Layout.Add( new PropertySheet( this ) );
			TagsField = sheet.AddLineEdit( "Tags", "" );
			DescriptionField = sheet.AddTextEdit( "Description", "" );

			AssetError = sheet.AddRow( new PropertyRowError() );
			AssetError.Visible = false;
		}
		Layout.AddSeparator();
		Layout.AddSpacingCell( 4 );

		OpenPreviewButton = Layout.Add( new Button("Open previews") );
		OpenPreviewButton.Clicked = () => OpenPreviewLocation();

		Layout.AddStretchCell();

	}

	protected virtual void StartInspecting( object obj )
	{
		Label.Text = $"Selected Object: {obj}";
		Reset();

		if ( obj is Asset asset )
		{
			try
			{
				AssetMeta meta = new AssetMeta( asset );
				PopulateFields( meta );
				CurrentAsset = meta;
				SBoxAsset = asset;
				OpenPreviewButton.Enabled = true;
			} catch (ArgumentException e)
			{
				Log.Warning( e );
				ShowError( e.Message );
			}
		}
		else
		{
			ShowError( "Selected object is not an asset." );
		}
	}

	protected void PopulateFields(AssetMeta meta)
	{
		TagsField.Value = meta.Tags;
		TagsField.Enabled = true;

		DescriptionField.PlainText = TextUtils.WriteDescription( meta );
		DescriptionField.Enabled = true;
		CurrentAsset = null;
	}

	protected void Reset()
	{
		AssetError.Visible = false;
		TagsField.Enabled = false;
		TagsField.Value = "";

		DescriptionField.Enabled = false;
		DescriptionField.PlainText = "";

		CurrentAsset = null;
		SBoxAsset = null;

		OpenPreviewButton.Enabled = false;
	}

	protected void ShowError(string message)
	{
		AssetError.Label.Text = message;
		AssetError.Visible = true;
	}

	protected virtual void OpenPreviewLocation()
	{
		if ( SBoxAsset == null ) return;

		string path = Path.Join( Path.GetDirectoryName( SBoxAsset.AbsolutePath ), "preview" );

		var sphereFile = Path.Join( path, CurrentAsset?.PolyID + "_sphere.jpg" );
		var planeFile = Path.Join( path, CurrentAsset?.PolyID + "_plane.jpg" );
		if ( File.Exists( sphereFile ) )
		{
			Utility.OpenFileFolder( sphereFile );
		}
		else if ( File.Exists( planeFile ) )
		{
			Utility.OpenFileFolder( planeFile );
		}
		else
		{
			Utility.OpenFolder( path );
		}
	}

	public override void OnDestroyed()
	{
		base.OnDestroyed();
		Utility.OnInspect -= StartInspecting;
	}

	[Sandbox.Event.Hotload]
	public void OnHotload()
	{
		CreateUI();
	}
}
