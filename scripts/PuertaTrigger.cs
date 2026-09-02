using Godot;
using System;

public partial class PuertaTrigger : Area2D
{
	// Path to the puzzle scene
	private string puzzleScenePath = "res://node_2d.tscn"; // Cambia la ruta a la correcta

	public override void _Ready()
	{
		// Conectar la señal de entrada de cuerpo al método OnBodyEntered
		Connect("body_entered", new Callable(this, nameof(OnBodyEntered)));
	}

	private void OnBodyEntered(Node body)
	{
		// Verificar si el cuerpo que entra es el jugador
		if (body.Name == "Hamster") // Asegúrate de que 'Hamster' es el nombre correcto del nodo del jugador
		{
			ChangeToPuzzleScene();
		}
	}

	private void ChangeToPuzzleScene()
	{
		// Cambiar a la escena del puzzle
		Error err = GetTree().ChangeSceneToFile(puzzleScenePath);
		if (err != Error.Ok)
		{
			GD.PrintErr("No se pudo cambiar a la escena del puzzle: ", err.ToString());
		}
	}
}
