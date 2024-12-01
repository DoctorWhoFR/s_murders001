using Sandbox;

public sealed class TestColliderAttack : Component
{

	[Property]
	public Collider Collider { get; set; }

	[Property]
	public SkinnedModelRenderer ModelRenderer { get; set; }

	// dont work
	void OnTriggerEnter(Collider collide)
	{
		Log.Info( $"Touching {collide}" );
	}

	// dont work
	void OnGameObject( GameObject collide )
	{
		Log.Info( $"Touching {collide}" );
	}

	protected override void OnStart()
	{
		Log.Info( Collider );
		Collider.OnTriggerEnter += OnTriggerEnter;
		Collider.OnObjectTriggerEnter += OnGameObject;

		if(ModelRenderer != null )
		{
			ModelRenderer.Set( "Dances", 2 );
		}

		Log.Info( "is executed" );
	}

	protected override void OnUpdate()
	{
		// work
		foreach ( var touching in Collider.Touching )
		{
			//Log.Info(touching);
		}
	}
}
