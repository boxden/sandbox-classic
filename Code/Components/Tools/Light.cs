[Library( "tool_light", Title = "Lights", Description = "A dynamic point light", Group = "construction" )]
public class LightTool : BaseTool
{
	PreviewModel PreviewModel;
	RealTimeSince timeSinceDisabled;

	protected override void OnAwake()
	{
		if ( IsProxy )
			return;

		PreviewModel = new PreviewModel
		{
			ModelPath = "models/light/light_tubular.vmdl",
			NormalOffset = 8f,
			PositionOffset = -Model.Load( "models/light/light_tubular.vmdl" ).Bounds.Center,
			FaceNormal = false
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
			if ( trace.Tags.Contains( "lamp" ) || trace.Tags.Contains( "player" ) )
				return true;

			var lamp = SpawnLamp( trace );

			PropHelper propHelper = lamp.Components.Get<PropHelper>();

			if ( !propHelper.IsValid() )
				return true;

			propHelper.Rope( trace.GameObject, Vector3.Zero, trace.GameObject.WorldTransform.PointToLocal( trace.EndPosition ) );

			return true;
		}

		return false;
	}

	void PositionLamp( GameObject lamp, SceneTraceResult trace )
	{
		lamp.WorldPosition = trace.HitPosition + PreviewModel.PositionOffset;
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

	GameObject SpawnLamp( SceneTraceResult trace )
	{
		var go = new GameObject()
		{
			Tags = { "solid", "lamp" }
		};

		PositionLamp( go, trace );

		var prop = go.AddComponent<Prop>();
		prop.Model = Model.Load( "models/light/light_tubular.vmdl" );

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

		var light = go.AddComponent<PointLight>();
		light.Shadows = false;
		light.Radius = 128;
		light.Attenuation = 1.0f;
		light.LightColor = Color.Random;

		go.NetworkSpawn();
		go.Network.SetOrphanedMode( NetworkOrphaned.Host );

		return go;
	}
}
