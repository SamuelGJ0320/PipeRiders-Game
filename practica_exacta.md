Práctica - Hipercasuales en Unity3D


Práctica - Hipercasuales en Unity3D 

Introducción 
Los juegos hipercasuales se caracterizan por tener mecánicas simples con gran potencial 
de rejugabilidad y momentos virales. Aplicando los conocimientos adquiridos en clase 
vamos a realizar un prototipo de juego hipercasual utilizando shaders personalizados en el 
motor Unity3D. 

Objetivos de aprendizaje 
● Familiarizarse con el motor de Unity3D que es estándar en la industria de medios 
interactivos y videojuegos 
● Aplicar conocimientos de matemáticas 3D para modelar interacciones para 
mecánicas de videojuegos 
● Escribir shaders personalizados para situaciones específicas dentro del videojuego 
● Interactuar con el motor de físicas en Unity3D para generar interacciones 

Unity3D 
Unity es el motor de videojuegos más utilizado en la industria en particular a nivel nacional, 
es un motor fácil de utilizar para principiantes que permite control de alto nivel a usuarios 
más avanzados. Utiliza como lenguaje C# y soporta la API de gráficos de OpenGL, por lo 
que todos los conocimientos adquiridos hasta el momento pueden ser aplicados en este 
motor. 

Unity tiene una ampla gama de funcionalidades que permiten desarrollar juegos sin 
preocuparse por los componentes de bajo nivel y sus implementaciones específicas, 
algunos ejemplos son: motor de físicas, sonido, animación, inteligencia artificial, etc. 

Juegos hipercasuales 
La última década en la industria de los 
videojuegos ha dado nacimiento a 
diversas tendencias, entre ellas una que 
ha destacado por su potencial comercial 
es la de los juegos hipercasuales en los 
cuales el modelo de interacción es simple 
pero permite una gran rejugabilidad. 

Entre algunos ejemplos podemos 
mencionar el hit Flappy Bird, Crossy 
Road, Jelly Jump, etc. Utilizan una 

estética simple y minimalista llena de 
colores vivos y atractivos. Crossy Road 

https://poki.com/en/g/crossy-road


Mecánica Principal 
La mecánica principal se define como la principal acción o serie de reglas que hacen un 
juego: en Super Mario las mecánicas principales son el movimiento y el salto, que cuando 
se combinan con los enemigos genera el ciclo principal del juego. En el caso de Crossy 
Road las mecánicas principales son el movimiento del personaje y los obstáculos que se 
mueven horizontalmente. Los juegos hipercasuales se caracterizan por tener mecánicas 
simples y minimalistas. 

Shaders 
Los juegos a menudo necesitan shaders personalizados para soportar situaciones 
específicas para cada uno: por ejemplo un juego en el que el jugador puede ver a sus 
enemigos através de las paredes o en los que necesitamos mejorar la visibilidad del 
personaje principal mientras se mueve por espacios que bloquean la cámara. 
Uno de los lenguajes soportados en Unity para escribir Shaders es GLSL que hemos venido 
utilizando en OpenTK. 

Shader de Transparencia    Metaballs para simulación de fluídos 

Para nuestra práctica vamos a escribir 2 o 3 (según la complejidad, se debe evaluar con el 
profesor) shaders personalizados que solucionen un problema o amplifiquen la interacción 
del juego. Vamos a utilizar Shaderlab. 

Entregas 
Las entregas se pueden hacer en equipos de máximo 2 personas, el alcance y nivel de 
polish esperado será más alto para los equipos de 2 personas. Durante la sustentación 
cada integrante deberá explicar su contribución específica al proyecto, en caso de que la 
sustentación no sea satisfactoria ese estudiante tendrá 0 asignado en la nota. 

Las entregas se harán a través de repositorios públicos en Github, el repositorio debe 
contener todo el código fuente del proyecto así mismo como un release que incluya el 
ejecutable para Windows del juego y cualquier documento adicional para esa entrega. 

https://docs.github.com/en/repositories/releasing-projects-on-github/managing-releases-in-a-repository


Se recomienda utilizar Gitflow para que sea más fácil la sustentación donde se pueda 
analizar los features específicos de cada integrante. 

Entrega Parcial: Mecánica Principal 
1. Mecánica Principal del Juego (50%) 
2. Diseño General del Videojuego utilizando la plantilla del One Page Design 
document. (20%) 
3. Diseño de un nivel o escenario jugable en Unity3D donde se demuestre la mecánica 
principal. (utilizar primitivas de Unity para diseñar el nivel con arte que no va a ser el 
final) (30%) 

Entrega Final: Prototipo 
1. Shaders funcionales integrados en el prototipo del juego (40%) 
2. Progresión de niveles o core loop completo para que el juego se pueda seguir 
jugando (puede utilizar generadores procedurales de niveles). (40%) 
3. Utilizar animaciones, sonido, sistemas de particulas y otros efectos para que el juego 
se vea y se sienta mejor y más completo. (20%) 

Recursos 
● https://learn.unity.com/project/unit-1-driving-simulation 
● https://docs.unity3d.com/Manual/Shaders.html 
● https://www.atlassian.com/git/tutorials/comparing-workflows/gitflow-workflow 
● https://docs.github.com/en/repositories/releasing-projects-on-github/managing-releases-in-a-repository 
