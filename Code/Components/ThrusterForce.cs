public class ThrusterForce : Component
{
	Rigidbody _rigidbody;
	Rigidbody rigidbody
	{
		get
		{
			if ( !_rigidbody.IsValid() )
			{
				_rigidbody = Components.Get<Rigidbody>();
			}
			return _rigidbody;
		}
	}

	protected override void OnUpdate()
	{
		rigidbody.ApplyForce( WorldRotation.Down * 50000000 * Time.Delta);
	}
}
