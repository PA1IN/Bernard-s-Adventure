using Godot;

public partial class Nob : CharacterBody2D
{
	[Export] public float speed = 100f; // Velocidad del enemigo
	[Export] public CharacterBody2D target; // Referencia al jugador (Hamster)
	private NavigationAgent2D navigationAgent; // Referencia al agente de navegación
	private Timer updateTimer; // Timer para actualizar la ruta de navegación

	public override void _Ready()
	{
		// Obtén la referencia al NavigationAgent2D
		navigationAgent = GetNode<NavigationAgent2D>("Navigation/NavigationAgent2D");

		// Obtén la referencia al hámster
		target = GetNodeOrNull<CharacterBody2D>("../Hamster");

		// Verificar la existencia de los nodos necesarios
		if (navigationAgent == null)
		{
			GD.PrintErr("No se encontró el nodo NavigationAgent2D.");
		}

		if (target == null)
		{
			GD.PrintErr("No se encontró al hámster.");
		}

		// Configura el Timer
		updateTimer = new Timer();
		AddChild(updateTimer);
		updateTimer.WaitTime = 0.5f; // Actualiza cada 0.5 segundos
		updateTimer.Timeout += OnTimerTimeout; // Conecta el Timer al método
		updateTimer.Start();

		// Configura el agente de navegación para actualizar la posición del hámster
		if (target != null)
		{
			navigationAgent.TargetPosition = target.Position;
		}
	}

	private void OnTimerTimeout()
	{
		if (target != null)
		{
			navigationAgent.TargetPosition = target.Position; // Actualiza la posición del objetivo
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (navigationAgent == null || target == null) return;

		// Mueve a Nob basándose en la velocidad calculada por el agente de navegación
		Vector2 nextPosition = navigationAgent.GetNextPathPosition();
		if (nextPosition != GlobalPosition) // Compara con la posición actual
		{
			Vector2 direction = (nextPosition - GlobalPosition).Normalized();
			Velocity = direction * speed;
			MoveAndSlide();
		}
	}


	public override void _Process(double delta)
	{
		// Manejo de colisiones con el hámster
		for (int i = 0; i < GetSlideCollisionCount(); i++)
		{
			var collision = GetSlideCollision(i);
			var collider = collision.GetCollider();
			if (collider is CharacterBody2D && collider == target)
			{
				OnCollisionWithHamster();
				break;
			}
		}
	}

	private void OnCollisionWithHamster()
	{
		GD.Print("¡Nob ha atrapado al hámster!");
		Velocity = Vector2.Zero; // Detiene a Nob
		RestartScene(); // Reinicia la escena
	}

	private void RestartScene()
	{
		GetTree().ReloadCurrentScene(); // Recarga la escena actual
	}
}
