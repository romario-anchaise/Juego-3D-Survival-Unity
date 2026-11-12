# Juego 3D Survival — Unity

Proyecto de Unity con una mini casa amueblada, terreno con cerros, pasto 3D,
arboles y ambiente nocturno de terror.

## Abrir el proyecto

1. Clona este repositorio y agregalo a Unity Hub como proyecto existente.
2. Usa **Unity 6000.3.22f1**, con **Universal Render Pipeline (URP)**.
3. Espera a que Unity instale las dependencias de `Packages/manifest.json`.
4. Importa los paquetes externos indicados abajo, conservando sus
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

## Blood Decals (Fab)

Para que aparezcan las texturas de sangre de la casa, descarga
`blooddecals_v10.unitypackage` desde
[Blood Decals - 3 Free Realistic PBR Decals](https://www.fab.com/listings/a4ffffd4-faa8-4e50-9ccd-278f1e7ec45c)
e impórtalo en Unity. Conserva la carpeta `Assets/BloodDecals` y sus archivos
`.meta` originales para que los materiales encuentren las texturas.

## Contenido propio

### Objetos del taller: dependencia local

El taller usa **Low Poly Garage Props - FREE SAMPLE 1.1.2**, de Amano007.
Cada colaborador debe obtener el paquete por su cuenta. Del ZIP, copia los
archivos de `Models/FBX` (los 25 FBX y las dos texturas PNG) directamente a
`Assets/ThirdParty/GarageProps`, conservando los `.meta` del repositorio.
Los materiales URP y las posiciones ya estan configurados en SampleScene.
La licencia permite usarlos en el juego, pero prohibe redistribuir los recursos
sueltos; por eso los FBX y PNG se excluyen de Git. Consulta el `LICENSE.txt`
de esa carpeta. Por ahora son objetos de ambientacion, sin recogida interactiva.

`Taller_Abandonado/Historia_Ambiental` contiene marcas de conteo, un simbolo
repetido dentro y fuera, aceite, sangre seca, un trapo y telaranas discretas.
La lampara del banco reutiliza `OldBulbFlicker` con intervalos de 14-27 segundos;
la luz general permanece encendida para mantener legible el espacio.
La entrada usa un aplique metalico con iluminacion gris verdosa dirigida al
umbral y al camino. El interior tiene menos luz general y sombras en el banco.

Las marcas del culto estan en `Taller_Abandonado/Marcas_Culto`: un pentagrama
interrumpido en la pared y otro parcialmente tapado por cajas, mas un emblema
de circulo abierto y tres trazos descendentes en el tablero y la entrada.
Son mascaras procedurales propias del shader `BloodWall`, con pigmento seco
e irregular; no requieren texturas ni paquetes externos. El parametro
`Marca del culto` queda en 0 para conservar las manchas anteriores.
Los prefabs reutilizables estan en `Assets/FearMansion/AmbienteTerror/Taller`.

La llave inglesa del banco se recoge con E al acercarse y mirar hacia ella,
en primera o tercera persona. `PlayerInventory.HasWorkshopTool` guarda la
recogida durante la partida; se reinicia al comenzar de nuevo la escena.
No puede recogerse a traves de paredes ni duplicarse. Aparece una confirmacion
breve y la herramienta desaparece del banco. Con la llave, vuelve a la reja
iluminada junto al sendero entre las casas y el taller. Apunta al cierre y
mantén E durante 2,5 segundos para aflojarlo. Cruza la reja, apunta a la
puerta del conductor y pulsa E para subir a la camioneta. Tras el arranque,
conduce en primera persona con WASD y sigue los postes hasta el final del
sendero. Espacio frena, el ratón permite mirar alrededor y E baja del vehículo
cuando está detenido. Pulsa R para volver a jugar después de escapar.

El chirrido atrae a El Enraizado desde el lado del taller. Si tiene al jugador
a la vista, lo sigue. Si pierde el rastro, investiga el último lugar donde lo
detectó y vuelve al sendero. Si te alcanza, pulsa R para reiniciar la partida.

La nota del taller, parcialmente cubierta por la caja roja, se lee con E.
El punto central debe apuntar al papel; la herramienta tambien exige apuntado
directo. Ambos objetivos usan triggers ajustados a su tamano y comprueban
distancia y obstaculos desde el jugador y la camara. La nota permanece abierta
sin exigir mantener el punto sobre el papel mientras se lee.
La nota cuenta que el vigilante siguió la voz de su hija al bosque y volvió
cubierto de raíces. Advierte que el chirrido del cierre delata al jugador.
Su primer cierre dispara `WorkshopWatcherEncounter`: falla la luz del banco,
se oye un roce espacial y aparece un vigilante inmovil entre los arboles.
Al verlo se reproduce una textura de tension breve; al apartar la mirada
desaparece. Ocurre una sola vez por partida, sin girar la camara ni perseguir
al jugador. Tambien desaparece si el jugador se acerca a 6 metros, aunque
mantenga la mirada fija. Si nadie lo descubre en 45 segundos, se retira silenciosamente.
Mientras se lee la nota, E no recoge la herramienta simultaneamente.
Los dos WAV de este encuentro se han sintetizado para el proyecto.
Un punto discreto centrado sirve de referencia visual en primera y tercera
persona. Se oculta al leer notas, liberar el cursor o pausar el juego.

El modelo **Sackhead Killer / BagMan** es de **Aredius (Daniel Arias)**.
Cada colaborador debe obtenerlo desde
https://arediuspsx.itch.io/sackhead-killer y copiar `BagMan_Mesh_Tpose.fbx`,
`BagMan_Idle.fbx` y `PSX_BagMan_TEXTURES.png` a `Assets/ThirdParty/BagMan`,
conservando los `.meta` del repositorio. Los originales se excluyen de Git
porque su licencia no permite redistribuirlos; la licencia se conserva junto
a ellos. El autor solicita que se le comunique el uso del personaje.

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
La camara de tercera persona sigue el CameraTarget del viajero desde el hombro
derecho, a 1.15 m de distancia. V alterna entre tercera y primera persona durante el
juego, con el cursor capturado. La primera persona usa `Camara_PrimeraPersona`
a 1.65 m del origen del jugador, con FOV 65 y sin mostrar el cuerpo; conserva
su sombra. A/D permiten desplazarse lateralmente y S retroceder sin girar el
cuerpo. Los cambios de vista son inmediatos para evitar atravesar el modelo.
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

### Linterna y ampliacion hacia el este

La linterna se encuentra sobre la mesa de `Casa_del_Vecino`, la casa a la
izquierda de la principal. Apunta con el punto central y pulsa E para recogerla.
F permite sacarla/encenderla y apagarla/guardarla, en ambas vistas. No se puede
usar antes de recogerla ni recoger dos veces; no consume bateria por ahora.
Tras 18-32 segundos de uso puede sufrir un apagon completo de 0.8-1.3 segundos
y recuperarse suavemente. La intensidad nominal del haz es 52.
El modelo utiliza geometria y materiales propios, con animacion de sacar y guardar.

`Ampliacion_Este` extiende el terreno 50 metros, con un sendero curvo de unos
130 metros, bosque, rocas, tres faroles, una casa reutilizada y un porton
provisional. Este porton reserva el lugar de una futura salida: aun no tiene
interaccion ni final de partida. El sendero parte hacia el sur desde el taller.
La iluminacion ambiental, niebla y vineta se ajustaron para distinguir mejor
el terreno sin perder el ambiente nocturno. Las luces adicionales con sombras
usan resoluciones limitadas para no saturar el atlas.

SampleScene incluye dos terrenos conectados y un camino de tierra hasta un taller
provisional. Cuatro faroles de madera con colisiones guian el recorrido.
En el dormitorio, acercate a la nota del escritorio y pulsa E para leerla;
E o Escape la cierran. El primer cierre reproduce dos pasos en el porche una vez
por partida. La escena incorpora pertenencias junto a la cama y una marca exterior
en el cristal. El enemigo y los objetos de escape siguen pendientes de implementar.

Los faroles Khronos y Paper001 son CC0; Caveat incluye su licencia OFL.
Los titulos de la nota usan Help Me 1.10, de GGBotNet, bajo SIL OFL 1.1.
La fuente original y su licencia se incluyen en `Assets/FearMansion/Fonts/HelpMe`.
El cuerpo mantiene Caveat y las indicaciones de teclas conservan su fuente legible.
Los creditos estan en las carpetas Iluminacion/KhronosLantern y NotaDormitorio,
dentro de Assets/FearMansion. La mochila y los pasos requieren los paquetes
Free Scavenger y Starter Assets descritos arriba.

## Audio narrativo y audifonos

Post Apocalyptic Wastelands (SubspaceAudio / Juhani Junkala, CC0) sustituye el
viento suave por una atmosfera oscura. Su nivel es bajo en exteriores, cae al
18% dentro de casas y se atenúa durante el encuentro del taller. Se transmite
en streaming y se repite con un fundido de cuatro segundos.
El golpe breve conservado en Tension_Aparicion suena al primer instante en que
el jugador mira al enemigo. Los créditos y la licencia están en
`Assets/Audio/Horror/CREDITOS.md`.

Seis focos LED domesticos de Poly Haven sustituyen o completan los puntos de luz
de las tres casas iluminadas. Incluyen roseta, cable y portalamparas; usan
materiales URP de 1K y emision limitada al globo. El foco del bano conserva
el parpadeo, ahora espaciado entre 17 y 29 segundos.

`Sonido_Narrativo` organiza la nueva capa de SampleScene:

- Piano al descubrir el cuerpo cubierto, una vez por partida.
- Voces en el bano al cerrar por primera vez la nota del dormitorio. Esperan
  al menos cinco segundos y respetan la pausa del efecto musical anterior.
- Fallo electrico sincronizado con la lampara del banco del taller, audible de
  cerca y con un intervalo minimo de 23 segundos entre sonidos.
- Crujido y movimiento leve de la puerta de la casa del extremo al entrar
  y darle la espalda, una vez por partida.
- Cuerdas graves al alcanzar la curva del camino este, una vez por partida.
- Seis variantes de pisadas en hojas: ocasionalmente una pisada extra queda
  detras del jugador cuando se detiene tras caminar en el exterior. Pausa de
  40–65 segundos y maximo cuatro apariciones por partida.

Las fuentes del entorno son mono con posicion 3D, caida de volumen por distancia
y sin Doppler. Voces, puerta y electricidad usan un filtro para amortiguar el
sonido tras paredes. Los dos acentos musicales conservan su estereo en 2D.
Los nuevos eventos respetan las notas abiertas y la aparicion del taller;
no sustituyen sus efectos anteriores. Esto usa audio espacial estandar de Unity,
sin un complemento HRTF.

Los originales, conversiones WAV y creditos CC0 estan en `Assets/Audio/Horror`.
Las zonas interiores excluidas para las pisadas se ajustan en el componente
`ForestFollowingSteps` si se mueven o agregan casas.

## Senalizacion rural
Little Wood Signpost, GetDeadEntertainment (CGTrader), modelo OBJ y texturas 1K.
Fuente: https://www.cgtrader.com/free-3d-models/exterior/street-exterior/little-wood-signpost
Tres avisos con colisiones situados fuera del sendero; las puntas indican el avance hacia la casa del extremo y el porton.

