# Pipe Riders Game

Prototipo de juego hipercasual hecho en Unity para la materia de Computacion Grafica.
La idea es simple: avanzar dentro de un tunel, esquivar obstaculos y terminar el nivel dentro del tiempo objetivo.

No es un juego comercial final; es un prototipo academico con enfasis en mecanica principal, shaders y polish visual/sonoro.

## Contexto del proyecto

- Carrera: Ingenieria de Sistemas
- Semestre: 7
- Motor: Unity
- Entrega: Prototipo final

## Mecanica principal

El jugador controla una moto dentro de un tunel con carriles circulares.

- Cambio de carril en tiempo real
- Obstaculos que penalizan velocidad
- Objetivo por tiempo
- Condicion opcional de cero choques para ganar

## Que incluye esta version

- Menu principal hecho por codigo (OnGUI)
- Selector de niveles
- Flujo de juego completo con varios niveles
- Feedback de choque:
	- sonido
	- oscurecimiento de pantalla
	- sistema de particulas de impacto
- Musica de fondo (general y por nivel)
- Shader personalizado para el tunel con progresion de color

## Shader personalizado

Se implemento un shader propio para el tunel:

- Nombre: `Custom/TunnelProgressURP`
- Archivo: `Assets/Shaders/TunnelProgressURP.shader`
- Uso: bandas de color por progreso y lineas de carril
- Integracion: material del tunel + parametros dinamicos desde script

## Controles

### En gameplay

- `A` / `Left Arrow`: mover a la izquierda
- `D` / `Right Arrow`: mover a la derecha
- `W` / `Up Arrow`: acelerar
- `Esc` o `P`: pausar/reanudar

### En menu principal

- `W` / `Up Arrow`: subir opcion
- `S` / `Down Arrow`: bajar opcion
- `Enter` o `Space`: confirmar

## Estructura importante del proyecto

- `Assets/Scripts/LanePlayerControllerCurvo.cs`
	- control del jugador
	- UI por codigo
	- flujo de niveles
	- particulas y audio de choque
- `Assets/Scripts/TunnelGenerator.cs`
	- generacion del tunel
	- obstaculos
	- envio de parametros al shader
- `Assets/Shaders/TunnelProgressURP.shader`
	- shader personalizado del tunel

## Requisitos

- Unity `6000.3.10f1`
- Plataforma objetivo: Windows

## Como ejecutar en editor

1. Abrir el proyecto con Unity `6000.3.10f1`.
2. Cargar la escena `Assets/Scenes/SampleScene.unity`.
3. Verificar referencias del player en Inspector:
	 - `TunnelGenerator`
	 - clips de audio
	 - prefab de particulas de choque (opcional)
4. Ejecutar con Play.

## Build local

En el repositorio ya hay una build de Windows en la carpeta `Builds/`.

## Notas

- El prototipo prioriza jugabilidad y claridad de la mecanica.
- El arte es funcional para sustentar la entrega, no un vertical slice final.
- La configuracion de dificultad se puede ajustar desde Inspector (velocidad, tiempo objetivo, cantidad de obstaculos, etc.).
