using Editor;

namespace PolyHaven;

[Tool("HDRI Porting", "download", "Import PolyHaven HDRIs" )]
public class HDRIPortingWindow : Dialog
{
	public HDRIPortingWindow()
	{
		DeleteOnClose = true;
		Size = new Vector2( 400, 200 );
		SetWindowIcon( "download" );

		CreateUI();
	}

	public void CreateUI()
	{
		SetLayout( LayoutMode.TopToBottom );
		Layout.Spacing = 4;

		var settings = Layout.Add( LayoutMode.TopToBottom );
		settings.Margin = 20;
		settings.Spacing = 8;

		var label = new Label( "hellooo" );
		settings.Add( label );

		Layout.AddStretchCell( 1 );
		Layout.AddSeparator();
		Layout.AddStretchCell( 1 );
	}

	[Sandbox.Event.Hotload]
	public void OnHotload()
	{
		CreateUI();
	}
}
