# Practicas de las semanas 9 y 10

## Abrir y jugar

La escena principal sigue siendo `Assets/Scenes/SampleScene.unity`. En Play: WASD camina, Shift corre, Espacio salta, mouse gira la vista y Escape libera el cursor. Delante del porche se activa la segunda camara y al alejarse vuelve la de exploracion.

Las escenas de ejercicios estan en `Assets/Practicas_S09_S10/Escenas/`:

| Escena | Actividad |
| --- | --- |
| S09_Orientacion | Artifact y cinco Target invisibles en Game; mover Artifact manualmente desde Scene |
| S09_Laberinto_Imposible | Proyeccion ortografica que superpone tramos separados; Play y P revela la profundidad |
| S10_Habitacion_Emocional | Misma geometria, cinco emociones; botones o teclas 1 a 5 en Play |

Pulsa F1 en Play para mostrar el selector de escenas. Se incluyen en Build Settings. La entrega preparada usa los videos de ambos Katas, una de las alternativas permitidas por las guias. Opcionalmente, Tools > Practicas > Crear build Windows de entrega genera `Builds/Practicas_S09_S10/Practicas.exe`. La compilacion opcional iniciada durante esta sesion se interrumpio antes de terminar, por lo que no se entrega un ejecutable verificado.

## Evidencias

`Evidencias/` contiene capturas reales renderizadas en Unity. Las dos del nivel usan el mismo encuadre; «antes» es una reconstruccion con luces, ambiente y volumen desactivados, no una fotografia historica. El estado final vuelve a restaurarse antes de guardar.

Los archivos Journal_S09.md y Journal_S10.md explican las decisiones y responden las preguntas tecnicas. Validacion_Tecnica.txt contiene solo resultados automatizados, cuando estos se han completado.

## Pendientes que requieren personas o una cuenta

- Mision de orientacion en pareja y playtest mudo de tres minutos: completar Playtest_Pendiente.md con observaciones reales.
- Diagrama a mano si se exige ese formato: usar la jerarquia digital del Journal como referencia.
- Publicar las evidencias o la build en el foro desde la cuenta del estudiante.

No se han inventado participantes, resultados del playtest ni envios al foro.

## Licencias y reproducibilidad

El proyecto usa recursos de Asset Store como dependencias. Sus archivos originales permanecen excluidos de Git. Antes de abrir una copia nueva, importar los paquetes enumerados en el README del proyecto, conservando sus carpetas y sus meta. Los ejercicios de cubos y los perfiles propios estan incluidos.

Las guias solicitan nombres Lab9_Team[N] y Level_01. Se conserva el nombre Survival y SampleScene para mantener las referencias del proyecto existente; son la escena y el proyecto equivalentes de esta entrega.

## Videos listos para entregar

Son comparaciones de renders reales de Unity, sin participantes ni gameplay humano simulado. Los subtitulos SRT identifican cada estado y pueden activarse en el reproductor.

- [Kata S09: Laberinto Imposible](Videos/Kata_S09_Laberinto_Imposible.mp4)
- [Kata S10 y teoria: Habitacion Emocional](Videos/Kata_S10_Habitacion_Emocional.mp4)
- [Nivel antes y despues](Videos/S10_Nivel_Antes_Despues.mp4)

## Comprobaciones realizadas

Validacion_Tecnica.txt registra la prueba automatizada de entrada/salida de la zona de camara, animacion, salto y colision de pared. Validacion_Transferencia.txt verifica que los recursos copiados al proyecto coinciden con los probados en Unity. Esto no equivale al playtest entre compañeros.
