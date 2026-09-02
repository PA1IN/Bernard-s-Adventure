using Godot;
using System;
using System.IO.Ports;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class Puzzle : Control
{
	private Label timerLabel;
	private Timer timer;
	private float timeLeft = 15.0f; // Duración del puzzle en segundos
	private List<char> pattern = new List<char>(); // Patrón de LEDs a seguir
	private List<char> playerInput = new List<char>(); // Entradas del jugador
	private int inputIndex = 0; // Índice actual de la entrada del jugador
	private Random random = new Random();
	private SerialPort serialPort;

	// Referencias a los nodos de los sprites
	private Sprite2D ledRojo;
	private Sprite2D ledAmarillo;
	private Sprite2D ledAzul;

	public override void _Ready()
	{
		// Inicializar el puerto serial
		serialPort = new SerialPort("COM8", 9600);
		if (!serialPort.IsOpen)
		{
			serialPort.Open();
			GD.Print("Serial port opened.");
		}

		// Obtener los nodos de los sprites
		ledRojo = GetNode<Sprite2D>("LEDRojo");
		ledAmarillo = GetNode<Sprite2D>("LEDAmarillo");
		ledAzul = GetNode<Sprite2D>("LEDAzul");

		// Inicializar el cronómetro
		timerLabel = GetNode<Label>("TimerLabel");
		timerLabel.Text = $"Tiempo restante: {timeLeft} sg";

		timer = new Timer();
		AddChild(timer);
		timer.WaitTime = 1.0f;
		timer.OneShot = false;
		timer.Connect("timeout", new Callable(this, nameof(OnTimerTimeout)));
		timer.Start();

		GeneratePattern();
	}

	private void GeneratePattern()
	{
		GD.Print("Generating pattern...");
		pattern.Clear();
		for (int i = 0; i < 3; i++) // Generar un patrón de 3 pasos
		{
			char step = (char)(random.Next(1, 4) + '0'); // Convertir números a ASCII ('1', '2', '3')
			pattern.Add(step);
			GD.Print($"Pattern part {i + 1}: LED index {step}");

			HighlightLED(step - '0'); // Encender LED correspondiente
			SendCommandToArduino(step); // Enviar comando al Arduino
			Task.Delay(500).Wait(); // Esperar un momento
			ResetLEDs(); // Apagar LEDs
		}
		AwaitPlayerInput();
	}

	private async void AwaitPlayerInput()
	{
		GD.Print("Waiting for player input...");
		playerInput.Clear();
		inputIndex = 0;

		while (inputIndex < pattern.Count)
		{
			if (serialPort.BytesToRead > 0)
			{
				char input = (char)serialPort.ReadByte(); // Leer carácter ASCII
				GD.Print($"Received input: {input}");
				playerInput.Add(input);
				inputIndex++;

				// Reflejar el botón presionado en el LED correspondiente
				HighlightLED(input - '0');
				await ToSignal(GetTree().CreateTimer(0.5f), "timeout"); // Esperar un momento
				ResetLEDs();
			}
			await ToSignal(GetTree().CreateTimer(0.1f), "timeout"); // Polling delay
		}
		CheckPattern();
	}

	private void CheckPattern()
	{
		GD.Print("Checking pattern...");
		for (int i = 0; i < pattern.Count; i++)
		{
			if (pattern[i] != playerInput[i])
			{
				GD.Print("Pattern incorrect.");
				ShowErrorEffect(); // Mostrar el efecto de error
				return;
			}
		}
		GD.Print("Pattern correct.");
		GeneratePattern(); // Generar un nuevo patrón si es correcto
	}

	private void HighlightLED(int ledIndex)
	{
		ResetLEDs(); // Apagar todos los LEDs primero
		switch (ledIndex)
		{
			case 1:
				ledRojo.Modulate = new Color(1, 0, 0); // Cambiar el color del LED rojo
				break;
			case 2:
				ledAmarillo.Modulate = new Color(1, 1, 0); // Cambiar el color del LED amarillo
				break;
			case 3:
				ledAzul.Modulate = new Color(0, 0, 1); // Cambiar el color del LED azul
				break;
		}
	}

	private void ResetLEDs()
	{
		// Apagar todos los LEDs
		ledRojo.Modulate = new Color(0.2f, 0.2f, 0.2f); // Cambiar el color a apagado
		ledAmarillo.Modulate = new Color(0.2f, 0.2f, 0);
		ledAzul.Modulate = new Color(0, 0, 0.2f);
	}

	private async void ShowErrorEffect()
	{
		GD.Print("Showing error effect...");
		for (int i = 0; i < 3; i++)
		{
			HighlightLED(1); // Parpadear todos los LEDs
			await ToSignal(GetTree().CreateTimer(0.2f), "timeout");
			ResetLEDs();
			await ToSignal(GetTree().CreateTimer(0.2f), "timeout");
		}
		GeneratePattern(); // Reiniciar patrón después del error
	}

	private void SendCommandToArduino(char command)
	{
		if (serialPort.IsOpen)
		{
			serialPort.Write(new[] { command }, 0, 1); // Enviar carácter ASCII
			GD.Print("Enviando comando al Arduino: " + command);
		}
	}

	private void OnTimerTimeout()
	{
		timeLeft -= 1.0f;
		timerLabel.Text = $"Tiempo restante: {timeLeft} sg";
		if (timeLeft <= 0)
		{
			timer.Stop();
			timerLabel.Text = "¡Tiempo agotado!";
			//EndPuzzle();
		}
	}

	private void EndPuzzle()
	{
		serialPort.Close(); // Cierra la conexión serial
		// Carga la escena principal como PackedScene
		PackedScene mainScene = (PackedScene)ResourceLoader.Load("res://Main.tscn");
		if (mainScene != null)
		{
			Node mainInstance = mainScene.Instantiate();
			GetTree().Root.AddChild(mainInstance); // Añadir la instancia de la escena del puzzle al árbol de la escena
			GetTree().SetCurrentScene(mainInstance);
		}
		else
		{
			GD.Print("Error loading scene: res://Main.tscn");
		}
	}
}
