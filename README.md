# Juego 3D Survival — Unity

Proyecto de Unity con una mini casa amueblada, terreno con cerros, pasto 3D,
arboles y ambiente nocturno de terror.

## Abrir el proyecto

1. Clona este repositorio y agregalo a Unity Hub como proyecto existente.
2. Usa **Unity 6000.3.22f1**, con **Universal Render Pipeline (URP)**.
3. Espera a que Unity instale las dependencias de `Packages/manifest.json`.
4. Importa los paquetes de Asset Store indicados abajo, conservando sus
   carpetas originales.
5. Abre `Assets/Scenes/SampleScene.unity`.

## Dependencias de Asset Store

Estos paquetes **no se incluyen en el repositorio publico**. Cada colaborador
debe obtenerlos e importarlos desde su cuenta de Unity, conforme a sus licencias.

| Paquete | Version utilizada | Carpeta esperada |
| --- | --- | --- |
| [House Interior - Free, de nappin](https://assetstore.unity.com/packages/3d/props/interior/house-interior-free-258782) | 2.0.0 | `Assets/nappin/HouseInteriorPack` |
| [Low Poly Environment - Nature Free, de Polytope Studio](https://assetstore.unity.com/packages/3d/environments/low-poly-environment-nature-free-lowpoly-medieval-fantasy-series-187052) | 1.1.2 | `Assets/Polytope Studio/Lowpoly_Environments` |
| [Low Poly - Apocalyptic Survivor/Assassin Character, de Shady_3d](https://assetstore.unity.com/packages/3d/characters/low-poly-apocalyptic-survivor-assassin-character-386630) | 1.0 | `Assets/Shady_3d` |
| [First Person + Third Person - Character Controllers, de Unity](https://assetstore.unity.com/packages/3d/characters/first-person-third-person-character-controllers-196526) | Consultar version en My Assets | `Assets/Starter Assets` |
| [AllSky Free - 10 Sky / Skybox Set](https://assetstore.unity.com/packages/2d/textures-materials/sky/allsky-free-10-sky-skybox-set-146014) | Consultar version en My Assets | `Assets/AllSkyFree` |
| [Free Scavenger](https://assetstore.unity.com/packages/3d/characters/humanoids/humans/free-scavenger-261065) | Consultar version en My Assets | `Assets/Homeless_people/Free_Scavenger` |

Para Free Scavenger, selecciona los tres materiales de su carpeta `Materials`
y usa **Edit > Rendering > Materials > Convert Selected Built-in Materials to
Current SRP** para convertirlos a URP. Esta conversion local evita el color rosa;
los materiales del paquete no se distribuyen en Git.

Hasta importar estos paquetes, los muebles y arboles pueden mostrar referencias
de mallas o texturas faltantes. Las carpetas originales y sus `.meta` se excluyen
de Git; conserva los `.meta` que vienen con cada paquete al importarlo.

Las piezas propias incluyen referencias a estos recursos externos. No se
redistribuyen sus modelos ni texturas, ni se otorga una licencia sobre ellos.

## Contenido propio

- `Assets/Editor`: generadores de prefabs, terreno, casa, vegetacion y ambiente.
- `Assets/FearMansion`: piezas de construccion, materiales URP y recursos generados.
- `Assets/Scenes/SampleScene.unity`: escena principal.
- `ProjectSettings` y `Packages`: configuracion reproducible del proyecto.

Los comandos de edicion estan en **Tools > Fear Mansion**. No regeneres piezas
sin revisar antes los cambios manuales de tu escena.

## Estado de este punto de guardado

La escena contiene la mini casa, un Terrain de 100 x 100 metros con cerros,
28 arboles y un ambiente nocturno con niebla. El viajero (`Jugado_Viajero`) usa
Free Scavenger con las animaciones humanoides de Starter Assets y el controlador
propio existente. `PlayerController3D.characterAnimator` apunta al Animator del
hijo `Scavenger Variant`; `PlayerAnimationEvents` reenvia los eventos de pasos
y aterrizaje al controlador del padre. El superviviente anterior queda desactivado.
La camara de tercera persona sigue el CameraTarget del viajero, centrada y a
2.7 metros de distancia. V alterna entre tercera y primera persona durante el
juego, con el cursor capturado. La primera persona usa `Camara_PrimeraPersona`
a 1.65 m del origen del jugador, con FOV 65 y sin mostrar el cuerpo; conserva
su sombra. A/D permiten desplazarse lateralmente y S retroceder sin girar el
cuerpo. Los cambios de vista son inmediatos para evitar atravesar el modelo;
la camara del porche vuelve a participar al regresar a tercera persona.
El cielo usa Night MoonBurst de AllSky Free, con una copia del material ajustada
para la noche. Se conservan la niebla fria y las luces calidas de la casa.

Controles: WASD para caminar, Shift para correr, Espacio para saltar y mouse para
girar la camara. Escape libera el cursor; un clic en Game lo vuelve a capturar.

El prefab reutilizable esta en `Assets/FearMansion/Personaje/Superviviente_TerceraPersona.prefab`.
Necesita los paquetes de personaje y Starter Assets importados. La camara de
seguimiento esta configurada en SampleScene.

Las carpetas `Library`, `Temp`, `Logs`, `UserSettings` y las compilaciones
se generan localmente y no forman parte del repositorio.

## Caserio y primera casa

SampleScene incluye dos terrenos conectados y un camino de tierra hasta un taller
provisional. Cuatro faroles de madera con colisiones guian el recorrido.
En el dormitorio, acercate a la nota del escritorio y pulsa E para leerla;
E o Escape la cierran. El primer cierre reproduce dos pasos en el porche una vez
por partida. La escena incorpora pertenencias junto a la cama y una marca exterior
en el cristal. El enemigo y los objetos de escape siguen pendientes de implementar.

Los faroles Khronos y Paper001 son CC0; Caveat incluye su licencia OFL.
Los creditos estan en las carpetas Iluminacion/KhronosLantern y NotaDormitorio,
dentro de Assets/FearMansion. La mochila y los pasos requieren los paquetes
Free Scavenger y Starter Assets descritos arriba.

## Practicas de las semanas 9 y 10

La guia de entrega y los Journals estan en `Entregas/Semanas9_10/LEEME.md`.
El controlador propio esta en `Assets/Scripts/PlayerController3D.cs`.
Las escenas didacticas y los perfiles de emocion estan en `Assets/Practicas_S09_S10`.
En Play, F1 abre el selector de escenas. La habitacion usa teclas 1-5 y el
laberinto usa P para mostrar la diferencia entre proyeccion y profundidad.

El playtest humano, el dibujo a mano si se exige y la publicacion en el foro
se registran como pendientes del estudiante. Las comprobaciones automaticas
se guardan aparte y no se presentan como observaciones de otros jugadores.
