[Library( "tool_thruster", Title = "Thruster", Description = "A rocket type thing that can push forwards and backward", Group = "construction" )]
public class Thruster : BaseTool
{
	PreviewModel PreviewModel;
	RealTimeSince timeSinceDisabled;

	protected override void OnAwake()
	{
		if ( IsProxy )
			return;

		PreviewModel = new PreviewModel
		{
			ModelPath = "models/thruster/thrusterprojector.vmdl",
			RotationOffset = new Angles( 90, 0, 0 ),
			FaceNormal = true
		};
	}

	protected override void OnUpdate()
	{
		if ( IsProxy )
			return;

		if ( timeSinceDisabled < Time.Delta * 5f || !Parent.IsValid() )
			return;

		var trace = Parent.BasicTraceTool();

		PreviewModel.Update( trace );
	}

	public override bool Primary( SceneTraceResult trace )
	{
		if ( !trace.Hit || !trace.GameObject.IsValid() )
			return false;

		if ( Input.Pressed( "attack1" ) )
		{
			if ( trace.Tags.Contains( "thruster" ) || trace.Tags.Contains( "player" ) )
				return true;

			var thruster = SpawnThruster( trace );

			PropHelper propHelper = thruster.Components.Get<PropHelper>();
			if ( !propHelper.IsValid() )
				return true;

			propHelper.Weld( trace.GameObject );

			return true;
		}

		return false;
	}

	void PositionThruster( GameObject thruster, SceneTraceResult trace )
	{
		thruster.WorldPosition = trace.HitPosition + trace.Normal;
		thruster.WorldRotation = Rotation.LookAt( trace.Normal ) * new Angles( 90, 0, 0 );
	}

	protected override void OnDestroy()
	{
		PreviewModel?.Destroy();
		base.OnDestroy();
	}

	public override void Disabled()
	{
		timeSinceDisabled = 0;
		PreviewModel?.Destroy();
	}

	GameObject SpawnThruster( SceneTraceResult trace )
	{
		var go = new GameObject()
		{
			Tags = { "solid", "thruster" }
		};

		PositionThruster( go, trace );

		var prop = go.AddComponent<Prop>();
		prop.Model = Model.Load( "models/thruster/thrusterprojector.vmdl" );

		var propHelper = go.AddComponent<PropHelper>();
		propHelper.Invincible = true;

		if ( prop.Components.TryGet<SkinnedModelRenderer>( out var renderer ) )
		{
			renderer.CreateBoneObjects = true;
		}

		var rb = propHelper.Rigidbody;
		if ( rb.IsValid() )
		{
			foreach ( var shape in rb.PhysicsBody.Shapes )
			{
				if ( !shape.IsMeshShape )
					continue;

				var newCollider = go.AddComponent<BoxCollider>();
				newCollider.Scale = prop.Model.PhysicsBounds.Size;
			}
		}

		go.AddComponent<ThrusterForce>();

		go.NetworkSpawn();
		go.Network.SetOrphanedMode( NetworkOrphaned.Host );

		return go;
	}
}
