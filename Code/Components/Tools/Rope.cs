[Library( "tool_rope", Title = "Rope", Description = "Join two things together with a rope", Group = "construction" )]
public class Rope : BaseTool
{
	GameObject roped;
	Vector3 ropedPos;

	public override bool Primary( SceneTraceResult trace )
	{
		if ( !trace.Hit )
			return false;

		if ( Input.Pressed( "attack1" )  )
		{
			

			if ( roped == null )
			{
				roped = trace.GameObject;
				ropedPos = trace.GameObject.WorldTransform.PointToLocal( trace.EndPosition );
				return true;
			}

			if ( trace.GameObject == roped )
				return false;

			float distance = Vector3.DistanceBetween( roped.WorldTransform.PointToWorld( ropedPos ), trace.EndPosition ) * 2;

			if ( trace.GameObject.Components.TryGet<PropHelper>( out var propHelper ))
			{
				propHelper.Rope( roped, propHelper.WorldTransform.PointToLocal( trace.EndPosition ), ropedPos, 0, distance);
			}
			else if ( roped.Components.TryGet<PropHelper>( out var selfPropHelper ) )
			{
				selfPropHelper.Rope( trace.GameObject, ropedPos, trace.GameObject.WorldTransform.PointToLocal( trace.EndPosition ), 0, distance );
			}

			roped = null;
			return true;
		}

		return false;
	}
}
