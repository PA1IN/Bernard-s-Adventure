using Godot;
using System;
using System.IO.Ports;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class Node2d : Control
{
	private SerialPort _serialPort;
	private List<int> _pattern = new List<int>(); // Patrón que el jugador debe seguir
	private int _currentIndex = 0; // Índice actual en el patrón
	private int patternLength = 3; // Longitud inicial del patrón
	private const int maxLength = 10; // Longitud máxima del patrón
	private bool isReconnecting = false; // Bandera para evitar reconexiones simultáneas
	private float timeLeft = 30.0f; // Tiempo límite en segundos

	// Referencias a los sprites de los LEDs
	private Sprite2D redLed;
	private Sprite2D blueLed;
	private Sprite2D yellowLed;

	// Referencias a los nodos de audio
	private AudioStreamPlayer successSound;
	private AudioStreamPlayer errorSound;
	private AudioStreamPlayer patternSound;

	// Referencias al cronómetro y etiqueta
	private Timer timer;
	private Label timerLabel;

	public override async void _Ready()
	{
		// Inicializar referencias a los sprites
		redLed = GetNode<Sprite2D>("RedLed");
		blueLed = GetNode<Sprite2D>("BlueLed");
		yellowLed = GetNode<Sprite2D>("YellowLed");

		// Inicializar referencias a los sonidos
		successSound = GetNode<AudioStreamPlayer>("SuccessSound");
		errorSound = GetNode<AudioStreamPlayer>("ErrorSound");
		patternSound = GetNode<AudioStreamPlayer>("PatternSound");

		// Inicializar referencias al cronómetro y etiqueta
		timer = GetNode<Timer>("Timer");
		timerLabel = GetNode<Label>("TimerLabel");
		timer.Connect("timeout", new Callable(this, nameof(OnTimerTimeout)));

		_serialPort = new SerialPort("COM8", 9600); // Cambia "COM8" por el puerto de tu Arduino

		// Esperar un momento antes de comenzar
		await ToSignal(GetTree().CreateTimer(1.0f), "timeout"); // Pausa inicial

		// Inicializar puerto serial con control de errores
		ConnectToSerial();

		// Generar el primer patrón y reproducirlo
		GeneratePattern();
		StartGame();
	}

	private void ConnectToSerial()
	{
		try
		{
			if (_serialPort == null)
			{
				_serialPort = new SerialPort("COM8", 9600); // Cambia "COM8" por el puerto de tu Arduino
			}

			if (!_serialPort.IsOpen)
			{
				_serialPort.Open();
				GD.Print("Conexión serial abierta.");
			}
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Error al abrir el puerto serial: {ex.Message}. Intentando reconectar...");
			isReconnecting = true;
			AttemptReconnect(); // Intentar reconexión
		}
	}

	private async void AttemptReconnect()
	{
		while (isReconnecting)
		{
			await ToSignal(GetTree().CreateTimer(2.0f), "timeout"); // Esperar 2 segundos antes de intentar reconectar
			try
			{
				if (_serialPort != null)
				{
					_serialPort.Close(); // Cerrar el puerto antes de intentar abrirlo nuevamente
				}

				_serialPort = new SerialPort("COM8", 9600); // Reinstanciar el puerto serial
				_serialPort.Open();
				GD.Print("Reconexión serial exitosa.");
				isReconnecting = false; // Reconexión exitosa, detener el bucle
			}
			catch (Exception ex)
			{
				GD.PrintErr($"Reconexión fallida: {ex.Message}. Intentando nuevamente...");
			}
		}
	}

	private void StartGame()
	{
		// Iniciar el cronómetro
		timeLeft = 15.0f; // Tiempo en segundos
		timer.Start(1.0f); // Cronómetro decrece cada segundo
		UpdateTimerLabel();
		PlayPattern();
	}

	private void UpdateTimerLabel()
	{
		timerLabel.Text = $"{Math.Max(0, (int)timeLeft)}(s)";
	}

	private void OnTimerTimeout()
	{
		timeLeft -= 1.0f; // Decrementar tiempo restante
		UpdateTimerLabel();

		if (timeLeft <= 0)
		{
			timer.Stop();
			timerLabel.Text = "¡Tiempo agotado!";
			EndPuzzle(); // Llamar a la función para terminar el minijuego
		}
	}

	private void EndPuzzle()
	{
		// Detener todo el juego y cargar la escena de derrota
		GD.Print("¡Juego terminado por tiempo!");
		if (_serialPort != null && _serialPort.IsOpen)
		{
			_serialPort.Close(); // Cierra la conexión serial
		}
		GetTree().ChangeSceneToFile("res://HasPerdido.tscn"); // Cambiar a la escena de derrota
	}

	public override void _Process(double delta)
	{
		try
		{
			if (_serialPort != null && _serialPort.IsOpen && _serialPort.BytesToRead > 0)
			{
				int maxReads = 5; // Límite de datos a leer por _Process
				int reads = 0;

				while (_serialPort.BytesToRead > 0 && reads < maxReads)
				{
					int buttonPressed = _serialPort.ReadByte();
					buttonPressed -= '0'; // Convertir carácter ASCII a número entero
					GD.Print("Botón presionado: " + buttonPressed);
					HandlePlayerInput(buttonPressed);
					reads++;
				}

				if (reads == maxReads)
				{
					GD.Print("Se alcanzó el límite de lecturas por ciclo, posibles datos acumulados.");
				}
			}
			else if (_serialPort == null || !_serialPort.IsOpen)
			{
				GD.PrintErr("El puerto serial no está abierto. Intentando reconectar...");
				if (!isReconnecting)
				{
					isReconnecting = true;
					AttemptReconnect(); // Intentar reconectar si el puerto no está abierto
				}
			}
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Error al leer del puerto serial: {ex.Message}");
			if (!isReconnecting)
			{
				isReconnecting = true;
				AttemptReconnect(); // Intentar reconexión si hay un error durante la lectura
			}
		}
	}

	private async void HandlePlayerInput(int input)
	{
		try
		{
			if (_pattern.Count == 0 || _currentIndex < 0 || _currentIndex >= _pattern.Count)
			{
				GD.PrintErr("El patrón no está activo o el índice está fuera de rango.");
				return; // Ignorar la entrada
			}
			SendCommandToArduino(input);
			AnimateLed(input, true);
			await ToSignal(GetTree().CreateTimer(0.2f), "timeout");
			AnimateLed(input, false);

			if (input == _pattern[_currentIndex])
			{
				GD.Print($"Jugador acertó el paso {_currentIndex + 1}.");
				_currentIndex++;

				if (_currentIndex >= _pattern.Count)
				{
					_currentIndex = 0;
					AnimateAllLeds(false);
					SendCommandToArduino(0);
					await ToSignal(GetTree().CreateTimer(1.0f), "timeout");

					GD.Print("¡Correcto! Generando nuevo patrón...");

					successSound.Play(); // Reproducir sonido de éxito

					// Reproducir la animación de parpadeo de los LEDs virtuales 3 veces
					for (int c = 0; c < 3; c++) // 3 ciclos de parpadeo
					{
						for (int i = 1; i <= 3; i++) // Recorre los LEDs en secuencia
						{
							AnimateLed(i, true); // Encender el LED correspondiente
							SendCommandToArduino(i); // Enviar comando al Arduino
							await ToSignal(GetTree().CreateTimer(0.15f), "timeout");
							AnimateLed(i, false); // Apagar el LED correspondiente
							SendCommandToArduino(0); // Apagar los LEDs en el Arduino
							await ToSignal(GetTree().CreateTimer(0.15f), "timeout");
						}
					}
					await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
					GeneratePattern();
					PlayPattern();
				}
			}
			else
			{
				GD.Print($"¡Error en el paso {_currentIndex + 1}! Game Over.");

				errorSound.Play(); // Reproducir sonido de error

				// Parpadeo de error: animación de encender y apagar todos los LEDs 3 veces
				for (int i = 0; i < 3; i++)
				{
					AnimateAllLeds(true); // Encender todos los LEDs virtuales
					SendCommandToArduino(4); // Comando especial para encender todos los LEDs físicos
					await ToSignal(GetTree().CreateTimer(0.2f), "timeout");
					AnimateAllLeds(false); // Apagar todos los LEDs virtuales
					SendCommandToArduino(0); // Apagar todos los LEDs físicos
					await ToSignal(GetTree().CreateTimer(0.2f), "timeout");
				}

				await ToSignal(GetTree().CreateTimer(1.0f), "timeout");
				_currentIndex = 0;
				patternLength = 3;
				_pattern.Clear();
				GeneratePattern();
				PlayPattern();
			}
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Error al procesar la entrada del jugador: {ex.Message}");
		}
	}

	private void GeneratePattern()
	{
		var random = new Random();
		_pattern.Clear();

		for (int i = 0; i < patternLength; i++)
		{
			int nextStep;
			do
			{
				nextStep = random.Next(1, 4); // Generar un número entre 1 y 3
			} while (_pattern.Count > 0 && nextStep == _pattern[^1]);

			_pattern.Add(nextStep);
		}

		patternLength = Math.Min(patternLength + 1, maxLength);

		GD.Print("Patrón:");
		foreach (int step in _pattern)
		{
			GD.Print(step);
		}

		_currentIndex = 0;
	}

	private async void PlayPattern()
	{
		var copiapatron = new List<int>(_pattern);
		AnimateAllLeds(false);
		SendCommandToArduino(0);
		await ToSignal(GetTree().CreateTimer(0.5f), "timeout");

		patternSound.Play(); // Reproducir sonido del patrón

		for (int i = 0; i < copiapatron.Count; i++)
		{
			try
			{
				int step = copiapatron[i];
				GD.Print($"Reproduciendo paso: {step}");
				AnimateLed(step, true);
				SendCommandToArduino(step);
				await ToSignal(GetTree().CreateTimer(1.0f), "timeout");

				AnimateLed(step, false);
				SendCommandToArduino(0);
				await ToSignal(GetTree().CreateTimer(0.2f), "timeout");
			}
			catch (Exception ex)
			{
				GD.PrintErr($"Error al reproducir el patrón: {ex.Message}");
				break;
			}
		}

		patternSound.Stop(); // Detener sonido del patrón

		AnimateAllLeds(false);
		SendCommandToArduino(0);
		GD.Print("Patrón reproducido completamente.");
	}

	private void SendCommandToArduino(int command)
	{
		try
		{
			if (_serialPort.IsOpen)
			{
				_serialPort.WriteLine(command.ToString());
				GD.Print("Enviando comando al Arduino: " + command);
			}
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Error al enviar comando al Arduino: {ex.Message}");
			if (!isReconnecting)
			{
				isReconnecting = true;
				AttemptReconnect();
			}
		}
	}

	private void AnimateLed(int step, bool on)
	{
		switch (step)
		{
			case 1:
				redLed.Visible = on;
				break;
			case 2:
				blueLed.Visible = on;
				break;
			case 3:
				yellowLed.Visible = on;
				break;
		}
	}

	private void AnimateAllLeds(bool on)
	{
		redLed.Visible = on;
		blueLed.Visible = on;
		yellowLed.Visible = on;
	}
}
