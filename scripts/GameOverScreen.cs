using Godot;

public partial class GameOverScreen : Control
{
	private Button botonReiniciar;
	private Button botonSalir;

	public override void _Ready()
	{
		// Obtener las referencias a los botones
		botonReiniciar = GetNode<Button>("BotonReiniciar");
		botonSalir = GetNode<Button>("BotonSalir");

		// Conectar las señales de los botones usando eventos de C#
		botonReiniciar.Pressed += AlPresionarReiniciar;
		botonSalir.Pressed += AlPresionarSalir;

		// Ocultar la pantalla de fin de juego al inicio
		Visible = false;
	}

	// Mostrar la pantalla de "Game Over"
	public void MostrarPantallaFin()
	{
		GD.Print("Pantalla de fin de juego activada.");
		Visible = true;
		GetTree().Paused = true; // Pausar el juego
	}

	// Función que se ejecuta al presionar el botón de reiniciar
	private void AlPresionarReiniciar()
	{
		GetTree().Paused = false;
		GetTree().ReloadCurrentScene(); // Reiniciar la escena actual
	}

	// Función que se ejecuta al presionar el botón de salir
	private void AlPresionarSalir()
	{
		GetTree().Quit(); // Salir del juego
	}
}
