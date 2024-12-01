using System;
using Sandbox;

public sealed class CustomPlayerController : Component
{

	enum WeaponType
	{
		None = 0,
		Gun = 1,
		Murderer = 2,
	}

	[Property]
	public PlayerType playerType { get; set; }

	[Property]
	public System.Guid Id { get; set; }

	[Property]
	public GameObject knifeModel { get; set; }

	public string Hash()
	{
		return $"{playerType}-{Id}";
	}


	protected override void OnUpdate()
	{
		base.OnUpdate();

		if(Input.Pressed("attack1"))
		{
			Log.Info( "attack1" );

			
		}
	}
}
