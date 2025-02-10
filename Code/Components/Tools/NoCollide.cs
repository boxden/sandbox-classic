[Library( "no_collide", Title = "No Collide", Description = "Removes Collison for props tag with the tool", Group = "construction" )]
public class NoCollide : BaseTool
{
	public override bool Primary( SceneTraceResult trace )
	{
		if ( !trace.Hit )
			return false;

		if ( Input.Pressed( "attack1" ) )
		{
			if ( trace.Component is MapCollider )
				return true;

			AddRemoveTag( trace.GameObject, "nocollide", "solid" );

			return true;
		}

		return false;
	}

	public override bool Secondary( SceneTraceResult trace )
	{
		if ( !trace.Hit )
			return false;

		if ( Input.Pressed( "attack2" ) )
		{
			if ( trace.Component is MapCollider )
				return true;

			AddRemoveTag( trace.GameObject, "solid", "nocollide" );

			return true;
		}

		return false;
	}

	[Rpc.Broadcast]
	void AddRemoveTag( GameObject gameObject, string add, string remove )
	{
		gameObject.Tags.Add( add );
		gameObject.Tags.Remove( remove );
	}
}
