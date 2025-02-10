using System.Diagnostics;

[Library( "tool_resizer", Title = "Resizer", Description = "Change the scale of things", Group = "construction" )]
public class Resizer : BaseTool
{
	public override bool Primary( SceneTraceResult trace )
	{
		if ( !trace.Hit )
			return false;

		var skinnedModelRenderer = trace.GameObject.GetComponent<SkinnedModelRenderer>();
		if ( skinnedModelRenderer.IsValid() )
		{
			var go = skinnedModelRenderer.GetBoneObject( trace.Bone );
			var size = go.WorldScale;
			size = size + (0.5f * Time.Delta);

			SetRagSize( trace.GameObject, size, trace.Bone );

			return true;
		}
		else
		{
			var go = trace.GameObject;
			var size = go.WorldScale;
			size = size + (0.5f * Time.Delta);

			SetPropSize( trace.GameObject, size );

			return true;
		}
	}

	public override bool Secondary( SceneTraceResult trace )
	{
		if ( !trace.Hit )
			return false;

		if ( !trace.GameObject.IsValid() )
			return false;

		var skinnedModelRenderer = trace.GameObject.GetComponent<SkinnedModelRenderer>();
		if (skinnedModelRenderer.IsValid())
		{
			var go = skinnedModelRenderer.GetBoneObject(trace.Bone);
			var size = go.WorldScale;
			size = size - (0.5f * Time.Delta);

			SetRagSize( trace.GameObject, size, trace.Bone );

			return true;
		}
		else
		{
			var go = trace.GameObject;
			var size = go.WorldScale;
			size = size - (0.5f * Time.Delta);

			SetPropSize( trace.GameObject, size );

			return true;
		}
	}

	[Rpc.Broadcast]
	void SetPropSize( GameObject gameObject, Vector3 size )
	{
		gameObject.WorldScale = size;
	}

	[Rpc.Broadcast]
	void SetRagSize( GameObject gameObject, Vector3 size, int index )
	{
		var skinnedModelRenderer = gameObject.GetComponent<SkinnedModelRenderer>();

		if ( !skinnedModelRenderer.IsValid() )
			return;

		var go = skinnedModelRenderer.GetBoneObject( index );

		go.Flags = GameObjectFlags.ProceduralBone;
		go.WorldScale = size;
	}
}
