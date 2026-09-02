using Godot;
using System;
using System.IO.Ports;  // Para la comunicación con Arduino
using System.Threading; // Para manejar los hilos

public partial class Hamster : CharacterBody2D
{
	public float speed = 1000f;  // Velocidad del personaje
	private Vector2 direction = Vector2.Zero;  // Dirección del movimiento
	private SerialPort serialPort;  // Puerto serie para la comunicación con el Arduino
	private Thread serialThread;  // Hilo para leer el puerto serie
	private string receivedData = "";  // Datos recibidos del Arduino
	private bool shouldStop = false;  // Control para detener el hilo
	private bool isMoving = false;    // Controla si el personaje está en movimiento
	private Sprite2D SPR;
	private AnimationPlayer animation;
  
		
	public override void _Ready()
	{
		// Se configura el puerto serie para conectarse al Arduino
		serialPort = new SerialPort("COM7", 9600); 
		serialPort.ReadTimeout = 500;
		serialPort.Open();

		// Iniciar el hilo para leer los datos del puerto serie
		serialThread = new Thread(new ThreadStart(ReadSerialPort));
		serialThread.Start();
		SPR = GetNode<Sprite2D>("Sprite2D");
		animation = GetNode<AnimationPlayer>("AnimationPlayer");


	}

	// Función que se ejecuta en un hilo separado para leer el puerto serie
	private void ReadSerialPort()
	{
		while (!shouldStop && serialPort.IsOpen)
		{
			try
			{
				// Leer datos del puerto serie
				receivedData = serialPort.ReadLine().Trim(); 
				GD.Print("Dato recibido: " + receivedData);  
			}
			catch (TimeoutException)
			{
			}
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		// Verificar si el personaje está en movimiento
		if (!isMoving)
		{
			// Verificar qué la salida ha llegado del Arduino
			if (receivedData.Trim() == "UP")
			{
				direction = Vector2.Up;
				isMoving = false;				
				GD.Print("Moviendo hacia ARRIBA");
				SPR.FlipH = true;
				animation.Play("right");
				
			}
			else if (receivedData.Trim() == "DOWN")
			{
				direction = Vector2.Down;
				isMoving = false;
				GD.Print("Moviendo hacia ABAJO");
				SPR.FlipH = false;
				animation.Play("left");
			}
			else if (receivedData.Trim() == "LEFT")
			{
				direction = Vector2.Left;
				isMoving = false;
				GD.Print("Moviendo hacia la IZQUIERDA");
				SPR.FlipH = false;
				animation.Play("left");
				
			}
			else if (receivedData.Trim() == "RIGHT")
			{
				direction = Vector2.Right;
				isMoving = false;
				GD.Print("Moviendo hacia la DERECHA");
				SPR.FlipH = true;
				animation.Play("right");
			}
			
			
			
		}
		// Mover el personaje si está en movimiento
		Velocity = direction * speed;
		//UpdateAnimations();
		MoveAndSlide();  // Mover el personaje y manejar colisiones
		// Resetear los datos recibidos después de procesar el movimiento
		receivedData = "";
	}


		/*private void UpdateAnimations()
	{
		if (direction == Vector2.Up)
		{
			animatedSprite.Play("move_up");
		}
		else if (direction == Vector2.Down)
		{
			animatedSprite.Play("move_down");
		}
		else if (direction == Vector2.Left)
		{
			animatedSprite.Play("move_left");
		}
		else if (direction == Vector2.Right)
		{
			animatedSprite.Play("move_right");
		}
		else
		{
			animatedSprite.Play("idle");
		}
	}*/

	// Cuando la escena se cierra o el nodo se borra
	public override void _ExitTree()
	{
		// Detener el hilo de manera segura
		shouldStop = true;

		// Esperar a que el hilo termine antes de cerrar el puerto serie
		if (serialThread.IsAlive)
		{
			serialThread.Join();  // Espera a que el hilo termine
		}

		// Cerrar el puerto serie
		serialPort.Close();
	}
}
