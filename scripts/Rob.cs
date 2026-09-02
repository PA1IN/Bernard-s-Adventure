/*using Godot;

public partial class Rob : CharacterBody2D
{
	[Export] public float speed = 150f; // Velocidad del robot
	[Export] public CharacterBody2D target;
	private NavigationAgent2D navigationAgent; // Referencia al agente de navegación

	//private CharacterBody2D target; // Referencia al hámster

	public override void _Ready()
	{
		// Obtén la referencia al NavigationAgent2D
		navigationAgent = GetNode<NavigationAgent2D>("$Navegation/NavigationAgent2D");

		// Obtén la referencia al hámster
		target = GetNodeOrNull<CharacterBody2D>("../Hamster");

		if (navigationAgent == null)
		{
			GD.PrintErr("No se encontró el nodo NavigationAgent2D.");
		}

		if (target == null)
		{
			GD.PrintErr("No se encontró al hámster.");
		}

		// Configura el agente de navegación
		navigationAgent.VelocityComputed += OnVelocityComputed; // Conecta el evento para manejar la navegación
	}

	public override void _PhysicsProcess(double delta)
	{
		if (navigationAgent == null || target == null) return;

		// Actualiza la posición objetivo del hámster
		navigationAgent.TargetPosition = target.Position;
	}

	private void OnVelocityComputed(Vector2 safeVelocity)
	{
		// Aplica la velocidad calculada al robot
		Velocity = safeVelocity * speed;
		MoveAndSlide();
	}

	public override void _Process(double delta)
	{
		// Manejo de colisiones con el hámster
		for (int i = 0; i < GetSlideCollisionCount(); i++)
		{
			var collision = GetSlideCollision(i);

			if (collision.GetCollider() == target)
			{
				OnCollisionWithHamster();
				break;
			}
		}
	}
	
	/*public void _on_timer_timeout()
	{
		navegationAgent.target_position = target.global_position;
	}*/

	/*private void OnCollisionWithHamster()
	{
		GD.Print("¡El robot ha atrapado al hámster!");
		Velocity = Vector2.Zero;

		RestartScene(); // Reiniciamos la escena
	}

	private void RestartScene()
	{
		GetTree().ReloadCurrentScene();
	}
}*/
