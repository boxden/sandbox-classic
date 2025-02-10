[Library( "tool_lamp", Title = "Lamps", Description = "Directional light source that casts shadows", Group = "construction" )]
public class Lamp : BaseTool
{
	PreviewModel PreviewModel;
	RealTimeSince timeSinceDisabled;

	protected override void OnAwake()
	{
		if ( IsProxy )
			return;

		PreviewModel = new PreviewModel
		{
			ModelPath = "models/torch/torch.vmdl",
			NormalOffset = 8f,
			PositionOffset = -Model.Load("models/torch/torch.vmdl").Bounds.Center,
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
		prop.Model = Model.Load( "models/torch/torch.vmdl" );

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

		var spotLight = go.AddComponent<SpotLight>();
		spotLight.Radius = 512;
		spotLight.Attenuation = 1.0f;
		spotLight.ConeInner = 25;
		spotLight.ConeOuter = 45;
		spotLight.LightColor = Color.Random;
		spotLight.Cookie = Texture.Load( "materials/effects/lightcookie.vtex_c" );

		go.GetComponent<ModelRenderer>().Tint = spotLight.LightColor;

		go.NetworkSpawn();
		go.Network.SetOrphanedMode( NetworkOrphaned.Host );

		return go;
	}
}
