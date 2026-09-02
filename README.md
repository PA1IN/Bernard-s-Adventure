# Bernard's Space Adventure



**Estudio:** Tylapia Studios
**Categoría:** Casual / Puzle / Indie
**Plataformas:** PC (Windows/Mac)

## Sobre el Juego

Bernard's Space Adventure es un videojuego que combina exploración y resolución de laberintos. El jugador controla a Bernard, un hámster dentro de una bola de plástico especial que tiene la capacidad de modificar la gravedad. El objetivo es escapar de una estación espacial abandonada, de estilo industrial y futurista en Pixel Art, mientras se es perseguido por enemigos.

## Historia

Eres Bernard, el hámster de Dewey de la serie "Malcolm en el medio". Tras ser liberado y recorrer el país en tu bola naranja, te infiltras accidentalmente en una misión de Space X. Ahora, atrapado en el espacio, debes navegar por pasillos oscuros y laboratorios de gravedad cero para encontrar la salida.

## Mecánicas y Control por Hardware (Arduino)

El aspecto más destacado del juego es su control intrínseco mediante hardware personalizado:

* **Movimiento por Giroscopio:** El desplazamiento de Bernard se controla inclinando físicamente una placa Arduino equipada con un módulo giroscopio MPU6050.


* **Puzles de Puertas:** Para acceder al siguiente nivel, el jugador debe superar un minijuego de memoria visual.


* **Hardware "Simón Dice":** Las puertas se hackean replicando patrones mediante una matriz de botones y luces LED instalados en la protoboard del Arduino.


* **Manipulación de Gravedad:** El jugador debe aprovechar los cambios de gravedad para esquivar escombros y zonas de gravedad cero.



## Enemigos

* **Chill Ovni (Nivel 1):** Un robot veloz, estilizado y caricaturesco montado en un platillo volador con luces neón y tonos morados.


* **Mecha Dog (Nivel 2):** Un perro mecánico con exoesqueleto de metal, originalmente diseñado como sistema de defensa, que puede volar y traspasar las paredes de la estación.



## Tecnologías y Herramientas

* **Motor de Juego:** Godot 4.


* **Controlador Físico:** Arduino UNO, módulo giroscopio, botones, resistencias pull-up y LEDs.


* **Desarrollo:** Visual Studio Code.


* **Estilo de Arte:** Pixel Art inspirado en obras como Alien y Dead Space.


* **Audio:** Sintetizadores atmosféricos y ritmos electrónicos que aumentan la tensión durante las persecuciones.



## Equipo de Desarrollo

Proyecto desarrollado en la Escuela de Ingeniería de la Universidad Católica del Norte (UCN Coquimbo) por el equipo Tylapia Studios:

* Ignacio Cruz Reyes


* Nicolás Cordero Varas


* Juan Contreras


* Cristóbal Chepilla Arriagada


* Pablo Solar


* David Ramos Guerra
