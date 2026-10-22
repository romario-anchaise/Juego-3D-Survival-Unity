# Juego 3D Survival — Unity

Proyecto de Unity con una mini casa amueblada, terreno con cerros, pasto 3D,
arboles y ambiente nocturno de terror.

## Abrir el proyecto

1. Clona este repositorio y agregalo a Unity Hub como proyecto existente.
2. Usa **Unity 6000.3.22f1**, con **Universal Render Pipeline (URP)**.
3. Espera a que Unity instale las dependencias de `Packages/manifest.json`.
4. Importa los dos paquetes de Asset Store indicados abajo, conservando sus
   carpetas originales.
5. Abre `Assets/Scenes/SampleScene.unity`.

## Dependencias de Asset Store

Estos paquetes **no se incluyen en el repositorio publico**. Cada colaborador
debe obtenerlos e importarlos desde su cuenta de Unity, conforme a sus licencias.

| Paquete | Version utilizada | Carpeta esperada |
| --- | --- | --- |
| [House Interior - Free, de nappin](https://assetstore.unity.com/packages/3d/props/interior/house-interior-free-258782) | 2.0.0 | `Assets/nappin/HouseInteriorPack` |
| [Low Poly Environment - Nature Free, de Polytope Studio](https://assetstore.unity.com/packages/3d/environments/low-poly-environment-nature-free-lowpoly-medieval-fantasy-series-187052) | 1.1.2 | `Assets/Polytope Studio/Lowpoly_Environments` |

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
28 arboles y un ambiente nocturno con niebla. El cambio a personaje en tercera
persona todavia esta pendiente. La posicion de Main Camera necesita revision:
en esta escena se detecto por debajo del terreno.

Las carpetas `Library`, `Temp`, `Logs`, `UserSettings` y las compilaciones
se generan localmente y no forman parte del repositorio.
